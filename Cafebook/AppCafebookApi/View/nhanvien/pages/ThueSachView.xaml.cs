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
    public partial class ThueSachView : Page
    {
        private static readonly HttpClient httpClient;
        private CaiDatThueSachDto _settings = new();
        private PhieuThueChiTietDto? _selectedPhieuChiTiet;
        private int? _idPhieuTraCanIn = null;
        private int _idPhieuGiaHan = 0; // Lưu tạm ID khi mở modal gia hạn

        private List<ChiTietSachTraUI_Dto> _danhSachUI_Tra = new();

        private DispatcherTimer _searchKhachTimer;
        private DispatcherTimer _searchSachTimer;
        private DispatcherTimer _searchPhieuTraTimer;
        private DispatcherTimer _autoRefreshTimer;

        private bool _isUpdatingKhachText = false;

        static ThueSachView()
        {
            httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl)) { httpClient.BaseAddress = new Uri(apiUrl); }
            else { httpClient.BaseAddress = new Uri("http://127.0.0.1:5166"); }
        }

        public ThueSachView()
        {
            InitializeComponent();

            _searchKhachTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchKhachTimer.Tick += async (s, e) => { _searchKhachTimer.Stop(); await SearchKhachHangAsync(); };

            _searchSachTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchSachTimer.Tick += async (s, e) => { _searchSachTimer.Stop(); await SearchSachAsync(); };

            _searchPhieuTraTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchPhieuTraTimer.Tick += async (s, e) => { _searchPhieuTraTimer.Stop(); await LoadPhieuTraAsync(false); };

            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoRefreshTimer.Tick += async (s, e) => { await LoadPhieuThueAsync(true); await LoadPhieuTraAsync(true); };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này.", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            ApplyPermissions();

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            await LoadSettingsAsync();
            await LoadPhieuThueAsync(false);
            await LoadPhieuTraAsync(false);
            if (FindName("LoadingOverlay") is Border loadEnd) loadEnd.Visibility = Visibility.Collapsed;

            _autoRefreshTimer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer.Stop();
            foreach (var item in _danhSachUI_Tra) item.PropertyChanged -= ItemTra_PropertyChanged;
        }

        private void ApplyPermissions()
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH"))
            {
                if (FindName("btnTaoPhieuThue") is Button btnTao) btnTao.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                _settings = await httpClient.GetFromJsonAsync<CaiDatThueSachDto>("api/app/nhanvien/thuesach/settings") ?? new();

                if (FindName("dpNgayHenTra") is DatePicker dp)
                {
                    dp.DisplayDateStart = DateTime.Today;
                    dp.DisplayDateEnd = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);
                    dp.SelectedDate = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);
                }
            }
            catch (Exception) { /* Silent Log */ }
        }

        private async Task LoadPhieuThueAsync(bool isBackground)
        {
            try
            {
                string search = "";
                string status = "Đang Thuê";

                if (FindName("txtSearchPhieuThue") is TextBox txtS) search = txtS.Text;
                if (FindName("cmbTrangThaiFilter") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item)
                    status = item.Content.ToString() ?? "Đang Thuê";

                var phieuList = await httpClient.GetFromJsonAsync<List<PhieuThueGridDto>>($"api/app/nhanvien/thuesach/phieuthue?search={Uri.EscapeDataString(search)}&status={Uri.EscapeDataString(status)}");
                if (FindName("dgPhieuThue") is DataGrid dg) dg.ItemsSource = phieuList;
            }
            catch (Exception) { /* Silent Log */ }
        }

        private async Task LoadPhieuTraAsync(bool isBackground)
        {
            try
            {
                string search = "";
                if (FindName("txtSearchPhieuTra") is TextBox txtP) search = txtP.Text;

                var phieuTraList = await httpClient.GetFromJsonAsync<List<PhieuTraGridDto>>($"api/app/nhanvien/thuesach/phieutra?search={Uri.EscapeDataString(search)}");
                if (FindName("dgPhieuTra") is DataGrid dg) dg.ItemsSource = phieuTraList;
            }
            catch (Exception) { /* Silent Log */ }
        }

        private void TxtKhachInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingKhachText) return;
            _searchKhachTimer.Stop(); _searchKhachTimer.Start();
        }

        private async Task SearchKhachHangAsync()
        {
            string query = "";
            if (FindName("txtSdtKH") is TextBox txtSdt && !string.IsNullOrWhiteSpace(txtSdt.Text)) query = txtSdt.Text;
            else if (FindName("txtHoTenKH") is TextBox txtTen && !string.IsNullOrWhiteSpace(txtTen.Text)) query = txtTen.Text;

            if (string.IsNullOrWhiteSpace(query))
            {
                if (FindName("lbKhachHangResults") is ListBox lbHide) { lbHide.ItemsSource = null; lbHide.Visibility = Visibility.Collapsed; }
                return;
            }
            try
            {
                var results = await httpClient.GetFromJsonAsync<List<KhachHangSearchDto>>($"api/app/nhanvien/thuesach/search-khachhang?query={query}");
                if (FindName("lbKhachHangResults") is ListBox lb)
                {
                    if (results != null && results.Any()) { lb.ItemsSource = results; lb.Visibility = Visibility.Visible; }
                    else { lb.Visibility = Visibility.Collapsed; }
                }
            }
            catch (Exception) { }
        }

        private void LbKhachHangResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("lbKhachHangResults") is ListBox lb && lb.SelectedItem is KhachHangSearchDto selected)
            {
                _isUpdatingKhachText = true;
                if (FindName("txtHoTenKH") is TextBox txtTen) txtTen.Text = selected.HoTen;
                if (FindName("txtSdtKH") is TextBox txtSdt) txtSdt.Text = selected.SoDienThoai;
                if (FindName("txtEmailKH") is TextBox txtEmail) txtEmail.Text = selected.Email;
                _isUpdatingKhachText = false;

                lb.Visibility = Visibility.Collapsed;
                lb.ItemsSource = null;
            }
        }

        private void TxtSearchSach_TextChanged(object sender, TextChangedEventArgs e) { _searchSachTimer.Stop(); _searchSachTimer.Start(); }

        private async Task SearchSachAsync()
        {
            string q = "";
            if (FindName("txtSearchSach") is TextBox txtS) q = txtS.Text;
            if (string.IsNullOrEmpty(q))
            {
                if (FindName("lbSachResults") is ListBox lbH) lbH.ItemsSource = null; return;
            }
            try
            {
                var results = await httpClient.GetFromJsonAsync<List<SachTimKiemDto>>($"api/app/nhanvien/thuesach/search-sach?query={q}");
                if (FindName("lbSachResults") is ListBox lb) lb.ItemsSource = results;
            }
            catch { }
        }

        private void LbSachResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("lbSachResults") is ListBox lb && lb.SelectedItem is SachTimKiemDto selectedSach)
            {
                if (FindName("dgSachChon") is DataGrid dg)
                {
                    var currentList = (dg.ItemsSource as List<SachTimKiemDto>) ?? new List<SachTimKiemDto>();
                    if (!currentList.Any(s => s.IdSach == selectedSach.IdSach))
                    {
                        currentList.Add(selectedSach);
                        dg.ItemsSource = null;
                        dg.ItemsSource = currentList;
                        UpdateTongCoc();
                    }
                }
                if (FindName("txtSearchSach") is TextBox txtS) txtS.Text = "";
                lb.ItemsSource = null;
            }
        }

        private void BtnXoaSachChon_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("dgSachChon") is DataGrid dg && dg.SelectedItem is SachTimKiemDto selected && dg.ItemsSource is List<SachTimKiemDto> currentList)
            {
                currentList.Remove(selected);
                dg.ItemsSource = null; dg.ItemsSource = currentList;
                UpdateTongCoc();
            }
        }

        private void UpdateTongCoc()
        {
            if (FindName("dgSachChon") is DataGrid dg)
            {
                var currentList = (dg.ItemsSource as List<SachTimKiemDto>) ?? new List<SachTimKiemDto>();
                decimal tongCoc = currentList.Sum(s => s.GiaBia);
                decimal phiThue = currentList.Count * _settings.PhiThue;

                if (FindName("lblTongCoc") is TextBlock lblCoc) lblCoc.Text = $"{tongCoc:N0} đ";
                if (FindName("lblPhiThue") is TextBlock lblPhi) lblPhi.Text = $"{phiThue:N0} đ";
            }
        }

        private async void BtnTaoPhieuThue_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("Xác nhận thuê sách này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.No) return;

            if (AuthService.CurrentUser == null) { MessageBox.Show("Lỗi phiên."); return; }

            string hoTen = "";
            if (FindName("txtHoTenKH") is TextBox txtTen) hoTen = txtTen.Text;

            if (string.IsNullOrWhiteSpace(hoTen)) { MessageBox.Show("Tên khách hàng là bắt buộc.", "Lỗi"); return; }

            var request = new PhieuThueRequestDto();
            request.IdNhanVien = AuthService.CurrentUser.IdNhanVien;

            string? sdt = null, email = null;
            if (FindName("txtSdtKH") is TextBox tSdt) sdt = string.IsNullOrWhiteSpace(tSdt.Text) ? null : tSdt.Text;
            if (FindName("txtEmailKH") is TextBox tEmail) email = string.IsNullOrWhiteSpace(tEmail.Text) ? null : tEmail.Text;

            request.KhachHangInfo = new KhachHangInfoDto { HoTen = hoTen, SoDienThoai = sdt, Email = email };

            if (FindName("dgSachChon") is DataGrid dg)
            {
                var sachList = (dg.ItemsSource as List<SachTimKiemDto>);
                if (sachList == null || !sachList.Any()) { MessageBox.Show("Chọn ít nhất 1 cuốn sách.", "Lỗi"); return; }
                request.SachCanThue = sachList.Select(s => new SachThueRequestDto { IdSach = s.IdSach, TienCoc = s.GiaBia }).ToList();
            }

            if (FindName("dpNgayHenTra") is DatePicker dp)
                request.NgayHenTra = dp.SelectedDate ?? DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);

            if (FindName("LoadingOverlay") is Border load) load.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/app/nhanvien/thuesach", request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    int idPhieu = jsonDoc.RootElement.GetProperty("idPhieuThueSach").GetInt32();

                    // Tắt loading TRƯỚC khi gọi Dialog QR
                    if (FindName("LoadingOverlay") is Border loadEnd) loadEnd.Visibility = Visibility.Collapsed;

                    decimal tongCoc = request.SachCanThue.Sum(s => s.TienCoc);
                    if (tongCoc > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(_settings.BankId) && !string.IsNullOrWhiteSpace(_settings.BankAccount))
                        {
                            string noiDungChuyenKhoan = $"Thanh toan coc PT{idPhieu:D6}";
                            var qrWindow = new VietQRWindow(_settings.BankId, _settings.BankAccount, _settings.BankAccountName, tongCoc, noiDungChuyenKhoan);
                            qrWindow.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Hệ thống chưa thiết lập thông tin ngân hàng trong cài đặt. Vui lòng thu tiền cọc bằng tiền mặt.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    ResetFormTaoPhieu();
                    await LoadPhieuThueAsync(false);

                    var printWindow = new PhieuThuePreviewWindow(idPhieu);
                    printWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
                    if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
                if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetFormTaoPhieu()
        {
            _isUpdatingKhachText = true;
            if (FindName("txtHoTenKH") is TextBox t1) t1.Text = "";
            if (FindName("txtSdtKH") is TextBox t2) t2.Text = "";
            if (FindName("txtEmailKH") is TextBox t3) t3.Text = "";
            _isUpdatingKhachText = false;

            if (FindName("lbKhachHangResults") is ListBox lb) { lb.ItemsSource = null; lb.Visibility = Visibility.Collapsed; }
            if (FindName("txtSearchSach") is TextBox ts) ts.Text = "";
            if (FindName("lbSachResults") is ListBox lbs) lbs.ItemsSource = null;
            if (FindName("dgSachChon") is DataGrid dg) dg.ItemsSource = null;
            if (FindName("dpNgayHenTra") is DatePicker dp)
            {
                dp.SelectedDate = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);
            }
            UpdateTongCoc();
        }

        private async void TxtSearchPhieuThue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return; await LoadPhieuThueAsync(false);
        }

        private async void CmbTrangThaiFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return; await LoadPhieuThueAsync(false);
        }

        private void TxtSearchPhieuTra_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchPhieuTraTimer.Stop(); _searchPhieuTraTimer.Start();
        }

        private async void DgPhieuThue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhieuThue") is DataGrid dg && dg.SelectedItem is PhieuThueGridDto selectedPhieu)
            {
                _idPhieuTraCanIn = null;
                await LoadChiTietPhieuCommon(selectedPhieu.IdPhieuThueSach, isHistoryTab: false);
            }
            else
            {
                if (FindName("TabPhieu") is TabControl tab && tab.SelectedIndex == 0)
                    if (FindName("panelChiTietPhieu") is StackPanel pnl) pnl.Visibility = Visibility.Collapsed;
            }
        }

        private async void DgPhieuTra_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhieuTra") is DataGrid dg && dg.SelectedItem is PhieuTraGridDto selectedTra)
            {
                _idPhieuTraCanIn = selectedTra.IdPhieuTra;
                await LoadChiTietPhieuCommon(selectedTra.IdPhieuThueSach, isHistoryTab: true);
            }
            else
            {
                if (FindName("TabPhieu") is TabControl tab && tab.SelectedIndex == 1)
                    if (FindName("panelChiTietPhieu") is StackPanel pnl) pnl.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadChiTietPhieuCommon(int idPhieuThue, bool isHistoryTab)
        {
            if (FindName("LoadingOverlay") is Border load) load.Visibility = Visibility.Visible;
            try
            {
                _selectedPhieuChiTiet = await httpClient.GetFromJsonAsync<PhieuThueChiTietDto>($"api/app/nhanvien/thuesach/chitiet/{idPhieuThue}");

                if (_selectedPhieuChiTiet != null && FindName("panelChiTietPhieu") is StackPanel panel)
                {
                    panel.Visibility = Visibility.Visible;
                    if (FindName("lblTenKH_ChiTiet") is TextBlock t1) t1.Text = _selectedPhieuChiTiet.HoTenKH;
                    if (FindName("lblSdtKH_ChiTiet") is TextBlock t2) t2.Text = _selectedPhieuChiTiet.SoDienThoaiKH;

                    if (isHistoryTab || _selectedPhieuChiTiet.TrangThaiPhieu == "Đã Trả")
                    {
                        if (FindName("dgSachTra") is DataGrid dg) dg.ItemsSource = _selectedPhieuChiTiet.SachDaThue;
                        if (FindName("panelTraSach") is StackPanel pnlTra) pnlTra.Visibility = Visibility.Collapsed;
                        if (FindName("btnGuiNhacHen") is Button btnNhac) btnNhac.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var sachChuaTra = _selectedPhieuChiTiet.SachDaThue.Where(s => !s.TinhTrang.Contains("Đã Trả")).ToList();

                        foreach (var item in _danhSachUI_Tra) item.PropertyChanged -= ItemTra_PropertyChanged;

                        _danhSachUI_Tra = sachChuaTra.Select(s => new ChiTietSachTraUI_Dto
                        {
                            IdPhieuThueSach = s.IdPhieuThueSach,
                            IdSach = s.IdSach,
                            TenSach = s.TenSach,
                            TienCoc = s.TienCoc,
                            TienPhat = s.TienPhat,
                            TinhTrang = s.TinhTrang,
                            IsSelected = true
                        }).ToList();

                        foreach (var item in _danhSachUI_Tra) item.PropertyChanged += ItemTra_PropertyChanged;

                        if (FindName("dgSachTra") is DataGrid dg) { dg.ItemsSource = _danhSachUI_Tra; }

                        if (FindName("panelTraSach") is StackPanel pnlTra) pnlTra.Visibility = Visibility.Visible;
                        UpdateTraSachSummary();

                        bool sapTre = sachChuaTra.Any(s => (s.NgayHenTra.Date - DateTime.Today).TotalDays <= 1);
                        if (FindName("btnGuiNhacHen") is Button btnNhac) btnNhac.Visibility = sapTre ? Visibility.Visible : Visibility.Collapsed;
                    }

                    if (FindName("btnInPhieuTra") is Button btnIn)
                    {
                        if (_selectedPhieuChiTiet.DsIdPhieuTra != null && _selectedPhieuChiTiet.DsIdPhieuTra.Any())
                        {
                            btnIn.Visibility = Visibility.Visible;
                            if (_idPhieuTraCanIn == null) _idPhieuTraCanIn = _selectedPhieuChiTiet.DsIdPhieuTra.First();
                            btnIn.Content = $"In Phiếu Trả #{_idPhieuTraCanIn}";
                        }
                        else { btnIn.Visibility = Visibility.Collapsed; }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed; }
        }

        private void ItemTra_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChiTietSachTraUI_Dto.IsSelected)) UpdateTraSachSummary();
        }

        private void UpdateTraSachSummary()
        {
            var selectedSach = _danhSachUI_Tra.Where(s => s.IsSelected).ToList();
            decimal tongPhat = selectedSach.Sum(s => s.TienPhat);
            decimal tongCoc = selectedSach.Sum(s => s.TienCoc);
            decimal tongPhi = selectedSach.Count * _settings.PhiThue;

            if (FindName("lblTongPhat") is TextBlock l1) l1.Text = $"{tongPhat:N0} đ";
            if (FindName("lblTongPhiThue_Tra") is TextBlock l2) l2.Text = $"{tongPhi:N0} đ";
            if (FindName("lblTongCoc_Tra") is TextBlock l3) l3.Text = $"{tongCoc:N0} đ";
        }

        private async void BtnXacNhanTra_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH")) { MessageBox.Show("Không có quyền."); return; }
            if (MessageBox.Show("Xác nhận trả sách?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            var selectedSach = _danhSachUI_Tra.Where(s => s.IsSelected).ToList();
            if (selectedSach.Count == 0 || _selectedPhieuChiTiet == null) { MessageBox.Show("Chọn 1 cuốn sách."); return; }
            if (AuthService.CurrentUser == null) { MessageBox.Show("Lỗi phiên."); return; }

            var request = new TraSachRequestDto
            {
                IdPhieuThueSach = _selectedPhieuChiTiet.IdPhieuThueSach,
                IdSachs = selectedSach.Select(s => s.IdSach).ToList(),
                IdNhanVien = AuthService.CurrentUser.IdNhanVien
            };

            if (FindName("LoadingOverlay") is Border load) load.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/app/nhanvien/thuesach/return", request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TraSachResponseDto>();
                    await LoadPhieuThueAsync(false);
                    await LoadPhieuTraAsync(false);

                    if (FindName("panelChiTietPhieu") is StackPanel pnl) pnl.Visibility = Visibility.Collapsed;
                    if (result != null) { var printW = new PhieuTraPreviewWindow(result.IdPhieuTra); printW.ShowDialog(); }
                }
                else { MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}"); }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed; }
        }

        private void DgPhieuThue_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is PhieuThueGridDto item)
            {
                if (item.TinhTrang == "Trễ Hạn") { e.Row.Background = new SolidColorBrush(Color.FromArgb(50, 239, 83, 80)); e.Row.ToolTip = "Trễ hạn trả sách."; }
                else { e.Row.Background = Brushes.Transparent; e.Row.ToolTip = null; }
            }
        }

        private async void BtnGuiNhacHangLoat_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("LoadingOverlay") is Border load) load.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PostAsync("api/app/nhanvien/thuesach/send-all-reminders", null);
                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    MessageBox.Show(jsonDoc.RootElement.GetProperty("message").GetString(), "Thành công");
                }
                else { MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}"); }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed; }
        }

        private async void BtnGuiNhacHen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChiTiet == null) return;
            if (FindName("LoadingOverlay") is Border load) load.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PostAsync($"api/app/nhanvien/thuesach/send-reminder/{_selectedPhieuChiTiet.IdPhieuThueSach}", null);
                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    MessageBox.Show(jsonDoc.RootElement.GetProperty("message").GetString(), "Thành công");
                }
                else { MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}"); }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed; }
        }

        private void BtnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPhieuChiTiet == null) return;
            new PhieuThuePreviewWindow(_selectedPhieuChiTiet.IdPhieuThueSach).ShowDialog();
        }

        private void BtnInPhieuTra_Click(object sender, RoutedEventArgs e)
        {
            if (_idPhieuTraCanIn.HasValue) new PhieuTraPreviewWindow(_idPhieuTraCanIn.Value).ShowDialog();
            else MessageBox.Show("Không tìm thấy thông tin phiếu trả.", "Lỗi");
        }

        private void BtnLienHe_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PhieuThueGridDto phieu)
            {
                MessageBox.Show($"Tên khách: {phieu.HoTenKH}\nSĐT liên hệ: {phieu.SoDienThoaiKH}", "Thông Tin Khách Hàng", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // TÍNH NĂNG MỚI: MỞ MODAL GIA HẠN CÓ CHỌN NGÀY
        private void BtnGiaHan_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH"))
            {
                MessageBox.Show("Bạn không có quyền Gia hạn phiếu thuê.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((sender as Button)?.DataContext is PhieuThueGridDto phieu)
            {
                _idPhieuGiaHan = phieu.IdPhieuThueSach;

                if (FindName("lblGiaHanInfo") is TextBlock lblInfo)
                    lblInfo.Text = $"Phiếu: PT{phieu.IdPhieuThueSach:D6} - Khách: {phieu.HoTenKH}";

                if (FindName("dpGiaHan") is DatePicker dp)
                {
                    dp.DisplayDateStart = DateTime.Today.AddDays(1); // Ít nhất là mượn tới ngày mai
                    dp.DisplayDateEnd = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa);
                    dp.SelectedDate = DateTime.Today.AddDays(_settings.SoNgayMuonToiDa); // Mặc định fill tối đa
                }

                if (FindName("GiaHanOverlay") is Border overlay) overlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnHuyGiaHan_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("GiaHanOverlay") is Border overlay) overlay.Visibility = Visibility.Collapsed;
        }

        private async void BtnXacNhanGiaHan_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("dpGiaHan") is DatePicker dp && dp.SelectedDate.HasValue)
            {
                var req = new GiaHanRequestDto
                {
                    IdPhieuThueSach = _idPhieuGiaHan,
                    NgayHenTraMoi = dp.SelectedDate.Value
                };

                if (FindName("LoadingOverlay") is Border load) load.Visibility = Visibility.Visible;
                if (FindName("GiaHanOverlay") is Border overlay) overlay.Visibility = Visibility.Collapsed;

                try
                {
                    var response = await httpClient.PostAsJsonAsync("api/app/nhanvien/thuesach/extend", req);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        MessageBox.Show(jsonDoc.RootElement.GetProperty("message").GetString(), "Thành công");

                        await LoadPhieuThueAsync(false);
                        if (_selectedPhieuChiTiet != null && _selectedPhieuChiTiet.IdPhieuThueSach == _idPhieuGiaHan)
                        {
                            await LoadChiTietPhieuCommon(_idPhieuGiaHan, false);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
                finally { if (FindName("LoadingOverlay") is Border loadE) loadE.Visibility = Visibility.Collapsed; }
            }
        }
    }
}