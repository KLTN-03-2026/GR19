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
        private List<QuanLySanPhamGridDto> _dataList = new();
        private List<LookupDanhMucDto> _danhMucList = new();
        private QuanLySanPhamDetailDto? _selectedItem;
        private string? _currentImgPath = null;
        private bool _deleteImgRequest = false;

        private ICollectionView? _spView;
        private string _currentSearchKey = "";
        private string _currentDanhMuc = "Tất cả";
        private bool _showHidden = false;
        private bool _isDataLoaded = false;
        public QuanLySanPhamView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            bool hasAccess = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_SAN_PHAM", "QL_DANH_MUC", "QL_DINH_LUONG");
            if (!hasAccess)
            {
                MessageBox.Show("Từ chối truy cập phân hệ Sản phẩm!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            await Task.Delay(350);
            if (!this.IsLoaded) return;
            try
            {
                await LoadDataAsync();
                _isDataLoaded = true;
            }
            catch (Exception)
            {
            }
            if (AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_SAN_PHAM"))
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
            if (FindName("btnNavDanhMuc") is Button b4) b4.Visibility = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_DANH_MUC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavDinhLuong") is Button b5) b5.Visibility = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_DINH_LUONG") ? Visibility.Visible : Visibility.Collapsed;

            // Quyền thao tác và Xuất Excel
            bool canEdit = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_SAN_PHAM");
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
                // 1. KIỂM TRA RAM (Hiển thị ngay lập tức)
                if (GlobalDataCache.QL_SanPhamCache != null && GlobalDataCache.QL_DanhMucCache != null)
                {
                    PopulateSanPhamUiFromRam();

                    // 2. Kích hoạt cập nhật ngầm API
                    _ = BackgroundRefreshAsync();
                    return;
                }

                // 3. Dự phòng (Fallback): Nếu RAM trống do lỗi, tải trực tiếp từ API
                await FetchApiAndSetupUI();
            }
            finally
            {
                if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (TÁCH LOGIC ĐỂ DÙNG CHUNG)
        // ==========================================

        private void PopulateSanPhamUiFromRam()
        {
            // Ép kiểu từ QuanLyDanhMucGridDto (trong RAM) sang LookupDanhMucDto
            if (GlobalDataCache.QL_DanhMucCache != null)
            {
                _danhMucList = GlobalDataCache.QL_DanhMucCache.Select(d => new LookupDanhMucDto
                {
                    Id = d.IdDanhMuc,
                    Ten = d.TenDanhMuc
                }).ToList();

                if (FindName("cmbDanhMuc") is ComboBox cb1)
                {
                    cb1.ItemsSource = _danhMucList;
                    ICollectionView view1 = CollectionViewSource.GetDefaultView(cb1.ItemsSource);
                    view1.Filter = item => string.IsNullOrEmpty(cb1.Text) || ((LookupDanhMucDto)item).Ten.IndexOf(cb1.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (FindName("cmbFilterDanhMuc") is ComboBox cb2)
                {
                    var flt = new List<LookupDanhMucDto> { new LookupDanhMucDto { Id = 0, Ten = "Tất cả" } };
                    flt.AddRange(_danhMucList);

                    int? currentFilterId = cb2.SelectedValue as int?;
                    cb2.ItemsSource = flt;
                    cb2.SelectedValue = currentFilterId ?? 0;
                }
            }

            if (GlobalDataCache.QL_SanPhamCache != null)
            {
                _dataList = GlobalDataCache.QL_SanPhamCache;
                foreach (var sp in _dataList) sp.SearchKeyword = RemoveVietnameseSigns(sp.TenSanPham);

                _spView = CollectionViewSource.GetDefaultView(_dataList);
                _spView.Filter = SanPhamFilter;

                if (FindName("dgSanPham") is DataGrid dg) dg.ItemsSource = _spView;

                FilterData();
            }
        }

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                // Bắn 2 luồng API cùng lúc để đồng bộ tốc độ cao
                var tDms = ApiClient.Instance.GetFromJsonAsync<List<LookupDanhMucDto>>("api/app/quanly-sanpham/lookup-danhmuc");
                var tSps = ApiClient.Instance.GetFromJsonAsync<List<QuanLySanPhamGridDto>>("api/app/quanly-sanpham");

                await Task.WhenAll(tDms, tSps);

                var dms = await tDms;
                var sps = await tSps;

                if (dms != null && sps != null)
                {
                    // Cập nhật lại RAM và biến cục bộ
                    GlobalDataCache.QL_SanPhamCache = sps;
                    _danhMucList = dms;
                    _dataList = sps;

                    // Ghi nhớ trạng thái UI hiện tại
                    int? currentFilterId = (FindName("cmbFilterDanhMuc") as ComboBox)?.SelectedValue as int?;
                    int? currentSelectedSpId = null;
                    if (FindName("dgSanPham") is DataGrid currentDg && currentDg.SelectedItem is QuanLySanPhamGridDto selectedItem)
                    {
                        currentSelectedSpId = selectedItem.IdSanPham;
                    }

                    // Setup Danh mục
                    if (FindName("cmbDanhMuc") is ComboBox cb1)
                    {
                        cb1.ItemsSource = _danhMucList;
                        ICollectionView view1 = CollectionViewSource.GetDefaultView(cb1.ItemsSource);
                        view1.Filter = item => string.IsNullOrEmpty(cb1.Text) || ((LookupDanhMucDto)item).Ten.IndexOf(cb1.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    if (FindName("cmbFilterDanhMuc") is ComboBox cb2)
                    {
                        var flt = new List<LookupDanhMucDto> { new LookupDanhMucDto { Id = 0, Ten = "Tất cả" } };
                        flt.AddRange(dms);
                        cb2.ItemsSource = flt;
                        cb2.SelectedValue = currentFilterId ?? 0;
                    }

                    // Setup Sản phẩm
                    foreach (var sp in _dataList) sp.SearchKeyword = RemoveVietnameseSigns(sp.TenSanPham);
                    _spView = CollectionViewSource.GetDefaultView(_dataList);
                    _spView.Filter = SanPhamFilter;

                    if (FindName("dgSanPham") is DataGrid dg)
                    {
                        dg.ItemsSource = _spView;

                        // Phục hồi dòng đang chọn để không bị văng thao tác của quản lý
                        if (currentSelectedSpId.HasValue)
                        {
                            var itemToSelect = _dataList.FirstOrDefault(x => x.IdSanPham == currentSelectedSpId);
                            if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                        }
                    }

                    FilterData();
                }
            }
            catch { /* Lỗi mạng thì bỏ qua, giữ nguyên UI cũ trên RAM */ }
        }

        private async Task FetchApiAndSetupUI()
        {
            var tDms = ApiClient.Instance.GetFromJsonAsync<List<LookupDanhMucDto>>("api/app/quanly-sanpham/lookup-danhmuc");
            var tSps = ApiClient.Instance.GetFromJsonAsync<List<QuanLySanPhamGridDto>>("api/app/quanly-sanpham");

            await Task.WhenAll(tDms, tSps);

            var dms = await tDms;
            var sps = await tSps;

            if (dms != null && sps != null)
            {
                GlobalDataCache.QL_SanPhamCache = sps;
                _danhMucList = dms;
                _dataList = sps;

                if (FindName("cmbDanhMuc") is ComboBox cb1)
                {
                    cb1.ItemsSource = _danhMucList;
                    ICollectionView view1 = CollectionViewSource.GetDefaultView(cb1.ItemsSource);
                    view1.Filter = item => string.IsNullOrEmpty(cb1.Text) || ((LookupDanhMucDto)item).Ten.IndexOf(cb1.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (FindName("cmbFilterDanhMuc") is ComboBox cb2)
                {
                    var flt = new List<LookupDanhMucDto> { new LookupDanhMucDto { Id = 0, Ten = "Tất cả" } };
                    flt.AddRange(dms);
                    cb2.ItemsSource = flt;
                    cb2.SelectedIndex = 0;
                }

                foreach (var sp in _dataList) sp.SearchKeyword = RemoveVietnameseSigns(sp.TenSanPham);
                _spView = CollectionViewSource.GetDefaultView(_dataList);
                _spView.Filter = SanPhamFilter;

                if (FindName("dgSanPham") is DataGrid dg) dg.ItemsSource = _spView;
                FilterData();
            }
        }

        private string RemoveVietnameseSigns(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            str = str.ToLower().Trim();
            string[] vietnameseSigns = new string[] {
                "aAeEoOuUiIdDyY", "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ", "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ", "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ", "íìịỉĩ", "ÍÌỊỈĨ", "đ", "Đ", "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return str;
        }

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

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            if (_spView == null) return;

            if (FindName("cmbFilterDanhMuc") is ComboBox cb && cb.SelectedItem is LookupDanhMucDto selectedDm)
                _currentDanhMuc = selectedDm.Ten;
            else
                _currentDanhMuc = "Tất cả";

            if (FindName("txtSearchSanPham") is TextBox txt)
                _currentSearchKey = string.IsNullOrEmpty(txt.Text) ? "" : RemoveVietnameseSigns(txt.Text);

            if (FindName("chkHienThiDaXoa") is CheckBox chk)
                _showHidden = chk.IsChecked == true;

            _spView.Refresh();
        }

        private bool SanPhamFilter(object obj)
        {
            if (obj is not QuanLySanPhamGridDto sp) return false;

            if (!_showHidden && !sp.TrangThaiKinhDoanh)
                return false;

            if (_currentDanhMuc != "Tất cả" && sp.TenDanhMuc.IndexOf(_currentDanhMuc, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            if (!string.IsNullOrEmpty(_currentSearchKey) && !sp.SearchKeyword.Contains(_currentSearchKey))
                return false;

            return true;
        }

        private async void DgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgSanPham") is DataGrid dg && dg.SelectedItem is QuanLySanPhamGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                _selectedItem = await ApiClient.Instance.GetFromJsonAsync<QuanLySanPhamDetailDto>($"api/app/quanly-sanpham/{item.IdSanPham}");
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
                var res = _selectedItem.IdSanPham == 0 ? await ApiClient.Instance.PostAsync("api/app/quanly-sanpham", content) : await ApiClient.Instance.PutAsync($"api/app/quanly-sanpham/{_selectedItem.IdSanPham}", content);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã lưu!"); await LoadDataAsync(); } else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM") || _selectedItem == null || _selectedItem.IdSanPham == 0) return;
            if (MessageBox.Show($"Bạn chắc chắn muốn xử lý sản phẩm '{_selectedItem.TenSanPham}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is System.Windows.Controls.Border l1) l1.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-sanpham/{_selectedItem.IdSanPham}");
                    string responseMsg = await res.Content.ReadAsStringAsync();

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show(responseMsg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        BtnLamMoiForm_Click(this, new RoutedEventArgs());
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi: {responseMsg}");
                    }
                }
                finally { if (FindName("LoadingOverlay") is System.Windows.Controls.Border l2) l2.Visibility = Visibility.Collapsed; }
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

        private void BtnNavDanhMuc_Click(object sender, RoutedEventArgs e) { 
            if (AuthService.CoQuyen("QL_DANH_MUC")) 
                this.NavigationService?.Navigate(new QuanLyDanhMucView()); 
        }
        private void BtnNavDinhLuong_Click(object sender, RoutedEventArgs e) {
            if (AuthService.CoQuyen("QL_DINH_LUONG")) 
                this.NavigationService?.Navigate(new QuanLyDinhLuongView()); 
        }
    }
}