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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

// KHẮC PHỤC LỖI AMBIGUOUS BORDER VỚI EPPLUS
using Border = System.Windows.Controls.Border;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyKhuyenMaiView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyKhuyenMaiGridDto> _allKhuyenMaiList = new();
        private QuanLyKhuyenMaiSaveDto? _selectedKhuyenMai = null;
        private List<QuanLyKhuyenMaiLookupDto> _sanPhamList = new();

        static QuanLyKhuyenMaiView()
        {
            httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") };
        }

        public QuanLyKhuyenMaiView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI"))
            {
                ApplyPermissions();
                return;
            }

            ApplyPermissions();
            if (FindName("cmbFilterTrangThai") is ComboBox cmbTT) cmbTT.SelectedIndex = 0;

            await LoadFiltersAsync();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI");
            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
            if (FindName("btnXuatExcel") is Button bx) bx.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetLoading(bool isLoading)
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var sp = await httpClient.GetFromJsonAsync<List<QuanLyKhuyenMaiLookupDto>>("api/app/quanly-khuyenmai/filters");
                if (sp != null && FindName("cmbSanPhamApDung") is ComboBox cmb)
                {
                    var viewList = new List<QuanLyKhuyenMaiLookupDto> { new QuanLyKhuyenMaiLookupDto { Id = 0, Ten = "Không áp dụng (cho toàn hóa đơn)" } };
                    viewList.AddRange(sp);
                    cmb.ItemsSource = viewList;
                }
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            SetLoading(true);
            try
            {
                string maKM = (FindName("txtTimKiem") as TextBox)?.Text ?? "";
                string trangThai = (FindName("cmbFilterTrangThai") as ComboBox)?.Text ?? "Tất cả";

                var res = await httpClient.GetFromJsonAsync<List<QuanLyKhuyenMaiGridDto>>($"api/app/quanly-khuyenmai/search?maKhuyenMai={maKM}&trangThai={trangThai}");
                if (res != null && FindName("dgKhuyenMai") is DataGrid dg)
                {
                    _allKhuyenMaiList = res;
                    dg.ItemsSource = _allKhuyenMaiList;
                    LamMoiUI();
                }
            }
            finally { SetLoading(false); }
        }

        private async void Filters_Changed(object sender, RoutedEventArgs e) { if (IsLoaded) await LoadDataAsync(); }

        private async void DgKhuyenMai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgKhuyenMai") is DataGrid dg && dg.SelectedItem is QuanLyKhuyenMaiGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblFormTitle") is TextBlock title) title.Text = "Cập nhật Khuyến Mãi";

                SetLoading(true);
                try
                {
                    var detail = await httpClient.GetFromJsonAsync<QuanLyKhuyenMaiSaveDto>($"api/app/quanly-khuyenmai/{item.IdKhuyenMai}");
                    if (detail != null)
                    {
                        _selectedKhuyenMai = detail;

                        if (FindName("txtMaKhuyenMai") is TextBox t1) { t1.Text = detail.MaKhuyenMai; t1.IsReadOnly = true; }
                        if (FindName("txtTenChuongTrinh") is TextBox t2) t2.Text = detail.TenChuongTrinh;
                        if (FindName("txtMoTa") is TextBox t3) t3.Text = detail.MoTa;
                        if (FindName("cmbLoaiGiamGia") is ComboBox cb1) cb1.SelectedIndex = detail.LoaiGiamGia == "PhanTram" ? 0 : 1;
                        if (FindName("txtGiaTriGiam") is TextBox t4) t4.Text = detail.GiaTriGiam.ToString(CultureInfo.InvariantCulture);
                        if (FindName("txtGiamToiDa") is TextBox t5) t5.Text = detail.GiamToiDa?.ToString(CultureInfo.InvariantCulture) ?? "";
                        if (FindName("txtHoaDonToiThieu") is TextBox t6) t6.Text = detail.HoaDonToiThieu?.ToString(CultureInfo.InvariantCulture) ?? "";
                        if (FindName("dpNgayBatDau") is DatePicker dp1) dp1.SelectedDate = detail.NgayBatDau;
                        if (FindName("dpNgayKetThuc") is DatePicker dp2) dp2.SelectedDate = detail.NgayKetThuc;
                        if (FindName("txtGioBatDau") is TextBox t7) t7.Text = detail.GioBatDau;
                        if (FindName("txtGioKetThuc") is TextBox t8) t8.Text = detail.GioKetThuc;
                        if (FindName("txtNgayTrongTuan") is TextBox t9) t9.Text = detail.NgayTrongTuan;
                        if (FindName("cmbSanPhamApDung") is ComboBox cb2) cb2.SelectedValue = detail.IdSanPhamApDung ?? 0;
                        if (FindName("txtSoLuongConLai") is TextBox t10) t10.Text = detail.SoLuongConLai?.ToString();
                        if (FindName("txtDieuKien") is TextBox t11) t11.Text = detail.DieuKienApDung;

                        if (FindName("cmbTrangThai") is ComboBox cb3) cb3.Text = detail.TrangThai;
                        if (FindName("pnlTrangThai") is StackPanel pnl) pnl.Visibility = Visibility.Visible;

                        if (FindName("btnThem") is Button bt) bt.Visibility = Visibility.Collapsed;
                        if (FindName("btnLuu") is Button bl) bl.Visibility = Visibility.Visible;
                        if (FindName("btnXoa") is Button bx) bx.Visibility = Visibility.Visible;
                        if (FindName("btnTamDung") is Button btd) btd.Visibility = Visibility.Visible;
                    }
                }
                finally { SetLoading(false); }
            }
        }

        private void LamMoiUI()
        {
            _selectedKhuyenMai = null;
            if (FindName("dgKhuyenMai") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblFormTitle") is TextBlock title) title.Text = "Thêm Khuyến Mãi Mới";

            if (FindName("txtMaKhuyenMai") is TextBox t1) { t1.Clear(); t1.IsReadOnly = false; }
            if (FindName("txtTenChuongTrinh") is TextBox t2) t2.Clear();
            if (FindName("txtMoTa") is TextBox t3) t3.Clear();
            if (FindName("cmbLoaiGiamGia") is ComboBox cb1) cb1.SelectedIndex = 0;
            if (FindName("txtGiaTriGiam") is TextBox t4) t4.Clear();
            if (FindName("txtGiamToiDa") is TextBox t5) t5.Clear();
            if (FindName("txtHoaDonToiThieu") is TextBox t6) t6.Text = "0";
            if (FindName("dpNgayBatDau") is DatePicker dp1) dp1.SelectedDate = DateTime.Today;
            if (FindName("dpNgayKetThuc") is DatePicker dp2) dp2.SelectedDate = DateTime.Today.AddDays(7);
            if (FindName("txtGioBatDau") is TextBox t7) t7.Clear();
            if (FindName("txtGioKetThuc") is TextBox t8) t8.Clear();
            if (FindName("txtNgayTrongTuan") is TextBox t9) t9.Clear();
            if (FindName("cmbSanPhamApDung") is ComboBox cb2 && cb2.Items.Count > 0) cb2.SelectedValue = 0;
            if (FindName("txtSoLuongConLai") is TextBox t10) t10.Clear();
            if (FindName("txtDieuKien") is TextBox t11) t11.Clear();

            if (FindName("pnlTrangThai") is StackPanel pnl) pnl.Visibility = Visibility.Collapsed;

            if (FindName("btnThem") is Button bt) bt.Visibility = Visibility.Visible;
            if (FindName("btnLuu") is Button bl) bl.Visibility = Visibility.Collapsed;
            if (FindName("btnXoa") is Button bx) bx.Visibility = Visibility.Collapsed;
            if (FindName("btnTamDung") is Button btd) btd.Visibility = Visibility.Collapsed;
        }

        private async void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            LamMoiUI();
            if (_allKhuyenMaiList.Count == 0) await LoadDataAsync();
        }

        private QuanLyKhuyenMaiSaveDto MapDtoFromUi()
        {
            string loai = "SoTien";
            if (FindName("cmbLoaiGiamGia") is ComboBox cb && cb.SelectedItem is ComboBoxItem item)
                loai = item.Tag?.ToString() ?? "SoTien";

            int spId = (FindName("cmbSanPhamApDung") as ComboBox)?.SelectedValue is int id ? id : 0;

            // Xử lý chuỗi thứ trong tuần, đảm bảo chỉ có số 2-8
            string daysRaw = (FindName("txtNgayTrongTuan") as TextBox)?.Text.Trim() ?? "";
            var validDays = daysRaw.Split(',').Select(x => x.Trim()).Where(x => int.TryParse(x, out int d) && d >= 2 && d <= 8).ToList();
            string daysSafe = validDays.Any() ? string.Join(",", validDays) : "";

            return new QuanLyKhuyenMaiSaveDto
            {
                MaKhuyenMai = (FindName("txtMaKhuyenMai") as TextBox)?.Text.Trim() ?? "",
                TenChuongTrinh = (FindName("txtTenChuongTrinh") as TextBox)?.Text.Trim() ?? "",
                MoTa = (FindName("txtMoTa") as TextBox)?.Text.Trim(),
                LoaiGiamGia = loai,
                GiaTriGiam = decimal.TryParse((FindName("txtGiaTriGiam") as TextBox)?.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var gt) ? gt : 0,
                GiamToiDa = decimal.TryParse((FindName("txtGiamToiDa") as TextBox)?.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var max) ? max : null,
                HoaDonToiThieu = decimal.TryParse((FindName("txtHoaDonToiThieu") as TextBox)?.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var min) ? min : null,
                NgayBatDau = (FindName("dpNgayBatDau") as DatePicker)?.SelectedDate ?? DateTime.Today,
                NgayKetThuc = (FindName("dpNgayKetThuc") as DatePicker)?.SelectedDate ?? DateTime.Today.AddDays(1),
                GioBatDau = (FindName("txtGioBatDau") as TextBox)?.Text.Trim(),
                GioKetThuc = (FindName("txtGioKetThuc") as TextBox)?.Text.Trim(),
                NgayTrongTuan = daysSafe,
                SoLuongConLai = int.TryParse((FindName("txtSoLuongConLai") as TextBox)?.Text, out int sl) ? sl : null,
                DieuKienApDung = (FindName("txtDieuKien") as TextBox)?.Text.Trim(),
                IdSanPhamApDung = spId > 0 ? spId : null
            };
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace((FindName("txtMaKhuyenMai") as TextBox)?.Text)) { MessageBox.Show("Vui lòng nhập Mã KM."); return false; }
            if (string.IsNullOrWhiteSpace((FindName("txtTenChuongTrinh") as TextBox)?.Text)) { MessageBox.Show("Vui lòng nhập Tên CT."); return false; }
            if (!decimal.TryParse((FindName("txtGiaTriGiam") as TextBox)?.Text, out var g) || g <= 0) { MessageBox.Show("Giá trị giảm > 0."); return false; }
            return true;
        }

        private async void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI") || !ValidateInput()) return;

            SetLoading(true);
            try
            {
                var dto = MapDtoFromUi();
                dto.TrangThai = "Hoạt động";
                var res = await httpClient.PostAsJsonAsync("api/app/quanly-khuyenmai", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Thêm mới thành công!"); await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { SetLoading(false); }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhuyenMai == null || !AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI") || !ValidateInput()) return;

            SetLoading(true);
            try
            {
                var dto = MapDtoFromUi();
                dto.IdKhuyenMai = _selectedKhuyenMai.IdKhuyenMai;
                dto.TrangThai = (FindName("cmbTrangThai") as ComboBox)?.Text ?? "Hoạt động";

                var res = await httpClient.PutAsJsonAsync($"api/app/quanly-khuyenmai/{dto.IdKhuyenMai}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Cập nhật thành công!"); await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { SetLoading(false); }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhuyenMai == null || !AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI")) return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa mã '{_selectedKhuyenMai.MaKhuyenMai}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SetLoading(true);
                try
                {
                    var res = await httpClient.DeleteAsync($"api/app/quanly-khuyenmai/{_selectedKhuyenMai.IdKhuyenMai}");
                    if (res.IsSuccessStatusCode) { MessageBox.Show("Đã xóa!"); LamMoiUI(); await LoadDataAsync(); }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { SetLoading(false); }
            }
        }

        private async void BtnTamDung_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhuyenMai == null || !AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI")) return;
            SetLoading(true);
            try
            {
                var res = await httpClient.PatchAsync($"api/app/quanly-khuyenmai/togglestatus/{_selectedKhuyenMai.IdKhuyenMai}", null);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã thay đổi trạng thái!"); await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            finally { SetLoading(false); }
        }

        // =======================================================
        // NÂNG CẤP XUẤT EXCEL PRO (ĐỊNH DẠNG HOÀN HẢO)
        // =======================================================
        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allKhuyenMaiList == null || !_allKhuyenMaiList.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Danh Sách Khuyến Mãi",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"DanhSachKhuyenMai_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
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
                        var ws = package.Workbook.Worksheets.Add("Danh sách Khuyến mãi");

                        // 1. Tạo Header
                        ws.Cells["A1"].Value = "DANH SÁCH KHUYẾN MÃI CAFEBOOK";
                        ws.Cells["A1:H1"].Merge = true;
                        ws.Cells["A1"].Style.Font.Size = 16;
                        ws.Cells["A1"].Style.Font.Bold = true;
                        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Row(1).Height = 30;

                        // 2. Ngày xuất
                        ws.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        ws.Cells["A2:H2"].Merge = true;
                        ws.Cells["A2"].Style.Font.Italic = true;
                        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        // 3. Tiêu đề cột
                        ws.Cells["A3"].Value = "Mã KM";
                        ws.Cells["B3"].Value = "Tên Chương Trình";
                        ws.Cells["C3"].Value = "Loại Giảm Giá";
                        ws.Cells["D3"].Value = "Giá Trị Giảm";
                        ws.Cells["E3"].Value = "Bắt Đầu";
                        ws.Cells["F3"].Value = "Kết Thúc";
                        ws.Cells["G3"].Value = "Còn Lại";
                        ws.Cells["H3"].Value = "Trạng Thái";

                        // 4. Đổ dữ liệu
                        int rowStart = 4;
                        int currentRow = rowStart;

                        foreach (var item in _allKhuyenMaiList)
                        {
                            ws.Cells[$"A{currentRow}"].Value = item.MaKhuyenMai;
                            ws.Cells[$"B{currentRow}"].Value = item.TenChuongTrinh;

                            // Tách số tiền/phần trăm từ chuỗi Format sẵn
                            bool isPhanTram = item.GiaTriGiam.Contains("%");
                            ws.Cells[$"C{currentRow}"].Value = isPhanTram ? "Phần trăm" : "Số tiền";

                            // Ghi lại giá trị gốc để Excel hiểu là Số (Number)
                            string rawVal = item.GiaTriGiam.Replace("%", "").Replace("đ", "").Replace(".", "").Replace(",", ".");
                            if (decimal.TryParse(rawVal, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                            {
                                ws.Cells[$"D{currentRow}"].Value = val;
                                ws.Cells[$"D{currentRow}"].Style.Numberformat.Format = isPhanTram ? "#,##0.##" : "#,##0";
                            }
                            else { ws.Cells[$"D{currentRow}"].Value = item.GiaTriGiam; }

                            ws.Cells[$"E{currentRow}"].Value = item.NgayBatDau;
                            ws.Cells[$"E{currentRow}"].Style.Numberformat.Format = "dd/MM/yyyy";

                            ws.Cells[$"F{currentRow}"].Value = item.NgayKetThuc;
                            ws.Cells[$"F{currentRow}"].Style.Numberformat.Format = "dd/MM/yyyy";

                            ws.Cells[$"G{currentRow}"].Value = item.SoLuongConLai;

                            ws.Cells[$"H{currentRow}"].Value = item.TrangThai;

                            // Đổi màu trạng thái
                            if (item.TrangThai == "Hoạt động") ws.Cells[$"H{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                            else if (item.TrangThai == "Tạm dừng") ws.Cells[$"H{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.OrangeRed);
                            else if (item.TrangThai == "Hết hạn") ws.Cells[$"H{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                            currentRow++;
                        }

                        // 5. Định dạng Table
                        var tableRange = ws.Cells[3, 1, currentRow - 1, 8];
                        var table = ws.Tables.Add(tableRange, "TableKhuyenMai");
                        table.TableStyle = TableStyles.Medium9;

                        // 6. AutoFit
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        package.Save();
                    }

                    string msg = $"Đã xuất Excel chuẩn tại:\n{sfd.FileName}\n\n• Chọn [Yes] để mở trực tiếp.\n• Chọn [No] để mở thư mục.";
                    var result = MessageBox.Show(msg, "Hoàn Tất", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    else if (result == MessageBoxResult.No) System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi Excel: " + ex.Message); }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService?.CanGoBack == true) this.NavigationService.GoBack();
        }
    }
}