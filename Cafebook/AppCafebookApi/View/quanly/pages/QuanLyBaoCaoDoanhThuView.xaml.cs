// File: AppCafebookApi/View/quanly/pages/QuanLyBaoCaoDoanhThuView.xaml.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls; // System.Windows.Controls.Border nằm ở đây
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using CafebookModel.Model.ModelApp.QuanLy;
using AppCafebookApi.Services; // Thêm để dùng AuthService

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyBaoCaoDoanhThuView : Page
    {
       // private static readonly HttpClient httpClient;
        private QuanLyBaoCaoTongHopDto? currentReportData;
        /*
        static QuanLyBaoCaoDoanhThuView()
        {
            ApiClient.Instance = new ApiClient.Instance
            {
                BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost"),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }
        */
        public QuanLyBaoCaoDoanhThuView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL") && !AuthService.CoQuyen("QL_BAO_CAO_DOANH_THU"))
            {
                MessageBox.Show("Từ chối truy cập!");
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            var now = DateTime.Now;
            if (FindName("dpStartDate") is DatePicker dpStart) dpStart.SelectedDate = new DateTime(now.Year, now.Month, 1);
            if (FindName("dpEndDate") is DatePicker dpEnd) dpEnd.SelectedDate = now;
        }

        private void ApplyPermissions()
        {
            bool canUseFull = AuthService.CoQuyen("FULL_QL") || AuthService.CoQuyen("QL_BAO_CAO_DOANH_THU");

            if (FindName("btnGenerate") is Button b1) b1.Visibility = canUseFull ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnExportToExcel") is Button b2) b2.Visibility = canUseFull ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DatePicker? dpStart = FindName("dpStartDate") as DatePicker;
            DatePicker? dpEnd = FindName("dpEndDate") as DatePicker;
            Button? btnGen = FindName("btnGenerate") as Button;

            System.Windows.Controls.Border? loading = FindName("LoadingOverlay") as System.Windows.Controls.Border;

            if (dpStart?.SelectedDate == null || dpEnd?.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày.", "Thiếu thông tin");
                return;
            }

            if (loading != null) loading.Visibility = Visibility.Visible;
            if (btnGen != null) btnGen.IsEnabled = false;

            var request = new QuanLyBaoCaoDoanhThuRequestDto
            {
                StartDate = dpStart.SelectedDate.Value,
                EndDate = dpEnd.SelectedDate.Value
            };

            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly/baocaodoanhthu/xem-bao-cao", request);
                if (response.IsSuccessStatusCode)
                {
                    currentReportData = await response.Content.ReadFromJsonAsync<QuanLyBaoCaoTongHopDto>();
                    if (currentReportData != null)
                    {
                        PopulateUi(currentReportData);
                    }
                }
                else
                {
                    MessageBox.Show($"Lỗi hệ thống: {response.ReasonPhrase}", "Lỗi API");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mất kết nối máy chủ: {ex.Message}", "Lỗi Mạng");
            }
            finally
            {
                if (loading != null) loading.Visibility = Visibility.Collapsed;
                if (btnGen != null) btnGen.IsEnabled = true;
            }
        }

        private void PopulateUi(QuanLyBaoCaoTongHopDto data)
        {
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            if (FindName("lblDoanhThuRong") is TextBlock txtDTR) txtDTR.Text = data.Kpi.DoanhThuRong.ToString("C0", culture);
            if (FindName("lblTongGiaVon") is TextBlock txtGV) txtGV.Text = data.Kpi.TongGiaVon.ToString("C0", culture);
            if (FindName("lblLoiNhuanGop") is TextBlock txtLNG) txtLNG.Text = data.Kpi.LoiNhuanGop.ToString("C0", culture);
            if (FindName("lblChiPhiOpex") is TextBlock txtOpex) txtOpex.Text = data.Kpi.ChiPhiOpex.ToString("C0", culture);
            if (FindName("lblLoiNhuanRong") is TextBlock txtLNR) txtLNR.Text = data.Kpi.LoiNhuanRong.ToString("C0", culture);
            if (FindName("dgGoiYDoanhThu") is DataGrid dgGoiY) dgGoiY.ItemsSource = data.GoiYDoanhThu;

            if (FindName("dgTopSanPham") is DataGrid dgTop) dgTop.ItemsSource = data.TopSanPham;

            var dtList = new List<KeyValuePair<string, decimal>>
            {
                new("Doanh thu bán hàng", data.ChiTietDoanhThu.TongDoanhThuBanHang),
                new("Doanh thu phí thuê sách", data.ChiTietDoanhThu.TongDoanhThuThueSach),
                new("Tổng giảm giá (-)", data.ChiTietDoanhThu.TongGiamGia),
                new("Tổng phụ thu (+)", data.ChiTietDoanhThu.TongPhuThu),
                new("DOANH THU RÒNG TỔNG", data.ChiTietDoanhThu.DoanhThuRong)
            };

            if (FindName("dgChiTietDoanhThu") is DataGrid dgDT) dgDT.ItemsSource = dtList;
        }

        private void BtnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (currentReportData == null)
            {
                MessageBox.Show("Chưa có dữ liệu để xuất! Vui lòng bấm 'Tạo Báo Cáo' trước.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DatePicker? dpStart = FindName("dpStartDate") as DatePicker;
            DatePicker? dpEnd = FindName("dpEndDate") as DatePicker;
            DateTime startDate = dpStart?.SelectedDate ?? DateTime.Now;
            DateTime endDate = dpEnd?.SelectedDate ?? DateTime.Now;

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Báo Cáo Doanh Thu",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"BaoCaoDoanhThu_{startDate:ddMMyyyy}_{endDate:ddMMyyyy}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Cafebook Admin");

                    FileInfo fileInfo = new FileInfo(sfd.FileName);

                    // Nếu file đang tồn tại, thử xóa nó. 
                    // (Nếu file đang mở, nó sẽ văng lỗi IOException ngay tại đây)
                    if (fileInfo.Exists) fileInfo.Delete();

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        var currencyFormat = "#,##0 \"đ\"";

                        // ==========================================
                        // SHEET 1: TỔNG QUAN KẾT QUẢ KINH DOANH
                        // ==========================================
                        var wsOverview = package.Workbook.Worksheets.Add("Tổng Quan Doanh Thu");

                        wsOverview.Cells["A1:E1"].Merge = true;
                        wsOverview.Cells["A1"].Value = "BÁO CÁO KẾT QUẢ HOẠT ĐỘNG KINH DOANH";
                        wsOverview.Cells["A1"].Style.Font.Size = 16;
                        wsOverview.Cells["A1"].Style.Font.Bold = true;
                        wsOverview.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        wsOverview.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsOverview.Row(1).Height = 30;

                        wsOverview.Cells["A2:E2"].Merge = true;
                        wsOverview.Cells["A2"].Value = $"Thời gian: Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}";
                        wsOverview.Cells["A2"].Style.Font.Italic = true;
                        wsOverview.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // --- SECTION 1: CHỈ SỐ KPI CHÍNH ---
                        wsOverview.Cells["A4:B4"].Merge = true;
                        wsOverview.Cells["A4"].Value = "I. CHỈ SỐ KPI CHÍNH";
                        wsOverview.Cells["A4"].Style.Font.Bold = true;
                        wsOverview.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        wsOverview.Cells["A4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        wsOverview.Cells["A5"].Value = "Doanh Thu Ròng"; wsOverview.Cells["B5"].Value = currentReportData.Kpi.DoanhThuRong;
                        wsOverview.Cells["A6"].Value = "Tổng Giá Vốn (COGS)"; wsOverview.Cells["B6"].Value = currentReportData.Kpi.TongGiaVon;
                        wsOverview.Cells["A7"].Value = "Lợi Nhuận Gộp"; wsOverview.Cells["B7"].Value = currentReportData.Kpi.LoiNhuanGop;
                        wsOverview.Cells["A8"].Value = "Chi Phí Vận Hành (OPEX)"; wsOverview.Cells["B8"].Value = currentReportData.Kpi.ChiPhiOpex;
                        wsOverview.Cells["A9"].Value = "LỢI NHUẬN RÒNG"; wsOverview.Cells["B9"].Value = currentReportData.Kpi.LoiNhuanRong;

                        wsOverview.Cells["A9:B9"].Style.Font.Bold = true;
                        wsOverview.Cells["B9"].Style.Font.Color.SetColor(currentReportData.Kpi.LoiNhuanRong >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red);
                        wsOverview.Cells["B5:B9"].Style.Numberformat.Format = currencyFormat;

                        // --- SECTION 2: CHI TIẾT DOANH THU ---
                        wsOverview.Cells["D4:E4"].Merge = true;
                        wsOverview.Cells["D4"].Value = "II. CHI TIẾT DOANH THU";
                        wsOverview.Cells["D4"].Style.Font.Bold = true;
                        wsOverview.Cells["D4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        wsOverview.Cells["D4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        wsOverview.Cells["D5"].Value = "1. Doanh thu bán hàng"; wsOverview.Cells["E5"].Value = currentReportData.ChiTietDoanhThu.TongDoanhThuBanHang;
                        wsOverview.Cells["D6"].Value = "2. Doanh thu phí thuê sách"; wsOverview.Cells["E6"].Value = currentReportData.ChiTietDoanhThu.TongDoanhThuThueSach;
                        wsOverview.Cells["D7"].Value = "3. Tổng giảm giá (-)"; wsOverview.Cells["E7"].Value = currentReportData.ChiTietDoanhThu.TongGiamGia;
                        wsOverview.Cells["D8"].Value = "4. Tổng phụ thu (+)"; wsOverview.Cells["E8"].Value = currentReportData.ChiTietDoanhThu.TongPhuThu;
                        wsOverview.Cells["D9"].Value = "TỔNG DOANH THU RÒNG"; wsOverview.Cells["E9"].Value = currentReportData.ChiTietDoanhThu.DoanhThuRong;

                        wsOverview.Cells["D9:E9"].Style.Font.Bold = true;
                        wsOverview.Cells["E5:E9"].Style.Numberformat.Format = currencyFormat;

                        wsOverview.Cells["D10"].Value = "Tổng số lượng hóa đơn"; wsOverview.Cells["E10"].Value = currentReportData.ChiTietDoanhThu.SoLuongHoaDon;
                        wsOverview.Cells["D11"].Value = "Giá trị trung bình/Hóa đơn"; wsOverview.Cells["E11"].Value = currentReportData.ChiTietDoanhThu.GiaTriTrungBinhHD;
                        wsOverview.Cells["E10"].Style.Numberformat.Format = "#,##0";
                        wsOverview.Cells["E11"].Style.Numberformat.Format = currencyFormat;

                        // --- SECTION 3: CHI TIẾT CHI PHÍ ---
                        wsOverview.Cells["A12:B12"].Merge = true;
                        wsOverview.Cells["A12"].Value = "III. CHI TIẾT CHI PHÍ";
                        wsOverview.Cells["A12"].Style.Font.Bold = true;
                        wsOverview.Cells["A12"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        wsOverview.Cells["A12"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        wsOverview.Cells["A13"].Value = "1. Giá vốn hàng bán"; wsOverview.Cells["B13"].Value = currentReportData.ChiTietChiPhi.TongGiaVon_COGS;
                        wsOverview.Cells["A14"].Value = "2. Chi phí lương nhân viên"; wsOverview.Cells["B14"].Value = currentReportData.ChiTietChiPhi.TongChiPhiLuong;
                        wsOverview.Cells["A15"].Value = "3. Chi phí xuất hủy hàng"; wsOverview.Cells["B15"].Value = currentReportData.ChiTietChiPhi.TongChiPhiHuyHang;

                        wsOverview.Cells["B13:B15"].Style.Numberformat.Format = currencyFormat;

                        // Autofit columns for Sheet 1
                        wsOverview.Column(1).Width = 25; // A
                        wsOverview.Column(2).Width = 20; // B
                        wsOverview.Column(3).Width = 5;  // C (Khoảng trắng)
                        wsOverview.Column(4).Width = 25; // D
                        wsOverview.Column(5).Width = 20; // E


                        // ==========================================
                        // SHEET 2: TOP SẢN PHẨM BÁN CHẠY
                        // ==========================================
                        var wsTop = package.Workbook.Worksheets.Add("Top Sản Phẩm");

                        wsTop.Cells["A1:C1"].Merge = true;
                        wsTop.Cells["A1"].Value = "TOP SẢN PHẨM BÁN CHẠY NHẤT";
                        wsTop.Cells["A1"].Style.Font.Size = 14;
                        wsTop.Cells["A1"].Style.Font.Bold = true;
                        wsTop.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsTop.Row(1).Height = 25;

                        wsTop.Cells["A3"].Value = "Tên Sản Phẩm";
                        wsTop.Cells["B3"].Value = "Số Lượng Đã Bán";
                        wsTop.Cells["C3"].Value = "Tổng Doanh Thu (VNĐ)";

                        int currentRow = 4;
                        foreach (var item in currentReportData.TopSanPham)
                        {
                            wsTop.Cells[$"A{currentRow}"].Value = item.TenSanPham;

                            wsTop.Cells[$"B{currentRow}"].Value = item.TongSoLuongBan;
                            wsTop.Cells[$"B{currentRow}"].Style.Numberformat.Format = "#,##0";

                            wsTop.Cells[$"C{currentRow}"].Value = item.TongDoanhThu;
                            wsTop.Cells[$"C{currentRow}"].Style.Numberformat.Format = currencyFormat;

                            currentRow++;
                        }

                        if (currentReportData.TopSanPham.Any())
                        {
                            var tableRange = wsTop.Cells[3, 1, currentRow - 1, 3];
                            var table = wsTop.Tables.Add(tableRange, "TableTopSP");
                            table.TableStyle = TableStyles.Medium9;
                        }

                        wsTop.Cells[wsTop.Dimension.Address].AutoFitColumns();

                        // ==========================================
                        // SHEET 3: GỢI Ý GIÁ BÁN
                        // ==========================================
                        if (currentReportData.GoiYDoanhThu != null && currentReportData.GoiYDoanhThu.Any())
                        {
                            var wsGoiY = package.Workbook.Worksheets.Add("Gợi Ý Giá Bán");

                            wsGoiY.Cells["A1:E1"].Merge = true;
                            wsGoiY.Cells["A1"].Value = "BẢNG PHÂN TÍCH VÀ GỢI Ý GIÁ BÁN";
                            wsGoiY.Cells["A1"].Style.Font.Size = 14;
                            wsGoiY.Cells["A1"].Style.Font.Bold = true;
                            wsGoiY.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            wsGoiY.Row(1).Height = 25;

                            wsGoiY.Cells["A3"].Value = "Tên Sản Phẩm";
                            wsGoiY.Cells["B3"].Value = "Giá Vốn (Gốc)";
                            wsGoiY.Cells["C3"].Value = "Giá Bán Hiện Tại";
                            wsGoiY.Cells["D3"].Value = "Biên Lợi Nhuận Hiện Tại (%)";
                            wsGoiY.Cells["E3"].Value = "Giá Gợi Ý (Margin 70%)";

                            int rGoiY = 4;
                            foreach (var item in currentReportData.GoiYDoanhThu)
                            {
                                wsGoiY.Cells[$"A{rGoiY}"].Value = item.TenSanPham;

                                wsGoiY.Cells[$"B{rGoiY}"].Value = item.GiaVon;
                                wsGoiY.Cells[$"C{rGoiY}"].Value = item.GiaBanHienTai;
                                wsGoiY.Cells[$"D{rGoiY}"].Value = item.TiLeLoiNhuanCu / 100; // Đổi ra tỷ lệ để Excel format %
                                wsGoiY.Cells[$"E{rGoiY}"].Value = item.GiaGoiY;

                                // Format
                                wsGoiY.Cells[$"B{rGoiY}:C{rGoiY}"].Style.Numberformat.Format = currencyFormat;
                                wsGoiY.Cells[$"D{rGoiY}"].Style.Numberformat.Format = "0.00%";
                                wsGoiY.Cells[$"E{rGoiY}"].Style.Numberformat.Format = currencyFormat;
                                wsGoiY.Cells[$"E{rGoiY}"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                                wsGoiY.Cells[$"E{rGoiY}"].Style.Font.Bold = true;

                                rGoiY++;
                            }

                            var tableRangeGoiY = wsGoiY.Cells[3, 1, rGoiY - 1, 5];
                            var tableGoiY = wsGoiY.Tables.Add(tableRangeGoiY, "TableGoiY");
                            tableGoiY.TableStyle = TableStyles.Medium14;

                            wsGoiY.Cells[wsGoiY.Dimension.Address].AutoFitColumns();
                        }

                        // --- Lưu File ---
                        package.Save();
                    }

                    string msg = $"Đã xuất báo cáo thành công tại:\n{sfd.FileName}\n\n" +
                                 $"• Chọn [Yes] để mở trực tiếp báo cáo.\n" +
                                 $"• Chọn [No] để mở thư mục.\n" +
                                 $"• Chọn [Cancel] để đóng.";

                    var result = MessageBox.Show(msg, "Xuất Excel Thành Công", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                    }
                }
                catch (System.IO.IOException)
                {
                    // BẮT RIÊNG LỖI FILE ĐANG ĐƯỢC MỞ
                    MessageBox.Show("Tệp Excel này đang được mở bởi một chương trình khác (ví dụ: Microsoft Excel).\n\nVui lòng đóng tệp đó lại trước khi bấm xuất báo cáo!",
                                    "File Đang Mở", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    // Bắt các lỗi hệ thống khác
                    MessageBox.Show("Có lỗi xảy ra khi tạo file Excel:\n\n" + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}