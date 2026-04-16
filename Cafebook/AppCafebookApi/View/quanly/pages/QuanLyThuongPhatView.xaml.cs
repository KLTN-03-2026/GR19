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
    public partial class QuanLyThuongPhatView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyThuongPhatGridDto> _allDataList = new();
        private QuanLyThuongPhatGridDto? _selectedItem = null;

        static QuanLyThuongPhatView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyThuongPhatView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_LUONG")) // Dùng chung quyền Lương
            {
                MessageBox.Show("Từ chối truy cập module Thưởng phạt!");
                this.NavigationService?.GoBack(); return;
            }

            ApplyPermissions();

            // Mặc định load tháng hiện tại
            if (FindName("dpFilterTuNgay") is DatePicker tu) tu.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (FindName("dpFilterDenNgay") is DatePicker den) den.SelectedDate = DateTime.Now;

            await LoadLookupsAsync();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_LUONG");
            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
            if (FindName("btnLamMoiForm") is Button btn1) btn1.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button btn2) btn2.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button btn3) btn3.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                // Tái sử dụng API lấy danh sách nhân viên từ module Chấm công
                var nvs = await httpClient.GetFromJsonAsync<List<ChamCongNhanVienLookupDto>>("api/app/quanly-chamcong/nhanvien-lookup");
                if (nvs != null)
                {
                    if (FindName("cmbNhanVien") is ComboBox cmbForm) cmbForm.ItemsSource = nvs;

                    if (FindName("cmbFilterNhanVien") is ComboBox cmbFilter)
                    {
                        var filterList = new List<ChamCongNhanVienLookupDto> { new ChamCongNhanVienLookupDto { IdNhanVien = 0, HoTen = "Tất cả" } };
                        filterList.AddRange(nvs);
                        cmbFilter.ItemsSource = filterList;
                        cmbFilter.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var queryParams = new List<string>();
                if (FindName("cmbFilterNhanVien") is ComboBox cmb && cmb.SelectedValue is int idNv && idNv > 0) queryParams.Add($"idNhanVien={idNv}");
                if (FindName("dpFilterTuNgay") is DatePicker tu && tu.SelectedDate.HasValue) queryParams.Add($"tuNgay={tu.SelectedDate.Value:yyyy-MM-dd}");
                if (FindName("dpFilterDenNgay") is DatePicker den && den.SelectedDate.HasValue) queryParams.Add($"denNgay={den.SelectedDate.Value:yyyy-MM-dd}");

                string url = "api/app/quanly-thuongphat/search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
                var res = await httpClient.GetFromJsonAsync<List<QuanLyThuongPhatGridDto>>(url);

                if (res != null && FindName("dgThuongPhat") is DataGrid dg)
                {
                    _allDataList = res;
                    dg.ItemsSource = _allDataList;
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private void DgThuongPhat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgThuongPhat") is DataGrid dg && dg.SelectedItem is QuanLyThuongPhatGridDto item)
            {
                _selectedItem = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblFormTitle") is TextBlock title) title.Text = "Cập nhật Thưởng / Phạt";

                // Map UI Form (Giả lập việc binding ngược từ danh sách)
                // Chú ý: Backend hiện tại trả về TenNguoiTao chứ không phải IdNhanVien trong GridDto. 
                // Do đó tính năng xem lại sẽ chỉ hiển thị Data cơ bản. Nếu sửa, ta cần IdNhanVien.
                // Ở đây ta khóa Form nếu Đã Chốt.

                bool isLocked = item.DaChot;

                if (FindName("cmbLoai") is ComboBox cbLoai) cbLoai.Text = item.Loai;
                if (FindName("dpNgayTao") is DatePicker dp) dp.SelectedDate = item.NgayTao;
                if (FindName("txtSoTien") is TextBox t1) t1.Text = item.SoTien.ToString("0.##");
                if (FindName("txtLyDo") is TextBox t2) t2.Text = item.LyDo;

                if (FindName("cmbNhanVien") is ComboBox cbNv) cbNv.IsEnabled = !isLocked;
                if (FindName("btnLuu") is Button b1) b1.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
                if (FindName("btnXoa") is Button b2) b2.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
                if (FindName("txtCanhBaoChot") is TextBlock txtW) txtW.Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = null;
            if (FindName("dgThuongPhat") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblFormTitle") is TextBlock title) title.Text = "Thêm Khoản Mới";

            if (FindName("cmbNhanVien") is ComboBox cb1) { cb1.SelectedItem = null; cb1.IsEnabled = true; }
            if (FindName("cmbLoai") is ComboBox cb2) cb2.SelectedIndex = 0;
            if (FindName("dpNgayTao") is DatePicker dp) dp.SelectedDate = DateTime.Today;
            if (FindName("txtSoTien") is TextBox t1) t1.Text = "";
            if (FindName("txtLyDo") is TextBox t2) t2.Text = "";

            if (FindName("btnLuu") is Button b1) b1.Visibility = Visibility.Visible;
            if (FindName("btnXoa") is Button b2) b2.Visibility = Visibility.Visible;
            if (FindName("txtCanhBaoChot") is TextBlock txtW) txtW.Visibility = Visibility.Collapsed;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            int idNv = (FindName("cmbNhanVien") as ComboBox)?.SelectedValue as int? ?? 0;
            if (idNv == 0) { MessageBox.Show("Vui lòng chọn Nhân viên!"); return; }

            if (!decimal.TryParse((FindName("txtSoTien") as TextBox)?.Text, out decimal soTien) || soTien <= 0)
            {
                MessageBox.Show("Số tiền phải lớn hơn 0!"); return;
            }

            var dto = new QuanLyThuongPhatSaveDto
            {
                IdNhanVien = idNv,
                IdNguoiTao = AuthService.CurrentUser?.IdNhanVien ?? 1,
                Loai = (FindName("cmbLoai") as ComboBox)?.Text ?? "Thưởng",
                NgayTao = (FindName("dpNgayTao") as DatePicker)?.SelectedDate ?? DateTime.Today,
                SoTien = soTien,
                LyDo = (FindName("txtLyDo") as TextBox)?.Text.Trim() ?? ""
            };

            if (string.IsNullOrEmpty(dto.LyDo)) { MessageBox.Show("Vui lòng nhập lý do!"); return; }

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _selectedItem == null
                    ? await httpClient.PostAsJsonAsync("api/app/quanly-thuongphat", dto)
                    : await httpClient.PutAsJsonAsync($"api/app/quanly-thuongphat/{_selectedItem.IdPhieuThuongPhat}", dto);

                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); BtnLamMoiForm_Click(this, new RoutedEventArgs()); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            if (MessageBox.Show("Xác nhận xóa bản ghi này?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var res = await httpClient.DeleteAsync($"api/app/quanly-thuongphat/{_selectedItem.IdPhieuThuongPhat}");
                    if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); await LoadDataAsync(); BtnLamMoiForm_Click(this, new RoutedEventArgs()); }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}