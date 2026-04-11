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
            if (!AuthService.CoQuyen("QL_BAN")) { MessageBox.Show("Từ chối!"); this.NavigationService?.GoBack(); return; }
            ApplyPermissions(); await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_BAN");
            if (FindName("btnThemMoi") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            // Ẩn hiện menu chức năng
            if (FindName("btnNavKhuVuc") is Button n1) n1.Visibility = AuthService.CoQuyen("QL_KHU_VUC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavSuCo") is Button n2) n2.Visibility = AuthService.CoQuyen("QL_SU_CO_BAN") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                var kv = await httpClient.GetFromJsonAsync<List<LookupKhuVucDto>>("api/app/quanly-ban/lookup-khuvuc");
                if (kv != null)
                {
                    _lookupKv = kv;
                    if (FindName("cmbKhuVuc") is ComboBox cb1) cb1.ItemsSource = _lookupKv;
                    if (FindName("cmbFilterKV") is ComboBox cb2)
                    {
                        var filters = new List<LookupKhuVucDto> { new LookupKhuVucDto { IdKhuVuc = 0, TenKhuVuc = "Tất cả" } };
                        filters.AddRange(_lookupKv); cb2.ItemsSource = filters; cb2.SelectedIndex = 0;
                    }
                }
                var bans = await httpClient.GetFromJsonAsync<List<QuanLyBanGridDto>>("api/app/quanly-ban");
                if (bans != null) { _dataList = bans; FilterData(); }
            }
            catch { }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => FilterData();
        private void FilterData()
        {
            if (!(FindName("dgBan") is DataGrid dg)) return;
            var q = _dataList.AsEnumerable();
            if (FindName("cmbFilterKV") is ComboBox cb && cb.SelectedValue is int idKv && idKv > 0) q = q.Where(x => x.IdKhuVuc == idKv);
            if (FindName("txtSearch") is TextBox txt && !string.IsNullOrEmpty(txt.Text)) q = q.Where(x => x.SoBan.ToLower().Contains(txt.Text.ToLower()));
            dg.ItemsSource = q.ToList();
        }

        private void DgBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgBan") is DataGrid dg && dg.SelectedItem is QuanLyBanGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Bàn";
                if (FindName("txtSoBan") is TextBox t1) t1.Text = item.SoBan;
                if (FindName("txtSoGhe") is TextBox t2) t2.Text = item.SoGhe.ToString();
                if (FindName("txtGhiChu") is TextBox t3) t3.Text = item.GhiChu;
                if (FindName("cmbKhuVuc") is ComboBox c1) c1.SelectedValue = item.IdKhuVuc;
                if (FindName("cmbTrangThai") is ComboBox c2) c2.Text = string.IsNullOrEmpty(item.TrangThai) ? "Trống" : item.TrangThai;
                if (FindName("btnLichSu") is Button bl) bl.Visibility = Visibility.Visible;
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN")) return;
            _selectedItem = new QuanLyBanGridDto(); _isAdding = true;
            if (FindName("dgBan") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm Bàn Mới";
            if (FindName("txtSoBan") is TextBox t1) t1.Text = "";
            if (FindName("txtSoGhe") is TextBox t2) t2.Text = "2";
            if (FindName("txtGhiChu") is TextBox t3) t3.Text = "";
            if (FindName("btnLichSu") is Button bl) bl.Visibility = Visibility.Collapsed;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN") || _selectedItem == null) return;

            string so = (FindName("txtSoBan") as TextBox)?.Text.Trim() ?? "";
            int idKv = (FindName("cmbKhuVuc") as ComboBox)?.SelectedValue as int? ?? 0;
            if (string.IsNullOrEmpty(so) || idKv == 0) { MessageBox.Show("Nhập đủ Số bàn và Khu vực!"); return; }
            int.TryParse((FindName("txtSoGhe") as TextBox)?.Text, out int ghe);

            var dto = new QuanLyBanSaveDto { SoBan = so, SoGhe = ghe, IdKhuVuc = idKv, TrangThai = (FindName("cmbTrangThai") as ComboBox)?.Text ?? "Trống", GhiChu = (FindName("txtGhiChu") as TextBox)?.Text };
            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _isAdding ? await httpClient.PostAsJsonAsync("api/app/quanly-ban", dto) : await httpClient.PutAsJsonAsync($"api/app/quanly-ban/{_selectedItem.IdBan}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_BAN") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa bàn?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

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
            if (!AuthService.CoQuyen("QL_BAN") || _selectedItem == null || _isAdding) return;
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
            if (AuthService.CoQuyen("QL_KHU_VUC")) this.NavigationService?.Navigate(new QuanLyKhuVucView());
            else MessageBox.Show("Bạn không có quyền!");
        }

        private void BtnNavSuCo_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_SU_CO_BAN")) this.NavigationService?.Navigate(new QuanLySuCoBanView());
            else MessageBox.Show("Bạn không có quyền!");
        }
    }
}