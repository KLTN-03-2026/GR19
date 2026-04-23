// File: AppCafebookApi/View/quanly/pages/QuanLyTongQuanView.xaml.cs
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
using System.Windows.Media;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyTongQuanView : Page
    {
        //private static readonly HttpClient httpClient;

        public SeriesCollection LineSeriesCollection { get; set; }
        public SeriesCollection BarSeriesCollection { get; set; }
        public SeriesCollection PieSeriesCollection { get; set; }

        public string[] LineLabels { get; set; }
        public string[] BarLabels { get; set; }

        public Func<double, string> CurrencyFormatter { get; set; }
        public Func<double, string> NumberFormatter { get; set; }
        /*
        static QuanLyTongQuanView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }
        */
        public QuanLyTongQuanView()
        {
            InitializeComponent();

            LineSeriesCollection = new SeriesCollection();
            BarSeriesCollection = new SeriesCollection();
            PieSeriesCollection = new SeriesCollection();

            LineLabels = Array.Empty<string>();
            BarLabels = Array.Empty<string>();

            CurrencyFormatter = value => value.ToString("N0") + " đ";
            NumberFormatter = value => value.ToString("N0");

            this.DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            bool hasAnyPermission = AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN", "QL_BAO_CAO_TON_KHO_SACH", "QL_BAO_CAO_TON_KHO_NL", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_BAO_CAO_DOANH_THU", "CM_CAI_DAT", "CM_NHAT_KY_HE_THONG");

            if (!hasAnyPermission)
            {
                MessageBox.Show("Bạn không có quyền truy cập phân hệ Tổng quan & Báo cáo!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            if (AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN"))
            {
                await LoadDashboardData();
            }
            else
            {
                // [FIX LỖI]: Ép kiểu sang UIElement vì đã xóa ScrollViewer
                if (FindName("GridDuLieuTongQuan") is UIElement gridData) gridData.Visibility = Visibility.Collapsed;
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
                var summary = await ApiClient.Instance.GetFromJsonAsync<QuanLyTongQuanDto>("api/app/quanly-tongquan/summary");
                if (summary != null)
                {
                    if (FindName("txtDoanhThu") is TextBlock t1) t1.Text = summary.TongDoanhThuHomNay.ToString("N0") + " đ";
                    if (FindName("txtSPBanChay") is TextBlock t3) t3.Text = string.IsNullOrEmpty(summary.SanPhamBanChayHomNay) ? "---" : summary.SanPhamBanChayHomNay;
                    if (FindName("txtSLBanChay") is TextBlock t4)
                    {
                        t4.Visibility = Visibility.Visible;
                        t4.Text = summary.SoLuongBanChayHomNay > 0 ? $"{summary.SoLuongBanChayHomNay} đã bán" : "0 đã bán";
                    }

                    if (summary.DoanhThu30Ngay != null && summary.DoanhThu30Ngay.Count > 0)
                    {
                        LineSeriesCollection.Clear();
                        LineSeriesCollection.Add(new LineSeries
                        {
                            Title = "Doanh thu",
                            Values = new ChartValues<double>(summary.DoanhThu30Ngay.Select(x => (double)x.TongTien)),
                            PointGeometrySize = 8,
                            StrokeThickness = 3,
                            Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                            Fill = new SolidColorBrush(Color.FromArgb(60, 33, 150, 243))
                        });
                        LineLabels = summary.DoanhThu30Ngay.Select(x => x.Ngay.ToString("dd/MM")).ToArray();
                        if (FindName("ChartDoanhThu") is CartesianChart chart1) chart1.Update(true, true);
                    }

                    if (summary.Top5SanPham != null && summary.Top5SanPham.Count > 0)
                    {
                        BarSeriesCollection.Clear();
                        BarSeriesCollection.Add(new ColumnSeries
                        {
                            Title = "Lượt bán",
                            Values = new ChartValues<int>(summary.Top5SanPham.Select(x => x.SoLuong)),
                            MaxColumnWidth = 40,
                            Fill = new SolidColorBrush(Color.FromRgb(255, 152, 0))
                        });
                        BarLabels = summary.Top5SanPham.Select(x => TrimName(x.TenSanPham)).ToArray();
                        if (FindName("ChartTopSp") is CartesianChart chart2) chart2.Update(true, true);
                    }

                    if (summary.CoCauDoanhThu != null && summary.CoCauDoanhThu.Count > 0)
                    {
                        PieSeriesCollection.Clear();
                        foreach (var item in summary.CoCauDoanhThu)
                        {
                            PieSeriesCollection.Add(new PieSeries
                            {
                                Title = item.TenDanhMuc,
                                Values = new ChartValues<double> { (double)item.GiaTri },
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0:P0}", chartPoint.Participation)
                            });
                        }
                        if (FindName("ChartCoCau") is PieChart chart3) chart3.Update(true, true);
                    }

                    DataContext = null;
                    DataContext = this;
                }
            }
            catch { }
        }

        private string TrimName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return name.Length > 12 ? name.Substring(0, 10) + "..." : name;
        }

        private void BtnXemDoanhThu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_DOANH_THU")) this.NavigationService?.Navigate(new QuanLyBaoCaoDoanhThuView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Báo cáo doanh thu", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnXemSach_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_TON_KHO_SACH")) this.NavigationService?.Navigate(new QuanLyBaoCaoTonKhoSachView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Báo cáo tồn kho sách", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnXemNguyenLieu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_TON_KHO_NL")) this.NavigationService?.Navigate(new QuanLyBaoCaoTonKhoNguyenLieuView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Báo cáo tồn kho nguyên liệu", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnXemNhanSu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_NHAN_SU")) this.NavigationService?.Navigate(new QuanLyBaoCaoNhanSuView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Báo cáo nhân sự", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnXemHieuSuat_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU")) this.NavigationService?.Navigate(new QuanLyBaoCaoHieuSuatView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Báo cáo hiệu suất nhân sự", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnCaiDat_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "CM_CAI_DAT")) this.NavigationService?.Navigate(new QuanLyCaiDatView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Cài đặt", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnNhatKyHeThong_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "CM_NHAT_KY_HE_THONG")) this.NavigationService?.Navigate(new QuanLyNhatKyView());
            else MessageBox.Show("Từ chối truy cập! bạn không có quyền xem Nhật ký hệ thống", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}