using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.Json;
using System.ComponentModel;
using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using AppCafebookApi.View.common;
using AppCafebookApi.View.Common;

namespace AppCafebookApi.View.nhanvien.pages
{
    public class SachChonUI_Dto : INotifyPropertyChanged
    {
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public decimal GiaBia { get; set; }
        private int _doMoiKhiThue = 100;
        public int DoMoiKhiThue { get => _doMoiKhiThue; set { _doMoiKhiThue = value; OnPropertyChanged(nameof(DoMoiKhiThue)); } }
        private string? _ghiChuKhiThue;
        public string? GhiChuKhiThue { get => _ghiChuKhiThue; set { _ghiChuKhiThue = value; OnPropertyChanged(nameof(GhiChuKhiThue)); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public partial class ThueSachView : Page
    {
        private static readonly HttpClient httpClient;
        private CaiDatThueSachDto _settings = new();
        private PhieuThueChiTietDto? _selectedPhieuChiTiet;
        private int? _idPhieuTraCanIn = null;
        private int _idPhieuGiaHan = 0;

        private List<ChiTietSachTraUI_Dto> _danhSachUI_Tra = new();

        private DispatcherTimer _searchKhachTimer;
        private DispatcherTimer _searchSachTimer;
        private DispatcherTimer _autoRefreshTimer;

        private bool _isUpdatingKhachText = false;

        // Biến tạm cho Popup
        private SachTimKiemDto? _tempSachTimKiem;
        private ChiTietSachTraUI_Dto? _tempSachTraPopup;

        static ThueSachView()
        {
            httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl)) httpClient.BaseAddress = new Uri(apiUrl);
            else httpClient.BaseAddress = new Uri("http://127.0.0.1:5166");
        }

        public ThueSachView()
        {
            InitializeComponent();

            _searchKhachTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchKhachTimer.Tick += async (s, e) => { _searchKhachTimer.Stop(); await SearchKhachHangAsync(); };

            _searchSachTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchSachTimer.Tick += async (s, e) => { _searchSachTimer.Stop(); await SearchSachAsync(); };

            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(300) };
            _autoRefreshTimer.Tick += async (s, e) => { await LoadPhieuThueAsync(true); await LoadPhieuTraAsync(true); };

            dpLocNgayThue.SelectedDate = null;
            dpLocNgayTra.SelectedDate = DateTime.Today;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này.", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // Đảm bảo Panel phải bị thu gọn hoàn toàn lúc load
            ClearRightPanel();

            LoadingOverlay.Visibility = Visibility.Visible;
            await LoadSettingsAsync();
            await LoadPhieuThueAsync(false);
            await LoadPhieuTraAsync(false);
            LoadingOverlay.Visibility = Visibility.Collapsed;

            _autoRefreshTimer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer.Stop();
            foreach (var item in _danhSachUI_Tra) item.PropertyChanged -= ItemTra_PropertyChanged;
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                _settings = await httpClient.GetFromJsonAsync<CaiDatThueSachDto>("api/app/nhanvien/thuesach/settings") ?? new();
                dpNgayHenTra.DisplayDateStart = DateTime.Today;
                dpNgayHenTra.DisplayDateEnd = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);
                dpNgayHenTra.SelectedDate = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);
            }
            catch { }
        }

        #region Logic Đóng Mở Bảng Bên Phải
        private void ClearRightPanel()
        {
            colRightPanel.Width = new GridLength(0);
            borderRightPanel.Visibility = Visibility.Collapsed;
        }

        private void ExpandRightPanel()
        {
            colRightPanel.Width = new GridLength(0.9, GridUnitType.Star);
            borderRightPanel.Visibility = Visibility.Visible;
        }

        private void TabPhieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Chỉ xóa panel nếu người dùng đổi Tab chính (tránh đụng chạm TabControl con)
            if (e.OriginalSource == TabPhieu)
            {
                ClearRightPanel();
                if (dgPhieuThue != null) dgPhieuThue.SelectedIndex = -1;
                if (dgPhieuTra != null) dgPhieuTra.SelectedIndex = -1;
            }
        }
        #endregion

        #region Tối ưu Tải Danh Sách Phân Trang/Ngày
        private async Task LoadPhieuThueAsync(bool isBackground)
        {
            try
            {
                string search = txtSearchPhieuThue.Text;
                string status = (cmbTrangThaiFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Đang Thuê";
                string dateParam = dpLocNgayThue.SelectedDate.HasValue ? $"&tuNgay={dpLocNgayThue.SelectedDate.Value:yyyy-MM-dd}&denNgay={dpLocNgayThue.SelectedDate.Value:yyyy-MM-dd}" : "";

                var url = $"api/app/nhanvien/thuesach/phieuthue?search={Uri.EscapeDataString(search)}&status={Uri.EscapeDataString(status)}{dateParam}";
                var phieuList = await httpClient.GetFromJsonAsync<List<PhieuThueGridDto>>(url);
                dgPhieuThue.ItemsSource = phieuList;
            }
            catch { }
        }

        private async Task LoadPhieuTraAsync(bool isBackground)
        {
            try
            {
                string search = txtSearchPhieuTra.Text;
                string dateParam = dpLocNgayTra.SelectedDate.HasValue ? $"&tuNgay={dpLocNgayTra.SelectedDate.Value:yyyy-MM-dd}&denNgay={dpLocNgayTra.SelectedDate.Value:yyyy-MM-dd}" : "";

                var url = $"api/app/nhanvien/thuesach/phieutra?search={Uri.EscapeDataString(search)}{dateParam}";
                var phieuTraList = await httpClient.GetFromJsonAsync<List<PhieuTraGridDto>>(url);
                dgPhieuTra.ItemsSource = phieuTraList;
            }
            catch { }
        }
        #endregion

        #region Sự kiện Tự động Lọc
        private void TxtSearchPhieuThue_TextChanged(object sender, TextChangedEventArgs e) { if (IsLoaded) _ = LoadPhieuThueAsync(false); }
        private void CmbTrangThaiFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) _ = LoadPhieuThueAsync(false); }
        private void DpLocNgayThue_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) _ = LoadPhieuThueAsync(false); }

        private void TxtSearchPhieuTra_TextChanged(object sender, TextChangedEventArgs e) { if (IsLoaded) _ = LoadPhieuTraAsync(false); }
        private void DpLocNgayTra_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) _ = LoadPhieuTraAsync(false); }
        #endregion

        #region Tìm kiếm Khách Hàng & Sách (Kèm Popup Thêm)
        private void TxtKhachInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingKhachText) return;
            _searchKhachTimer.Stop(); _searchKhachTimer.Start();
        }

        private async Task SearchKhachHangAsync()
        {
            string query = !string.IsNullOrWhiteSpace(txtSdtKH.Text) ? txtSdtKH.Text : txtHoTenKH.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                lbKhachHangResults.ItemsSource = null; lbKhachHangResults.Visibility = Visibility.Collapsed;
                return;
            }
            try
            {
                var results = await httpClient.GetFromJsonAsync<List<KhachHangSearchDto>>($"api/app/nhanvien/thuesach/search-khachhang?query={query}");
                if (results != null && results.Any()) { lbKhachHangResults.ItemsSource = results; lbKhachHangResults.Visibility = Visibility.Visible; }
                else { lbKhachHangResults.Visibility = Visibility.Collapsed; }
            }
            catch { }
        }

        private void LbKhachHangResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbKhachHangResults.SelectedItem is KhachHangSearchDto selected)
            {
                _isUpdatingKhachText = true;
                txtHoTenKH.Text = selected.HoTen;
                txtSdtKH.Text = selected.SoDienThoai;
                txtEmailKH.Text = selected.Email;
                _isUpdatingKhachText = false;
                lbKhachHangResults.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtSearchSach_TextChanged(object sender, TextChangedEventArgs e) { _searchSachTimer.Stop(); _searchSachTimer.Start(); }

        private async Task SearchSachAsync()
        {
            string q = txtSearchSach.Text;
            if (string.IsNullOrEmpty(q)) { lbSachResults.ItemsSource = null; lbSachResults.Visibility = Visibility.Collapsed; return; }
            try
            {
                var results = await httpClient.GetFromJsonAsync<List<SachTimKiemDto>>($"api/app/nhanvien/thuesach/search-sach?query={q}");
                lbSachResults.ItemsSource = results;
                lbSachResults.Visibility = Visibility.Visible;
            }
            catch { }
        }

        // Bấm chọn sách -> Hiện Popup nhập % mới
        private void LbSachResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbSachResults.SelectedItem is SachTimKiemDto selectedSach)
            {
                var currentList = (dgSachChon.ItemsSource as List<SachChonUI_Dto>) ?? new List<SachChonUI_Dto>();
                if (currentList.Any(s => s.IdSach == selectedSach.IdSach))
                {
                    MessageBox.Show("Sách này đã có trong danh sách chọn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    lbSachResults.Visibility = Visibility.Collapsed;
                    return;
                }

                _tempSachTimKiem = selectedSach;
                lblTenSachThuePopup.Text = selectedSach.TenSach;
                txtDoMoiThuePopup.Text = "100";
                txtGhiChuThuePopup.Text = "";
                PopupNhapSachThue.Visibility = Visibility.Visible;

                txtSearchSach.Text = "";
                lbSachResults.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnXacNhanNhapSachThue_Click(object sender, RoutedEventArgs e)
        {
            if (_tempSachTimKiem == null) return;

            if (!int.TryParse(txtDoMoiThuePopup.Text, out int domoi) || domoi <= 0 || domoi > 100)
            {
                MessageBox.Show("Vui lòng nhập độ mới hợp lệ (1 - 100).", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentList = (dgSachChon.ItemsSource as List<SachChonUI_Dto>) ?? new List<SachChonUI_Dto>();
            currentList.Add(new SachChonUI_Dto
            {
                IdSach = _tempSachTimKiem.IdSach,
                TenSach = _tempSachTimKiem.TenSach,
                GiaBia = _tempSachTimKiem.GiaBia,
                DoMoiKhiThue = domoi,
                GhiChuKhiThue = txtGhiChuThuePopup.Text.Trim()
            });

            dgSachChon.ItemsSource = null;
            dgSachChon.ItemsSource = currentList;
            UpdateTongCoc();

            PopupNhapSachThue.Visibility = Visibility.Collapsed;
            _tempSachTimKiem = null;
        }

        private void BtnHuyNhapSachThue_Click(object sender, RoutedEventArgs e)
        {
            PopupNhapSachThue.Visibility = Visibility.Collapsed;
            _tempSachTimKiem = null;
        }

        private void BtnXoaSachChon_Click(object sender, RoutedEventArgs e)
        {
            if (dgSachChon.SelectedItem is SachChonUI_Dto selected && dgSachChon.ItemsSource is List<SachChonUI_Dto> currentList)
            {
                currentList.Remove(selected);
                dgSachChon.ItemsSource = null; dgSachChon.ItemsSource = currentList;
                UpdateTongCoc();
            }
        }

        private void UpdateTongCoc()
        {
            var currentList = (dgSachChon.ItemsSource as List<SachChonUI_Dto>) ?? new List<SachChonUI_Dto>();
            lblTongCoc.Text = $"{currentList.Sum(s => s.GiaBia):N0} đ";
            lblPhiThue.Text = $"{currentList.Count * _settings.PhiThue:N0} đ";
        }
        #endregion

        #region Thao Tác Tạo Phiếu Thuê Mới
        private async void BtnTaoPhieuThue_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CurrentUser == null) return;
            if (string.IsNullOrWhiteSpace(txtHoTenKH.Text)) { MessageBox.Show("Vui lòng nhập Tên khách hàng."); return; }

            var sachList = (dgSachChon.ItemsSource as List<SachChonUI_Dto>);
            if (sachList == null || !sachList.Any()) { MessageBox.Show("Vui lòng chọn ít nhất 1 cuốn sách."); return; }

            var request = new PhieuThueRequestDto
            {
                IdNhanVien = AuthService.CurrentUser.IdNhanVien,
                KhachHangInfo = new KhachHangInfoDto { HoTen = txtHoTenKH.Text, SoDienThoai = string.IsNullOrWhiteSpace(txtSdtKH.Text) ? null : txtSdtKH.Text, Email = string.IsNullOrWhiteSpace(txtEmailKH.Text) ? null : txtEmailKH.Text },
                SachCanThue = sachList.Select(s => new SachThueRequestDto { IdSach = s.IdSach, TienCoc = s.GiaBia, DoMoiKhiThue = s.DoMoiKhiThue, GhiChuKhiThue = s.GhiChuKhiThue }).ToList(),
                NgayHenTra = dpNgayHenTra.SelectedDate ?? DateTime.Today.AddDays(_settings.SoNgayMuonToiDa)
            };

            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/app/nhanvien/thuesach", request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    int idPhieu = jsonDoc.RootElement.GetProperty("idPhieuThueSach").GetInt32();
                    LoadingOverlay.Visibility = Visibility.Collapsed;

                    decimal tongCoc = request.SachCanThue.Sum(s => s.TienCoc);
                    if (tongCoc > 0 && !string.IsNullOrWhiteSpace(_settings.BankId))
                    {
                        new VietQRWindow(_settings.BankId, _settings.BankAccount, _settings.BankAccountName, tongCoc, $"Thanh toan coc PT{idPhieu}").ShowDialog();
                    }

                    ResetFormTaoPhieu();
                    await LoadPhieuThueAsync(false);
                    new PhieuThuePreviewWindow(idPhieu).ShowDialog();
                }
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private void ResetFormTaoPhieu()
        {
            _isUpdatingKhachText = true;
            txtHoTenKH.Text = ""; txtSdtKH.Text = ""; txtEmailKH.Text = "";
            _isUpdatingKhachText = false;
            dgSachChon.ItemsSource = null;
            UpdateTongCoc();
        }
        #endregion

        #region Xử lý Chọn Phiếu & Trả Sách (Popup Đánh Giá)
        private async void DgPhieuThue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPhieuThue.SelectedItem is PhieuThueGridDto selected)
            {
                _idPhieuTraCanIn = null;
                await LoadChiTietPhieuCommon(selected.IdPhieuThueSach, false);
            }
            else { ClearRightPanel(); }
        }

        private async void DgPhieuTra_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPhieuTra.SelectedItem is PhieuTraGridDto selected)
            {
                _idPhieuTraCanIn = selected.IdPhieuTra;
                await LoadChiTietPhieuCommon(selected.IdPhieuThueSach, true);
            }
            else { ClearRightPanel(); }
        }

        private async Task LoadChiTietPhieuCommon(int idPhieuThue, bool isHistoryTab)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                _selectedPhieuChiTiet = await httpClient.GetFromJsonAsync<PhieuThueChiTietDto>($"api/app/nhanvien/thuesach/chitiet/{idPhieuThue}");
                if (_selectedPhieuChiTiet != null)
                {
                    ExpandRightPanel();
                    lblTenKH_ChiTiet.Text = _selectedPhieuChiTiet.HoTenKH;
                    lblSdtKH_ChiTiet.Text = _selectedPhieuChiTiet.SoDienThoaiKH;

                    // KIỂM TRA TRẠNG THÁI: KHÓA FORM NẾU ĐÃ TRẢ HOÀN TOÀN HOẶC Ở TAB LỊCH SỬ
                    if (isHistoryTab || _selectedPhieuChiTiet.TrangThaiPhieu == "Đã Trả")
                    {
                        panelTraSachInput.IsEnabled = false;
                        btnXacNhanTra.Visibility = Visibility.Collapsed;
                        btnGuiNhacHen.Visibility = Visibility.Collapsed;
                        dgSachTra.ItemsSource = _selectedPhieuChiTiet.SachDaThue;
                    }
                    else
                    {
                        panelTraSachInput.IsEnabled = true;
                        btnXacNhanTra.Visibility = Visibility.Visible;

                        var sachChuaTra = _selectedPhieuChiTiet.SachDaThue.Where(s => !s.TinhTrang.Contains("Đã Trả")).ToList();
                        foreach (var item in _danhSachUI_Tra) item.PropertyChanged -= ItemTra_PropertyChanged;

                        _danhSachUI_Tra = sachChuaTra.Select(s => new ChiTietSachTraUI_Dto
                        {
                            IdPhieuThueSach = s.IdPhieuThueSach,
                            IdSach = s.IdSach,
                            TenSach = s.TenSach,
                            TienCoc = s.TienCoc,
                            TienPhat = s.TienPhat,
                            DoMoiKhiThue = s.DoMoiKhiThue,
                            DoMoiKhiTra = s.DoMoiKhiThue,
                            IsSelected = true
                        }).ToList();

                        foreach (var item in _danhSachUI_Tra) item.PropertyChanged += ItemTra_PropertyChanged;
                        dgSachTra.ItemsSource = _danhSachUI_Tra;
                        UpdateTraSachSummary();

                        btnGuiNhacHen.Visibility = sachChuaTra.Any(s => (s.NgayHenTra.Date - DateTime.Today).TotalDays <= 1) ? Visibility.Visible : Visibility.Collapsed;
                    }

                    if (_selectedPhieuChiTiet.DsIdPhieuTra != null && _selectedPhieuChiTiet.DsIdPhieuTra.Any())
                    {
                        btnInPhieuTra.Visibility = Visibility.Visible;
                        if (_idPhieuTraCanIn == null) _idPhieuTraCanIn = _selectedPhieuChiTiet.DsIdPhieuTra.First();
                    }
                    else btnInPhieuTra.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        // --- Logic cho Popup Đánh giá Trả sách ---
        private void BtnMoPopupDanhGiaTra_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ChiTietSachTraUI_Dto item)
            {
                _tempSachTraPopup = item;
                lblTenSachTraPopup.Text = item.TenSach;
                lblDoMoiThuePopup.Text = $"Độ mới lúc thuê là: {item.DoMoiKhiThue}%";
                txtDoMoiTraPopup.Text = item.DoMoiKhiTra.ToString();
                txtGhiChuTraPopup.Text = item.GhiChuKhiTra ?? "";

                PopupDanhGiaTra.Visibility = Visibility.Visible;
            }
        }

        private void BtnXacNhanDanhGiaTra_Click(object sender, RoutedEventArgs e)
        {
            if (_tempSachTraPopup == null) return;

            if (!int.TryParse(txtDoMoiTraPopup.Text, out int domoi) || domoi < 0 || domoi > 100)
            {
                MessageBox.Show("Vui lòng nhập độ mới hợp lệ (0 - 100).", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cập nhật Dto (Sự kiện PropertyChanged sẽ tự chạy để update UI bảng và tổng tiền)
            _tempSachTraPopup.DoMoiKhiTra = domoi;
            _tempSachTraPopup.GhiChuKhiTra = txtGhiChuTraPopup.Text.Trim();

            PopupDanhGiaTra.Visibility = Visibility.Collapsed;
            _tempSachTraPopup = null;
        }

        private void BtnHuyDanhGiaTra_Click(object sender, RoutedEventArgs e)
        {
            PopupDanhGiaTra.Visibility = Visibility.Collapsed;
            _tempSachTraPopup = null;
        }

        private void ItemTra_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChiTietSachTraUI_Dto.IsSelected) || e.PropertyName == nameof(ChiTietSachTraUI_Dto.DoMoiKhiTra))
            {
                if (sender is ChiTietSachTraUI_Dto item)
                {
                    if (item.DoMoiKhiTra > item.DoMoiKhiThue) item.DoMoiKhiTra = item.DoMoiKhiThue;
                    if (item.DoMoiKhiTra < 0) item.DoMoiKhiTra = 0;
                    int diff = item.DoMoiKhiThue - item.DoMoiKhiTra;
                    item.TienPhatHuHong = diff > 0 ? diff * _settings.PhatGiamDoMoi1Percent : 0;
                }
                UpdateTraSachSummary();
            }
        }

        private void UpdateTraSachSummary()
        {
            var selectedSach = _danhSachUI_Tra.Where(s => s.IsSelected).ToList();
            decimal tongPhat = selectedSach.Sum(s => s.TienPhat + s.TienPhatHuHong);
            decimal tongPhi = selectedSach.Count * _settings.PhiThue;
            decimal tongCocHoan = selectedSach.Sum(s => s.TienCoc) - tongPhi - tongPhat;

            lblTongPhat.Text = $"{tongPhat:N0} đ";
            lblTongPhiThue_Tra.Text = $"{tongPhi:N0} đ";
            lblTongCoc_Tra.Text = $"{tongCocHoan:N0} đ";
            lblTongCoc_Tra.Foreground = tongCocHoan < 0 ? (SolidColorBrush)FindResource("ErrorBrush") : (SolidColorBrush)FindResource("SuccessBrush");
        }

        private async void BtnXacNhanTra_Click(object sender, RoutedEventArgs e)
        {
            var selectedSach = _danhSachUI_Tra.Where(s => s.IsSelected).ToList();
            if (selectedSach.Count == 0 || _selectedPhieuChiTiet == null || AuthService.CurrentUser == null) return;

            var request = new TraSachRequestDto
            {
                IdPhieuThueSach = _selectedPhieuChiTiet.IdPhieuThueSach,
                IdNhanVien = AuthService.CurrentUser.IdNhanVien,
                DanhSachTra = selectedSach.Select(s => new TraSachItemRequestDto { IdSach = s.IdSach, DoMoiKhiTra = s.DoMoiKhiTra, GhiChuKhiTra = s.GhiChuKhiTra }).ToList()
            };

            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/app/nhanvien/thuesach/return", request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TraSachResponseDto>();
                    await LoadPhieuThueAsync(false); await LoadPhieuTraAsync(false);
                    ClearRightPanel();
                    if (result != null) new PhieuTraPreviewWindow(result.IdPhieuTra).ShowDialog();
                }
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
            }
            catch { }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region Các chức năng phụ (Gia Hạn, In, Đổi Màu)
        private void DgPhieuThue_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is PhieuThueGridDto item && item.TinhTrang == "Trễ Hạn")
            {
                e.Row.Background = new SolidColorBrush(Color.FromArgb(40, 229, 57, 53)); // Đỏ nhạt
            }
        }

        private void BtnLienHe_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PhieuThueGridDto p) MessageBox.Show($"Khách: {p.HoTenKH}\nSĐT: {p.SoDienThoaiKH}", "Liên Hệ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGiaHan_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PhieuThueGridDto p)
            {
                _idPhieuGiaHan = p.IdPhieuThueSach;
                lblGiaHanInfo.Text = $"Phiếu: {p.IdPhieuThueSach} - {p.HoTenKH}";
                dpGiaHan.DisplayDateStart = DateTime.Today.AddDays(1);
                GiaHanOverlay.Visibility = Visibility.Visible;
            }
        }
        private void BtnHuyGiaHan_Click(object sender, RoutedEventArgs e) { GiaHanOverlay.Visibility = Visibility.Collapsed; }
        private async void BtnXacNhanGiaHan_Click(object sender, RoutedEventArgs e)
        {
            if (dpGiaHan.SelectedDate.HasValue)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                GiaHanOverlay.Visibility = Visibility.Collapsed;
                try
                {
                    var req = new GiaHanRequestDto { IdPhieuThueSach = _idPhieuGiaHan, NgayHenTraMoi = dpGiaHan.SelectedDate.Value };
                    var res = await httpClient.PostAsJsonAsync("api/app/nhanvien/thuesach/extend", req);
                    if (res.IsSuccessStatusCode) { await LoadPhieuThueAsync(false); }
                }
                catch { }
                finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
            }
        }

        private async void BtnGuiNhacHen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChiTiet == null) return;
            LoadingOverlay.Visibility = Visibility.Visible;
            await httpClient.PostAsync($"api/app/nhanvien/thuesach/send-reminder/{_selectedPhieuChiTiet.IdPhieuThueSach}", null);
            LoadingOverlay.Visibility = Visibility.Collapsed;
            MessageBox.Show("Gửi mail nhắc hẹn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnInPhieu_Click(object sender, RoutedEventArgs e) { if (_selectedPhieuChiTiet != null) new PhieuThuePreviewWindow(_selectedPhieuChiTiet.IdPhieuThueSach).ShowDialog(); }
        private void BtnInPhieuTra_Click(object sender, RoutedEventArgs e) { if (_idPhieuTraCanIn.HasValue) new PhieuTraPreviewWindow(_idPhieuTraCanIn.Value).ShowDialog(); }
        #endregion
    }

    public class TinhTrangColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string status = value as string ?? "";
            if (status == "Đúng Hạn") return Brushes.Green;
            if (status == "Trễ Hạn") return Brushes.Red;
            if (status == "Hoàn tất") return Brushes.Gray;
            return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }
}