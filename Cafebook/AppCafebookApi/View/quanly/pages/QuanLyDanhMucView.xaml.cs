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
    public partial class QuanLyDanhMucView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyDanhMucGridDto> _dataList = new();
        private QuanLyDanhMucGridDto? _selectedItem;
        private bool _isAdding = false;

       // static QuanLyDanhMucView() { ApiClient.Instance = new ApiClient.Instance { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLyDanhMucView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            if (!AuthService.CoQuyen("QL_DANH_MUC")) { MessageBox.Show("Từ chối!"); this.NavigationService?.GoBack(); return; }
            ApplyPermissions(); await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_DANH_MUC");
            if (FindName("btnThemMoi") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try { var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDanhMucGridDto>>("api/app/quanly-danhmuc"); if (res != null) { _dataList = res; FilterData(); } }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();
        private void FilterData() { if (FindName("dgDanhMuc") is DataGrid dg) { string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? ""; dg.ItemsSource = string.IsNullOrEmpty(k) ? _dataList : _dataList.Where(x => x.TenDanhMuc.ToLower().Contains(k)).ToList(); } }

        private void DgDanhMuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDanhMuc") is DataGrid dg && dg.SelectedItem is QuanLyDanhMucGridDto item) { _selectedItem = item; _isAdding = false; if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true; if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Danh mục"; if (FindName("txtTenDanhMuc") is TextBox t1) t1.Text = item.TenDanhMuc; }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e) { _selectedItem = new QuanLyDanhMucGridDto(); _isAdding = true; if (FindName("dgDanhMuc") is DataGrid dg) dg.SelectedItem = null; if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true; if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm mới Danh mục"; if (FindName("txtTenDanhMuc") is TextBox t1) t1.Text = ""; }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DANH_MUC") || _selectedItem == null) return;
            string ten = (FindName("txtTenDanhMuc") as TextBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(ten)) { MessageBox.Show("Nhập tên danh mục!"); return; }
            var dto = new QuanLyDanhMucSaveDto { TenDanhMuc = ten };
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try { var res = _isAdding ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-danhmuc", dto) : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-danhmuc/{_selectedItem.IdDanhMuc}", dto); if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); } }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e) { if (!AuthService.CoQuyen("QL_DANH_MUC") || _selectedItem == null || _isAdding) return; if (MessageBox.Show("Xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-danhmuc/{_selectedItem.IdDanhMuc}"); if (res.IsSuccessStatusCode) await LoadDataAsync(); } }
        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}