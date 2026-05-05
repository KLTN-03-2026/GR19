using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils;
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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;
using DataGrid = System.Windows.Controls.DataGrid;
using TextBlock = System.Windows.Controls.TextBlock;
using DatePicker = System.Windows.Controls.DatePicker;
using Border = System.Windows.Controls.Border;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyBaoCaoHieuSuatView : Page
    {
        private QuanLyBaoCaoHieuSuatTongHopDto? currentReportData;

        private bool _isDataLoaded = false;

        public QuanLyBaoCaoHieuSuatView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL") && !AuthService.CoQuyen("QL_BAO_CAO_HIEU_SUAT_NHAN_SU"))
            {
                MessageBox.Show("Bạn không có quyền truy cập!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);
            if (!this.IsLoaded) return;
            try
            {
                ApplyPermissions();

                var now = DateTime.Now;
                if (FindName("dpStartDate") is DatePicker dpS) dpS.SelectedDate = new DateTime(now.Year, now.Month, 1);
                if (FindName("dpEndDate") is DatePicker dpE) dpE.SelectedDate = now;

                await LoadFiltersAsync();
                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        private void ApplyPermissions()
        {
            bool canExport = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL") || AuthService.CoQuyen("QL_BAO_CAO_HIEU_SUAT_NHAN_SU");
            if (FindName("btnExportToExcel") is Button btnEx) btnEx.Visibility = canExport ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<QuanLyBaoCaoHieuSuat_FiltersDto>("api/app/quanly/baocaohieusuat/filters");
                if (response != null && response.VaiTros != null)
                {
                    var vaiTros = response.VaiTros;
                    if (FindName("cmbVaiTro") is ComboBox cmb)
                    {
                        vaiTros.Insert(0, new QuanLyFilterLookupDto { Id = 0, Ten = "-- Tất cả --" });
                        cmb.ItemsSource = vaiTros;
                        cmb.SelectedIndex = 0;
                    }
                }
            }
            catch { /* Ignore silently */ }
        }

        private async void CmbSearchNhanVien_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb == null) return;
            if (e.Key == System.Windows.Input.Key.Up || e.Key == System.Windows.Input.Key.Down ||
                e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Escape)
                return;

            string keyword = cmb.Text;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                cmb.IsDropDownOpen = false;
                return;
            }

            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyFilterLookupDto>>($"api/app/quanly/baocaohieusuat/search-nhan-vien?keyword={keyword}");

                if (response != null && response.Any())
                {
                    cmb.ItemsSource = response;
                    cmb.Text = keyword; 
                    cmb.IsDropDownOpen = true;

                    if (cmb.Template.FindName("PART_EditableTextBox", cmb) is TextBox textBox)
                    {
                        textBox.SelectionStart = textBox.Text.Length;
                    }
                }
                else
                {
                    cmb.IsDropDownOpen = false;
                }
            }
            catch { /* Bỏ qua ngoại lệ mạng nếu user gõ quá nhanh */ }
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DatePicker? dpStart = FindName("dpStartDate") as DatePicker;
            DatePicker? dpEnd = FindName("dpEndDate") as DatePicker;
            Border? loading = FindName("LoadingOverlay") as Border;

            if (dpStart?.SelectedDate == null || dpEnd?.SelectedDate == null) return;

            if (loading != null) loading.Visibility = Visibility.Visible;

            int? selectedVaiTro = (FindName("cmbVaiTro") as ComboBox)?.SelectedValue as int?;
            if (selectedVaiTro == 0) selectedVaiTro = null;

            int? selectedNhanVienId = null;
            string? searchText = null;

            if (FindName("cmbSearchNhanVien") is ComboBox cmbSearch)
            {
                selectedNhanVienId = cmbSearch.SelectedValue as int?;
                searchText = cmbSearch.Text;

                if (selectedNhanVienId.HasValue)
                {
                    searchText = null;
                }
            }

            var request = new QuanLyBaoCaoHieuSuatRequestDto
            {
                StartDate = dpStart.SelectedDate.Value,
                EndDate = dpEnd.SelectedDate.Value,
                VaiTroId = selectedVaiTro,
                NhanVienId = selectedNhanVienId,
                SearchText = searchText
            };

            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly/baocaohieusuat/report", request);
                if (response.IsSuccessStatusCode)
                {
                    currentReportData = await response.Content.ReadFromJsonAsync<QuanLyBaoCaoHieuSuatTongHopDto>();
                    if (currentReportData != null) PopulateUi(currentReportData);
                }
                else MessageBox.Show("Lỗi lấy dữ liệu từ Server.", "Lỗi");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi mạng: " + ex.Message, "Lỗi"); }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private void PopulateUi(QuanLyBaoCaoHieuSuatTongHopDto data)
        {
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            if (FindName("lblTongDoanhThu") is TextBlock t1) t1.Text = data.Kpi.TongDoanhThu.ToString("C0", culture);
            if (FindName("lblTongGioLam") is TextBlock t2) t2.Text = data.Kpi.TongGioLam.ToString("N2");
            if (FindName("lblTongSoCaLam") is TextBlock t3) t3.Text = data.Kpi.TongSoCaLam.ToString("N0");
            if (FindName("lblTongLanHuyMon") is TextBlock t4) t4.Text = data.Kpi.TongLanHuyMon.ToString("N0");

            if (FindName("dgSales") is DataGrid d1) d1.ItemsSource = data.SalesPerformance;
            if (FindName("dgOperations") is DataGrid d2) d2.ItemsSource = data.OperationalPerformance;
            if (FindName("dgAttendance") is DataGrid d3) d3.ItemsSource = data.Attendance;
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();

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
                Title = "Lưu Báo Cáo Hiệu Suất",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"BaoCaoHieuSuat_{startDate:ddMMyyyy}_{endDate:ddMMyyyy}.xlsx"
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
                        var numberFormat = "#,##0";
                        var wsOverview = package.Workbook.Worksheets.Add("Tổng Quan KPI");

                        wsOverview.Cells["A1:D1"].Merge = true;
                        wsOverview.Cells["A1"].Value = "BÁO CÁO TỔNG QUAN HIỆU SUẤT NHÂN SỰ";
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
                        wsOverview.Cells["A4"].Value = "I. CHỈ SỐ KPI CHÍNH CỦA ĐỘI NGŨ";
                        wsOverview.Cells["A4"].Style.Font.Bold = true;
                        wsOverview.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        wsOverview.Cells["A4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                        wsOverview.Cells["A5"].Value = "1. Tổng Doanh Thu Đã Bán"; wsOverview.Cells["B5"].Value = currentReportData.Kpi.TongDoanhThu;
                        wsOverview.Cells["A6"].Value = "2. Tổng Giờ Làm (Hệ thống)"; wsOverview.Cells["B6"].Value = currentReportData.Kpi.TongGioLam;
                        wsOverview.Cells["A7"].Value = "3. Tổng Số Ca Đã Chấm Công"; wsOverview.Cells["B7"].Value = currentReportData.Kpi.TongSoCaLam;
                        wsOverview.Cells["A8"].Value = "4. Tổng Lượt Hủy Món (Toàn quán)"; wsOverview.Cells["B8"].Value = currentReportData.Kpi.TongLanHuyMon;

                        wsOverview.Cells["B5"].Style.Numberformat.Format = currencyFormat;
                        wsOverview.Cells["B5"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        wsOverview.Cells["B5"].Style.Font.Bold = true;

                        wsOverview.Cells["B6"].Style.Numberformat.Format = "#,##0.00";
                        wsOverview.Cells["B7"].Style.Numberformat.Format = numberFormat;

                        wsOverview.Cells["B8"].Style.Numberformat.Format = numberFormat;
                        wsOverview.Cells["B8"].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                        wsOverview.Column(1).Width = 35;
                        wsOverview.Column(2).Width = 20;

                        if (currentReportData.SalesPerformance.Any())
                        {
                            var ws1 = package.Workbook.Worksheets.Add("Bán Hàng (Sales)");
                            ws1.Cells["A1:F1"].Merge = true;
                            ws1.Cells["A1"].Value = "CHI TIẾT HIỆU SUẤT BÁN HÀNG THEO NHÂN VIÊN";
                            ws1.Cells["A1"].Style.Font.Size = 14;
                            ws1.Cells["A1"].Style.Font.Bold = true;
                            ws1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws1.Row(1).Height = 25;

                            ws1.Cells["A3"].LoadFromCollection(currentReportData.SalesPerformance, true, TableStyles.Medium9);

                            ws1.Column(3).Style.Numberformat.Format = currencyFormat;
                            ws1.Column(4).Style.Numberformat.Format = numberFormat;
                            ws1.Column(5).Style.Numberformat.Format = currencyFormat;
                            ws1.Column(6).Style.Numberformat.Format = numberFormat;

                            ws1.Cells[ws1.Dimension.Address].AutoFitColumns();
                        }

                        if (currentReportData.OperationalPerformance.Any())
                        {
                            var ws2 = package.Workbook.Worksheets.Add("Vận Hành");
                            ws2.Cells["A1:F1"].Merge = true;
                            ws2.Cells["A1"].Value = "CHI TIẾT VẬN HÀNH (NHẬP/KIỂM/HỦY KHO)";
                            ws2.Cells["A1"].Style.Font.Size = 14;
                            ws2.Cells["A1"].Style.Font.Bold = true;
                            ws2.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws2.Row(1).Height = 25;

                            ws2.Cells["A3"].LoadFromCollection(currentReportData.OperationalPerformance, true, TableStyles.Medium10);

                            ws2.Column(3).Style.Numberformat.Format = numberFormat;
                            ws2.Column(4).Style.Numberformat.Format = numberFormat;
                            ws2.Column(5).Style.Numberformat.Format = numberFormat;
                            ws2.Column(6).Style.Numberformat.Format = numberFormat;

                            ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
                        }

                        if (currentReportData.Attendance.Any())
                        {
                            var ws3 = package.Workbook.Worksheets.Add("Chấm Công");
                            ws3.Cells["A1:G1"].Merge = true;
                            ws3.Cells["A1"].Value = "BÁO CÁO CHẤM CÔNG VÀ ĐƠN XIN NGHỈ PHÉP";
                            ws3.Cells["A1"].Style.Font.Size = 14;
                            ws3.Cells["A1"].Style.Font.Bold = true;
                            ws3.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws3.Row(1).Height = 25;

                            ws3.Cells["A3"].LoadFromCollection(currentReportData.Attendance, true, TableStyles.Medium11);

                            ws3.Column(3).Style.Numberformat.Format = numberFormat;
                            ws3.Column(4).Style.Numberformat.Format = "#,##0.00";
                            ws3.Column(5).Style.Numberformat.Format = numberFormat;
                            ws3.Column(6).Style.Numberformat.Format = numberFormat;
                            ws3.Column(7).Style.Numberformat.Format = numberFormat;

                            var tbl = ws3.Tables[0];
                            var choDuyetRange = ws3.Cells[tbl.Address.Start.Row + 1, 7, tbl.Address.End.Row, 7];
                            var condRule = choDuyetRange.ConditionalFormatting.AddGreaterThan();
                            condRule.Formula = "0";
                            condRule.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            condRule.Style.Fill.BackgroundColor.Color = System.Drawing.Color.LightYellow;
                            condRule.Style.Font.Color.Color = System.Drawing.Color.DarkOrange;
                            condRule.Style.Font.Bold = true;

                            ws3.Cells[ws3.Dimension.Address].AutoFitColumns();
                        }

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