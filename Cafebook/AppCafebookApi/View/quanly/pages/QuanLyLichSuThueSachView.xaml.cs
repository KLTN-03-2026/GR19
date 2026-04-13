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
        private static readonly HttpClient httpClient;

        static QuanLyLichSuThueSachView()
        {
            httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") };
        }

        public QuanLyLichSuThueSachView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // BẢO MẬT LỚP 2: Chặn truy cập Page
            if (!AuthService.CoQuyen("FULL_QL", "QL_LICH_SU_THUE_SACH"))
            {
                MessageBox.Show("Từ chối truy cập module Lịch sử thuê sách!");
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            // Nếu có quyền thì tự động load dữ liệu 30 ngày gần nhất
            if (AuthService.CoQuyen("FULL_QL", "QL_LICH_SU_THUE_SACH"))
            {
                if (FindName("dpTuNgay") is DatePicker tu) tu.SelectedDate = DateTime.Today.AddDays(-30);
                if (FindName("dpDenNgay") is DatePicker den) den.SelectedDate = DateTime.Today;
                await LoadDataAsync(DateTime.Today.AddDays(-30), DateTime.Today);
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

                var res = await httpClient.GetFromJsonAsync<BaoCaoLichSuThueDto>(url);
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