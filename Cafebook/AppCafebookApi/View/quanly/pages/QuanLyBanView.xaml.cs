using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
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
        private static readonly HttpClient httpClient;
        private List<QuanLyBanGridDto> _dataList = new();
        private List<LookupKhuVucDto> _lookupKv = new();
        private QuanLyBanGridDto? _selectedItem;
        private bool _isAdding = false;

        static QuanLyBanView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLyBanView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // 1. CHÌA KHÓA CỔNG
            bool hasAnyPermission = AuthService.CoQuyen("FULL_QL", "QL_BAN", "QL_SU_CO_BAN", "QL_KHU_VUC");
            if (!hasAnyPermission)
            {
                MessageBox.Show("Bạn không có quyền truy cập phân hệ Bàn & Khu vực!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            // 2. CHÌA KHÓA PHÒNG
            if (AuthService.CoQuyen("FULL_QL", "QL_BAN"))
            {
                await LoadDataAsync();
            }
            else
            {
                if (FindName("GridDuLieuBan") is Grid gridData) gridData.Visibility = Visibility.Collapsed;
                if (FindName("txtThongBaoKhongCoQuyen") is Border txtThongBao) txtThongBao.Visibility = Visibility.Visible;
            }
        }

        private void ApplyPermissions()
        {
            if (FindName("btnNavKhuVuc") is Button b1) b1.Visibility = AuthService.CoQuyen("FULL_QL", "QL_KHU_VUC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavSuCo") is Button b2) b2.Visibility = AuthService.CoQuyen("FULL_QL", "QL_SU_CO_BAN") ? Visibility.Visible : Visibility.Collapsed;

            bool canEdit = AuthService.CoQuyen("FULL_QL", "QL_BAN");
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
                var kvs = await httpClient.GetFromJsonAsync<List<LookupKhuVucDto>>("api/app/quanly-ban/lookup-khuvuc");
                if (kvs != null)
                {
                    _lookupKv = kvs;
                    if (FindName("cmbKhuVuc") is ComboBox cb) cb.ItemsSource = _lookupKv;
                    if (FindName("cmbFilterKhuVuc") is ComboBox cbF)
                    {
                        var list = new List<LookupKhuVucDto> { new LookupKhuVucDto { IdKhuVuc = 0, TenKhuVuc = "Tất cả" } };
                        list.AddRange(_lookupKv);
                        cbF.ItemsSource = list; cbF.SelectedIndex = 0;
                    }
                }

                var bans = await httpClient.GetFromJsonAsync<List<QuanLyBanGridDto>>("api/app/quanly-ban");
                if (bans != null) { _dataList = bans; FilterData(); }
            }
            catch { }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
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
            if (!AuthService.CoQuyen("FULL_QL", "QL_BAN")) return;
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
                var res = _isAdding ? await httpClient.PostAsJsonAsync("api/app/quanly-ban", dto) : await httpClient.PutAsJsonAsync($"api/app/quanly-ban/{_selectedItem!.IdBan}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_BAN") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa bàn này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-ban/{_selectedItem.IdBan}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_BAN") || _selectedItem == null || _isAdding) return;
            try
            {
                var his = await httpClient.GetFromJsonAsync<QuanLyBanHistoryDto>($"api/app/quanly-ban/{_selectedItem.IdBan}/history");
                if (his != null) MessageBox.Show($"Phục vụ: {his.SoLuotPhucVu}\nDoanh thu: {his.TongDoanhThu:N0}\nĐặt trước: {his.SoLuotDatTruoc}", "Lịch sử Bàn");
            }
            catch { }
        }

        // ĐIỀU HƯỚNG (BẢO MẬT LỚP 2)
        private void BtnNavKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_KHU_VUC")) this.NavigationService?.Navigate(new QuanLyKhuVucView());
            else MessageBox.Show("Bạn không có quyền truy cập!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnNavSuCo_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_SU_CO_BAN")) this.NavigationService?.Navigate(new QuanLySuCoBanView());
            else MessageBox.Show("Bạn không có quyền truy cập!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
}