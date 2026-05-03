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
    public partial class QuanLyLichSuThueSachView : Page
    {
        private bool _isDataLoaded = false;

        public QuanLyLichSuThueSachView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_LICH_SU_THUE_SACH"))
            {
                MessageBox.Show("Từ chối truy cập module Lịch sử thuê sách!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                ApplyPermissions();

                var tuNgay = DateTime.Today.AddDays(-30);
                var denNgay = DateTime.Today;

                if (FindName("dpTuNgay") is DatePicker tu) tu.SelectedDate = tuNgay;
                if (FindName("dpDenNgay") is DatePicker den) den.SelectedDate = denNgay;

                await LoadDataAsync(tuNgay, denNgay);

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải lịch sử thuê sách: {ex.Message}");
            }
        }

        private void ApplyPermissions()
        {
            // BẢO MẬT LỚP 1: Ẩn/Hiện UI
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_LICH_SU_THUE_SACH");
            if (FindName("GridDuLieu") is Border b1) b1.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b2) b2.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task LoadDataAsync(DateTime? fromDate, DateTime? toDate)
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                string url = "api/app/quanly-lichsuthuesach/data";
                var queryParams = new List<string>();

                if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

                if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

                var res = await ApiClient.Instance.GetFromJsonAsync<BaoCaoLichSuThueDto>(url);
                if (res != null)
                {
                    if (FindName("dgSachQuaHan") is DataGrid dg1) dg1.ItemsSource = res.SachQuaHan;
                    if (FindName("dgLichSuThue") is DataGrid dg2) dg2.ItemsSource = res.LichSuThue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối máy chủ: {ex.Message}");
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e)
        {
            DateTime? fromDate = (FindName("dpTuNgay") as DatePicker)?.SelectedDate;
            DateTime? toDate = (FindName("dpDenNgay") as DatePicker)?.SelectedDate;

            if (fromDate > toDate)
            {
                MessageBox.Show("Từ ngày không thể lớn hơn Đến ngày!", "Lỗi chọn ngày");
                return;
            }

            await LoadDataAsync(fromDate, toDate);
        }

        private async void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("dpTuNgay") is DatePicker tu) tu.SelectedDate = null;
            if (FindName("dpDenNgay") is DatePicker den) den.SelectedDate = null;
            await LoadDataAsync(null, null);
        }

        private void BtnLienHe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SachQuaHanGridDto item)
            {
                Clipboard.SetText(item.SoDienThoai);
                MessageBox.Show($"Đã copy Số điện thoại của khách hàng: {item.SoDienThoai}\n\nBạn có thể dán vào Zalo/Tin nhắn để liên hệ thu hồi sách '{item.TenSach}'.", "Liên hệ Khách hàng");
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}