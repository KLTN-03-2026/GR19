using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.Common
{
    public partial class PhieuLuongPreviewWindow : Window
    {
        private int _idPhieuLuong;

        public PhieuLuongPreviewWindow(int idPhieuLuong)
        {
            InitializeComponent();
            _idPhieuLuong = idPhieuLuong;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

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
                            t10.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                        }
                        else
                        {
                            t10.Text = "Trạng thái: CHƯA PHÁT";
                            t10.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                        }
                    }
                }
            }
            catch { MessageBox.Show("Không thể tải chi tiết phiếu lương."); }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // 1. Tạm ẩn 2 nút để khi in không bị hiện lên phiếu
                    btnInPhieu.Visibility = Visibility.Collapsed;
                    btnDong.Visibility = Visibility.Collapsed;

                    if (FindName("PrintArea") is FrameworkElement pArea)
                    {
                        // 2. Lấy ScrollViewer chứa giao diện phiếu
                        var scrollViewer = pArea.Parent as ScrollViewer;
                        if (scrollViewer == null)
                        {
                            btnInPhieu.Visibility = Visibility.Visible;
                            btnDong.Visibility = Visibility.Visible;
                            return;
                        }

                        // 3. TẠM THỜI gỡ Border (PrintArea) ra khỏi ScrollViewer
                        var originalContent = scrollViewer.Content;
                        scrollViewer.Content = null;

                        // Lưu lại Width cũ (450)
                        double oldWidth = pArea.Width;

                        // 4. Đặt lại kích thước để tự động co giãn theo chiều rộng khổ giấy
                        pArea.Width = printDialog.PrintableAreaWidth;
                        pArea.Height = double.NaN; // Tự động co giãn chiều cao

                        // 5. Cập nhật layout để WPF tính toán lại kích thước thật
                        pArea.Measure(new Size(printDialog.PrintableAreaWidth, double.PositiveInfinity));
                        pArea.Arrange(new Rect(new Point(0, 0), pArea.DesiredSize));

                        // 6. Ra lệnh In
                        printDialog.PrintVisual(pArea, "In Phiếu Lương Cafebook");

                        // 7. Trả lại thông số giao diện như cũ sau khi in xong
                        pArea.Width = oldWidth;
                        pArea.Height = double.NaN;
                        scrollViewer.Content = originalContent;
                    }

                    // Hiện lại nút
                    btnInPhieu.Visibility = Visibility.Visible;
                    btnDong.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in ấn: Vui lòng kiểm tra lại thiết lập máy in.\nChi tiết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                // Đảm bảo nút luôn được hiện lại nếu có lỗi xảy ra
                btnInPhieu.Visibility = Visibility.Visible;
                btnDong.Visibility = Visibility.Visible;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}