using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.Common
{
    public partial class PhieuLuongPreviewWindow : Window
    {
        //private static readonly HttpClient httpClient;
        private int _idPhieuLuong;

        //static PhieuLuongPreviewWindow() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public PhieuLuongPreviewWindow(int idPhieuLuong)
        {
            InitializeComponent();
            _idPhieuLuong = idPhieuLuong;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            await LoadChiTietAsync();
        }

        private async Task LoadChiTietAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var detail = await ApiClient.Instance.GetFromJsonAsync<PhatLuongDetailDto>($"api/app/phatluong/chitiet/{_idPhieuLuong}");
                if (detail != null)
                {
                    // Gán thông tin Cấu hình quán
                    if (FindName("lblTenQuan") is TextBlock tq) tq.Text = detail.TenQuan.ToUpper();
                    if (FindName("lblDiaChiQuan") is TextBlock tdc) { tdc.Text = string.IsNullOrEmpty(detail.DiaChiQuan) ? "" : $"Địa chỉ: {detail.DiaChiQuan}"; tdc.Visibility = string.IsNullOrEmpty(detail.DiaChiQuan) ? Visibility.Collapsed : Visibility.Visible; }
                    if (FindName("lblSdtQuan") is TextBlock tsdt) { tsdt.Text = string.IsNullOrEmpty(detail.SoDienThoaiQuan) ? "" : $"Điện thoại: {detail.SoDienThoaiQuan}"; tsdt.Visibility = string.IsNullOrEmpty(detail.SoDienThoaiQuan) ? Visibility.Collapsed : Visibility.Visible; }

                    if (FindName("lblKyLuong") is TextBlock t1) t1.Text = $"Kỳ Lương: {detail.KyLuong}";
                    if (FindName("lblTenNhanVien") is TextBlock t2) t2.Text = detail.TenNhanVien;
                    if (FindName("lblNgayChot") is TextBlock t3) t3.Text = detail.NgayChot.ToString("dd/MM/yyyy HH:mm");

                    if (FindName("lblLuongCoBan") is TextBlock t4) t4.Text = detail.LuongCoBan.ToString("N0") + " đ";
                    if (FindName("lblTongGioLam") is TextBlock t5) t5.Text = detail.TongGioLam.ToString("0.##") + " h";
                    if (FindName("lblLuongGoc") is TextBlock t6) t6.Text = detail.LuongGoc.ToString("N0") + " đ";

                    if (FindName("dgThuongPhat") is DataGrid dg) dg.ItemsSource = detail.DanhSachThuongPhat;

                    if (FindName("lblTongThuong") is TextBlock t7) t7.Text = detail.TienThuong.ToString("N0") + " đ";
                    if (FindName("lblTongKhauTru") is TextBlock t8) t8.Text = detail.KhauTru.ToString("N0") + " đ";

                    if (FindName("lblThucLanh") is TextBlock t9) t9.Text = detail.ThucLanh.ToString("N0") + " đ";

                    if (FindName("lblTrangThai") is TextBlock t10)
                    {
                        if (detail.TrangThai == "Đã phát")
                        {
                            t10.Text = "Trạng thái: ĐÃ THANH TOÁN LƯƠNG";
                            t10.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)); // Green
                            if (FindName("btnXacNhanPhat") is Button bX) bX.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            t10.Text = "Trạng thái: CHƯA PHÁT";
                        }
                    }
                }
            }
            catch { MessageBox.Show("Không thể tải chi tiết phiếu lương."); }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXacNhanPhat_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận đã thanh toán Lương bằng Tiền mặt / Chuyển khoản cho nhân viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    HttpResponseMessage res = await ApiClient.Instance.PutAsJsonAsync($"api/app/phatluong/xacnhan/{_idPhieuLuong}", new { });
                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Xác nhận thành công!");
                        this.DialogResult = true; // Báo hiệu cho màn hình danh sách load lại
                        this.Close();
                    }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true && FindName("printArea") is Border pArea)
                {
                    if (pArea.Parent is ScrollViewer scrollViewer) scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

                    printDialog.PrintVisual(pArea, "In Phiếu Lương");

                    if (pArea.Parent is ScrollViewer sv) sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
            }
            catch { MessageBox.Show("Lỗi in ấn."); }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}