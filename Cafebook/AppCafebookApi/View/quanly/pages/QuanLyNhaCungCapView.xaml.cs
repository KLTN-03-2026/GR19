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
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;
using OfficeOpenXml.Style;


namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyNhaCungCapView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyNhaCungCapGridDto> _dataList = new();
        private QuanLyNhaCungCapGridDto? _selectedItem;
        private bool _isAdding = false;

        static QuanLyNhaCungCapView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyNhaCungCapView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("QL_NHA_CUNG_CAP")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_NHA_CUNG_CAP");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is System.Windows.Controls.Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await httpClient.GetFromJsonAsync<List<QuanLyNhaCungCapGridDto>>("api/app/quanly-nhacungcap");
                if (res != null) { _dataList = res; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgNhaCungCap") is DataGrid dg)) return;
            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            dg.ItemsSource = string.IsNullOrEmpty(k) ? _dataList : _dataList.Where(x => x.TenNhaCungCap.ToLower().Contains(k) || (x.SoDienThoai != null && x.SoDienThoai.Contains(k))).ToList();
        }

        private void DgNhaCungCap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgNhaCungCap") is DataGrid dg && dg.SelectedItem is QuanLyNhaCungCapGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Nhà Cung Cấp";

                if (FindName("txtTenNhaCungCap") is TextBox t1) t1.Text = item.TenNhaCungCap;
                if (FindName("txtSoDienThoai") is TextBox t2) t2.Text = item.SoDienThoai;
                if (FindName("txtEmail") is TextBox t3) t3.Text = item.Email;
                if (FindName("txtDiaChi") is TextBox t4) t4.Text = item.DiaChi;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NHA_CUNG_CAP")) return;
            _selectedItem = new QuanLyNhaCungCapGridDto(); _isAdding = true;
            if (FindName("dgNhaCungCap") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm Nhà Cung Cấp Mới";

            if (FindName("txtTenNhaCungCap") is TextBox t1) t1.Text = "";
            if (FindName("txtSoDienThoai") is TextBox t2) t2.Text = "";
            if (FindName("txtEmail") is TextBox t3) t3.Text = "";
            if (FindName("txtDiaChi") is TextBox t4) t4.Text = "";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NHA_CUNG_CAP") || _selectedItem == null) return;

            string ten = (FindName("txtTenNhaCungCap") as TextBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(ten)) { MessageBox.Show("Nhập tên Nhà cung cấp!"); return; }

            var dto = new QuanLyNhaCungCapSaveDto
            {
                TenNhaCungCap = ten,
                SoDienThoai = (FindName("txtSoDienThoai") as TextBox)?.Text,
                Email = (FindName("txtEmail") as TextBox)?.Text,
                DiaChi = (FindName("txtDiaChi") as TextBox)?.Text
            };

            if (FindName("LoadingOverlay") is System.Windows.Controls.Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = _isAdding ? await httpClient.PostAsJsonAsync("api/app/quanly-nhacungcap", dto) : await httpClient.PutAsJsonAsync($"api/app/quanly-nhacungcap/{_selectedItem.IdNhaCungCap}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NHA_CUNG_CAP") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa Nhà cung cấp này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-nhacungcap/{_selectedItem.IdNhaCungCap}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công!"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_dataList == null || !_dataList.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Danh Sách Nhà Cung Cấp",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"DanhSachNhaCungCap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // Thiết lập License cho EPPlus 8 (Sử dụng phi thương mại cá nhân)
                    ExcelPackage.License.SetNonCommercialPersonal("Cafebook Admin");

                    FileInfo fileInfo = new FileInfo(sfd.FileName);

                    // Xóa file cũ nếu đã tồn tại để ghi đè
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        // Tạo một Worksheet mới
                        var ws = package.Workbook.Worksheets.Add("Danh sách NCC");

                        // 1. Tạo Tiêu đề lớn (Header)
                        ws.Cells["A1"].Value = "DANH SÁCH NHÀ CUNG CẤP CAFEBOOK";
                        ws.Cells["A1:E1"].Merge = true; // Gộp ô từ A1 đến E1
                        ws.Cells["A1"].Style.Font.Size = 16;
                        ws.Cells["A1"].Style.Font.Bold = true;
                        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Row(1).Height = 30;

                        // 2. Tạo dòng mô tả ngày xuất
                        ws.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        ws.Cells["A2:E2"].Merge = true;
                        ws.Cells["A2"].Style.Font.Italic = true;
                        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        // 3. Đổ dữ liệu Header của bảng (Bắt đầu từ dòng 3)
                        ws.Cells["A3"].Value = "Mã NCC";
                        ws.Cells["B3"].Value = "Tên Nhà Cung Cấp";
                        ws.Cells["C3"].Value = "Số Điện Thoại";
                        ws.Cells["D3"].Value = "Email";
                        ws.Cells["E3"].Value = "Địa Chỉ";

                        // 4. Đổ dữ liệu các dòng
                        int rowStart = 4;
                        int currentRow = rowStart;

                        foreach (var item in _dataList)
                        {
                            ws.Cells[$"A{currentRow}"].Value = item.IdNhaCungCap;
                            ws.Cells[$"B{currentRow}"].Value = item.TenNhaCungCap;
                            ws.Cells[$"C{currentRow}"].Value = item.SoDienThoai;
                            ws.Cells[$"D{currentRow}"].Value = item.Email;
                            ws.Cells[$"E{currentRow}"].Value = item.DiaChi;

                            currentRow++;
                        }

                        // 5. Định dạng toàn bộ vùng dữ liệu thành Excel Table (Có sẵn Filter và Style màu xen kẽ)
                        var tableRange = ws.Cells[3, 1, currentRow - 1, 5]; // Từ dòng 3, cột 1 đến dòng cuối, cột 5
                        var table = ws.Tables.Add(tableRange, "TableNCC");
                        table.TableStyle = TableStyles.Medium9; // Theme màu xanh biển nhạt chuẩn của Excel

                        // 6. Căn chỉnh độ rộng cột tự động (AutoFit)
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();

                        // Lưu file
                        package.Save();
                    }

                    // Hộp thoại điều hướng mở file thông minh
                    string msg = $"Đã xuất file Excel chuẩn tại:\n{sfd.FileName}\n\n" +
                                 $"• Chọn [Yes] để mở trực tiếp bảng tính.\n" +
                                 $"• Chọn [No] để mở thư mục chứa file.\n" +
                                 $"• Chọn [Cancel] để đóng.";

                    var result = MessageBox.Show(msg, "Xuất Excel Hoàn Tất", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tạo file Excel: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}