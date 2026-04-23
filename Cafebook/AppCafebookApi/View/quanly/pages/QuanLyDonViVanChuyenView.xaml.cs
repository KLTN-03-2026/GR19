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
    public partial class QuanLyDonViVanChuyenView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyDonViVanChuyenGridDto> _dataList = new();
        private QuanLyDonViVanChuyenGridDto? _selectedItem;
        private bool _isAdding = false;

        //static QuanLyDonViVanChuyenView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyDonViVanChuyenView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // BẢO MẬT LỚP 2
            if (!AuthService.CoQuyen("QL_NGUOI_GIAO_HANG")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            // BẢO MẬT LỚP 1 VÀ FINDNAME PROTECTION
            bool canEdit = AuthService.CoQuyen("QL_NGUOI_GIAO_HANG");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDonViVanChuyenGridDto>>("api/app/quanly-donvivanchuyen");
                if (res != null) { _dataList = res; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void Filter_Changed(object sender, EventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgDonVi") is DataGrid dg)) return;
            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            string trangThai = (FindName("cmbFilterTrangThai") as ComboBox)?.Text ?? "Tất cả";

            var filtered = _dataList.AsEnumerable();
            if (!string.IsNullOrEmpty(k)) filtered = filtered.Where(x => x.TenNguoiGiaoHang.ToLower().Contains(k) || (x.SoDienThoai != null && x.SoDienThoai.Contains(k)));
            if (trangThai != "Tất cả") filtered = filtered.Where(x => x.TrangThai == trangThai);

            dg.ItemsSource = filtered.ToList();
        }

        private void DgDonVi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDonVi") is DataGrid dg && dg.SelectedItem is QuanLyDonViVanChuyenGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Đơn vị Vận chuyển";

                if (FindName("txtTen") is TextBox t1) t1.Text = item.TenNguoiGiaoHang;
                if (FindName("txtSdt") is TextBox t2) t2.Text = item.SoDienThoai;
                if (FindName("cmbTrangThai") is ComboBox c1) c1.Text = item.TrangThai;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = new QuanLyDonViVanChuyenGridDto(); _isAdding = true;
            if (FindName("dgDonVi") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm Đơn vị / Shipper mới";

            if (FindName("txtTen") is TextBox t1) t1.Text = "";
            if (FindName("txtSdt") is TextBox t2) t2.Text = "";
            if (FindName("cmbTrangThai") is ComboBox c1) c1.SelectedIndex = 0; // Mặc định Sẵn sàng
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string ten = (FindName("txtTen") as TextBox)?.Text.Trim() ?? "";
            string sdt = (FindName("txtSdt") as TextBox)?.Text.Trim() ?? "";
            string trangThai = (FindName("cmbTrangThai") as ComboBox)?.Text ?? "Sẵn sàng";

            if (string.IsNullOrEmpty(ten)) { MessageBox.Show("Vui lòng nhập Tên Đơn vị/Shipper!"); return; }

            var dto = new QuanLyDonViVanChuyenSaveDto { TenNguoiGiaoHang = ten, SoDienThoai = sdt, TrangThai = trangThai };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = _isAdding ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-donvivanchuyen", dto) : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-donvivanchuyen/{_selectedItem!.IdNguoiGiaoHang}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null || _isAdding) return;
            if (MessageBox.Show($"Xóa đơn vị '{_selectedItem.TenNguoiGiaoHang}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-donvivanchuyen/{_selectedItem.IdNguoiGiaoHang}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}