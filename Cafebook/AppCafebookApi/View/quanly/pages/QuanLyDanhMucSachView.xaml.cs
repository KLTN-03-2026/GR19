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
    public partial class QuanLyDanhMucSachView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyDanhMucSachItemDto> _currentDataList = new();
        private QuanLyDanhMucSachItemDto? _selectedItem = null;
        private string _currentEndpoint = "tacgia"; // Mặc định là Tác giả

//        static QuanLyDanhMucSachView() { ApiClient.Instance = new ApiClient.Instance { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyDanhMucSachView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // BẢO MẬT LỚP 2
            if (!AuthService.CoQuyen("FULL_QL", "QL_DANH_MUC_SACH"))
            {
                MessageBox.Show("Từ chối truy cập module Danh mục sách!");
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            // BẢO MẬT LỚP 1 VÀ FINDNAME
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_DANH_MUC_SACH");
            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDanhMucSachItemDto>>($"api/app/quanly-danhmucsach/{_currentEndpoint}");
                if (res != null)
                {
                    _currentDataList = res;
                    FilterData();
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void CmbLoaiDanhMuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("cmbLoaiDanhMuc") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item)
            {
                _currentEndpoint = item.Tag?.ToString() ?? "tacgia";

                // Đổi Label cho phù hợp
                if (FindName("lblTenField") is TextBlock lblTen) lblTen.Text = $"Tên {item.Content} (*)";

                BtnLamMoiForm_Click(this, new RoutedEventArgs());
                await LoadDataAsync();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (FindName("dgDanhMuc") is DataGrid dg)
            {
                string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
                var filtered = string.IsNullOrEmpty(k) ? _currentDataList : _currentDataList.Where(x => x.Ten.ToLower().Contains(k)).ToList();
                dg.ItemsSource = filtered;
            }
        }

        private void DgDanhMuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDanhMuc") is DataGrid dg && dg.SelectedItem is QuanLyDanhMucSachItemDto item)
            {
                _selectedItem = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblFormTitle") is TextBlock title) title.Text = "Chỉnh sửa chi tiết";

                if (FindName("txtTen") is TextBox t1) t1.Text = item.Ten;
                if (FindName("txtMoTa") is TextBox t2) t2.Text = item.MoTa;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = null;
            if (FindName("dgDanhMuc") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblFormTitle") is TextBlock title) title.Text = "Thêm Mới";

            if (FindName("txtTen") is TextBox t1) t1.Text = "";
            if (FindName("txtMoTa") is TextBox t2) t2.Text = "";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string ten = (FindName("txtTen") as TextBox)?.Text.Trim() ?? "";
            string moTa = (FindName("txtMoTa") as TextBox)?.Text.Trim() ?? "";

            if (string.IsNullOrEmpty(ten)) { MessageBox.Show("Vui lòng nhập tên danh mục!"); return; }

            var dto = new QuanLyDanhMucSachSaveDto { Ten = ten, MoTa = moTa };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _selectedItem == null
                    ? await ApiClient.Instance.PostAsJsonAsync($"api/app/quanly-danhmucsach/{_currentEndpoint}", dto)
                    : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-danhmucsach/{_currentEndpoint}/{_selectedItem.Id}", dto);

                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); BtnLamMoiForm_Click(this, new RoutedEventArgs()); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            if (MessageBox.Show($"Bạn chắc chắn xóa '{_selectedItem.Ten}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-danhmucsach/{_currentEndpoint}/{_selectedItem.Id}");
                    if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); BtnLamMoiForm_Click(this, new RoutedEventArgs()); }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}