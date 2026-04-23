// [DatBanView.xaml.cs]
using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelApp.NhanVien.DatBan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class DatBanView : Page
    {
        //private static readonly HttpClient httpClient;

        private ObservableCollection<PhieuDatBanDto> PhieuDatBans { get; set; }
        private ObservableCollection<BanDatBanDto> AvailableBans { get; set; }

        private List<PhieuDatBanDto> _allPhieuDatBansCache = new List<PhieuDatBanDto>();
        private List<BanDatBanDto> _allBansCache = new List<BanDatBanDto>();
        private List<KhuVucDatBanDto> _allKhuVucCache = new List<KhuVucDatBanDto>();

        private PhieuDatBanDto? _selectedPhieu;
        private System.Text.Json.JsonSerializerOptions _jsonOptions;

        private DispatcherTimer _searchKhachTimer;
        private DispatcherTimer _autoRefreshTimer;
        private bool _isUpdatingKhachText = false;

        private (TimeSpan Open, TimeSpan Close) _openingHours = (new TimeSpan(6, 0, 0), new TimeSpan(23, 0, 0));
        private List<string> _validHours = new List<string>();
        private List<string> _validMinutes = Enumerable.Range(0, 60).Select(m => m.ToString("00")).ToList();
        /*
        static DatBanView()
        {
            httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                httpClient.BaseAddress = new Uri(apiUrl);
            }
        }
        */
        public DatBanView()
        {
            InitializeComponent();
            PhieuDatBans = new ObservableCollection<PhieuDatBanDto>();
            AvailableBans = new ObservableCollection<BanDatBanDto>();
            this.DataContext = this;

            dgPhieuDatBan.ItemsSource = PhieuDatBans;
            cmbBan.ItemsSource = AvailableBans;

            _jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            _searchKhachTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchKhachTimer.Tick += async (s, e) => { _searchKhachTimer.Stop(); await SearchKhachHangAsync(); };

            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoRefreshTimer.Tick += async (s, e) => await SilentlyRefreshDataAsync();

            cmbSearchTrangThai.SelectedIndex = 0;
            cmbTrangThai.SelectedIndex = 0;
            dpThoiGianDat.SelectedDate = DateTime.Now;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_DAT_BAN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập trang Đặt Bàn!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack) this.NavigationService.GoBack();
                return;
            }

            if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
            {
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            if (ApiClient.Instance.BaseAddress == null)
            {
                MessageBox.Show("Chưa cấu hình URL Server.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadAllDataAsync();
            _autoRefreshTimer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer.Stop();
            _searchKhachTimer.Stop();
        }

        private void InitializeTimePickers(bool useCache)
        {
            if (useCache)
            {
                _validHours.Clear();
                int gioBatDau = _openingHours.Open.Hours;
                int gioCuoiCung = _openingHours.Close.Hours - 1;

                for (int h = gioBatDau; h <= gioCuoiCung; h++)
                {
                    _validHours.Add(h.ToString("00"));
                }
            }
            else
            {
                _validHours = Enumerable.Range(0, 24).Select(h => h.ToString("00")).ToList();
            }

            cmbHour.ItemsSource = _validHours;
            cmbMinute.ItemsSource = _validMinutes;

            var now = DateTime.Now;
            string currentHour = now.ToString("HH");

            if (_validHours.Contains(currentHour)) cmbHour.Text = currentHour;
            else if (now.TimeOfDay < _openingHours.Open) cmbHour.Text = _openingHours.Open.ToString("hh");
            else cmbHour.Text = _validHours.LastOrDefault() ?? "00";

            cmbMinute.Text = now.ToString("mm");
            dpThoiGianDat.SelectedDate = now.Date;
        }

        #region Tải và Lọc Dữ Liệu

        private async Task LoadAllDataAsync()
        {
            if (AuthService.CurrentUser == null || string.IsNullOrEmpty(AuthService.AuthToken))
            {
                return;
            }
            LoadingOverlay.Visibility = Visibility.Visible;

            // Đợi tất cả API tải xong hoàn toàn
            await Task.WhenAll(
                LoadOpeningHoursAsync(),
                LoadPhieuDatBansAsync(),
                LoadAvailableBansAsync(),
                LoadKhuVucAsync()
            );

            // FIX RACE CONDITION: Sau khi chắc chắn Bàn và Khu vực đã có data cache, mới chọn SelectedIndex = 0
            if (cmbFilterKhuVuc_Form.Items.Count > 0)
            {
                cmbFilterKhuVuc_Form.SelectedIndex = 0; // Dòng này sẽ tự kích hoạt CmbFilterKhuVuc_Form_SelectionChanged
            }
            else
            {
                CmbFilterKhuVuc_Form_SelectionChanged(null!, null!);
            }

            InitializeTimePickers(true);
            ApplyFilter();
            ClearForm();
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private async Task SilentlyRefreshDataAsync()
        {
            if (AuthService.CurrentUser == null || string.IsNullOrEmpty(AuthService.AuthToken))
            {
                _autoRefreshTimer?.Stop();
                return;
            }
            try
            {
                await LoadPhieuDatBansAsync();
                await LoadAvailableBansAsync();
                ApplyFilter();
                // Làm mới lại danh sách bàn đang trống theo khu vực hiện tại
                CmbFilterKhuVuc_Form_SelectionChanged(null!, null!);
            }
            catch { }
        }

        private async Task LoadPhieuDatBansAsync()
        {
            try
            {
                var data = await ApiClient.Instance.GetFromJsonAsync<List<PhieuDatBanDto>>("api/app/datban/list", _jsonOptions);
                _allPhieuDatBansCache.Clear();
                if (data != null) _allPhieuDatBansCache = data;
            }
            catch (Exception ex) { Debug.WriteLine("Lỗi phiếu: " + ex.Message); }
        }

        private async Task LoadAvailableBansAsync()
        {
            try
            {
                var data = await ApiClient.Instance.GetFromJsonAsync<List<BanDatBanDto>>("api/app/datban/available-bans", _jsonOptions);
                _allBansCache.Clear();
                if (data != null) _allBansCache = data;
            }
            catch (Exception ex) { Debug.WriteLine("Lỗi bàn: " + ex.Message); }
        }

        private async Task LoadKhuVucAsync()
        {
            try
            {
                _allKhuVucCache = (await ApiClient.Instance.GetFromJsonAsync<List<KhuVucDatBanDto>>("api/app/datban/khuvuc", _jsonOptions)) ?? new List<KhuVucDatBanDto>();

                var filterList = new List<KhuVucDatBanDto>(_allKhuVucCache);
                filterList.Insert(0, new KhuVucDatBanDto { IdKhuVuc = 0, TenKhuVuc = "Tất cả Khu vực" });

                cmbFilterKhuVuc_Form.ItemsSource = filterList;
                cmbFilterKhuVuc_Form.DisplayMemberPath = "TenKhuVuc";
                cmbFilterKhuVuc_Form.SelectedValuePath = "IdKhuVuc";
            }
            catch (Exception ex) { Debug.WriteLine("Lỗi khu vực: " + ex.Message); }
        }

        private async Task LoadOpeningHoursAsync()
        {
            try
            {
                string settingValue = await ApiClient.Instance.GetStringAsync("api/app/datban/opening-hours");
                ParseOpeningHoursClient(settingValue);
            }
            catch
            {
                ParseOpeningHoursClient("07:00 - 22:00");
            }
        }

        private void ParseOpeningHoursClient(string settingValue)
        {
            try
            {
                var match = Regex.Match(settingValue, @"(\d{2}:\d{2})\s*-\s*(\d{2}:\d{2})");
                if (match.Success)
                {
                    TimeSpan.TryParse(match.Groups[1].Value, out TimeSpan open);
                    TimeSpan.TryParse(match.Groups[2].Value, out TimeSpan close);
                    _openingHours = (open, close);
                }
            }
            catch { _openingHours = (new TimeSpan(7, 0, 0), new TimeSpan(22, 0, 0)); }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e) { ApplyFilter(); }
        private void TxtSearch_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ApplyFilter(); }

        private void BtnShowHistory_Click(object sender, RoutedEventArgs e)
        {
            bool showHistory = btnShowHistory.IsChecked == true;
            if (showHistory)
            {
                btnShowHistory.Content = "Hiện Đơn Đang Chờ";
                cmbSearchTrangThai.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnShowHistory.Content = "Hiện Lịch Sử Hủy";
                cmbSearchTrangThai.Visibility = Visibility.Visible;
                cmbSearchTrangThai.SelectedIndex = 0;
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allPhieuDatBansCache == null) return;
            IEnumerable<PhieuDatBanDto> filteredList;
            bool showHistory = btnShowHistory.IsChecked == true;

            if (showHistory)
            {
                filteredList = _allPhieuDatBansCache.Where(p => p.TrangThai == "Đã hủy" || p.TrangThai == "Khách đã đến");
            }
            else
            {
                var trangThaiFilter = (cmbSearchTrangThai.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
                if (trangThaiFilter == "Tất cả")
                    filteredList = _allPhieuDatBansCache.Where(p => p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đã xác nhận");
                else
                    filteredList = _allPhieuDatBansCache.Where(p => p.TrangThai == trangThaiFilter);
            }

            var filterText = txtSearch.Text.ToLower().Trim();
            if (!string.IsNullOrEmpty(filterText))
            {
                filteredList = filteredList.Where(p => (p.TenKhachHang != null && RemoveDiacritics(p.TenKhachHang.ToLower()).Contains(RemoveDiacritics(filterText))) ||
                                                       (p.SoDienThoai != null && p.SoDienThoai.Contains(filterText)));
            }

            var filterDate = dpSearchDate.SelectedDate;
            if (filterDate.HasValue) filteredList = filteredList.Where(p => p.ThoiGianDat.Date == filterDate.Value.Date);

            filteredList = showHistory ? filteredList.OrderByDescending(p => p.ThoiGianDat) : filteredList.OrderBy(p => p.ThoiGianDat);

            var selectedId = _selectedPhieu?.IdPhieuDatBan;

            PhieuDatBans.Clear();
            foreach (var item in filteredList) PhieuDatBans.Add(item);

            if (selectedId.HasValue)
            {
                dgPhieuDatBan.SelectedItem = PhieuDatBans.FirstOrDefault(p => p.IdPhieuDatBan == selectedId.Value);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            dpSearchDate.SelectedDate = null;
            cmbSearchTrangThai.SelectedIndex = 0;
            btnShowHistory.IsChecked = false;
            await LoadAllDataAsync();
        }

        #endregion

        #region Xử lý Form (Thêm/Sửa/Xóa)

        private void DgPhieuDatBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPhieu = dgPhieuDatBan.SelectedItem as PhieuDatBanDto;
            if (_selectedPhieu == null)
            {
                ClearForm();
                return;
            }

            _isUpdatingKhachText = true;
            txtTenKhach.Text = _selectedPhieu.TenKhachHang;
            txtSdtKH.Text = _selectedPhieu.SoDienThoai;
            txtEmailKH.Text = _selectedPhieu.Email;
            _isUpdatingKhachText = false;

            lbKhachHangResults.Visibility = Visibility.Collapsed;
            lbKhachHangResults.ItemsSource = null;

            dpThoiGianDat.SelectedDate = _selectedPhieu.ThoiGianDat;
            cmbHour.Text = _selectedPhieu.ThoiGianDat.ToString("HH");
            cmbMinute.Text = _selectedPhieu.ThoiGianDat.ToString("mm");
            txtSoLuongKhach.Text = _selectedPhieu.SoLuongKhach.ToString();
            txtGhiChu.Text = _selectedPhieu.GhiChu;

            foreach (ComboBoxItem item in cmbTrangThai.Items)
            {
                if (item.Content.ToString() == _selectedPhieu.TrangThai)
                {
                    cmbTrangThai.SelectedItem = item; break;
                }
            }

            CmbFilterKhuVuc_Form_SelectionChanged(null!, null!);

            if (!AvailableBans.Any(b => b.IdBan == _selectedPhieu.IdBan))
            {
                AvailableBans.Add(new BanDatBanDto { 
                    IdBan = _selectedPhieu.IdBan, 
                    SoBan = _selectedPhieu.SoBan, 
                    TenKhuVuc = _selectedPhieu.TenKhuVuc, 
                    IdKhuVuc = 0, 
                    SoGhe = _selectedPhieu.SoGhe 
                });
            }
            cmbBan.SelectedValue = _selectedPhieu.IdBan;

            if (_selectedPhieu.TrangThai == "Đã xác nhận")
            {
                btnXacNhanDen_Form.Visibility = Visibility.Visible;
                menuXacNhanDen.IsEnabled = true; menuHuyPhieu.IsEnabled = true;
            }
            else if (_selectedPhieu.TrangThai == "Chờ xác nhận")
            {
                btnXacNhanDen_Form.Visibility = Visibility.Collapsed;
                menuXacNhanDen.IsEnabled = true; menuHuyPhieu.IsEnabled = true;
            }
            else
            {
                btnXacNhanDen_Form.Visibility = Visibility.Collapsed;
                menuXacNhanDen.IsEnabled = false; menuHuyPhieu.IsEnabled = false;
            }
            btnSua.IsEnabled = true; btnXoa.IsEnabled = true;
        }

        private void ClearForm()
        {
            _selectedPhieu = null;

            _isUpdatingKhachText = true;
            txtTenKhach.Text = string.Empty;
            txtSdtKH.Text = string.Empty;
            txtEmailKH.Text = string.Empty;
            _isUpdatingKhachText = false;

            lbKhachHangResults.ItemsSource = null;
            lbKhachHangResults.Visibility = Visibility.Collapsed;
            cmbBan.SelectedIndex = -1;
            cmbBan.Text = string.Empty;

            dpThoiGianDat.SelectedDate = DateTime.Now.Date;
            cmbHour.Text = DateTime.Now.ToString("HH");
            cmbMinute.Text = DateTime.Now.ToString("mm");

            txtSoLuongKhach.Text = "1";
            cmbTrangThai.SelectedIndex = 0;
            txtGhiChu.Text = string.Empty;
            dgPhieuDatBan.SelectedIndex = -1;

            btnSua.IsEnabled = false; btnXoa.IsEnabled = false;
            btnXacNhanDen_Form.Visibility = Visibility.Collapsed;
            menuXacNhanDen.IsEnabled = false; menuHuyPhieu.IsEnabled = false;
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private bool ValidateForm(out PhieuDatBanCreateUpdateDto? dto)
        {
            dto = null;
            if (AuthService.CurrentUser == null) { MessageBox.Show("Phiên đăng nhập hết hạn."); return false; }

            if (string.IsNullOrWhiteSpace(txtTenKhach.Text) || string.IsNullOrWhiteSpace(txtSdtKH.Text) ||
                cmbBan.SelectedValue == null || dpThoiGianDat.SelectedDate == null ||
                string.IsNullOrWhiteSpace(cmbHour.Text) || string.IsNullOrWhiteSpace(cmbMinute.Text) ||
                string.IsNullOrWhiteSpace(txtSoLuongKhach.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ các trường bắt buộc (*)."); return false;
            }

            if (!int.TryParse(cmbHour.Text, out int hour) || hour < 0 || hour > 23) { MessageBox.Show("Giờ không hợp lệ."); return false; }
            if (!int.TryParse(cmbMinute.Text, out int minute) || minute < 0 || minute > 59) { MessageBox.Show("Phút không hợp lệ."); return false; }

            DateTime selectedDate = dpThoiGianDat.SelectedDate.Value;
            DateTime thoiGianDat = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, minute, 0);

            if (thoiGianDat < DateTime.Now.AddMinutes(5))
            {
                MessageBox.Show("Thời gian đặt phải trong tương lai.", "Lỗi"); return false;
            }

            var timeOfDay = thoiGianDat.TimeOfDay;
            if (timeOfDay < _openingHours.Open || timeOfDay > _openingHours.Close)
            {
                MessageBox.Show($"Giờ đặt nằm ngoài giờ mở cửa ({_openingHours.Open:hh\\:mm} - {_openingHours.Close:hh\\:mm})."); return false;
            }

            var selectedBan = cmbBan.SelectedItem as BanDatBanDto;
            int soKhach = int.Parse(txtSoLuongKhach.Text);
            if (selectedBan != null && soKhach > selectedBan.SoGhe)
            {
                MessageBox.Show($"Số lượng khách ({soKhach}) vượt quá số ghế của bàn ({selectedBan.SoGhe})."); return false;
            }

            dto = new PhieuDatBanCreateUpdateDto
            {
                TenKhachHang = txtTenKhach.Text.Trim(),
                SoDienThoai = txtSdtKH.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmailKH.Text) ? null : txtEmailKH.Text.Trim(),
                IdBan = (int)cmbBan.SelectedValue,
                ThoiGianDat = thoiGianDat,
                SoLuongKhach = soKhach,
                GhiChu = txtGhiChu.Text.Trim(),
                TrangThai = (cmbTrangThai.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Đã xác nhận",
                IdNhanVienTao = AuthService.CurrentUser.IdNhanVien
            };
            return true;
        }

        private async void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(out var dto)) return;
            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/datban/create-staff", dto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Thêm phiếu đặt bàn thành công.");
                    await LoadAllDataAsync();
                }
                else MessageBox.Show("Lỗi tạo phiếu: " + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi API: " + ex.Message); }
        }

        private async void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieu == null || !ValidateForm(out var dto) || dto == null) return;
            dto.IdPhieuDatBan = _selectedPhieu.IdPhieuDatBan;
            try
            {
                var response = await ApiClient.Instance.PutAsJsonAsync($"api/app/datban/update/{dto.IdPhieuDatBan}", dto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật phiếu thành công.");
                    await LoadAllDataAsync();
                }
                else MessageBox.Show("Lỗi cập nhật: " + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi API: " + ex.Message); }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieu == null) { MessageBox.Show("Vui lòng chọn phiếu để xóa."); return; }
            if (MessageBox.Show($"Chắc chắn xóa phiếu của '{_selectedPhieu.TenKhachHang}'?", "Xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
            await HuyPhieu(_selectedPhieu.IdPhieuDatBan, true);
        }

        private void MenuSuaPhieu_Click(object sender, RoutedEventArgs e) { if (_selectedPhieu == null) MessageBox.Show("Chọn phiếu để sửa."); }
        private void MenuXoaPhieu_Click(object sender, RoutedEventArgs e) { BtnXoa_Click(sender, e); }

        #endregion

        #region Xử lý Xác nhận / Hủy

        private async void BtnXacNhanDen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieu == null) return;
            if (AuthService.CurrentUser == null) { MessageBox.Show("Phiên đăng nhập hết hạn."); return; }
            if (_selectedPhieu.TrangThai != "Đã xác nhận") { MessageBox.Show("Chỉ xác nhận cho phiếu 'Đã xác nhận'."); return; }

            var request = new XacNhanKhachDenRequestDto { IdPhieuDatBan = _selectedPhieu.IdPhieuDatBan, IdNhanVien = AuthService.CurrentUser.IdNhanVien };
            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/datban/xacnhan-den", request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<XacNhanKhachDenResponseDto>(_jsonOptions);
                    MessageBox.Show("Đã xác nhận khách đến. Chuyển đến Sơ đồ bàn...");

                    if (this.NavigationService != null && result != null)
                        this.NavigationService.Navigate(new SoDoBanView(_selectedPhieu.IdBan));
                    else if (Application.Current.MainWindow is ManHinhNhanVien mainWindow && result != null)
                        mainWindow.MainFrame.Navigate(new SoDoBanView(_selectedPhieu.IdBan));
                }
                else MessageBox.Show("Lỗi xác nhận: " + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi API: " + ex.Message); }
        }

        private async void BtnHuyPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieu == null) return;
            if (MessageBox.Show($"Bạn có chắc muốn HỦY phiếu của '{_selectedPhieu.TenKhachHang}'?", "Hủy", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
            await HuyPhieu(_selectedPhieu.IdPhieuDatBan, false);
        }

        private async Task HuyPhieu(int idPhieu, bool isDelete)
        {
            try
            {
                var response = await ApiClient.Instance.PostAsync($"api/app/datban/huy/{idPhieu}", null);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(isDelete ? "Xóa (hủy) phiếu thành công." : "Hủy phiếu thành công.");
                    await LoadAllDataAsync();
                }
                else MessageBox.Show("Lỗi hủy phiếu: " + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi API: " + ex.Message); }
        }

        #endregion

        #region Helpers Form (Tìm kiếm, Input)

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); e.Handled = regex.IsMatch(e.Text);
        }

        private void CmbFilterKhuVuc_Form_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AvailableBans.Clear();
            var selectedKhuVuc = cmbFilterKhuVuc_Form.SelectedItem as KhuVucDatBanDto;
            IEnumerable<BanDatBanDto> filteredBans = _allBansCache;

            if (selectedKhuVuc != null && selectedKhuVuc.IdKhuVuc > 0)
                filteredBans = _allBansCache.Where(b => b.IdKhuVuc == selectedKhuVuc.IdKhuVuc);

            foreach (var ban in filteredBans.OrderBy(b => b.SoBan))
            {
                AvailableBans.Add(ban);
            }

            if (cmbBan.SelectedValue != null && !AvailableBans.Any(b => b.IdBan == (int)cmbBan.SelectedValue))
            {
                cmbBan.SelectedIndex = -1;
                cmbBan.Text = string.Empty;
            }
        }

        // Hàm helper để loại bỏ dấu tiếng Việt
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }

        private void CmbBan_KeyUp(object sender, KeyEventArgs e)
        {
            if (cmbBan.ItemsSource == null) cmbBan.ItemsSource = AvailableBans;
            if (e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Escape || e.Key == Key.Left || e.Key == Key.Right) return;

            string searchText = RemoveDiacritics(cmbBan.Text.ToLower());
            var selectedKhuVucId = (cmbFilterKhuVuc_Form.SelectedValue as int?) ?? 0;

            var filteredBans = _allBansCache
                .Where(b => (selectedKhuVucId == 0 || b.IdKhuVuc == selectedKhuVucId))
                .Where(b => b.SoBan != null && RemoveDiacritics(b.SoBan.ToLower()).Contains(searchText));

            AvailableBans.Clear();
            foreach (var ban in filteredBans.OrderBy(b => b.SoBan))
            {
                AvailableBans.Add(ban);
            }
            cmbBan.IsDropDownOpen = true;
        }

        private void TxtKhachInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingKhachText) return;
            _searchKhachTimer.Stop();
            _searchKhachTimer.Start();
        }

        private async Task SearchKhachHangAsync()
        {
            string query = !string.IsNullOrWhiteSpace(txtSdtKH.Text) ? txtSdtKH.Text : txtTenKhach.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                lbKhachHangResults.ItemsSource = null;
                lbKhachHangResults.Visibility = Visibility.Collapsed;
                return;
            }
            try
            {
                var results = await ApiClient.Instance.GetFromJsonAsync<List<KhachHangLookupDto>>($"api/app/datban/search-customer?query={query}", _jsonOptions);
                if (results != null && results.Any())
                {
                    lbKhachHangResults.ItemsSource = results;
                    lbKhachHangResults.Visibility = Visibility.Visible;
                }
                else
                {
                    lbKhachHangResults.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void LbKhachHangResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbKhachHangResults.SelectedItem is KhachHangLookupDto selected)
            {
                _isUpdatingKhachText = true;
                txtTenKhach.Text = selected.HoTen;
                txtSdtKH.Text = selected.SoDienThoai;
                txtEmailKH.Text = selected.Email;
                _isUpdatingKhachText = false;

                lbKhachHangResults.Visibility = Visibility.Collapsed;
                lbKhachHangResults.ItemsSource = null;
            }
        }

        private void CmbTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Escape) return;
            var cmb = sender as ComboBox; if (cmb == null) return;
            var source = (cmb.Name == "cmbHour") ? _validHours : _validMinutes;
            string searchText = cmb.Text;
            var filteredList = source.Where(t => t.StartsWith(searchText)).ToList();
            cmb.ItemsSource = filteredList; cmb.IsDropDownOpen = true;
        }

        #endregion
    }

    public class TrangThaiDatBanToBrushConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? trangThai = value as string;
            if (trangThai == null) return Brushes.Gray;
            switch (trangThai)
            {
                case "Chờ xác nhận": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA726"));
                case "Đã xác nhận": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#42A5F5"));
                case "Khách đã đến": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66BB6A"));
                case "Đã hủy": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF5350"));
                default: return Brushes.LightGray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}