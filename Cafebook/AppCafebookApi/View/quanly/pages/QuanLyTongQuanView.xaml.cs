using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyTongQuanView : Page
    {
        private static readonly HttpClient httpClient;
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        static QuanLyTongQuanView()
        {
            // NGUYÊN TẮC 2: Dynamic URL
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }

        public QuanLyTongQuanView()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
            Labels = Array.Empty<string>();
            YFormatter = value => value.ToString("N0") + " đ";
            this.DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Gắn Bearer Token
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // NGUYÊN TẮC 3: Lớp 2 - Chặn truy cập trang
            if (!AuthService.CoQuyen("QL_TONG_QUAN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Dashboard Tổng Quan!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            // NGUYÊN TẮC 3: Lớp 1 - Ẩn hiện UI chi tiết theo quyền
            ApplyPermissions();

            await LoadDashboardDataAsync();
        }

        private void ApplyPermissions()
        {
            // NGUYÊN TẮC 4: FindName Protection
            var b1 = FindName("btnExportRevenue") as Button;
            var b2 = FindName("btnExportTonKhoSach") as Button;
            var b3 = FindName("btnExportNguyenLieu") as Button;
            var b4 = FindName("btnExportPerformance") as Button;
            var b5 = FindName("btnCaiDat") as Button;
            var header = FindName("txtBaoCaoTacVu") as TextBlock;

            // Cập nhật lại các quyền Báo Cáo chuyên biệt
            if (b1 != null) b1.Visibility = AuthService.CoQuyen("QL_BAO_CAO_DOANH_THU") ? Visibility.Visible : Visibility.Collapsed;
            if (b2 != null) b2.Visibility = AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_SACH") ? Visibility.Visible : Visibility.Collapsed;
            if (b3 != null) b3.Visibility = AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_NL") ? Visibility.Visible : Visibility.Collapsed;
            if (b4 != null) b4.Visibility = AuthService.CoQuyen("QL_BAO_CAO_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
            if (b5 != null) b5.Visibility = AuthService.CoQuyen("CM_CAI_DAT") ? Visibility.Visible : Visibility.Collapsed;

            // Ẩn/Hiện tiêu đề "Báo cáo & Tác vụ"
            if (header != null)
            {
                bool hasAnyPermission = AuthService.CoQuyen("QL_BAO_CAO_DOANH_THU") ||
                                        AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_SACH") ||
                                        AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_NL") ||
                                        AuthService.CoQuyen("QL_BAO_CAO_NHAN_SU") ||
                                        AuthService.CoQuyen("CM_CAI_DAT");
                header.Visibility = hasAnyPermission ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<QuanLyTongQuanDto>("api/app/quanly-tongquan/summary");
                if (response != null)
                {
                    // NGUYÊN TẮC 4: Tìm đúng tên Control trong giao diện gốc của bạn để bơm dữ liệu
                    if (FindName("txtTongDoanhThu") is TextBlock t1) t1.Text = response.TongDoanhThuHomNay.ToString("N0") + " VNĐ";
                    if (FindName("txtTongDonHang") is TextBlock t2) t2.Text = response.TongDonHangHomNay.ToString();
                    if (FindName("txtSanPhamBanChay") is TextBlock t3) t3.Text = response.SanPhamBanChayHomNay;

                    if (response.DoanhThu30Ngay != null && response.DoanhThu30Ngay.Any())
                    {
                        SeriesCollection.Clear();
                        SeriesCollection.Add(new LineSeries
                        {
                            Title = "Doanh thu",
                            Values = new ChartValues<decimal>(response.DoanhThu30Ngay.Select(x => x.TongTien))
                        });
                        Labels = response.DoanhThu30Ngay.Select(x => x.Ngay.ToString("dd/MM")).ToArray();

                        this.DataContext = null;
                        this.DataContext = this;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi API", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Bắt buộc có để thỏa mãn file XAML gốc của bạn
        private void CartesianChart_Loaded(object sender, RoutedEventArgs e) { }

        // --- Các hàm Click điều hướng (Tích hợp bảo mật lớp 2) ---
        private void BtnExportRevenue_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_BAO_CAO_DOANH_THU"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoDoanhThuView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnExportTonKhoSach_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_SACH"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoTonKhoSachView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnExportNguyenLieu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_NL"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoTonKhoNguyenLieuView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnExportPerformance_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_BAO_CAO_NHAN_SU"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoHieuSuatView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCaiDat_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("CM_CAI_DAT"))
            {
                this.NavigationService?.Navigate(new QuanLyCaiDatHeThongView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền truy cập Cài đặt hệ thống!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}