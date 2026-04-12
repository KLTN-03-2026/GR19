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
    public partial class QuanLyPhuThuView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyPhuThuGridDto> _dataList = new();
        private QuanLyPhuThuGridDto? _selectedItem;
        private bool _isAdding = false;

        static QuanLyPhuThuView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyPhuThuView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            if (!AuthService.CoQuyen("QL_PHU_THU")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_PHU_THU");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await httpClient.GetFromJsonAsync<List<QuanLyPhuThuGridDto>>("api/app/quanly-phuthu");
                if (res != null) { _dataList = res; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void Filter_Changed(object sender, EventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgPhuThu") is DataGrid dg)) return;
            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            string loai = (FindName("cmbFilterLoai") as ComboBox)?.Text ?? "Tất cả";

            var filtered = _dataList.AsEnumerable();
            if (!string.IsNullOrEmpty(k)) filtered = filtered.Where(x => x.TenPhuThu.ToLower().Contains(k));
            if (loai != "Tất cả") filtered = filtered.Where(x => x.LoaiGiaTri == loai);

            dg.ItemsSource = filtered.ToList();
        }

        private void DgPhuThu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhuThu") is DataGrid dg && dg.SelectedItem is QuanLyPhuThuGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Phụ thu";

                if (FindName("txtTenPhuThu") is TextBox t1) t1.Text = item.TenPhuThu;
                if (FindName("txtGiaTri") is TextBox t2) t2.Text = item.GiaTri.ToString();
                if (FindName("cmbLoaiPhuThu") is ComboBox c1) c1.Text = item.LoaiGiaTri;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = new QuanLyPhuThuGridDto(); _isAdding = true;
            if (FindName("dgPhuThu") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm Phụ thu mới";
            if (FindName("txtTenPhuThu") is TextBox t1) t1.Text = "";
            if (FindName("txtGiaTri") is TextBox t2) t2.Text = "0";
            if (FindName("cmbLoaiPhuThu") is ComboBox c1) c1.SelectedIndex = 0;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string ten = (FindName("txtTenPhuThu") as TextBox)?.Text.Trim() ?? "";
            decimal.TryParse((FindName("txtGiaTri") as TextBox)?.Text, out decimal giaTri);
            string loai = (FindName("cmbLoaiPhuThu") as ComboBox)?.Text ?? "VNĐ";

            if (string.IsNullOrEmpty(ten) || giaTri < 0) { MessageBox.Show("Vui lòng nhập Tên và Giá trị!"); return; }

            var dto = new QuanLyPhuThuSaveDto { TenPhuThu = ten, GiaTri = giaTri, LoaiGiaTri = loai };
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = _isAdding ? await httpClient.PostAsJsonAsync("api/app/quanly-phuthu", dto) : await httpClient.PutAsJsonAsync($"api/app/quanly-phuthu/{_selectedItem!.IdPhuThu}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null || _isAdding) return;
            if (MessageBox.Show($"Xóa phụ thu '{_selectedItem.TenPhuThu}'?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-phuthu/{_selectedItem.IdPhuThu}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}