using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Border = System.Windows.Controls.Border;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyKhachHangView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyKhachHangGridDto> _allKhachHangList = new();
        private QuanLyKhachHangDetailDto? _selectedKhachHang = null;

        static QuanLyKhachHangView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyKhachHangView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG"))
            {
                ApplyPermissions();
                return;
            }

            ApplyPermissions();
            await LoadKhachHangAsync();
        }

        private void ApplyPermissions()
        {
            bool hasQuyenKH = AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG");

            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyenKH ? Visibility.Collapsed : Visibility.Visible;

            if (FindName("btnSuaDiem") is Button bs) bs.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnKhoa") is Button bk) bk.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button bx) bx.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;

            bool hasQuyenKM = AuthService.CoQuyen("FULL_QL", "QL_KHUYEN_MAI");
            if (FindName("btnNavKhuyenMai") is Button btnKM) btnKM.Visibility = hasQuyenKM ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadKhachHangAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await httpClient.GetFromJsonAsync<List<QuanLyKhachHangGridDto>>("api/app/quanly-khachhang");
                if (res != null) { _allKhachHangList = res; FilterData(); }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        // ==========================================
        // THUẬT TOÁN BỎ DẤU TIẾNG VIỆT (Nâng cấp)
        // ==========================================
        private string RemoveVietnameseSigns(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            str = str.ToLower().Trim();
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return str;
        }

        private void Filters_Changed(object sender, RoutedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgKhachHang") is DataGrid dg)) return;
            var query = _allKhachHangList.AsEnumerable();

            // Tìm kiếm thông minh (Có dấu/Không dấu)
            if (FindName("txtSearch") is TextBox txt && !string.IsNullOrWhiteSpace(txt.Text))
            {
                string searchKey = RemoveVietnameseSigns(txt.Text);

                query = query.Where(x =>
                    RemoveVietnameseSigns(x.HoTen).Contains(searchKey) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(searchKey)) ||
                    (x.Email != null && RemoveVietnameseSigns(x.Email).Contains(searchKey))
                );
            }

            if (FindName("cmbLoaiTK") is ComboBox cmb && cmb.SelectedIndex > 0)
                query = query.Where(x => x.TaiKhoanTam == (cmb.SelectedIndex == 1));

            if (FindName("chkHideLocked") is CheckBox chk && chk.IsChecked == true)
                query = query.Where(x => !x.BiKhoa);

            dg.ItemsSource = query.ToList();
        }

        private async void DgKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgKhachHang") is DataGrid dg && dg.SelectedItem is QuanLyKhachHangGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var detail = await httpClient.GetFromJsonAsync<QuanLyKhachHangDetailDto>($"api/app/quanly-khachhang/{item.IdKhachHang}");
                    if (detail != null)
                    {
                        _selectedKhachHang = detail;
                        if (FindName("txtHoTen") is TextBox t1) t1.Text = detail.HoTen;
                        if (FindName("txtSdt") is TextBox t2) t2.Text = detail.SoDienThoai;
                        if (FindName("txtDiem") is TextBox t3) t3.Text = detail.DiemTichLuy.ToString("N0");
                        if (FindName("txtEmail") is TextBox t4) t4.Text = detail.Email;
                        if (FindName("txtDiaChi") is TextBox t5) t5.Text = detail.DiaChi;

                        if (FindName("txtTrangThai") is TextBlock txtTT)
                        {
                            if (detail.BiKhoa)
                            {
                                txtTT.Text = $"Bị khóa đến: {detail.ThoiGianMoKhoa?.ToString("dd/MM/yyyy HH:mm") ?? "Vĩnh viễn"}\nLý do: {detail.LyDoKhoa}";
                                txtTT.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                                if (FindName("btnKhoa") is Button bk) bk.Visibility = Visibility.Collapsed;
                                if (FindName("btnMoKhoa") is Button bm) bm.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                txtTT.Text = "Trạng thái: Hoạt động";
                                txtTT.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                                if (FindName("btnKhoa") is Button bk) bk.Visibility = Visibility.Visible;
                                if (FindName("btnMoKhoa") is Button bm) bm.Visibility = Visibility.Collapsed;
                            }
                        }

                        if (FindName("AvatarPreview") is Image img)
                        {
                            string fullUrl = string.IsNullOrEmpty(detail.AnhDaiDien) ? "" : $"{(AppConfigManager.GetApiServerUrl() ?? "http://localhost").TrimEnd('/')}/{detail.AnhDaiDien.TrimStart('/')}";
                            img.Source = HinhAnhHelper.LoadImage(fullUrl, HinhAnhPaths.DefaultAvatar);
                        }
                    }
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ NÚT BẤM VÀ POPUP
        // ==========================================
        private void BtnHuyPopup_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("PopupKhoa") is Border pk) pk.Visibility = Visibility.Collapsed;
            if (FindName("PopupDiem") is Border pd) pd.Visibility = Visibility.Collapsed;
            if (FindName("PopupXemAnh") is Border pa) pa.Visibility = Visibility.Collapsed;
        }

        private void BtnKhoa_Click(object sender, RoutedEventArgs e) { if (FindName("PopupKhoa") is Border p) p.Visibility = Visibility.Visible; }
        private void BtnSuaDiem_Click(object sender, RoutedEventArgs e) { if (FindName("PopupDiem") is Border p) p.Visibility = Visibility.Visible; }

        private void Avatar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selectedKhachHang == null) return;
            if (FindName("PopupXemAnh") is Border popup && FindName("ImgFullAvatar") is Image imgFull && FindName("AvatarPreview") is Image imgThumb)
            {
                imgFull.Source = imgThumb.Source;
                popup.Visibility = Visibility.Visible;
            }
        }

        private async void BtnXacNhanKhoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG")) return;
            string lyDo = (FindName("txtLyDoKhoa") as TextBox)?.Text.Trim() ?? "";
            int? soNgay = null;
            if (FindName("cmbThoiGianKhoa") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int ngay) && ngay > 0)
                soNgay = ngay;

            if (FindName("PopupKhoa") is Border p) p.Visibility = Visibility.Collapsed;
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var req = new KhoaKhachHangRequestDto { LyDoKhoa = lyDo, SoNgayKhoa = soNgay };
                await httpClient.PostAsJsonAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}/khoa", req);
                MessageBox.Show("Khóa thành công. Hệ thống đang gửi Email.");

                // GIỮ LẠI ID ĐỂ TỰ ĐỘNG CHỌN LẠI SAU KHI LOAD
                int currentId = _selectedKhachHang.IdKhachHang;
                await LoadKhachHangAsync();

                if (FindName("dgKhachHang") is DataGrid dg)
                {
                    var itemToSelect = _allKhachHangList.FirstOrDefault(x => x.IdKhachHang == currentId);
                    if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXacNhanDiem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG")) return;
            string diemStr = (FindName("txtDiemThayDoi") as TextBox)?.Text.Trim() ?? "0";
            if (!int.TryParse(diemStr, out int diemThayDoi)) { MessageBox.Show("Sai định dạng số!"); return; }

            if (FindName("PopupDiem") is Border p) p.Visibility = Visibility.Collapsed;
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var req = new CapNhatDiemKhachHangDto { DiemThayDoi = diemThayDoi, LyDo = "Cập nhật thủ công" };
                await httpClient.PostAsJsonAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}/diem", req);
                MessageBox.Show("Cập nhật điểm thành công!");

                // GIỮ LẠI ID ĐỂ TỰ ĐỘNG CHỌN LẠI SAU KHI LOAD
                int currentId = _selectedKhachHang.IdKhachHang;
                await LoadKhachHangAsync();

                if (FindName("dgKhachHang") is DataGrid dg)
                {
                    var itemToSelect = _allKhachHangList.FirstOrDefault(x => x.IdKhachHang == currentId);
                    if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnMoKhoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG")) return;

            // Thêm thông báo hỏi xác nhận cho an toàn
            if (MessageBox.Show($"Bạn có chắc chắn muốn mở khóa cho tài khoản '{_selectedKhachHang.HoTen}' không?", "Xác nhận mở khóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                await httpClient.PostAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}/mokhoa", null);
                MessageBox.Show("Đã mở khóa tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // GIỮ LẠI ID ĐỂ TỰ ĐỘNG CHỌN LẠI SAU KHI LOAD
                int currentId = _selectedKhachHang.IdKhachHang;
                await LoadKhachHangAsync();

                if (FindName("dgKhachHang") is DataGrid dg)
                {
                    var itemToSelect = _allKhachHangList.FirstOrDefault(x => x.IdKhachHang == currentId);
                    if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG")) return;

            // XÂY DỰNG CÂU CẢNH BÁO CHI TIẾT
            string thongBao = $"Bạn có chắc chắn muốn xóa khách hàng '{_selectedKhachHang.HoTen}' không?\n\n" +
                              $"LƯU Ý (Hệ thống áp dụng Xóa Mềm):\n" +
                              $"❌ BỊ XÓA (Ẩn đi): Khách hàng này sẽ biến mất khỏi danh sách và không thể đăng nhập hay tạo giao dịch mới.\n" +
                              $"✅ ĐƯỢC GIỮ LẠI: Toàn bộ lịch sử mua hàng, hóa đơn và phiếu thuê sách cũ của khách này vẫn được bảo lưu an toàn trong cơ sở dữ liệu để đảm bảo tính chính xác của báo cáo doanh thu.";

            if (MessageBox.Show(thongBao, "Xác nhận Xóa Khách Hàng", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var response = await httpClient.DeleteAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Đã xóa khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (FindName("formChiTiet") is StackPanel f) f.IsEnabled = false;
                        await LoadKhachHangAsync();
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi xảy ra khi xóa khách hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi API", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allKhachHangList == null || !_allKhachHangList.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Danh Sách Khách Hàng",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"KhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
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
                        var ws = package.Workbook.Worksheets.Add("Danh sách Khách Hàng");

                        ws.Cells["A1"].Value = "DANH SÁCH KHÁCH HÀNG CAFEBOOK";
                        ws.Cells["A1:G1"].Merge = true;
                        ws.Cells["A1"].Style.Font.Size = 16;
                        ws.Cells["A1"].Style.Font.Bold = true;
                        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Row(1).Height = 30;

                        ws.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        ws.Cells["A2:G2"].Merge = true;
                        ws.Cells["A2"].Style.Font.Italic = true;
                        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells["A3"].Value = "Mã KH";
                        ws.Cells["B3"].Value = "Tên Khách Hàng";
                        ws.Cells["C3"].Value = "SĐT";
                        ws.Cells["D3"].Value = "Email";
                        ws.Cells["E3"].Value = "Loại Tài Khoản";
                        ws.Cells["F3"].Value = "Điểm TL";
                        ws.Cells["G3"].Value = "Trạng Thái";

                        int currentRow = 4;
                        foreach (var item in _allKhachHangList)
                        {
                            ws.Cells[$"A{currentRow}"].Value = item.IdKhachHang;
                            ws.Cells[$"B{currentRow}"].Value = item.HoTen;
                            ws.Cells[$"C{currentRow}"].Value = item.SoDienThoai;
                            ws.Cells[$"D{currentRow}"].Value = item.Email;
                            ws.Cells[$"E{currentRow}"].Value = item.LoaiTaiKhoan;
                            ws.Cells[$"F{currentRow}"].Value = item.DiemTichLuy;
                            ws.Cells[$"G{currentRow}"].Value = item.TrangThai;

                            if (item.BiKhoa) ws.Cells[$"G{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            else ws.Cells[$"G{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Green);

                            currentRow++;
                        }

                        var tableRange = ws.Cells[3, 1, currentRow - 1, 7];
                        var table = ws.Tables.Add(tableRange, "TableKhachHang");
                        table.TableStyle = TableStyles.Medium9;
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();

                        package.Save();
                    }

                    var result = MessageBox.Show($"Đã xuất file Excel chuẩn tại:\n{sfd.FileName}\n\n• Chọn [Yes] để mở trực tiếp.\n• Chọn [No] để mở thư mục.", "Hoàn Tất", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    else if (result == MessageBoxResult.No) System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi Excel: " + ex.Message); }
            }
        }

        private void BtnNavKhuyenMai_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("QL_KHUYEN_MAI"))
                this.NavigationService?.Navigate(new QuanLyKhuyenMaiView());
        }
    }
}