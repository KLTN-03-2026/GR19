using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyBanView : Page
    {
        private List<QuanLyBanGridDto> _dataList = new();
        private List<LookupKhuVucDto> _lookupKv = new();
        private QuanLyBanGridDto? _selectedItem;
        private bool _isAdding = false;

        private bool _isDataLoaded = false;

        public QuanLyBanView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            bool hasAnyPermission = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_BAN", "QL_SU_CO_BAN", "QL_KHU_VUC");
            if (!hasAnyPermission)
            {
                MessageBox.Show("Bạn không có quyền truy cập phân hệ Bàn & Khu vực!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            ApplyPermissions();

            try
            {
                if (AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_BAN"))
                {
                    await LoadDataAsync();
                }
                else
                {
                    if (FindName("GridDuLieuBan") is Grid gridData) gridData.Visibility = Visibility.Collapsed;
                    if (FindName("txtThongBaoKhongCoQuyen") is Border txtThongBao) txtThongBao.Visibility = Visibility.Visible;
                }

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyPermissions()
        {
            if (FindName("btnNavKhuVuc") is Button b1) b1.Visibility = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHU_VUC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavSuCo") is Button b2) b2.Visibility = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_SU_CO_BAN") ? Visibility.Visible : Visibility.Collapsed;

            bool canEdit = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_BAN");
            if (FindName("btnThem") is Button btnThem) btnThem.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button btnLuu) btnLuu.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button btnXoa) btnXoa.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;

            try
            {
                // 1. KIỂM TRA RAM (Lấy dữ liệu Khu Vực và Bàn hiển thị lập tức)
                if (GlobalDataCache.QL_BanCache != null && GlobalDataCache.QL_KhuVucCache != null)
                {
                    PopulateBanUiFromRam();

                    // 2. Kích hoạt cập nhật ngầm API
                    _ = BackgroundRefreshAsync();
                    return;
                }

                // 3. Fallback: Nếu RAM bị lỗi hoặc trống, gọi tải API trực tiếp
                await FetchApiAndSetupUI();
            }
            finally
            {
                if (overlay != null) overlay.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (TÁCH LOGIC ĐỂ DÙNG CHUNG)
        // ==========================================

        private void PopulateBanUiFromRam()
        {
            // Ép kiểu nhanh từ QuanLyKhuVucDto (RAM) sang LookupKhuVucDto (ComboBox cần)
            if (GlobalDataCache.QL_KhuVucCache != null)
            {
                _lookupKv = GlobalDataCache.QL_KhuVucCache.Select(k => new LookupKhuVucDto
                {
                    IdKhuVuc = k.IdKhuVuc,
                    TenKhuVuc = k.TenKhuVuc
                }).ToList();

                if (FindName("cmbKhuVuc") is ComboBox cb) cb.ItemsSource = _lookupKv;
                if (FindName("cmbFilterKhuVuc") is ComboBox cbF)
                {
                    var list = new List<LookupKhuVucDto> { new LookupKhuVucDto { IdKhuVuc = 0, TenKhuVuc = "Tất cả" } };
                    list.AddRange(_lookupKv);

                    int? currentFilterId = cbF.SelectedValue as int?;
                    cbF.ItemsSource = list;
                    cbF.SelectedValue = currentFilterId ?? 0; // Giữ nguyên bộ lọc cũ nếu đang chọn
                }
            }

            if (GlobalDataCache.QL_BanCache != null)
            {
                _dataList = GlobalDataCache.QL_BanCache;
                FilterData();
            }
        }

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                // Bắn 2 luồng API cùng lúc để tối đa hóa tốc độ đồng bộ ngầm
                var tKvs = ApiClient.Instance.GetFromJsonAsync<List<LookupKhuVucDto>>("api/app/quanly-ban/lookup-khuvuc");
                var tBans = ApiClient.Instance.GetFromJsonAsync<List<QuanLyBanGridDto>>("api/app/quanly-ban");

                await Task.WhenAll(tKvs, tBans);

                var kvs = await tKvs;
                var bans = await tBans;

                if (kvs != null && bans != null)
                {
                    // Cập nhật lại RAM
                    GlobalDataCache.QL_BanCache = bans;
                    _dataList = bans;
                    _lookupKv = kvs;

                    // Ghi nhớ trạng thái đang chọn trên UI
                    int? currentFilterId = (FindName("cmbFilterKhuVuc") as ComboBox)?.SelectedValue as int?;
                    int? currentSelectedBanId = _selectedItem?.IdBan;

                    // Vẽ lại giao diện ngầm
                    if (FindName("cmbKhuVuc") is ComboBox cb) cb.ItemsSource = _lookupKv;
                    if (FindName("cmbFilterKhuVuc") is ComboBox cbF)
                    {
                        var list = new List<LookupKhuVucDto> { new LookupKhuVucDto { IdKhuVuc = 0, TenKhuVuc = "Tất cả" } };
                        list.AddRange(_lookupKv);
                        cbF.ItemsSource = list;
                        cbF.SelectedValue = currentFilterId ?? 0;
                    }

                    FilterData();

                    // Phục hồi lại dòng đang click chọn (nếu có)
                    if (currentSelectedBanId.HasValue && FindName("dgBan") is DataGrid dg)
                    {
                        var itemToSelect = _dataList.FirstOrDefault(x => x.IdBan == currentSelectedBanId);
                        if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                    }
                }
            }
            catch { /* Lỗi mạng thì bỏ qua, giữ nguyên UI cũ */ }
        }

        private async Task FetchApiAndSetupUI()
        {
            var tKvs = ApiClient.Instance.GetFromJsonAsync<List<LookupKhuVucDto>>("api/app/quanly-ban/lookup-khuvuc");
            var tBans = ApiClient.Instance.GetFromJsonAsync<List<QuanLyBanGridDto>>("api/app/quanly-ban");

            await Task.WhenAll(tKvs, tBans);

            var kvs = await tKvs;
            var bans = await tBans;

            if (kvs != null && bans != null)
            {
                _lookupKv = kvs;
                _dataList = bans;
                GlobalDataCache.QL_BanCache = bans;

                if (FindName("cmbKhuVuc") is ComboBox cb) cb.ItemsSource = _lookupKv;
                if (FindName("cmbFilterKhuVuc") is ComboBox cbF)
                {
                    var list = new List<LookupKhuVucDto> { new LookupKhuVucDto { IdKhuVuc = 0, TenKhuVuc = "Tất cả" } };
                    list.AddRange(_lookupKv);
                    cbF.ItemsSource = list;
                    cbF.SelectedIndex = 0;
                }

                FilterData();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (FindName("dgBan") is not DataGrid dg) return;
            var q = _dataList.AsEnumerable();
            int idKv = (FindName("cmbFilterKhuVuc") as ComboBox)?.SelectedValue as int? ?? 0;
            if (idKv > 0) q = q.Where(x => x.IdKhuVuc == idKv);
            dg.ItemsSource = q.ToList();
        }

        private void DgBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgBan") is DataGrid dg && dg.SelectedItem is QuanLyBanGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock t) t.Text = "Sửa Bàn";

                if (FindName("txtSoBan") is TextBox t1) t1.Text = item.SoBan;
                if (FindName("cmbKhuVuc") is ComboBox cb) cb.SelectedValue = item.IdKhuVuc;
                if (FindName("txtSoGhe") is TextBox t2) t2.Text = item.SoGhe.ToString();
                if (FindName("cmbTrangThai") is ComboBox cbt) cbt.Text = item.TrangThai;
                if (FindName("txtGhiChu") is TextBox t3) t3.Text = item.GhiChu;
            }
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = new QuanLyBanGridDto(); _isAdding = true;
            if (FindName("dgBan") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock t) t.Text = "Thêm Bàn Mới";

            if (FindName("txtSoBan") is TextBox t1) t1.Text = "";
            if (FindName("cmbKhuVuc") is ComboBox cb) cb.SelectedItem = null;
            if (FindName("txtSoGhe") is TextBox t2) t2.Text = "4";
            if (FindName("cmbTrangThai") is ComboBox cbt) cbt.SelectedIndex = 0;
            if (FindName("txtGhiChu") is TextBox t3) t3.Text = "";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_BAN")) return;
            string soBan = (FindName("txtSoBan") as TextBox)?.Text.Trim() ?? "";
            int idKv = (FindName("cmbKhuVuc") as ComboBox)?.SelectedValue as int? ?? 0;

            if (string.IsNullOrEmpty(soBan) || idKv == 0) { MessageBox.Show("Nhập Số Bàn và chọn Khu vực!"); return; }

            int.TryParse((FindName("txtSoGhe") as TextBox)?.Text, out int soGhe);

            var dto = new QuanLyBanSaveDto
            {
                SoBan = soBan,
                IdKhuVuc = idKv,
                SoGhe = soGhe,
                TrangThai = (FindName("cmbTrangThai") as ComboBox)?.Text ?? "Trống",
                GhiChu = (FindName("txtGhiChu") as TextBox)?.Text
            };

            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                var res = _isAdding ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-ban", dto)
                                    : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-ban/{_selectedItem!.IdBan}", dto);

                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_BAN") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa bàn này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-ban/{_selectedItem.IdBan}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_BAN") || _selectedItem == null || _isAdding) return;
            try
            {
                var his = await ApiClient.Instance.GetFromJsonAsync<QuanLyBanHistoryDto>($"api/app/quanly-ban/{_selectedItem.IdBan}/history");
                if (his != null) MessageBox.Show($"Phục vụ: {his.SoLuotPhucVu}\nDoanh thu: {his.TongDoanhThu:N0}\nĐặt trước: {his.SoLuotDatTruoc}", "Lịch sử Bàn");
            }
            catch { }
        }

        private void BtnNavKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHU_VUC")) this.NavigationService?.Navigate(new QuanLyKhuVucView());
            else MessageBox.Show("Bạn không có quyền truy cập!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnNavSuCo_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_SU_CO_BAN")) this.NavigationService?.Navigate(new QuanLySuCoBanView());
            else MessageBox.Show("Bạn không có quyền truy cập!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
}