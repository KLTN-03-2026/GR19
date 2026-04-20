// File: AppCafebookApi/View/quanly/pages/QuanLyBaoCaoTonKhoSachView.xaml.cs
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
    public partial class QuanLyBaoCaoTonKhoSachView : Page
    {
        private static readonly HttpClient httpClient;
        private QuanLyBaoCaoSachTongHopDto? currentReportData;

        static QuanLyBaoCaoTonKhoSachView()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost"),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public QuanLyBaoCaoTonKhoSachView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Bảo mật Lớp 2
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL") && !AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_SACH"))
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
            bool canExport = AuthService.CoQuyen("FULL_QL") || AuthService.CoQuyen("QL_BAO_CAO_TON_KHO_SACH");
            if (FindName("btnExportToExcel") is Button btnEx) btnEx.Visibility = canExport ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<QuanLyBaoCaoTonKhoSach_FiltersDto>("api/app/quanly/baocaotonkhosach/filters");
                if (response != null)
                {
                    if (FindName("cmbTheLoai") is ComboBox cmbTL)
                    {
                        var tlList = response.TheLoais ?? new List<QuanLyFilterLookupDto>();
                        tlList.Insert(0, new QuanLyFilterLookupDto { Id = 0, Ten = "-- Tất cả Thể loại --" });
                        cmbTL.ItemsSource = tlList;
                        cmbTL.SelectedIndex = 0;
                    }

                    if (FindName("cmbTacGia") is ComboBox cmbTG)
                    {
                        var tgList = response.TacGias ?? new List<QuanLyFilterLookupDto>();
                        tgList.Insert(0, new QuanLyFilterLookupDto { Id = 0, Ten = "-- Tất cả Tác giả --" });
                        cmbTG.ItemsSource = tgList;
                        cmbTG.SelectedIndex = 0;
                    }
                }
            }
            catch { /* Ignore silently */ }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Border? loading = FindName("LoadingOverlay") as System.Windows.Controls.Border;
            if (loading != null) loading.Visibility = Visibility.Visible;

            int? selectedTheLoai = (FindName("cmbTheLoai") as ComboBox)?.SelectedValue as int?;
            if (selectedTheLoai == 0) selectedTheLoai = null;

            int? selectedTacGia = (FindName("cmbTacGia") as ComboBox)?.SelectedValue as int?;
            if (selectedTacGia == 0) selectedTacGia = null;

            var request = new QuanLyBaoCaoSachRequestDto
            {
                SearchText = (FindName("txtSearch") as TextBox)?.Text,
                TheLoaiId = selectedTheLoai,
                TacGiaId = selectedTacGia
            };

            try
            {
                var response = await httpClient.PostAsJsonAsync("api/app/quanly/baocaotonkhosach/report", request);
                if (response.IsSuccessStatusCode)
                {
                    currentReportData = await response.Content.ReadFromJsonAsync<QuanLyBaoCaoSachTongHopDto>();
                    if (currentReportData != null) PopulateUi(currentReportData);
                }
                else MessageBox.Show("Lỗi lấy dữ liệu từ Server.", "Lỗi");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi mạng: " + ex.Message, "Lỗi"); }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private void PopulateUi(QuanLyBaoCaoSachTongHopDto data)
        {
            if (FindName("lblTongDauSach") is TextBlock t1) t1.Text = data.Kpi.TongDauSach.ToString("N0");
            if (FindName("lblTongSoLuong") is TextBlock t2) t2.Text = data.Kpi.TongSoLuong.ToString("N0");
            if (FindName("lblDangChoThue") is TextBlock t3) t3.Text = data.Kpi.DangChoThue.ToString("N0");
            if (FindName("lblSanSang") is TextBlock t4) t4.Text = data.Kpi.SanSang.ToString("N0");

            if (FindName("dgInventoryDetails") is DataGrid d1) d1.ItemsSource = data.ChiTietTonKho;
            if (FindName("dgRentedOverdue") is DataGrid d2) d2.ItemsSource = data.SachTreHan;
            if (FindName("dgTopRented") is DataGrid d3) d3.ItemsSource = data.TopSachThue;
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
                Title = "Lưu Báo Cáo Tồn Kho Sách",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"BaoCaoTonKhoSach_{DateTime.Now:ddMMyyyy_HHmm}.xlsx"
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
                        var numberFormat = "#,##0";
                        var dateFormat = "dd/MM/yyyy HH:mm";

                        // ==========================================
                        // SHEET 1: TỔNG QUAN KPI
                        // ==========================================
                        var wsOverview = package.Workbook.Worksheets.Add("Tổng Quan Kho");
                        wsOverview.Cells["A1:B1"].Merge = true;
                        wsOverview.Cells["A1"].Value = "BÁO CÁO TỔNG QUAN TỒN KHO SÁCH";
                        wsOverview.Cells["A1"].Style.Font.Size = 16;
                        wsOverview.Cells["A1"].Style.Font.Bold = true;
                        wsOverview.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        wsOverview.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsOverview.Row(1).Height = 30;

                        wsOverview.Cells["A2:B2"].Merge = true;
                        wsOverview.Cells["A2"].Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        wsOverview.Cells["A2"].Style.Font.Italic = true;
                        wsOverview.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        wsOverview.Cells["A4"].Value = "1. Tổng Đầu Sách"; wsOverview.Cells["B4"].Value = currentReportData.Kpi.TongDauSach;
                        wsOverview.Cells["A5"].Value = "2. Tổng Số Lượng Sách (Cuốn)"; wsOverview.Cells["B5"].Value = currentReportData.Kpi.TongSoLuong;
                        wsOverview.Cells["A6"].Value = "3. Đang Cho Thuê"; wsOverview.Cells["B6"].Value = currentReportData.Kpi.DangChoThue;
                        wsOverview.Cells["A7"].Value = "4. Sẵn Sàng Cho Thuê"; wsOverview.Cells["B7"].Value = currentReportData.Kpi.SanSang;

                        wsOverview.Cells["B4:B7"].Style.Numberformat.Format = numberFormat;
                        wsOverview.Cells["B4:B7"].Style.Font.Bold = true;
                        wsOverview.Cells["B6"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        wsOverview.Cells["B7"].Style.Font.Color.SetColor(System.Drawing.Color.Green);

                        wsOverview.Column(1).Width = 30;
                        wsOverview.Column(2).Width = 20;

                        // ==========================================
                        // SHEET 2: CHI TIẾT TỒN KHO
                        // ==========================================
                        if (currentReportData.ChiTietTonKho.Any())
                        {
                            var ws1 = package.Workbook.Worksheets.Add("Chi Tiết Tồn Kho");
                            ws1.Cells["A1:F1"].Merge = true;
                            ws1.Cells["A1"].Value = "CHI TIẾT TỒN KHO VÀ MƯỢN SÁCH";
                            ws1.Cells["A1"].Style.Font.Size = 14;
                            ws1.Cells["A1"].Style.Font.Bold = true;
                            ws1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws1.Row(1).Height = 25;

                            ws1.Cells["A3"].LoadFromCollection(currentReportData.ChiTietTonKho, true, TableStyles.Medium9);

                            ws1.Column(4).Style.Numberformat.Format = numberFormat;
                            ws1.Column(5).Style.Numberformat.Format = numberFormat;
                            ws1.Column(6).Style.Numberformat.Format = numberFormat;

                            // Tô màu đỏ những sách đã HẾT HÀNG (Còn Lại = 0)
                            int r1 = 4;
                            foreach (var item in currentReportData.ChiTietTonKho)
                            {
                                if (item.SoLuongConLai <= 0)
                                {
                                    ws1.Cells[$"F{r1}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                    ws1.Cells[$"F{r1}"].Style.Font.Bold = true;
                                }
                                r1++;
                            }
                            ws1.Cells[ws1.Dimension.Address].AutoFitColumns();
                        }

                        // ==========================================
                        // SHEET 3: SÁCH TRỄ HẠN
                        // ==========================================
                        if (currentReportData.SachTreHan.Any())
                        {
                            var ws2 = package.Workbook.Worksheets.Add("Sách Đang Thuê & Trễ Hạn");
                            ws2.Cells["A1:F1"].Merge = true;
                            ws2.Cells["A1"].Value = "DANH SÁCH MƯỢN VÀ TRỄ HẠN TRẢ SÁCH";
                            ws2.Cells["A1"].Style.Font.Size = 14;
                            ws2.Cells["A1"].Style.Font.Bold = true;
                            ws2.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws2.Row(1).Height = 25;

                            ws2.Cells["A3"].LoadFromCollection(currentReportData.SachTreHan, true, TableStyles.Medium10);

                            ws2.Column(4).Style.Numberformat.Format = dateFormat;
                            ws2.Column(5).Style.Numberformat.Format = dateFormat;

                            // Tô màu đỏ những mục chữ bắt đầu bằng "Trễ"
                            int r2 = 4;
                            foreach (var item in currentReportData.SachTreHan)
                            {
                                if (item.TinhTrang.StartsWith("Trễ"))
                                {
                                    ws2.Cells[$"F{r2}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                    ws2.Cells[$"F{r2}"].Style.Font.Bold = true;
                                }
                                else
                                {
                                    ws2.Cells[$"F{r2}"].Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
                                }
                                r2++;
                            }
                            ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
                        }

                        // ==========================================
                        // SHEET 4: TOP SÁCH THUÊ
                        // ==========================================
                        if (currentReportData.TopSachThue.Any())
                        {
                            var ws3 = package.Workbook.Worksheets.Add("Top Sách Thuê");
                            ws3.Cells["A1:C1"].Merge = true;
                            ws3.Cells["A1"].Value = "TOP SÁCH ĐƯỢC THUÊ NHIỀU NHẤT";
                            ws3.Cells["A1"].Style.Font.Size = 14;
                            ws3.Cells["A1"].Style.Font.Bold = true;
                            ws3.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws3.Row(1).Height = 25;

                            ws3.Cells["A3"].LoadFromCollection(currentReportData.TopSachThue, true, TableStyles.Medium11);
                            ws3.Column(3).Style.Numberformat.Format = numberFormat;
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
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    else if (result == MessageBoxResult.No)
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                }
                catch (IOException)
                {
                    MessageBox.Show("Tệp Excel này đang được mở bởi một chương trình khác (ví dụ: Microsoft Excel).\n\nVui lòng đóng tệp đó lại trước khi bấm xuất báo cáo!", "File Đang Mở", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra khi tạo file Excel:\n\n" + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}