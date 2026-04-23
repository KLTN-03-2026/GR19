// File: AppCafebookApi/View/quanly/pages/QuanLyBaoCaoNhanSuView.xaml.cs
using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyBaoCaoNhanSuView : Page
    {
        //private static readonly HttpClient httpClient;

        // [FIX LỖI]: Thêm biến lưu trữ dữ liệu hiện tại để xuất Excel
        private QuanLyBaoCaoNhanSuTongHopDto? currentReportData;

        public SeriesCollection LuongChartSeries { get; set; }
        public string[] ChartLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        /*
        static QuanLyBaoCaoNhanSuView()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost"),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }
        */
        public QuanLyBaoCaoNhanSuView()
        {
            InitializeComponent();

            LuongChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Tổng Lương",
                    Values = new ChartValues<decimal>(),
                    LineSmoothness = 0
                }
            };
            ChartLabels = Array.Empty<string>();
            YFormatter = value => value.ToString("N0") + " đ";

            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Bảo mật Lớp 2
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL") && !AuthService.CoQuyen("QL_BAO_CAO_NHAN_SU"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            // Bảo mật Lớp 1
            ApplyPermissions();

            var now = DateTime.Now;
            if (FindName("dpStartDate") is DatePicker dpS) dpS.SelectedDate = new DateTime(now.Year, now.Month, 1);
            if (FindName("dpEndDate") is DatePicker dpE) dpE.SelectedDate = now;

            await LoadFiltersAsync();
        }

        private void ApplyPermissions()
        {
            bool canExport = AuthService.CoQuyen("FULL_QL") || AuthService.CoQuyen("QL_BAO_CAO_NHAN_SU");
            if (FindName("btnExport") is Button btnEx) btnEx.Visibility = canExport ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<QuanLyBaoCaoNhanSu_FiltersDto>("api/app/quanly/baocaonhansu/filters");
                if (response != null)
                {
                    if (FindName("cmbVaiTro") is ComboBox cmbVT)
                    {
                        var vaiTros = response.VaiTros ?? new List<QuanLyFilterLookupDto>();
                        vaiTros.Insert(0, new QuanLyFilterLookupDto { Id = 0, Ten = "-- Tất cả Vai trò --" });
                        cmbVT.ItemsSource = vaiTros;
                        cmbVT.SelectedIndex = 0;
                    }

                    if (FindName("cmbNhanVien") is ComboBox cmbNV)
                    {
                        var nhanViens = response.NhanViens ?? new List<QuanLyFilterLookupDto>();
                        nhanViens.Insert(0, new QuanLyFilterLookupDto { Id = 0, Ten = "-- Tất cả Nhân viên --" });
                        cmbNV.ItemsSource = nhanViens;
                        cmbNV.SelectedIndex = 0;
                    }
                }
            }
            catch { /* Ignore silently */ }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DatePicker? dpStart = FindName("dpStartDate") as DatePicker;
            DatePicker? dpEnd = FindName("dpEndDate") as DatePicker;
            System.Windows.Controls.Border? loading = FindName("LoadingOverlay") as System.Windows.Controls.Border;

            if (dpStart?.SelectedDate == null || dpEnd?.SelectedDate == null) return;

            if (loading != null) loading.Visibility = Visibility.Visible;

            int? selectedVaiTro = (FindName("cmbVaiTro") as ComboBox)?.SelectedValue as int?;
            if (selectedVaiTro == 0) selectedVaiTro = null;

            int? selectedNhanVien = (FindName("cmbNhanVien") as ComboBox)?.SelectedValue as int?;
            if (selectedNhanVien == 0) selectedNhanVien = null;

            var request = new QuanLyBaoCaoNhanSuRequestDto
            {
                StartDate = dpStart.SelectedDate.Value,
                EndDate = dpEnd.SelectedDate.Value,
                VaiTroId = selectedVaiTro,
                NhanVienId = selectedNhanVien
            };

            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly/baocaonhansu/report", request);
                if (response.IsSuccessStatusCode)
                {
                    // [FIX LỖI]: Gán dữ liệu vào biến currentReportData
                    currentReportData = await response.Content.ReadFromJsonAsync<QuanLyBaoCaoNhanSuTongHopDto>();
                    if (currentReportData != null) PopulateUi(currentReportData);
                }
                else MessageBox.Show("Lỗi lấy dữ liệu từ Server.", "Lỗi");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi mạng: " + ex.Message, "Lỗi"); }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private void PopulateUi(QuanLyBaoCaoNhanSuTongHopDto data)
        {
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            // 1. KPI
            if (FindName("lblTongLuongDaTra") is TextBlock t1) t1.Text = data.Kpi.TongLuongDaTra.ToString("C0", culture);
            if (FindName("lblTongGioLam") is TextBlock t2) t2.Text = data.Kpi.TongGioLam.ToString("F2");
            if (FindName("lblTongSoNgayNghi") is TextBlock t3) t3.Text = data.Kpi.TongSoNgayNghi.ToString("N0");

            // 2. Cập nhật DataGrid
            if (FindName("dgLuong") is DataGrid d1) d1.ItemsSource = data.BangLuongChiTiet;
            if (FindName("dgNghiPhep") is DataGrid d2) d2.ItemsSource = data.ThongKeNghiPhep;

            // 3. Cập nhật Biểu đồ
            LuongChartSeries[0].Values.Clear();
            LuongChartSeries[0].Values.AddRange(data.LuongChartData.Select(d => d.TongTien).Cast<object>());
            ChartLabels = data.LuongChartData.Select(d => d.Ngay.ToString("dd/MM")).ToArray();

            // Refresh UI context cho Biểu đồ
            DataContext = null;
            DataContext = this;
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
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
                Title = "Lưu Báo Cáo Nhân Sự",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"BaoCaoNhanSu_{startDate:ddMMyyyy}_{endDate:ddMMyyyy}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Cafebook Admin");

                    FileInfo fileInfo = new FileInfo(sfd.FileName);

                    // Nếu file đang mở bởi Excel, lệnh Delete này sẽ văng lỗi IOException ngay lập tức
                    if (fileInfo.Exists) fileInfo.Delete();

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        var currencyFormat = "#,##0 \"đ\"";
                        var numberFormat = "#,##0";
                        var decimalFormat = "#,##0.00";

                        // ==========================================
                        // SHEET 1: TỔNG QUAN KPI
                        // ==========================================
                        var wsOverview = package.Workbook.Worksheets.Add("Tổng Quan KPI");

                        wsOverview.Cells["A1:D1"].Merge = true;
                        wsOverview.Cells["A1"].Value = "BÁO CÁO TỔNG QUAN NHÂN SỰ & LƯƠNG";
                        wsOverview.Cells["A1"].Style.Font.Size = 16;
                        wsOverview.Cells["A1"].Style.Font.Bold = true;
                        wsOverview.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        wsOverview.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsOverview.Row(1).Height = 30;

                        wsOverview.Cells["A2:D2"].Merge = true;
                        wsOverview.Cells["A2"].Value = $"Thời gian: Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}";
                        wsOverview.Cells["A2"].Style.Font.Italic = true;
                        wsOverview.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // --- SECTION KPI ---
                        wsOverview.Cells["A4:B4"].Merge = true;
                        wsOverview.Cells["A4"].Value = "I. CHỈ SỐ KPI CHÍNH";
                        wsOverview.Cells["A4"].Style.Font.Bold = true;
                        wsOverview.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        wsOverview.Cells["A4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        wsOverview.Cells["A5"].Value = "1. Tổng Lương Đã Thanh Toán"; wsOverview.Cells["B5"].Value = currentReportData.Kpi.TongLuongDaTra;
                        wsOverview.Cells["A6"].Value = "2. Tổng Giờ Làm"; wsOverview.Cells["B6"].Value = currentReportData.Kpi.TongGioLam;
                        wsOverview.Cells["A7"].Value = "3. Tổng Số Ngày Nghỉ"; wsOverview.Cells["B7"].Value = currentReportData.Kpi.TongSoNgayNghi;

                        // Định dạng màu sắc
                        wsOverview.Cells["B5"].Style.Numberformat.Format = currencyFormat;
                        wsOverview.Cells["B5"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        wsOverview.Cells["B5"].Style.Font.Bold = true;

                        wsOverview.Cells["B6"].Style.Numberformat.Format = decimalFormat;
                        wsOverview.Cells["B7"].Style.Numberformat.Format = numberFormat;

                        // Căn lề độ rộng
                        wsOverview.Column(1).Width = 35;
                        wsOverview.Column(2).Width = 20;

                        // ==========================================
                        // SHEET 2: BẢNG LƯƠNG CHI TIẾT
                        // ==========================================
                        if (currentReportData.BangLuongChiTiet.Any())
                        {
                            var ws1 = package.Workbook.Worksheets.Add("Bảng Lương Chi Tiết");
                            ws1.Cells["A1:H1"].Merge = true;
                            ws1.Cells["A1"].Value = "CHI TIẾT BẢNG LƯƠNG NHÂN VIÊN";
                            ws1.Cells["A1"].Style.Font.Size = 14;
                            ws1.Cells["A1"].Style.Font.Bold = true;
                            ws1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws1.Row(1).Height = 25;

                            ws1.Cells["A3"].LoadFromCollection(currentReportData.BangLuongChiTiet, true, TableStyles.Medium9);

                            // Cột 4: Lương Cơ Bản, Cột 5: Giờ Làm, Cột 6: Thưởng, Cột 7: Khấu trừ, Cột 8: Thực lãnh
                            ws1.Column(4).Style.Numberformat.Format = currencyFormat;
                            ws1.Column(5).Style.Numberformat.Format = decimalFormat;
                            ws1.Column(6).Style.Numberformat.Format = currencyFormat;
                            ws1.Column(7).Style.Numberformat.Format = currencyFormat;
                            ws1.Column(8).Style.Numberformat.Format = currencyFormat;

                            // Tô đậm cột Thực Lãnh
                            var tbl = ws1.Tables[0];
                            ws1.Cells[tbl.Address.Start.Row + 1, 8, tbl.Address.End.Row, 8].Style.Font.Bold = true;
                            ws1.Cells[tbl.Address.Start.Row + 1, 8, tbl.Address.End.Row, 8].Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);

                            ws1.Cells[ws1.Dimension.Address].AutoFitColumns();
                        }

                        // ==========================================
                        // SHEET 3: THỐNG KÊ NGHỈ PHÉP
                        // ==========================================
                        if (currentReportData.ThongKeNghiPhep.Any())
                        {
                            var ws2 = package.Workbook.Worksheets.Add("Thống Kê Nghỉ Phép");
                            ws2.Cells["A1:E1"].Merge = true;
                            ws2.Cells["A1"].Value = "THỐNG KÊ ĐƠN XIN NGHỈ & NGÀY NGHỈ";
                            ws2.Cells["A1"].Style.Font.Size = 14;
                            ws2.Cells["A1"].Style.Font.Bold = true;
                            ws2.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws2.Row(1).Height = 25;

                            ws2.Cells["A3"].LoadFromCollection(currentReportData.ThongKeNghiPhep, true, TableStyles.Medium10);

                            ws2.Column(4).Style.Numberformat.Format = numberFormat;
                            ws2.Column(5).Style.Numberformat.Format = numberFormat;

                            // [Tính năng Xịn Xò]: Tô đỏ nhân viên nghỉ phép > 0 ngày
                            var tbl = ws2.Tables[0];
                            var ngayNghiRange = ws2.Cells[tbl.Address.Start.Row + 1, 5, tbl.Address.End.Row, 5];
                            var condRule = ngayNghiRange.ConditionalFormatting.AddGreaterThan();
                            condRule.Formula = "0";
                            condRule.Style.Font.Color.Color = System.Drawing.Color.Red;
                            condRule.Style.Font.Bold = true;

                            ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
                        }

                        // --- LƯU FILE ---
                        package.Save();
                    }

                    // Hộp thoại xác nhận xịn xò
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
                    // [BẮT RIÊNG LỖI FILE ĐANG MỞ]
                    MessageBox.Show("Tệp Excel này đang được mở bởi một chương trình khác (ví dụ: Microsoft Excel).\n\nVui lòng đóng tệp đó lại trước khi bấm xuất báo cáo!",
                                    "File Đang Mở", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra khi tạo file Excel:\n\n" + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}