using System.Net.Http.Headers;
using System.Net.Http.Json;
using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class PhieuLuongView : Page
    {
        /*private static readonly HttpClient httpClient;

        static PhieuLuongView()
        {
            httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166") };
        }
        */
        public PhieuLuongView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_PHIEU_LUONG"))
            {
                MessageBox.Show("Bạn không có quyền xem Phiếu Lương!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            await LoadDanhSachPhieuLuongAsync();
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService?.CanGoBack == true) this.NavigationService.GoBack();
        }

        private async Task LoadDanhSachPhieuLuongAsync()
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                // SỬA URL: Gắn trực tiếp idNhanVien vào đường dẫn
                var response = await ApiClient.Instance.GetFromJsonAsync<PhieuLuongViewDto>($"api/app/nhanvien/phieuluong/list/{idNhanVien}");

                if (response != null && response.DanhSachPhieuLuong.Any())
                {
                    if (FindName("lbPhieuLuong") is ListBox lb)
                    {
                        lb.ItemsSource = response.DanhSachPhieuLuong;
                        lb.SelectedIndex = 0; // Tự động load phiếu đầu tiên
                    }
                }
                else
                {
                    if (FindName("panelChonPhieu") is StackPanel pnlChon) pnlChon.Visibility = Visibility.Visible;
                    if (FindName("panelChiTiet") is ScrollViewer pnlCT) pnlCT.Visibility = Visibility.Collapsed;
                    if (FindName("txtChonPhieu") is TextBlock txtChon) txtChon.Text = "Bạn chưa có phiếu lương nào trong hệ thống.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadDanhSachPhieuLuongAsync Error]: {ex.Message}");
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadChiTietPhieuLuongAsync(int idPhieuLuong)
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                // SỬA URL: Gắn trực tiếp idNhanVien vào đường dẫn
                var data = await ApiClient.Instance.GetFromJsonAsync<PhieuLuongChiTietDto>($"api/app/nhanvien/phieuluong/detail/{idNhanVien}/{idPhieuLuong}");
                if (data == null) return;

                if (FindName("lblTieuDeChiTiet") is TextBlock t1) t1.Text = $"Chi tiết phiếu lương tháng {data.Thang}/{data.Nam}";
                if (FindName("lblThucLanh") is TextBlock t2) t2.Text = data.ThucLanh.ToString("N0") + " ₫";

                if (FindName("lblTrangThai") is TextBlock tTrangThai)
                {
                    if (data.TrangThai == "Đã phát")
                    {
                        tTrangThai.Text = $"Đã nhận lương ngày {data.NgayPhatLuong:dd/MM/yyyy}";
                        tTrangThai.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                    }
                    else
                    {
                        tTrangThai.Text = "Đã chốt (Đang chờ kế toán/quản lý phát lương)";
                        tTrangThai.Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117));
                    }
                }

                if (FindName("lblLuongCoBan") is TextBlock t3) t3.Text = data.LuongCoBan.ToString("N0") + " ₫/giờ";
                if (FindName("lblTongGioLam") is TextBlock t4) t4.Text = data.TongGioLam.ToString("N2") + " giờ";
                if (FindName("lblTienLuongTheoGio") is TextBlock t5) t5.Text = data.TienLuongTheoGio.ToString("N0") + " ₫";

                if (FindName("dgThuong") is DataGrid dgT) dgT.ItemsSource = data.DanhSachThuong;
                if (FindName("lblTongThuong") is TextBlock t6) t6.Text = $"Tổng thưởng: +{data.TongTienThuong:N0} ₫";

                if (FindName("dgKhauTru") is DataGrid dgP) dgP.ItemsSource = data.DanhSachPhat;
                if (FindName("lblTongKhauTru") is TextBlock t7) t7.Text = $"Tổng phạt: {data.TongKhauTru:N0} ₫";

                if (FindName("panelChonPhieu") is StackPanel pnlChon) pnlChon.Visibility = Visibility.Collapsed;
                if (FindName("panelChiTiet") is ScrollViewer pnlCT) pnlCT.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadChiTietPhieuLuongAsync Error]: {ex.Message}");
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed;
            }
        }

        private async void LbPhieuLuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("lbPhieuLuong") is ListBox lb && lb.SelectedItem is PhieuLuongItemDto selectedItem)
            {
                await LoadChiTietPhieuLuongAsync(selectedItem.IdPhieuLuong);
            }
        }
    }
}