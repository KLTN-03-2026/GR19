using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using AppCafebookApi.Services;

namespace AppCafebookApi.View.Common
{
    public partial class PhieuGoiMonPreviewWindow : Window
    {
        private readonly int _idHoaDon;
        private static readonly HttpClient _httpClient;

        // ======================================================
        // NÂNG CẤP 1: DYNAMIC URL (Tuyệt đối không hardcode)
        // ======================================================
        static PhieuGoiMonPreviewWindow()
        {
            _httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                _httpClient.BaseAddress = new Uri(apiUrl);
            }
        }

        public PhieuGoiMonPreviewWindow(int idHoaDon)
        {
            InitializeComponent();
            _idHoaDon = idHoaDon;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Chặn crash nếu chưa có API
            if (_httpClient.BaseAddress == null)
            {
                MessageBox.Show("Hệ thống chưa được cấu hình URL Server.", "Thiếu cấu hình");
                this.Close();
                return;
            }

            // Gắn Token
            if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            try
            {
                var data = await _httpClient.GetFromJsonAsync<PhieuGoiMonPrintDto>($"api/app/nhanvien/goimon/print-data/{_idHoaDon}");
                if (data != null)
                {
                    lblTenQuan.Text = data.TenQuan;
                    lblDiaChiQuan.Text = data.DiaChiQuan;
                    lblSdtQuan.Text = $"SĐT: {data.SdtQuan}";
                    lblIdPhieu.Text = $"Mã: {data.IdPhieu}";
                    lblNgayTao.Text = data.NgayTao.ToString("dd/MM/yyyy HH:mm");
                    lblSoBan.Text = data.SoBan;
                    lblTenNhanVien.Text = data.TenNhanVien;

                    dgChiTiet.ItemsSource = data.ChiTiet;

                    lblTongTienGoc.Text = data.TongTienGoc.ToString("N0") + " đ";
                    lblGiamGia.Text = data.GiamGia.ToString("N0") + " đ";
                    lblThanhTien.Text = data.ThanhTien.ToString("N0") + " đ";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu in: {ex.Message}", "Lỗi API");
                this.Close();
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    btnPrint.Visibility = Visibility.Collapsed;
                    printDialog.PrintVisual(PrintArea, "Phiếu Gọi Món Cafebook");
                    btnPrint.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi In");
            }
            finally
            {
                btnPrint.Visibility = Visibility.Visible;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}