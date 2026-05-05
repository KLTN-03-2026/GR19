using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyThuongPhatView : Page
    {
        private List<QuanLyThuongPhatGridDto> _allDataList = new();
        private QuanLyThuongPhatGridDto? _selectedItem = null;
        private bool _isDataLoaded = false;

        public QuanLyThuongPhatView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_LUONG"))
            {
                MessageBox.Show("Bạn không có quyền truy cập module này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack(); return;
            }

            await Task.Delay(350);
            if (!this.IsLoaded) return;

            try
            {
                ApplyPermissions();
                await LoadDataAsync();
                _isDataLoaded = true;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private void ApplyPermissions()
        {
            bool hasQuyen = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_LUONG");
            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task LoadDataAsync(string keyword = "")
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                string url = "api/app/quanly-thuongphat/search";
                if (!string.IsNullOrEmpty(keyword)) url += $"?keyword={keyword}";

                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyThuongPhatGridDto>>(url);
                if (res != null && FindName("dgThuongPhat") is DataGrid dg)
                {
                    _allDataList = res;
                    dg.ItemsSource = _allDataList;
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e)
        {
            string search = (FindName("txtSearch") as TextBox)?.Text.Trim() ?? "";
            await LoadDataAsync(search);
        }

        private void DgThuongPhat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgThuongPhat") is DataGrid dg && dg.SelectedItem is QuanLyThuongPhatGridDto item)
            {
                _selectedItem = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblFormTitle") is TextBlock title) title.Text = "Cập nhật Mẫu";

                if (FindName("cmbLoai") is ComboBox cbLoai) cbLoai.Text = item.Loai;
                if (FindName("txtLyDo") is TextBox tLyDo) tLyDo.Text = item.TenMau;
                if (FindName("txtSoTien") is TextBox tTien) tTien.Text = item.SoTien.ToString("0.##");
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = null;
            if (FindName("dgThuongPhat") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblFormTitle") is TextBlock title) title.Text = "Thêm Mẫu Mới";

            if (FindName("cmbLoai") is ComboBox cbLoai) cbLoai.SelectedIndex = 0;
            if (FindName("txtLyDo") is TextBox tLyDo) tLyDo.Text = "";
            if (FindName("txtSoTien") is TextBox tTien) tTien.Text = "";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse((FindName("txtSoTien") as TextBox)?.Text, out decimal soTien) || soTien <= 0)
            {
                MessageBox.Show("Số tiền phải lớn hơn 0!"); return;
            }

            var dto = new QuanLyThuongPhatSaveDto
            {
                Loai = (FindName("cmbLoai") as ComboBox)?.Text ?? "Thưởng",
                TenMau = (FindName("txtLyDo") as TextBox)?.Text.Trim() ?? "",
                SoTien = soTien
            };

            if (string.IsNullOrEmpty(dto.TenMau)) { MessageBox.Show("Vui lòng nhập Tên Mẫu!"); return; }

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _selectedItem == null
                    ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-thuongphat", dto)
                    : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-thuongphat/{_selectedItem.IdMau}", dto);

                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu mẫu thành công!"); await LoadDataAsync(); BtnLamMoiForm_Click(this, new RoutedEventArgs()); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            if (MessageBox.Show("Xác nhận xóa mẫu này?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-thuongphat/{_selectedItem.IdMau}");
                    if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); BtnLamMoiForm_Click(this, new RoutedEventArgs()); }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}
