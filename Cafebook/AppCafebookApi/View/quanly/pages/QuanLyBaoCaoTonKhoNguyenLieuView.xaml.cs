// File: AppCafebookApi/View/quanly/pages/QuanLyBaoCaoTonKhoNguyenLieuView.xaml.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using CafebookModel.Model.ModelApp.QuanLy;
using AppCafebookApi.Services;
using CafebookModel.Utils;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyBaoCaoTonKhoNguyenLieuView : Page
    {
        private static readonly HttpClient httpClient;
        private QuanLyBaoCaoTonKhoNguyenLieuTongHopDto? currentReportData;

        static QuanLyBaoCaoTonKhoNguyenLieuView()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost"),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public QuanLyBaoCaoTonKhoNguyenLieuView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Bảo mật Lớp 2
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL") && !AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_NL"))
            {
                MessageBox.Show("Bạn không có quyền truy cập!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();
            await LoadFiltersAsync();
        }

        private void ApplyPermissions()
        {
            bool canExport = AuthService.CoQuyen("FULL_QL") || AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_NL");
            if (FindName("btnExportToExcel") is Button btnEx) btnEx.Visibility = canExport ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                // [FIX LỖI]: Gọi thẳng DTO, tự động map dữ liệu chính xác 100%
                var response = await httpClient.GetFromJsonAsync<QuanLyBaoCaoTonKho_FiltersDto>("api/app/quanly/baocaotonkhonguyenlieu/filters");
                if (response != null && response.NhaCungCaps != null)
                {
                    if (FindName("cmbNhaCungCap") is ComboBox cmb)
                    {
                        var nccList = response.NhaCungCaps;
                        nccList.Insert(0, new QuanLyFilterLookupDto { Id = 0, Ten = "-- Tất cả Nhà Cung Cấp --" });
                        cmb.ItemsSource = nccList;
                        cmb.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // In log ra để biết nếu đứt mạng
                Console.WriteLine("Lỗi load filter: " + ex.Message);
            }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Border? loading = FindName("LoadingOverlay") as System.Windows.Controls.Border;
            if (loading != null) loading.Visibility = Visibility.Visible;

            int? selectedNcc = (FindName("cmbNhaCungCap") as ComboBox)?.SelectedValue as int?;
            if (selectedNcc == 0) selectedNcc = null;

            var request = new QuanLyBaoCaoTonKhoNguyenLieuRequestDto
            {
                SearchText = (FindName("txtSearch") as TextBox)?.Text,
                NhaCungCapId = selectedNcc,
                ShowLowStockOnly = (FindName("chkLowStock") as CheckBox)?.IsChecked ?? false
            };

            try
            {
                var response = await httpClient.PostAsJsonAsync("api/app/quanly/baocaotonkhonguyenlieu/report", request);
                if (response.IsSuccessStatusCode)
                {
                    currentReportData = await response.Content.ReadFromJsonAsync<QuanLyBaoCaoTonKhoNguyenLieuTongHopDto>();
                    if (currentReportData != null) PopulateUi(currentReportData);
                }
                else MessageBox.Show("Lỗi lấy dữ liệu từ Server.", "Lỗi");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi mạng: " + ex.Message, "Lỗi"); }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private void PopulateUi(QuanLyBaoCaoTonKhoNguyenLieuTongHopDto data)
        {
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            if (FindName("lblTongGiaTriTonKho") is TextBlock t1) t1.Text = data.Kpi.TongGiaTriTonKho.ToString("C0", culture);
            if (FindName("lblSoLuongSPSapHet") is TextBlock t2) t2.Text = data.Kpi.SoLuongSPSapHet.ToString("N0");
            if (FindName("lblTongGiaTriDaHuy") is TextBlock t3) t3.Text = data.Kpi.TongGiaTriDaHuy.ToString("C0", culture);

            if (FindName("dgChiTietTonKho") is DataGrid d1) d1.ItemsSource = data.ChiTietTonKho;
            if (FindName("dgLichSuKiemKe") is DataGrid d2) d2.ItemsSource = data.LichSuKiemKe;
            if (FindName("dgLichSuHuyHang") is DataGrid d3) d3.ItemsSource = data.LichSuHuyHang;
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();

        private void BtnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (currentReportData == null)
            {
                MessageBox.Show("Chưa có dữ liệu để xuất! Vui lòng tạo báo cáo trước.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Báo Cáo Tồn Kho",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"BaoCaoTonKho_{DateTime.Now:ddMMyyyy_HHmm}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Cafebook Admin");
                    FileInfo fileInfo = new FileInfo(sfd.FileName);
                    if (fileInfo.Exists) fileInfo.Delete();

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        var currencyFormat = "#,##0 \"đ\"";
                        var decimalFormat = "#,##0.00";
                        var numberFormat = "#,##0";
                        var dateFormat = "dd/MM/yyyy HH:mm";

                        // ==========================================
                        // SHEET 1: TỔNG QUAN KPI
                        // ==========================================
                        var wsOverview = package.Workbook.Worksheets.Add("Tổng Quan KPI");
                        wsOverview.Cells["A1:B1"].Merge = true;
                        wsOverview.Cells["A1"].Value = "BÁO CÁO TỔNG QUAN KHO NGUYÊN LIỆU";
                        wsOverview.Cells["A1"].Style.Font.Size = 16;
                        wsOverview.Cells["A1"].Style.Font.Bold = true;
                        wsOverview.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        wsOverview.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsOverview.Row(1).Height = 30;

                        wsOverview.Cells["A2:B2"].Merge = true;
                        wsOverview.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        wsOverview.Cells["A2"].Style.Font.Italic = true;
                        wsOverview.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        wsOverview.Cells["A4"].Value = "1. Tổng Giá Trị Tồn Kho Ước Tính"; wsOverview.Cells["B4"].Value = currentReportData.Kpi.TongGiaTriTonKho;
                        wsOverview.Cells["A5"].Value = "2. Số Lượng NL Sắp Hết"; wsOverview.Cells["B5"].Value = currentReportData.Kpi.SoLuongSPSapHet;
                        wsOverview.Cells["A6"].Value = "3. Tổng Giá Trị Đã Hủy"; wsOverview.Cells["B6"].Value = currentReportData.Kpi.TongGiaTriDaHuy;

                        wsOverview.Cells["B4"].Style.Numberformat.Format = currencyFormat;
                        wsOverview.Cells["B4"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        wsOverview.Cells["B4"].Style.Font.Bold = true;

                        wsOverview.Cells["B5"].Style.Numberformat.Format = numberFormat;
                        wsOverview.Cells["B5"].Style.Font.Color.SetColor(System.Drawing.Color.DarkOrange);
                        wsOverview.Cells["B5"].Style.Font.Bold = true;

                        wsOverview.Cells["B6"].Style.Numberformat.Format = currencyFormat;
                        wsOverview.Cells["B6"].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                        wsOverview.Column(1).Width = 35;
                        wsOverview.Column(2).Width = 20;

                        // ==========================================
                        // SHEET 2: CHI TIẾT TỒN KHO
                        // ==========================================
                        if (currentReportData.ChiTietTonKho.Any())
                        {
                            var ws1 = package.Workbook.Worksheets.Add("Chi Tiết Tồn Kho");
                            ws1.Cells["A1:E1"].Merge = true;
                            ws1.Cells["A1"].Value = "CHI TIẾT TỒN KHO NGUYÊN LIỆU";
                            ws1.Cells["A1"].Style.Font.Size = 14;
                            ws1.Cells["A1"].Style.Font.Bold = true;
                            ws1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws1.Row(1).Height = 25;

                            ws1.Cells["A3"].LoadFromCollection(currentReportData.ChiTietTonKho, true, TableStyles.Medium9);

                            ws1.Column(3).Style.Numberformat.Format = decimalFormat;
                            ws1.Column(4).Style.Numberformat.Format = decimalFormat;

                            // [TÍNH NĂNG XỊN XÒ]: Tự động tô đỏ chữ "Sắp hết"
                            int r1 = 4;
                            foreach (var item in currentReportData.ChiTietTonKho)
                            {
                                if (item.TinhTrang == "Sắp hết")
                                {
                                    ws1.Cells[$"E{r1}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                    ws1.Cells[$"E{r1}"].Style.Font.Bold = true;
                                }
                                else
                                {
                                    ws1.Cells[$"E{r1}"].Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
                                }
                                r1++;
                            }

                            ws1.Cells[ws1.Dimension.Address].AutoFitColumns();
                        }

                        // ==========================================
                        // SHEET 3: LỊCH SỬ KIỂM KÊ
                        // ==========================================
                        if (currentReportData.LichSuKiemKe.Any())
                        {
                            var ws2 = package.Workbook.Worksheets.Add("Lịch Sử Kiểm Kê");
                            ws2.Cells["A1:F1"].Merge = true;
                            ws2.Cells["A1"].Value = "LỊCH SỬ KIỂM KÊ (CÁC MỤC CÓ CHÊNH LỆCH)";
                            ws2.Cells["A1"].Style.Font.Size = 14;
                            ws2.Cells["A1"].Style.Font.Bold = true;
                            ws2.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws2.Row(1).Height = 25;

                            ws2.Cells["A3"].LoadFromCollection(currentReportData.LichSuKiemKe, true, TableStyles.Medium10);

                            ws2.Column(1).Style.Numberformat.Format = dateFormat;
                            ws2.Column(3).Style.Numberformat.Format = decimalFormat;
                            ws2.Column(4).Style.Numberformat.Format = decimalFormat;
                            ws2.Column(5).Style.Numberformat.Format = decimalFormat;

                            // Tô màu chênh lệch: Âm là đỏ, Dương là xanh
                            int r2 = 4;
                            foreach (var item in currentReportData.LichSuKiemKe)
                            {
                                if (item.ChenhLech < 0) ws2.Cells[$"E{r2}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                else if (item.ChenhLech > 0) ws2.Cells[$"E{r2}"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                                ws2.Cells[$"E{r2}"].Style.Font.Bold = true;
                                r2++;
                            }

                            ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
                        }

                        // ==========================================
                        // SHEET 4: LỊCH SỬ HỦY HÀNG
                        // ==========================================
                        if (currentReportData.LichSuHuyHang.Any())
                        {
                            var ws3 = package.Workbook.Worksheets.Add("Lịch Sử Hủy Hàng");
                            ws3.Cells["A1:E1"].Merge = true;
                            ws3.Cells["A1"].Value = "LỊCH SỬ XUẤT HỦY HÀNG HÓA";
                            ws3.Cells["A1"].Style.Font.Size = 14;
                            ws3.Cells["A1"].Style.Font.Bold = true;
                            ws3.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws3.Row(1).Height = 25;

                            ws3.Cells["A3"].LoadFromCollection(currentReportData.LichSuHuyHang, true, TableStyles.Medium11);

                            ws3.Column(1).Style.Numberformat.Format = dateFormat;
                            ws3.Column(3).Style.Numberformat.Format = decimalFormat;
                            ws3.Column(4).Style.Numberformat.Format = currencyFormat;

                            // Tô đỏ cột Giá trị Hủy
                            var tbl = ws3.Tables[0];
                            ws3.Cells[tbl.Address.Start.Row + 1, 4, tbl.Address.End.Row, 4].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            ws3.Cells[tbl.Address.Start.Row + 1, 4, tbl.Address.End.Row, 4].Style.Font.Bold = true;

                            ws3.Cells[ws3.Dimension.Address].AutoFitColumns();
                        }

                        // --- LƯU FILE ---
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
                    MessageBox.Show("Tệp Excel này đang được mở bởi một chương trình khác (ví dụ: Microsoft Excel).\n\nVui lòng đóng tệp đó lại trước khi bấm xuất báo cáo!",
                                    "File Đang Mở", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra khi tạo file Excel:\n\n" + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}