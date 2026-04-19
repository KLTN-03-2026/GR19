using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;

namespace AppCafebookApi.View.common
{
    public partial class PhieuTraPreviewWindow : Window
    {
        private static readonly HttpClient httpClient;
        private readonly int _idPhieuTra;

        static PhieuTraPreviewWindow()
        {
            httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                httpClient.BaseAddress = new Uri(apiUrl);
            }
            else
            {
                httpClient.BaseAddress = new Uri("http://127.0.0.1:5166"); // Fallback an toàn
            }
        }

        public PhieuTraPreviewWindow(int idPhieuTra)
        {
            InitializeComponent();
            _idPhieuTra = idPhieuTra;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = await httpClient.GetFromJsonAsync<PhieuTraPrintDto>($"api/app/nhanvien/thuesach/print-data/tra/{_idPhieuTra}");
                if (data == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu phiếu trả.");
                    this.Close();
                    return;
                }

                // Đổ dữ liệu vào UI
                lblTenQuan.Text = data.TenQuan;
                lblDiaChiQuan.Text = data.DiaChiQuan;
                lblSdtQuan.Text = $"SĐT: {data.SdtQuan}";
                lblMaPhieu.Text = $"Mã: {data.IdPhieuTra}";
                lblNgayTao.Text = $"Ngày: {data.NgayTra:dd/MM/yyyy HH:mm}";

                lblTenKhach.Text = data.TenKhachHang;
                lblSdtKhach.Text = $"SĐT: {data.SdtKhachHang}";
                lblTenNhanVien.Text = data.TenNhanVien;
                lblPhieuThueGoc.Text = data.IdPhieuThue;

                dgChiTiet.ItemsSource = data.ChiTiet;

                lblTongCoc.Text = $"{data.TongTienCoc:N0} đ";
                lblPhiThue.Text = $"- {data.TongPhiThue:N0} đ";
                lblTongPhat.Text = $"- {data.TongTienPhat:N0} đ";
                lblTongHoanTra.Text = $"{data.TongHoanTra:N0} đ";
                lblDiemTichLuy.Text = $"+ {data.DiemTichLuy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu in: {ex.Message}", "Lỗi API");
                this.Close();
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // 1. Tạm ẩn nút In
                    if (sender is Button btnPrint)
                        btnPrint.Visibility = Visibility.Collapsed;

                    // 2. Tìm và ẩn nút Đóng (Tên chính xác trong XAML là "btnClose" với chữ b thường)
                    if (FindName("btnClose") is Button btnClose)
                        btnClose.Visibility = Visibility.Collapsed;

                    // 3. Thực hiện in vùng phiếu
                    if (FindName("printArea") is UIElement area)
                        printDialog.PrintVisual(area, "In Phiếu Trả Sách");

                    // 4. Hiện lại nút In
                    if (sender is Button btnPrintRe)
                        btnPrintRe.Visibility = Visibility.Visible;

                    // 5. Hiện lại nút Đóng
                    if (FindName("btnClose") is Button btnCloseRe)
                        btnCloseRe.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}