using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Style; // Thêm thư viện Style của EPPlus
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyDonHangView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyDonHangGridDto> _orderList = new();
        private QuanLyDonHangGridDto? _selectedOrder;

        //static QuanLyDonHangView() { ApiClient.Instance = new ApiClient.Instance { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyDonHangView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // 1. CHÌA KHÓA CỔNG (Cho phép vào nếu có ít nhất 1 quyền trong nhóm Đơn hàng)
            bool hasAccess = AuthService.CoQuyen("FULL_QL", "QL_DON_HANG", "QL_PHU_THU", "QL_NGUOI_GIAO_HANG");
            if (!hasAccess)
            {
                MessageBox.Show("Từ chối truy cập phân hệ Đơn hàng!");
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            // 2. CHÌA KHÓA PHÒNG (Xem dữ liệu đơn hàng)
            if (AuthService.CoQuyen("FULL_QL", "QL_DON_HANG"))
            {
                // Hiện dữ liệu, ẩn thông báo
                if (FindName("GridDuLieuDonHang") is System.Windows.Controls.Grid g) g.Visibility = Visibility.Visible;
                if (FindName("txtThongBaoKhongCoQuyen") is System.Windows.Controls.Border b) b.Visibility = Visibility.Collapsed;

                // Gán ngày mặc định (Tự động trigger Filter_Changed để load dữ liệu)
                if (FindName("dpTuNgay") is DatePicker tuNgay) tuNgay.SelectedDate = DateTime.Today.AddDays(-7);
                if (FindName("dpDenNgay") is DatePicker denNgay) denNgay.SelectedDate = DateTime.Today;

                // FIX LỖI: Sửa LoadDataAsync thành LoadOrdersAsync
                await LoadOrdersAsync();
            }
            else
            {
                // Ẩn dữ liệu, hiện khiên bảo mật
                if (FindName("GridDuLieuDonHang") is System.Windows.Controls.Grid g) g.Visibility = Visibility.Collapsed;
                if (FindName("txtThongBaoKhongCoQuyen") is System.Windows.Controls.Border b) b.Visibility = Visibility.Visible;
            }
        }

        private void ApplyPermissions()
        {
            // Ẩn hiện các nút chức năng Header
            if (FindName("btnNavPhuThu") is Button b1) b1.Visibility = AuthService.CoQuyen("FULL_QL", "QL_PHU_THU") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavShipper") is Button b2) b2.Visibility = AuthService.CoQuyen("FULL_QL", "QL_NGUOI_GIAO_HANG") ? Visibility.Visible : Visibility.Collapsed;

            // Nút Xuất Excel và Hủy đơn thuộc về QL_DON_HANG
            bool canOrder = AuthService.CoQuyen("FULL_QL", "QL_DON_HANG");
            if (FindName("btnXuatExcel") is Button b3) b3.Visibility = canOrder ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnHuyDon") is Button b4) b4.Visibility = canOrder ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadOrdersAsync()
        {
            if (FindName("LoadingOverlay") is System.Windows.Controls.Border l) l.Visibility = Visibility.Visible;
            try
            {
                var tuNgay = (FindName("dpTuNgay") as DatePicker)?.SelectedDate?.ToString("yyyy-MM-dd");
                var denNgay = (FindName("dpDenNgay") as DatePicker)?.SelectedDate?.ToString("yyyy-MM-dd");

                var cmb = FindName("cmbFilterTrangThai") as ComboBox;
                var status = cmb?.Text == "Tất cả" ? "" : cmb?.Text; // Tránh gửi "Tất cả" lên API
                var search = (FindName("txtSearch") as TextBox)?.Text.Trim();

                string url = $"api/app/quanly-donhang?tuNgay={tuNgay}&denNgay={denNgay}&trangThai={status}&search={search}";
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDonHangGridDto>>(url);

                if (res != null)
                {
                    _orderList = res;
                    if (FindName("dgDonHang") is DataGrid dg) dg.ItemsSource = _orderList;
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        // Hỗ trợ tự động load khi thay đổi Combobox / DatePicker trên XAML
        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) // Chỉ load khi giao diện đã dựng xong
            {
                await LoadOrdersAsync();
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e) => await LoadOrdersAsync();

        private async void DgDonHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDonHang") is DataGrid dg && dg.SelectedItem is QuanLyDonHangGridDto item)
            {
                _selectedOrder = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;

                if (FindName("btnHuyDon") is Button btnHuy) btnHuy.IsEnabled = item.TrangThai != "Đã thanh toán" && item.TrangThai != "Đã hủy";

                try
                {
                    var detail = await ApiClient.Instance.GetFromJsonAsync<QuanLyDonHangDetailDto>($"api/app/quanly-donhang/{item.IdHoaDon}");
                    if (detail != null)
                    {
                        if (FindName("txtMaHD") is TextBlock t1) t1.Text = $"Mã HĐ: {detail.IdHoaDon}";
                        if (FindName("txtThoiGian") is TextBlock t2) t2.Text = $"Thời gian: {detail.ThoiGianTao:dd/MM/yyyy HH:mm}";
                        if (FindName("txtNhanVien") is TextBlock t3) t3.Text = $"Nhân viên: {detail.NhanVien}";
                        if (FindName("txtKhachHang") is TextBlock t4) t4.Text = $"Khách hàng: {detail.KhachHang}";
                        if (FindName("txtTrangThai") is TextBlock t5) { t5.Text = $"Trạng thái: {detail.TrangThai}"; t5.Foreground = detail.TrangThai == "Đã thanh toán" ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red; }

                        if (FindName("dgChiTiet") is DataGrid dgc) dgc.ItemsSource = detail.ChiTiet;

                        if (FindName("txtPhuThu") is TextBlock p1) p1.Text = $"Phụ thu: {detail.PhuThu:N0}";
                        if (FindName("txtGiamGia") is TextBlock p2) p2.Text = $"Giảm giá: {detail.GiamGia:N0}";
                        if (FindName("txtGhiChu") is TextBlock p3) p3.Text = $"Ghi chú: {detail.GhiChu}";
                        if (FindName("txtTongTien") is TextBlock p4) p4.Text = $"{detail.TongTien:N0} đ";

                        if (FindName("txtShipper") is TextBlock s1) s1.Text = $"Shipper: {detail.NguoiGiaoHang}";
                        if (FindName("txtNguoiNhan") is TextBlock s2) s2.Text = $"Người nhận: {detail.KhachHang}";
                        if (FindName("txtSDTGiao") is TextBlock s3) s3.Text = $"SĐT: {detail.SoDienThoaiGiaoHang}";
                        if (FindName("txtDiaChiGiao") is TextBlock s4) s4.Text = $"Địa chỉ: {detail.DiaChiGiaoHang}";
                    }
                }
                catch { }
            }
        }

        private async Task UpdateStatusAsync(string newStatus)
        {
            if (_selectedOrder == null) return;
            var dto = new QuanLyDonHangUpdateStatusDto { TrangThai = newStatus };

            if (FindName("LoadingOverlay") is System.Windows.Controls.Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-donhang/{_selectedOrder.IdHoaDon}/status", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Cập nhật thành công!"); await LoadOrdersAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnHuyDon_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn HỦY đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                await UpdateStatusAsync("Hủy");
        }

        // =======================================================
        // NÂNG CẤP XUẤT EXCEL PRO CHUẨN CAFEBOOK
        // =======================================================
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_orderList == null || !_orderList.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Danh Sách Đơn Hàng",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"DanhSachDonHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // Thiết lập License EPPlus
                    ExcelPackage.License.SetNonCommercialPersonal("Cafebook Admin");

                    FileInfo fileInfo = new FileInfo(sfd.FileName);
                    if (fileInfo.Exists) fileInfo.Delete();

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        var ws = package.Workbook.Worksheets.Add("Danh sách Đơn hàng");

                        // 1. Tạo Header
                        ws.Cells["A1"].Value = "DANH SÁCH ĐƠN HÀNG CAFEBOOK";
                        ws.Cells["A1:E1"].Merge = true;
                        ws.Cells["A1"].Style.Font.Size = 16;
                        ws.Cells["A1"].Style.Font.Bold = true;
                        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Row(1).Height = 30;

                        // 2. Ngày xuất
                        ws.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        ws.Cells["A2:E2"].Merge = true;
                        ws.Cells["A2"].Style.Font.Italic = true;
                        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        // 3. Tiêu đề cột
                        ws.Cells["A3"].Value = "Mã ĐH";
                        ws.Cells["B3"].Value = "Thời Gian Tạo";
                        ws.Cells["C3"].Value = "Bàn / Khu Vực";
                        ws.Cells["D3"].Value = "Tổng Tiền (VNĐ)";
                        ws.Cells["E3"].Value = "Trạng Thái";

                        // 4. Đổ dữ liệu
                        int rowStart = 4;
                        int currentRow = rowStart;

                        foreach (var item in _orderList)
                        {
                            ws.Cells[$"A{currentRow}"].Value = item.IdHoaDon;

                            ws.Cells[$"B{currentRow}"].Value = item.ThoiGianTao;
                            ws.Cells[$"B{currentRow}"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                            ws.Cells[$"C{currentRow}"].Value = item.TenBan;

                            ws.Cells[$"D{currentRow}"].Value = item.TongTien;
                            ws.Cells[$"D{currentRow}"].Style.Numberformat.Format = "#,##0";

                            ws.Cells[$"E{currentRow}"].Value = item.TrangThai;

                            // Đổi màu trạng thái cho sinh động
                            if (item.TrangThai == "Đã hủy")
                                ws.Cells[$"E{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            else if (item.TrangThai == "Đã thanh toán")
                                ws.Cells[$"E{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Green);

                            currentRow++;
                        }

                        // 5. Định dạng Table
                        var tableRange = ws.Cells[3, 1, currentRow - 1, 5];
                        var table = ws.Tables.Add(tableRange, "TableDonHang");
                        table.TableStyle = TableStyles.Medium9;

                        // 6. Căn chỉnh cột tự động
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();

                        package.Save();
                    }

                    // Hỏi xem file
                    string msg = $"Đã xuất file Excel chuẩn tại:\n{sfd.FileName}\n\n" +
                                 $"• Chọn [Yes] để mở trực tiếp bảng tính.\n" +
                                 $"• Chọn [No] để mở thư mục chứa file.\n" +
                                 $"• Chọn [Cancel] để đóng.";

                    var result = MessageBox.Show(msg, "Xuất Excel Hoàn Tất", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    else if (result == MessageBoxResult.No)
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tạo file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnNavPhuThu_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_PHU_THU"))
                this.NavigationService?.Navigate(new QuanLyPhuThuView());
        }
        private void BtnNavShipper_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_NGUOI_GIAO_HANG"))
                this.NavigationService?.Navigate(new QuanLyDonViVanChuyenView());
        }
    }
}