// File: AppCafebookApi/View/quanly/pages/QuanLyDeXuatView.xaml.cs
using System;
using System.Collections.Generic;
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
    public partial class QuanLyDeXuatView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyDeXuatGridDto> _allDeXuatList = new();
        private QuanLyDeXuatGridDto? _selectedItem = null;
        /*
        static QuanLyDeXuatView()
        {
            httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") };
        }
        */
        public QuanLyDeXuatView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
            {
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            // Lớp bảo mật 2 (Chặn truy cập Page)
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_DE_XUAT");
            if (!hasQuyen)
            {
                MessageBox.Show("Bạn không có quyền truy cập module này!", "Từ chối");
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions(hasQuyen);
            await LoadLookupsAsync();
            await LoadDataAsync();
        }

        // Lớp bảo mật 1 (Ẩn/Hiện UI theo quyền)
        private void ApplyPermissions(bool hasQuyen)
        {
            if (FindName("GridDuLieuDeXuat") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
        }

        private string GetCurrentLoaiDoiTuong()
        {
            if (FindName("cmbFilterLoaiDoiTuong") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item)
            {
                return item.Tag?.ToString() ?? "SACH";
            }
            return "SACH";
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                string type = GetCurrentLoaiDoiTuong();
                var lookups = await ApiClient.Instance.GetFromJsonAsync<List<DeXuatLookupDto>>($"api/app/quanly-dexuat/lookup?loaiDoiTuong={type}");

                if (lookups != null)
                {
                    if (FindName("cmbDoiTuongGoc") is ComboBox cmbGoc) cmbGoc.ItemsSource = lookups;
                    if (FindName("cmbDoiTuongDeXuat") is ComboBox cmbDX) cmbDX.ItemsSource = lookups;
                }
            }
            catch { /* Xử lý lỗi load dropdown (nếu có) */ }
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                string type = GetCurrentLoaiDoiTuong();
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDeXuatGridDto>>($"api/app/quanly-dexuat?loaiDoiTuong={type}");
                if (res != null)
                {
                    _allDeXuatList = res;
                    if (FindName("dgDeXuat") is DataGrid dg) dg.ItemsSource = _allDeXuatList;
                }
                BtnLamMoiForm_Click(this, new RoutedEventArgs());
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed;
            }
        }

        private async void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                await LoadLookupsAsync();
                await LoadDataAsync();
            }
        }

        private void DgDeXuat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDeXuat") is DataGrid dg && dg.SelectedItem is QuanLyDeXuatGridDto item)
            {
                _selectedItem = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblFormTitle") is TextBlock title) title.Text = "Chi tiết Đề Xuất";

                // Sử dụng FindName nghiêm ngặt
                if (FindName("cmbDoiTuongGoc") is ComboBox c1) c1.SelectedValue = item.IdGoc;
                if (FindName("cmbDoiTuongDeXuat") is ComboBox c2) c2.SelectedValue = item.IdDeXuat;
                if (FindName("txtDoLienQuan") is TextBox t1) t1.Text = item.DoLienQuan.ToString();
                if (FindName("txtLoaiDeXuat") is TextBox t2) t2.Text = item.LoaiDeXuat;

                // Khóa ComboBox khi Edit (vì cấu trúc DB sử dụng Composite Key, đổi gốc/đề xuất cần xóa và tạo mới)
                if (FindName("cmbDoiTuongGoc") is ComboBox lc1) lc1.IsEnabled = false;
                if (FindName("cmbDoiTuongDeXuat") is ComboBox lc2) lc2.IsEnabled = false;
                if (FindName("txtLoaiDeXuat") is TextBox lt2) lt2.IsEnabled = false;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = null;
            if (FindName("dgDeXuat") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblFormTitle") is TextBlock title) title.Text = "Thêm Đề Xuất Mới";

            if (FindName("cmbDoiTuongGoc") is ComboBox c1) { c1.SelectedIndex = -1; c1.IsEnabled = true; }
            if (FindName("cmbDoiTuongDeXuat") is ComboBox c2) { c2.SelectedIndex = -1; c2.IsEnabled = true; }
            if (FindName("txtDoLienQuan") is TextBox t1) t1.Text = "0.5";
            if (FindName("txtLoaiDeXuat") is TextBox t2) { t2.Text = ""; t2.IsEnabled = true; }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            int? idGoc = (FindName("cmbDoiTuongGoc") as ComboBox)?.SelectedValue as int?;
            int? idDeXuat = (FindName("cmbDoiTuongDeXuat") as ComboBox)?.SelectedValue as int?;
            string doLienQuanText = (FindName("txtDoLienQuan") as TextBox)?.Text.Trim() ?? "0";
            string loaiDeXuat = (FindName("txtLoaiDeXuat") as TextBox)?.Text.Trim() ?? "";

            if (idGoc == null || idDeXuat == null || string.IsNullOrEmpty(loaiDeXuat))
            {
                MessageBox.Show("Vui lòng chọn đủ đối tượng gốc, đối tượng đề xuất và loại đề xuất!");
                return;
            }

            if (!double.TryParse(doLienQuanText, out double doLienQuan))
            {
                MessageBox.Show("Độ liên quan phải là số hợp lệ!");
                return;
            }

            var dto = new QuanLyDeXuatSaveDto
            {
                LoaiDoiTuong = GetCurrentLoaiDoiTuong(),
                IdGoc = idGoc.Value,
                IdDeXuat = idDeXuat.Value,
                DoLienQuan = doLienQuan,
                LoaiDeXuat = loaiDeXuat
            };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res;
                if (_selectedItem == null)
                {
                    res = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-dexuat", dto);
                }
                else
                {
                    // Đối với composite key, nếu muốn Update DoLienQuan, cần tạo API PUT chuyên dụng (nếu cần).
                    // Tạm thời cơ chế xoá/thêm đã thay thế nếu đổi LoaiDeXuat.
                    MessageBox.Show("Để cập nhật, vui lòng Xóa dữ liệu cũ và Thêm lại Đề xuất mới với Độ liên quan điều chỉnh.");
                    return;
                }

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Lưu thành công!");
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            if (MessageBox.Show($"Bạn chắc chắn xóa đề xuất này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    string type = GetCurrentLoaiDoiTuong();
                    string requestUrl = $"api/app/quanly-dexuat?loaiDoiTuong={type}&idGoc={_selectedItem.IdGoc}&idDeXuat={_selectedItem.IdDeXuat}&loaiDeXuat={Uri.EscapeDataString(_selectedItem.LoaiDeXuat)}";

                    var res = await ApiClient.Instance.DeleteAsync(requestUrl);
                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Xóa thành công!");
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                    }
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        #region ĐIỀU HƯỚNG
        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Không có trang trước đó để quay lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}