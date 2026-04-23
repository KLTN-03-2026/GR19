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
    public partial class QuanLyThongBaoView : Page
    {
        //private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") };
        private List<QuanLyThongBaoGridDto> _allData = new();
        private QuanLyThongBaoGridDto? _selectedItem = null;

        // Danh sách TẤT CẢ các loại thông báo dùng cho bộ lọc
        private readonly List<string> _allTypes = new() { "Tất cả", "SuCoBan", "HetHang", "DonXinNghi", "Kho", "DatBan", "CanhBaoKho", "PhieuGoiMon", "DonHangMoi", "PhanHoiKhachHang","DangKyLichMoi", "ThongBaoNhanVien", "ThongBaoQuanLy", "ThongBaoToanNhanVien" };

        // Danh sách THỦ CÔNG chỉ dành cho Tạo Mới
        private readonly List<string> _manualTypes = new() { "ThongBaoNhanVien", "ThongBaoQuanLy", "ThongBaoToanNhanVien" };

        public QuanLyThongBaoView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // Cho phép Admin (FULL_QL) hoặc Quyền quản lý thông báo được xem
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_THONG_BAO") || AuthService.CoQuyen("FULL_QL", "FULL_QL");
            if (!hasQuyen)
            {
                GridDuLieu.Visibility = Visibility.Collapsed;
                txtThongBaoKhongCoQuyen.Visibility = Visibility.Visible;
                return;
            }

            dpFilterTuNgay.SelectedDate = DateTime.Today.AddDays(-7);
            dpFilterDenNgay.SelectedDate = DateTime.Today;

            cmbFilterLoai.ItemsSource = _allTypes;
            cmbFilterLoai.SelectedIndex = 0;

            SetFormComboBoxManualOnly();
            await LoadDataAsync();
        }

        private void SetFormComboBoxManualOnly()
        {
            cmbFormLoai.ItemsSource = _manualTypes.Select(t => new { Display = t, Value = t }).ToList();
            cmbFormLoai.SelectedIndex = 0;
            cmbFormLoai.IsEnabled = true;
            lblThongTinHeThong.Visibility = Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                var queryParams = new List<string>();
                if (dpFilterTuNgay.SelectedDate.HasValue) queryParams.Add($"tuNgay={dpFilterTuNgay.SelectedDate.Value:yyyy-MM-dd}");
                if (dpFilterDenNgay.SelectedDate.HasValue) queryParams.Add($"denNgay={dpFilterDenNgay.SelectedDate.Value:yyyy-MM-dd}");

                if (cmbFilterLoai.SelectedItem != null && cmbFilterLoai.SelectedItem.ToString() != "Tất cả")
                    queryParams.Add($"loaiThongBao={cmbFilterLoai.SelectedItem.ToString()}");

                if (!string.IsNullOrWhiteSpace(txtKeyword.Text))
                    queryParams.Add($"keyword={txtKeyword.Text.Trim()}");

                string url = "api/app/quanly-thongbao/search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyThongBaoGridDto>>(url);

                if (res != null)
                {
                    _allData = res;
                    dgThongBao.ItemsSource = _allData;
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}"); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private void DgThongBao_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgThongBao.SelectedItem is QuanLyThongBaoGridDto item)
            {
                _selectedItem = item;
                formChiTiet.IsEnabled = true;
                lblFormTitle.Text = "Cập nhật Thông báo";

                txtNoiDung.Text = item.NoiDung;
                chkDaXem.IsChecked = item.DaXem;

                // Xử lý ComboBox Loại Thông báo
                cmbFormLoai.ItemsSource = new List<object> { new { Display = item.LoaiThongBao, Value = item.LoaiThongBao } };
                cmbFormLoai.SelectedValue = item.LoaiThongBao;

                // Khóa không cho sửa Loại nếu là thông báo cũ (Bảo vệ dữ liệu hệ thống)
                cmbFormLoai.IsEnabled = false;

                lblThongTinHeThong.Visibility = item.IsSystemAlert ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = null;
            dgThongBao.SelectedItem = null;
            formChiTiet.IsEnabled = true;
            lblFormTitle.Text = "Tạo Thông báo Thủ công";

            txtNoiDung.Text = "";
            chkDaXem.IsChecked = false;

            SetFormComboBoxManualOnly();
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Do cmbFormLoai có DisplayMemberPath/SelectedValuePath là đối tượng vô danh nên lấy string value
            var loai = cmbFormLoai.SelectedValue?.ToString() ?? "";
            var noiDung = txtNoiDung.Text.Trim();
            var daXem = chkDaXem.IsChecked ?? false;

            if (string.IsNullOrEmpty(noiDung)) { MessageBox.Show("Vui lòng nhập nội dung!"); return; }

            var dto = new QuanLyThongBaoSaveDto
            {
                LoaiThongBao = loai,
                NoiDung = noiDung,
                DaXem = daXem,
                IdNhanVienTao = AuthService.CurrentUser?.IdNhanVien ?? 1
            };

            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _selectedItem == null
                    ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-thongbao", dto)
                    : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-thongbao/{_selectedItem.IdThongBao}", dto);

                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            catch { MessageBox.Show("Lỗi kết nối API"); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            if (MessageBox.Show("Xác nhận xóa thông báo này?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-thongbao/{_selectedItem.IdThongBao}");
                    if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}