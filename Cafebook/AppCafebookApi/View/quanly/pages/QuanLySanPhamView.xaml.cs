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
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLySanPhamView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLySanPhamGridDto> _dataList = new();
        private List<LookupDanhMucDto> _danhMucList = new();
        private QuanLySanPhamDetailDto? _selectedItem;
        private string? _currentImgPath = null;
        private bool _deleteImgRequest = false;

        static QuanLySanPhamView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLySanPhamView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // 1. CHÌA KHÓA CỔNG
            bool hasAccess = AuthService.CoQuyen("FULL_QL", "QL_SAN_PHAM", "QL_DANH_MUC", "QL_DINH_LUONG");
            if (!hasAccess)
            {
                MessageBox.Show("Từ chối truy cập phân hệ Sản phẩm!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            // 2. CHÌA KHÓA PHÒNG
            if (AuthService.CoQuyen("FULL_QL", "QL_SAN_PHAM"))
            {
                if (FindName("GridDuLieuSanPham") is System.Windows.Controls.Grid g) g.Visibility = Visibility.Visible;
                if (FindName("txtThongBaoKhongCoQuyen") is System.Windows.Controls.Border b) b.Visibility = Visibility.Collapsed;
                await LoadDataAsync();
            }
            else
            {
                if (FindName("GridDuLieuSanPham") is System.Windows.Controls.Grid g) g.Visibility = Visibility.Collapsed;
                if (FindName("txtThongBaoKhongCoQuyen") is System.Windows.Controls.Border b) b.Visibility = Visibility.Visible;
            }
        }

        private void ApplyPermissions()
        {
            // Nút điều hướng Menu Header
            if (FindName("btnNavDanhMuc") is Button b4) b4.Visibility = AuthService.CoQuyen("FULL_QL", "QL_DANH_MUC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavDinhLuong") is Button b5) b5.Visibility = AuthService.CoQuyen("FULL_QL", "QL_DINH_LUONG") ? Visibility.Visible : Visibility.Collapsed;

            // Quyền thao tác và Xuất Excel
            bool canEdit = AuthService.CoQuyen("FULL_QL", "QL_SAN_PHAM");
            if (FindName("btnXuatExcel") is Button btnXuat) btnXuat.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is System.Windows.Controls.Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var dms = await httpClient.GetFromJsonAsync<List<LookupDanhMucDto>>("api/app/quanly-sanpham/lookup-danhmuc");
                if (dms != null)
                {
                    _danhMucList = dms;
                    // Nạp danh mục cho Form Nhập liệu + Thiết lập filter tìm kiếm
                    if (FindName("cmbDanhMuc") is ComboBox cb1)
                    {
                        cb1.ItemsSource = _danhMucList;
                        ICollectionView view1 = CollectionViewSource.GetDefaultView(cb1.ItemsSource);
                        view1.Filter = item => string.IsNullOrEmpty(cb1.Text) || ((LookupDanhMucDto)item).Ten.IndexOf(cb1.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    // Nạp danh mục cho Cột Lọc bên ngoài + Thiết lập filter tìm kiếm
                    if (FindName("cmbFilterDanhMuc") is ComboBox cb2)
                    {
                        var flt = new List<LookupDanhMucDto> { new LookupDanhMucDto { Id = 0, Ten = "Tất cả" } };
                        flt.AddRange(dms);
                        cb2.ItemsSource = flt;
                        cb2.SelectedIndex = 0;
                        ICollectionView view2 = CollectionViewSource.GetDefaultView(cb2.ItemsSource);
                        view2.Filter = item => string.IsNullOrEmpty(cb2.Text) || ((LookupDanhMucDto)item).Ten.IndexOf(cb2.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
                var sps = await httpClient.GetFromJsonAsync<List<QuanLySanPhamGridDto>>("api/app/quanly-sanpham");
                if (sps != null) { _dataList = sps; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        // ===============================================
        // TÍNH NĂNG TÌM KIẾM ĐỀ XUẤT CHO COMBOBOX
        // ===============================================
        private void CmbDanhMuc_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is ComboBox cmb)
            {
                CollectionViewSource.GetDefaultView(cmb.ItemsSource).Refresh();
                cmb.IsDropDownOpen = true;
            }
        }

        private void CmbFilterDanhMuc_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is ComboBox cmb)
            {
                CollectionViewSource.GetDefaultView(cmb.ItemsSource).Refresh();
                cmb.IsDropDownOpen = true;
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgSanPham") is DataGrid dg)) return;
            var q = _dataList.AsEnumerable();

            // Fix lấy Text thay vì SelectedValue nếu người dùng gõ
            if (FindName("cmbFilterDanhMuc") is ComboBox cb && !string.IsNullOrEmpty(cb.Text) && cb.Text != "Tất cả")
                q = q.Where(x => x.TenDanhMuc.IndexOf(cb.Text, StringComparison.OrdinalIgnoreCase) >= 0);

            if (FindName("txtSearchSanPham") is TextBox t && !string.IsNullOrEmpty(t.Text))
                q = q.Where(x => x.TenSanPham.ToLower().Contains(t.Text.ToLower()));

            dg.ItemsSource = q.ToList();
        }

        private async void DgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgSanPham") is DataGrid dg && dg.SelectedItem is QuanLySanPhamGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                _selectedItem = await httpClient.GetFromJsonAsync<QuanLySanPhamDetailDto>($"api/app/quanly-sanpham/{item.IdSanPham}");
                if (_selectedItem != null)
                {
                    if (FindName("txtTenSanPham") is TextBox t1) t1.Text = _selectedItem.TenSanPham;
                    if (FindName("txtDonGia") is TextBox t2) t2.Text = _selectedItem.GiaBan.ToString();
                    if (FindName("cmbDanhMuc") is ComboBox c1) c1.SelectedValue = _selectedItem.IdDanhMuc;
                    if (FindName("cmbNhomIn") is ComboBox c2) c2.Text = _selectedItem.NhomIn;
                    if (FindName("cmbTrangThai") is ComboBox c3) c3.SelectedIndex = _selectedItem.TrangThaiKinhDoanh ? 0 : 1;
                    if (FindName("txtMoTa") is TextBox t3) t3.Text = _selectedItem.MoTa;

                    _currentImgPath = null; _deleteImgRequest = false;
                    string url = string.IsNullOrEmpty(_selectedItem.HinhAnh) ? "" : (AppConfigManager.GetApiServerUrl() + _selectedItem.HinhAnh);
                    if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage(url, HinhAnhPaths.DefaultFoodIcon);
                }
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM")) return;
            _selectedItem = new QuanLySanPhamDetailDto();
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("dgSanPham") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("txtTenSanPham") is TextBox t1) t1.Text = "";
            if (FindName("txtDonGia") is TextBox t2) t2.Text = "0";
            if (FindName("txtMoTa") is TextBox t3) t3.Text = "";
            if (FindName("cmbDanhMuc") is ComboBox c1) c1.SelectedItem = null;
            if (FindName("cmbTrangThai") is ComboBox c3) c3.SelectedIndex = 0;
            _currentImgPath = null; _deleteImgRequest = false;
            if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage("", HinhAnhPaths.DefaultFoodIcon);
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e) { var op = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp" }; if (op.ShowDialog() == true) { _currentImgPath = op.FileName; _deleteImgRequest = false; if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage(_currentImgPath, HinhAnhPaths.DefaultFoodIcon); } }
        private void BtnXoaAnh_Click(object sender, RoutedEventArgs e) { _currentImgPath = null; _deleteImgRequest = true; if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage("", HinhAnhPaths.DefaultFoodIcon); }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM") || _selectedItem == null) return;
            string ten = (FindName("txtTenSanPham") as TextBox)?.Text.Trim() ?? "";
            int idDm = (FindName("cmbDanhMuc") as ComboBox)?.SelectedValue as int? ?? 0;
            if (string.IsNullOrEmpty(ten) || idDm == 0) { MessageBox.Show("Nhập Tên SP và Danh mục!"); return; }
            decimal.TryParse((FindName("txtDonGia") as TextBox)?.Text, out decimal gia);
            bool isSelling = (FindName("cmbTrangThai") as ComboBox)?.SelectedIndex == 0;

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(ten), "TenSanPham"); content.Add(new StringContent(gia.ToString()), "GiaBan");
            content.Add(new StringContent(idDm.ToString()), "IdDanhMuc"); content.Add(new StringContent((FindName("cmbNhomIn") as ComboBox)?.Text ?? "Khác"), "NhomIn");
            content.Add(new StringContent(isSelling.ToString()), "TrangThaiKinhDoanh"); content.Add(new StringContent((FindName("txtMoTa") as TextBox)?.Text ?? ""), "MoTa");
            content.Add(new StringContent(_deleteImgRequest.ToString()), "DeleteImage");
            if (!string.IsNullOrEmpty(_currentImgPath)) { var fileContent = new ByteArrayContent(File.ReadAllBytes(_currentImgPath)); fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); content.Add(fileContent, "AnhBia", Path.GetFileName(_currentImgPath)); }

            if (FindName("LoadingOverlay") is System.Windows.Controls.Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var res = _selectedItem.IdSanPham == 0 ? await httpClient.PostAsync("api/app/quanly-sanpham", content) : await httpClient.PutAsync($"api/app/quanly-sanpham/{_selectedItem.IdSanPham}", content);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã lưu!"); await LoadDataAsync(); } else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM") || _selectedItem == null || _selectedItem.IdSanPham == 0) return;
            if (MessageBox.Show("Xóa SP này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-sanpham/{_selectedItem.IdSanPham}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã xóa"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
            }
        }

        // ===============================================
        // TÍNH NĂNG XUẤT EXCEL 
        // ===============================================
        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_dataList == null || !_dataList.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Danh Sách Sản Phẩm",
                Filter = "Excel Workbook|*.xlsx", // Đổi sang đuôi .xlsx
                FileName = $"DanhSachSanPham_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
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
                        var ws = package.Workbook.Worksheets.Add("Danh sách SP");

                        // 1. Tạo Tiêu đề lớn (Header)
                        ws.Cells["A1"].Value = "DANH SÁCH SẢN PHẨM CAFEBOOK";
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
                        ws.Cells["A3"].Value = "ID";
                        ws.Cells["B3"].Value = "Tên Sản Phẩm";
                        ws.Cells["C3"].Value = "Danh Mục";
                        ws.Cells["D3"].Value = "Giá Bán (VNĐ)";
                        ws.Cells["E3"].Value = "Trạng Thái";

                        // 4. Đổ dữ liệu các dòng
                        int rowStart = 4;
                        int currentRow = rowStart;

                        foreach (var item in _dataList)
                        {
                            ws.Cells[$"A{currentRow}"].Value = item.IdSanPham;
                            ws.Cells[$"B{currentRow}"].Value = item.TenSanPham;
                            ws.Cells[$"C{currentRow}"].Value = item.TenDanhMuc;

                            // Gán số thực để Excel hiểu đây là Number, không phải Text
                            ws.Cells[$"D{currentRow}"].Value = item.GiaBan;
                            ws.Cells[$"D{currentRow}"].Style.Numberformat.Format = "#,##0"; // Định dạng có dấu phẩy hàng nghìn

                            ws.Cells[$"E{currentRow}"].Value = item.TrangThaiKinhDoanh ? "Đang bán" : "Tạm ngưng";

                            // Đổi màu chữ trạng thái cho trực quan
                            if (!item.TrangThaiKinhDoanh)
                            {
                                ws.Cells[$"E{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            }

                            currentRow++;
                        }

                        // 5. Định dạng toàn bộ vùng dữ liệu thành Excel Table (Có sẵn Filter và Style màu xen kẽ)
                        var tableRange = ws.Cells[3, 1, currentRow - 1, 5]; // Từ dòng 3, cột 1 đến dòng cuối, cột 5
                        var table = ws.Tables.Add(tableRange, "TableSanPham");
                        table.TableStyle = TableStyles.Medium9; // Theme màu xanh biển nhạt chuẩn của Excel

                        // 6. Căn chỉnh độ rộng cột tự động (AutoFit)
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();

                        // Lưu file
                        package.Save();
                    }

                    // Hộp thoại điều hướng mở file (giữ nguyên trải nghiệm UX cũ)
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

        private void BtnNavDanhMuc_Click(object sender, RoutedEventArgs e) { if (AuthService.CoQuyen("QL_DANH_MUC")) this.NavigationService?.Navigate(new QuanLyDanhMucView()); }
        private void BtnNavDinhLuong_Click(object sender, RoutedEventArgs e) { if (AuthService.CoQuyen("QL_DINH_LUONG")) this.NavigationService?.Navigate(new QuanLyDinhLuongView()); }
    }
}