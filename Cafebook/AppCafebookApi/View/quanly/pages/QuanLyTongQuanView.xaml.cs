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
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:";
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
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            // 1. KIỂM TRA CHÌA KHÓA CỔNG
            bool hasAnyPermission = AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN", "QL_BAO_CAO_TON_KHO_SACH", "QL_BAO_CAO_TON_KHO_NL", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_BAO_CAO_DOANH_THU", "CM_CAI_DAT", "CM_NHAT_KY_HE_THONG");

            if (!hasAnyPermission)
            {
                MessageBox.Show("Bạn không có quyền truy cập phân hệ Tổng quan & Báo cáo!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions(); // Ẩn hiện các nút chức năng bên phải

            // 2. KIỂM TRA CHÌA KHÓA PHÒNG (Có quyền xem Dashboard không?)
            if (AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN"))
            {
                await LoadDashboardData();
            }
            else
            {
                if (FindName("GridDuLieuTongQuan") is ScrollViewer gridData) gridData.Visibility = Visibility.Collapsed;
                if (FindName("txtThongBaoKhongCoQuyen") is Border txtThongBao) txtThongBao.Visibility = Visibility.Visible;
            }
        }

        private void ApplyPermissions()
        {
            if (FindName("btnXemDoanhThu") is Button b1) b1.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_DOANH_THU") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXemSach") is Button b2) b2.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_TON_KHO_SACH") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXemNguyenLieu") is Button b3) b3.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_TON_KHO_NL") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXemNhanSu") is Button b4) b4.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXemHieuSuat") is Button b5) b5.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnCaiDat") is Button b6) b6.Visibility = AuthService.CoQuyen("FULL_QL", "CM_CAI_DAT") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNhatKyHeThong") is Button b7) b7.Visibility = AuthService.CoQuyen("FULL_QL", "CM_NHAT_KY_HE_THONG") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Gọi API lấy dữ liệu DTO gốc của bạn
                var summary = await httpClient.GetFromJsonAsync<QuanLyTongQuanDto>("api/app/quanly-tongquan/summary");
                if (summary != null)
                {
                    // Map đúng các thuộc tính trong QuanLyTongQuanDto.cs
                    if (FindName("txtDoanhThu") is TextBlock t1) t1.Text = summary.TongDoanhThuHomNay.ToString("N0") + " đ";
                    if (FindName("txtDonHang") is TextBlock t2) t2.Text = summary.TongDonHangHomNay.ToString();
                    if (FindName("txtSPBanChay") is TextBlock t3) t3.Text = string.IsNullOrEmpty(summary.SanPhamBanChayHomNay) ? "---" : summary.SanPhamBanChayHomNay;

                    // Do API cũ chưa có trả về 2 trường này nên tạm ẩn/để trống
                    if (FindName("txtSLBanChay") is TextBlock t4) t4.Visibility = Visibility.Collapsed;
                    if (FindName("txtNhanVien") is TextBlock t5) t5.Text = "---";

                    var chartData = summary.DoanhThu30Ngay;
                    if (chartData != null && chartData.Count > 0)
                    {
                        SeriesCollection.Clear();
                        var lineSeries = new LineSeries
                        {
                            Title = "Doanh thu",
                            Values = new ChartValues<double>(chartData.Select(x => (double)x.TongTien)),
                            PointGeometrySize = 10,
                            StrokeThickness = 3,
                            Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)),
                            Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 33, 150, 243))
                        };
                        SeriesCollection.Add(lineSeries);

                        // Format Ngay từ DB thành "dd/MM"
                        Labels = chartData.Select(x => x.Ngay.ToString("dd/MM")).ToArray();

                        var chart = FindName("CartesianChart") as CartesianChart;
                        if (chart != null)
                        {
                            chart.Update(true, true);
                        }
                    }
                }
            }
            catch { }
        }

        private void BtnXemDoanhThu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_DOANH_THU"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoDoanhThuView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXemSach_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_TON_KHO_SACH"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoTonKhoSachView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXemNguyenLieu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_TON_KHO_NL"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoTonKhoNguyenLieuView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXemNhanSu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_NHAN_SU"))
            {
                this.NavigationService?.Navigate(new QuanLyBaoCaoNhanSuView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền thực hiện tác vụ này!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnXemHieuSuat_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU"))
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
            if (AuthService.CoQuyen("FULL_QL", "CM_CAI_DAT"))
            {
                this.NavigationService?.Navigate(new QuanLyCaiDatView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền truy cập Cài đặt hệ thống!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /*
        private void BtnThongBao_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_THONG_BAO"))
            {
                this.NavigationService?.Navigate(new QuanLyThongBaoView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền truy cập Quản lý thông báo!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        */
        private void BtnNhatKyHeThong_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "CM_NHAT_KY_HE_THONG"))
            {
                this.NavigationService?.Navigate(new QuanLyNhatKyView());
            }
            else
            {
                MessageBox.Show("Bạn không có quyền truy cập Nhật ký hệ thống!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}