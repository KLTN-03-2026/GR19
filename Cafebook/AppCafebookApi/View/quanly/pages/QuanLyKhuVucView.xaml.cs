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
    public partial class QuanLyKhuVucView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyKhuVucDto> _dataList = new();
        private QuanLyKhuVucDto? _selectedItem;
        private bool _isAdding = false;

        //static QuanLyKhuVucView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLyKhuVucView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            if (!AuthService.CoQuyen("QL_KHU_VUC")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }
            ApplyPermissions(); await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_KHU_VUC");
            if (FindName("btnThemMoi") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyKhuVucDto>>("api/app/quanly-khuvuc");
                if (res != null) { _dataList = res; FilterData(); }
            }
            catch { }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();
        private void FilterData()
        {
            if (!(FindName("dgKhuVuc") is DataGrid dg)) return;
            string key = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            dg.ItemsSource = string.IsNullOrEmpty(key) ? _dataList : _dataList.Where(x => x.TenKhuVuc.ToLower().Contains(key)).ToList();
        }

        private void DgKhuVuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgKhuVuc") is DataGrid dg && dg.SelectedItem is QuanLyKhuVucDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Khu vực";
                if (FindName("txtTenKhuVuc") is TextBox t1) t1.Text = item.TenKhuVuc;
                if (FindName("txtMoTa") is TextBox t2) t2.Text = item.MoTa;
            }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_KHU_VUC")) return;
            _selectedItem = new QuanLyKhuVucDto(); _isAdding = true;
            if (FindName("dgKhuVuc") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm mới Khu vực";
            if (FindName("txtTenKhuVuc") is TextBox t1) t1.Text = "";
            if (FindName("txtMoTa") is TextBox t2) t2.Text = "";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_KHU_VUC") || _selectedItem == null) return;
            string ten = (FindName("txtTenKhuVuc") as TextBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(ten)) { MessageBox.Show("Chưa nhập tên!"); return; }

            var dto = new QuanLyKhuVucSaveDto { TenKhuVuc = ten, MoTa = (FindName("txtMoTa") as TextBox)?.Text };
            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _isAdding ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-khuvuc", dto) : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-khuvuc/{_selectedItem.IdKhuVuc}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_KHU_VUC") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var overlay = FindName("LoadingOverlay") as Border;
            if (overlay != null) overlay.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-khuvuc/{_selectedItem.IdKhuVuc}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (overlay != null) overlay.Visibility = Visibility.Collapsed; }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}