using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CafebookModel.Model.ModelApp.QuanLy;
using AppCafebookApi.Services;
using CafebookModel.Utils;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyBanView : Page
    {
        private static readonly HttpClient httpClient;

        private List<QuanLyKhuVucDto> _khuVucList = new List<QuanLyKhuVucDto>();
        private List<QuanLyBanGridDto> _allBansList = new List<QuanLyBanGridDto>();
        private List<QuanLyKhuVucDto> _filterKhuVucList = new List<QuanLyKhuVucDto>();
        private List<QuanLyBanThongBaoDto> _allThongBaoList = new List<QuanLyBanThongBaoDto>();

        private object? _selectedItem;
        private bool _isAddingNew = false;
        private bool _showSuCoHistory = false;

        static QuanLyBanView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }

        public QuanLyBanView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("QL_BAN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Quản lý Sơ đồ bàn!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();
            await LoadDataAsync();
            await LoadThongBaoSuCoAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_BAN");

            if (FindName("btnThemKhuVuc") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnThemBan") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b4) b4.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        // ========================================================================
        // TẢI DỮ LIỆU
        // ========================================================================
        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border overlay) overlay.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<QuanLyKhuVucDto>>("api/app/quanly-ban/khuvuc-tree");
                if (response != null)
                {
                    _khuVucList = response;

                    if (FindName("cmbKhuVuc") is ComboBox cmbKv) cmbKv.ItemsSource = _khuVucList;
                    if (FindName("dgKhuVuc") is DataGrid dgKv) dgKv.ItemsSource = _khuVucList;

                    _allBansList = _khuVucList.SelectMany(kv => kv.Bans.Select(b => { b.TenKhuVuc = kv.TenKhuVuc; return b; })).ToList();

                    if (FindName("dgAllBans") is DataGrid dgBan) dgBan.ItemsSource = _allBansList;

                    _filterKhuVucList = new List<QuanLyKhuVucDto>(_khuVucList);
                    _filterKhuVucList.Insert(0, new QuanLyKhuVucDto { IdKhuVuc = 0, TenKhuVuc = "--- Tất cả Khu vực ---" });

                    if (FindName("cmbFilterKhuVuc") is ComboBox cmbFilter)
                    {
                        cmbFilter.ItemsSource = _filterKhuVucList;
                        cmbFilter.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border overlayEnd) overlayEnd.Visibility = Visibility.Collapsed; }
        }

        private async Task LoadThongBaoSuCoAsync()
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<QuanLyBanThongBaoDto>>("api/app/thongbao/all");
                if (response != null)
                {
                    _allThongBaoList = response.Where(t => t.LoaiThongBao == "SuCoBan").ToList();
                    ApplySuCoFilter();
                }
            }
            catch { /* Bỏ qua lỗi phụ của ThongBao */ }
        }

        // ========================================================================
        // SỰ KIỆN GIAO DIỆN (ĐÃ BỔ SUNG ĐẦY ĐỦ ĐỂ KHẮC PHỤC CS1061)
        // ========================================================================

        private void CmbFilterKhuVuc_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyBanFilter();
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyBanFilter();
        private void TxtSearchKhuVuc_TextChanged(object sender, TextChangedEventArgs e) => ApplyKhuVucFilter();

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("txtSearchKhuVuc") is TextBox t1) t1.Text = "";
            if (FindName("cmbFilterKhuVuc") is ComboBox c1) c1.SelectedIndex = 0;
            if (FindName("txtSearch") is TextBox t2) t2.Text = "";
            ApplyKhuVucFilter();
            ApplyBanFilter();
        }

        private void ApplyBanFilter()
        {
            if (!(FindName("dgAllBans") is DataGrid dg)) return;
            var filtered = _allBansList.AsEnumerable();

            if (FindName("cmbFilterKhuVuc") is ComboBox cmb && cmb.SelectedItem is QuanLyKhuVucDto kv && kv.IdKhuVuc > 0)
                filtered = filtered.Where(b => b.IdKhuVuc == kv.IdKhuVuc);

            if (FindName("txtSearch") is TextBox txt && !string.IsNullOrEmpty(txt.Text.Trim()))
            {
                string search = txt.Text.Trim().ToLower();
                filtered = filtered.Where(b => b.SoBan.ToLower().Contains(search) || (b.GhiChu != null && b.GhiChu.ToLower().Contains(search)));
            }

            dg.ItemsSource = filtered.ToList();
        }

        private void ApplyKhuVucFilter()
        {
            if (!(FindName("dgKhuVuc") is DataGrid dg)) return;
            var filtered = _khuVucList.AsEnumerable();

            if (FindName("txtSearchKhuVuc") is TextBox txt && !string.IsNullOrEmpty(txt.Text.Trim()))
            {
                string search = txt.Text.Trim().ToLower();
                filtered = filtered.Where(k => k.TenKhuVuc.ToLower().Contains(search));
            }
            dg.ItemsSource = filtered.ToList();
        }

        private void DgKhuVuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgKhuVuc") is DataGrid dg && dg.SelectedItem is QuanLyKhuVucDto selected)
            {
                _selectedItem = selected;
                _isAddingNew = false;
                if (FindName("dgAllBans") is DataGrid dgBan) dgBan.SelectedItem = null;
                PopulateForm();
            }
        }

        private void DgAllBans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgAllBans") is DataGrid dg && dg.SelectedItem is QuanLyBanGridDto selected)
            {
                _selectedItem = _khuVucList.SelectMany(kv => kv.Bans).FirstOrDefault(b => b.IdBan == selected.IdBan);
                _isAddingNew = false;
                if (FindName("dgKhuVuc") is DataGrid dgKv) dgKv.SelectedItem = null;
                PopulateForm();
            }
        }

        private void BtnThemKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN")) return;
            _selectedItem = new QuanLyKhuVucDto();
            _isAddingNew = true;
            if (FindName("dgKhuVuc") is DataGrid dgKv) dgKv.SelectedItem = null;
            if (FindName("dgAllBans") is DataGrid dgBan) dgBan.SelectedItem = null;
            PopulateForm();
        }

        private void BtnThemBan_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN")) return;
            _selectedItem = new QuanLyBanGridDto { IdKhuVuc = _khuVucList.FirstOrDefault()?.IdKhuVuc ?? 0, TrangThai = "Trống" };
            _isAddingNew = true;
            if (FindName("dgKhuVuc") is DataGrid dgKv) dgKv.SelectedItem = null;
            if (FindName("dgAllBans") is DataGrid dgBan) dgBan.SelectedItem = null;
            PopulateForm();
        }

        private void PopulateForm()
        {
            var panelKV = FindName("panelKhuVuc") as StackPanel;
            var panelBan = FindName("panelBan") as StackPanel;
            var formChiTiet = FindName("formChiTiet") as Grid;
            var lblTitle = FindName("lblFormTitle") as TextBlock;
            var btnLichSu = FindName("btnXemLichSu") as Button; // FIX LỖI CS0165

            if (panelKV != null) panelKV.Visibility = Visibility.Collapsed;
            if (panelBan != null) panelBan.Visibility = Visibility.Collapsed;
            if (formChiTiet != null) { formChiTiet.IsEnabled = false; formChiTiet.DataContext = null; }
            if (btnLichSu != null) btnLichSu.Visibility = Visibility.Collapsed;

            if (_selectedItem == null)
            {
                if (lblTitle != null) lblTitle.Text = "Chọn một mục để xem chi tiết";
                return;
            }

            if (formChiTiet != null) formChiTiet.IsEnabled = true;

            if (_selectedItem is QuanLyKhuVucDto kv)
            {
                if (lblTitle != null) lblTitle.Text = _isAddingNew ? "Thêm Khu Vực Mới" : "Chi Tiết Khu Vực";
                if (panelKV != null) panelKV.Visibility = Visibility.Visible;
                if (formChiTiet != null) formChiTiet.DataContext = kv;
            }
            else if (_selectedItem is QuanLyBanGridDto ban)
            {
                if (lblTitle != null) lblTitle.Text = _isAddingNew ? "Thêm Bàn Mới" : "Chi Tiết Bàn";
                if (panelBan != null) panelBan.Visibility = Visibility.Visible;
                if (formChiTiet != null) formChiTiet.DataContext = ban;

                if (FindName("cmbKhuVuc") is ComboBox c1) c1.SelectedValue = ban.IdKhuVuc;
                if (FindName("cmbTrangThai") is ComboBox c2) c2.Text = string.IsNullOrEmpty(ban.TrangThai) ? "Trống" : ban.TrangThai;

                if (!_isAddingNew && btnLichSu != null) btnLichSu.Visibility = Visibility.Visible;
            }
        }

        // ========================================================================
        // TAB SỰ CỐ BÀN
        // ========================================================================
        private void ApplySuCoFilter()
        {
            if (FindName("lblSuCoTitle") is TextBlock title && FindName("lbThongBaoBan") is ListBox lb)
            {
                title.Text = _showSuCoHistory ? "Lịch sử Sự cố (Đã xử lý)" : "Sự cố Bàn cần xử lý";
                lb.ItemsSource = _allThongBaoList.Where(t => t.DaXem == _showSuCoHistory).OrderByDescending(t => t.ThoiGianTao).ToList();
            }
        }

        private void BtnToggleSuCoHistory_Click(object sender, RoutedEventArgs e)
        {
            _showSuCoHistory = (sender as ToggleButton)?.IsChecked == true;
            ApplySuCoFilter();
        }

        private async void BtnDanhDauDaDoc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is QuanLyBanThongBaoDto tb && !tb.DaXem)
            {
                try
                {
                    var response = await httpClient.PostAsync($"api/app/thongbao/mark-as-read/{tb.IdThongBao}", null);
                    if (response.IsSuccessStatusCode)
                    {
                        var item = _allThongBaoList.FirstOrDefault(x => x.IdThongBao == tb.IdThongBao);
                        if (item != null) item.DaXem = true;
                        ApplySuCoFilter();
                    }
                }
                catch { }
            }
        }

        // ========================================================================
        // ACTION LƯU / XÓA / XEM LỊCH SỬ
        // ========================================================================
        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN")) { MessageBox.Show("Từ chối truy cập!"); return; }
            if (_selectedItem == null) return;

            if (FindName("LoadingOverlay") is Border overlay) overlay.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage response;
                if (_selectedItem is QuanLyKhuVucDto kv)
                {
                    var txtTenKhuVuc = FindName("txtTenKhuVuc") as TextBox;
                    var txtMoTa = FindName("txtMoTaKhuVuc") as TextBox;

                    string tenKv = txtTenKhuVuc?.Text.Trim() ?? "";
                    if (string.IsNullOrEmpty(tenKv)) { MessageBox.Show("Tên khu vực không được để trống."); return; }

                    var dto = new QuanLyKhuVucSaveDto { TenKhuVuc = tenKv, MoTa = txtMoTa?.Text };
                    response = _isAddingNew ? await httpClient.PostAsJsonAsync("api/app/quanly-ban/khuvuc", dto)
                                            : await httpClient.PutAsJsonAsync($"api/app/quanly-ban/khuvuc/{kv.IdKhuVuc}", dto);
                }
                else if (_selectedItem is QuanLyBanGridDto ban)
                {
                    var txtSoBan = FindName("txtSoBan") as TextBox;
                    var txtSoGhe = FindName("txtSoGhe") as TextBox;
                    var cmbKv = FindName("cmbKhuVuc") as ComboBox;
                    var cmbTt = FindName("cmbTrangThai") as ComboBox;
                    var txtGhiChu = FindName("txtGhiChu") as TextBox;

                    string soBan = txtSoBan?.Text.Trim() ?? "";
                    if (string.IsNullOrEmpty(soBan)) { MessageBox.Show("Số bàn không được trống."); return; }

                    int.TryParse(txtSoGhe?.Text, out int soGhe);
                    int idKv = (int)(cmbKv?.SelectedValue ?? 0);
                    if (idKv == 0) { MessageBox.Show("Vui lòng chọn Khu vực."); return; }

                    var dto = new QuanLyBanSaveDto { SoBan = soBan, SoGhe = soGhe, IdKhuVuc = idKv, TrangThai = cmbTt?.Text ?? "Trống", GhiChu = txtGhiChu?.Text };
                    response = _isAddingNew ? await httpClient.PostAsJsonAsync("api/app/quanly-ban/ban", dto)
                                            : await httpClient.PutAsJsonAsync($"api/app/quanly-ban/ban/{ban.IdBan}", dto);
                }
                else return;

                if (response.IsSuccessStatusCode) { MessageBox.Show("Đã lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border overlayEnd) overlayEnd.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN")) { MessageBox.Show("Từ chối truy cập!"); return; }
            if (_selectedItem == null || _isAddingNew) return;

            if (MessageBox.Show("Xóa mục này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            if (FindName("LoadingOverlay") is Border overlay) overlay.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage response;
                if (_selectedItem is QuanLyKhuVucDto kv) response = await httpClient.DeleteAsync($"api/app/quanly-ban/khuvuc/{kv.IdKhuVuc}");
                else if (_selectedItem is QuanLyBanGridDto b) response = await httpClient.DeleteAsync($"api/app/quanly-ban/ban/{b.IdBan}");
                else return;

                if (response.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); _selectedItem = null; await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border overlayEnd) overlayEnd.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXemLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN")) return;
            if (!(_selectedItem is QuanLyBanGridDto ban)) return;

            if (FindName("LoadingOverlay") is Border overlay) overlay.Visibility = Visibility.Visible;
            try
            {
                var history = await httpClient.GetFromJsonAsync<QuanLyBanHistoryDto>($"api/app/quanly-ban/ban/{ban.IdBan}/history");
                if (history != null)
                    MessageBox.Show($"Lịch sử Bàn: {ban.SoBan}\n\n- Phục vụ: {history.SoLuotPhucVu} lượt\n- Doanh thu: {history.TongDoanhThu:N0} đ\n- Đặt trước: {history.SoLuotDatTruoc} lượt", "Lịch sử Bàn");
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải lịch sử: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border overlayEnd) overlayEnd.Visibility = Visibility.Collapsed; }
        }
    }
}