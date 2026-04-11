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
    public partial class QuanLyNguyenLieuView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyNguyenLieuGridDto> _dataList = new();
        private QuanLyNguyenLieuGridDto? _selectedItem;
        private bool _isAdding = false;

        static QuanLyNguyenLieuView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyNguyenLieuView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // BẢO MẬT LỚP 2
            if (!AuthService.CoQuyen("QL_NGUYEN_LIEU")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            // BẢO MẬT LỚP 1 VÀ FINDNAME
            bool canEdit = AuthService.CoQuyen("QL_NGUYEN_LIEU");
            if (FindName("btnThemMoi") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            // Quyền truy cập Đơn vị Quy đổi
            if (FindName("btnQuanLyDVT") is Button b4) b4.Visibility = AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await httpClient.GetFromJsonAsync<List<QuanLyNguyenLieuGridDto>>("api/app/quanly-nguyenlieu");
                if (res != null) { _dataList = res; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgNguyenLieu") is DataGrid dg)) return;
            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            dg.ItemsSource = string.IsNullOrEmpty(k) ? _dataList : _dataList.Where(x => x.TenNguyenLieu.ToLower().Contains(k)).ToList();
        }

        private void DgNguyenLieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgNguyenLieu") is DataGrid dg && dg.SelectedItem is QuanLyNguyenLieuGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Nguyên Liệu";
                if (FindName("txtTenNL") is TextBox t1) t1.Text = item.TenNguyenLieu;
                if (FindName("cmbDVT") is ComboBox c2) c2.Text = item.DonViTinh;
                if (FindName("txtNguong") is TextBox t3) t3.Text = item.TonKhoToiThieu.ToString();
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NGUYEN_LIEU")) return;
            _selectedItem = new QuanLyNguyenLieuGridDto(); _isAdding = true;
            if (FindName("dgNguyenLieu") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm Nguyên Liệu Mới";
            if (FindName("txtTenNL") is TextBox t1) t1.Text = "";
            if (FindName("cmbDVT") is ComboBox c2) c2.Text = "";
            if (FindName("txtNguong") is TextBox t3) t3.Text = "0";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NGUYEN_LIEU") || _selectedItem == null) return;

            string ten = (FindName("txtTenNL") as TextBox)?.Text.Trim() ?? "";
            string dvt = (FindName("cmbDVT") as ComboBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(dvt)) { MessageBox.Show("Nhập Tên NL và ĐVT!"); return; }
            decimal.TryParse((FindName("txtNguong") as TextBox)?.Text, out decimal nguong);

            var dto = new QuanLyNguyenLieuSaveDto { TenNguyenLieu = ten, DonViTinh = dvt, TonKhoToiThieu = nguong };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = _isAdding ? await httpClient.PostAsJsonAsync("api/app/quanly-nguyenlieu", dto) : await httpClient.PutAsJsonAsync($"api/app/quanly-nguyenlieu/{_selectedItem.IdNguyenLieu}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NGUYEN_LIEU") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa nguyên liệu này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-nguyenlieu/{_selectedItem.IdNguyenLieu}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
        }

        private void BtnQuanLyDVT_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI"))
                this.NavigationService?.Navigate(new QuanLyDonViChuyenDoiView());
            else
                MessageBox.Show("Bạn không có quyền quản lý đơn vị chuyển đổi!");
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}