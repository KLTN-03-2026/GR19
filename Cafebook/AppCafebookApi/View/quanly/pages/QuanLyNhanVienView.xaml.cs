using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using AppCafebookApi.View.Common;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyNhanVienView : Page
    {
        private static readonly HttpClient httpClient;

        private List<QuanLyNhanVienGridDto> _allNhanVienList = new List<QuanLyNhanVienGridDto>();
        private List<RoleLookupDto> _vaiTroList = new List<RoleLookupDto>();
        private QuanLyNhanVienDetailDto? _selectedNhanVien = null;

        private string? _currentAvatarFilePath = null;
        private bool _deleteAvatarRequest = false;

        static QuanLyNhanVienView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };
        }

        public QuanLyNhanVienView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            AutoWireEvents();
            ApplyPermissions();
            await LoadDanhSachVaiTro();
            await LoadDanhSachNhanVien();

            BtnLamMoiForm_Click(null, null);
        }

        private void AutoWireEvents()
        {
            if (FindName("txtSearchNhanVien") is TextBox txtSearchNV) txtSearchNV.TextChanged += (s, ev) => ApplyFilter();
            if (FindName("cmbFilterVaiTro") is ComboBox cmbFilter) cmbFilter.SelectionChanged += (s, ev) => ApplyFilter();

            if (FindName("dgNhanVien") is DataGrid dg)
            {
                dg.SelectionChanged -= DgNhanVien_SelectionChanged;
                dg.SelectionChanged += DgNhanVien_SelectionChanged;
                dg.LoadingRow -= DgNhanVien_LoadingRow;
                dg.LoadingRow += DgNhanVien_LoadingRow;
            }
        }

        private void UpdateUIState()
        {
            Button? btnThem = FindName("btnThem") as Button;
            Button? btnLuu = FindName("btnLuu") as Button;
            Button? btnXoa = FindName("btnXoa") as Button;
            Button? btnTrangThai = FindName("btnCaiDatTrangThai") as Button;

            if (FindName("formChiTiet") is Grid form) form.IsEnabled = true;

            if (_selectedNhanVien == null)
            {
                if (btnThem != null) btnThem.Visibility = Visibility.Visible;
                if (btnLuu != null) btnLuu.Visibility = Visibility.Collapsed;
                if (btnXoa != null) btnXoa.Visibility = Visibility.Collapsed;
                if (btnTrangThai != null) btnTrangThai.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (btnThem != null) btnThem.Visibility = Visibility.Collapsed;
                if (btnLuu != null) btnLuu.Visibility = Visibility.Visible;
                if (btnXoa != null) btnXoa.Visibility = Visibility.Visible;
                if (btnTrangThai != null) btnTrangThai.Visibility = Visibility.Visible;

                bool isCurrentUser = AuthService.CurrentUser != null && _selectedNhanVien.IdNhanVien == AuthService.CurrentUser.IdNhanVien;

                if (isCurrentUser)
                {
                    if (btnLuu != null) btnLuu.IsEnabled = false;
                    if (btnXoa != null) btnXoa.IsEnabled = false;
                    if (btnTrangThai != null) btnTrangThai.IsEnabled = false;

                    if (FindName("formChiTiet") is Grid currentForm) currentForm.IsEnabled = false;
                }
                else
                {
                    if (btnLuu != null) btnLuu.IsEnabled = true;
                    if (btnXoa != null) btnXoa.IsEnabled = true;
                    if (btnTrangThai != null) btnTrangThai.IsEnabled = true;
                }
            }
        }

        private void DgNhanVien_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is QuanLyNhanVienGridDto rowData && AuthService.CurrentUser != null)
            {
                if (rowData.IdNhanVien == AuthService.CurrentUser.IdNhanVien)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                }
                else
                {
                    e.Row.FontWeight = FontWeights.Normal;
                    e.Row.ClearValue(Control.BackgroundProperty);
                }
            }
        }

        private void ApplyPermissions()
        {
            if (AuthService.CoQuyen("FULL_QL")) return;

            if (FindName("btnPhanQuyen") is Button btnPhanQuyen) btnPhanQuyen.Visibility = AuthService.CoQuyen("QL_PHAN_QUYEN") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnGoToDonXinNghi") is Button btnNghi) btnNghi.Visibility = AuthService.CoQuyen("QL_DON_XIN_NGHI") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnGoToLichLamViec") is Button btnLich) btnLich.Visibility = AuthService.CoQuyen("QL_LICH_LAM_VIEC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnGoToBaoCao") is Button btnBaoCao) btnBaoCao.Visibility = AuthService.CoQuyen("QL_BAO_CAO_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnGoToCaiDat") is Button btnCaiDat) btnCaiDat.Visibility = AuthService.CoQuyen("QL_CAI_DAT_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDanhSachVaiTro()
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<RoleLookupDto>>("api/app/quanly-nhanvien/roles-lookup");
                if (response != null)
                {
                    _vaiTroList = response;
                    if (FindName("cmbVaiTro") is ComboBox cmbVaiTro)
                    {
                        cmbVaiTro.ItemsSource = _vaiTroList;
                        cmbVaiTro.DisplayMemberPath = "Name";
                        cmbVaiTro.SelectedValuePath = "Id";
                    }

                    if (FindName("cmbFilterVaiTro") is ComboBox cmbFilterVaiTro)
                    {
                        var filterList = new List<RoleLookupDto> { new RoleLookupDto { Id = 0, Name = "Tất cả" } };
                        filterList.AddRange(_vaiTroList);
                        cmbFilterVaiTro.ItemsSource = filterList;
                        cmbFilterVaiTro.DisplayMemberPath = "Name";
                        cmbFilterVaiTro.SelectedValuePath = "Id";
                        cmbFilterVaiTro.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private async Task LoadDanhSachNhanVien()
        {
            try
            {
                if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
                var response = await httpClient.GetFromJsonAsync<List<QuanLyNhanVienGridDto>>("api/app/quanly-nhanvien");
                if (response != null)
                {
                    _allNhanVienList = response;
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách: {ex.Message}");
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Collapsed;
            }
        }

        private void Filter_Changed(object sender, TextChangedEventArgs e) { ApplyFilter(); }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { ApplyFilter(); }

        private void ApplyFilter()
        {
            if (_allNhanVienList == null) return;

            var query = _allNhanVienList.AsEnumerable();
            string keyword = "";

            if (FindName("txtSearchNhanVien") is TextBox t1) keyword = t1.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    (x.HoTen != null && x.HoTen.ToLower().Contains(keyword)) ||
                    (x.TenDangNhap != null && x.TenDangNhap.ToLower().Contains(keyword)));
            }

            if (FindName("cmbFilterVaiTro") is ComboBox cmbFilter && cmbFilter.SelectedValue != null && (int)cmbFilter.SelectedValue > 0)
            {
                var selectedRoleName = (cmbFilter.SelectedItem as RoleLookupDto)?.Name;
                query = query.Where(x => x.TenVaiTro == selectedRoleName);
            }

            if (FindName("dgNhanVien") is DataGrid dg) dg.ItemsSource = query.ToList();
        }

        private async void DgNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dg && dg.SelectedItem is QuanLyNhanVienGridDto selectedGridItem)
            {
                try
                {
                    if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
                    var detail = await httpClient.GetFromJsonAsync<QuanLyNhanVienDetailDto>($"api/app/quanly-nhanvien/{selectedGridItem.IdNhanVien}");

                    if (detail != null)
                    {
                        _selectedNhanVien = detail;

                        if (FindName("txtHoTen") is TextBox txtHoTen) txtHoTen.Text = detail.HoTen;
                        if (FindName("txtTenDangNhap") is TextBox txtTDN) txtTDN.Text = detail.TenDangNhap;
                        if (FindName("cmbVaiTro") is ComboBox cmbVT) cmbVT.SelectedValue = detail.IdVaiTro;
                        if (FindName("txtLuongCoBan") is TextBox txtLCB) txtLCB.Text = detail.LuongCoBan.ToString("0.##");
                        if (FindName("txtSoDienThoai") is TextBox txtSDT) txtSDT.Text = detail.SoDienThoai;
                        if (FindName("txtEmail") is TextBox txtEmail) txtEmail.Text = detail.Email;
                        if (FindName("txtDiaChi") is TextBox txtDiaChi) txtDiaChi.Text = detail.DiaChi;
                        if (FindName("dpNgayVaoLam") is DatePicker dp) dp.SelectedDate = detail.NgayVaoLam;
                        if (FindName("cmbTrangThai") is ComboBox cbTrangThai) cbTrangThai.Text = detail.TrangThaiLamViec;

                        if (FindName("txtMatKhau") is PasswordBox pwdMatKhau) pwdMatKhau.Password = "";

                        if (FindName("lblMatKhauInfo") is TextBlock lblMkInfo) lblMkInfo.Visibility = Visibility.Visible;

                        _currentAvatarFilePath = null;
                        _deleteAvatarRequest = false;
                        string avatarUrl = string.IsNullOrEmpty(detail.AnhDaiDienUrl) ? "" : AppConfigManager.GetApiServerUrl() + detail.AnhDaiDienUrl;

                        var imgSrc = HinhAnhHelper.LoadImage(avatarUrl, HinhAnhPaths.DefaultAvatar);
                        if (FindName("AvatarPreview") is ImageBrush i1) i1.ImageSource = imgSrc;

                        UpdateUIState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết: {ex.Message}");
                }
                finally
                {
                    if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnLamMoiForm_Click(object? sender, RoutedEventArgs? e)
        {
            _selectedNhanVien = null;
            if (FindName("dgNhanVien") is DataGrid dg) dg.SelectedItem = null;

            if (FindName("txtHoTen") is TextBox txtHoTen) txtHoTen.Clear();
            if (FindName("txtTenDangNhap") is TextBox txtTDN) txtTDN.Clear();
            if (FindName("txtLuongCoBan") is TextBox txtLCB) txtLCB.Clear();
            if (FindName("txtSoDienThoai") is TextBox txtSDT) txtSDT.Clear();
            if (FindName("txtEmail") is TextBox txtEmail) txtEmail.Clear();
            if (FindName("txtDiaChi") is TextBox txtDiaChi) txtDiaChi.Clear();
            if (FindName("cmbVaiTro") is ComboBox cmbVT) cmbVT.SelectedIndex = -1;
            if (FindName("dpNgayVaoLam") is DatePicker dp) dp.SelectedDate = DateTime.Now;
            if (FindName("cmbTrangThai") is ComboBox cbTrangThai) cbTrangThai.SelectedIndex = 0;

            if (FindName("txtMatKhau") is PasswordBox pwdMatKhau) pwdMatKhau.Password = "";
            if (FindName("lblMatKhauInfo") is TextBlock lblMkInfo) lblMkInfo.Visibility = Visibility.Collapsed;

            _currentAvatarFilePath = null;
            _deleteAvatarRequest = false;

            var imgSrc = HinhAnhHelper.LoadImage("", HinhAnhPaths.DefaultAvatar);
            if (FindName("AvatarPreview") is ImageBrush i1) i1.ImageSource = imgSrc;

            UpdateUIState();
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentAvatarFilePath = openFileDialog.FileName;
                _deleteAvatarRequest = false;

                var imgSrc = new BitmapImage(new Uri(_currentAvatarFilePath));
                if (FindName("AvatarPreview") is ImageBrush i1) i1.ImageSource = imgSrc;
            }
        }

        private void BtnXoaAnh_Click(object sender, RoutedEventArgs e)
        {
            _currentAvatarFilePath = null;
            _deleteAvatarRequest = true;

            var imgSrc = HinhAnhHelper.LoadImage("", HinhAnhPaths.DefaultAvatar);
            if (FindName("AvatarPreview") is ImageBrush i1) i1.ImageSource = imgSrc;
        }

        private async Task SendSaveRequest(bool isUpdate)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_NHAN_VIEN"))
            {
                MessageBox.Show("Bạn không có quyền thực hiện chức năng này!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- BẮT ĐẦU: KIỂM TRA BẮT BUỘC TRÊN GIAO DIỆN ---
            TextBox? txtHoTen = FindName("txtHoTen") as TextBox;
            TextBox? txtTDN = FindName("txtTenDangNhap") as TextBox;
            TextBox? txtSDT = FindName("txtSoDienThoai") as TextBox;
            TextBox? txtEmail = FindName("txtEmail") as TextBox;
            TextBox? txtDiaChi = FindName("txtDiaChi") as TextBox;
            PasswordBox? pwdMatKhau = FindName("txtMatKhau") as PasswordBox;
            ComboBox? cmbVaiTro = FindName("cmbVaiTro") as ComboBox;

            if (txtHoTen == null || string.IsNullOrWhiteSpace(txtHoTen.Text) ||
                txtTDN == null || string.IsNullOrWhiteSpace(txtTDN.Text) ||
                txtSDT == null || string.IsNullOrWhiteSpace(txtSDT.Text) ||
                txtEmail == null || string.IsNullOrWhiteSpace(txtEmail.Text) ||
                txtDiaChi == null || string.IsNullOrWhiteSpace(txtDiaChi.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tất cả thông tin (Họ tên, Tên đăng nhập, Số điện thoại, Email, Địa chỉ)!", "Nhắc nhở nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!isUpdate && (pwdMatKhau == null || string.IsNullOrWhiteSpace(pwdMatKhau.Password)))
            {
                MessageBox.Show("Mật khẩu là bắt buộc khi tạo nhân viên mới.", "Nhắc nhở nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbVaiTro == null || cmbVaiTro.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng phân công Vai trò cho nhân viên.", "Nhắc nhở nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --- KẾT THÚC: KIỂM TRA BẮT BUỘC ---

            try
            {
                if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;

                using var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(txtHoTen.Text.Trim()), "HoTen");
                formData.Add(new StringContent(txtTDN.Text.Trim()), "TenDangNhap");

                string pass = pwdMatKhau?.Password ?? "";
                if (!string.IsNullOrEmpty(pass)) formData.Add(new StringContent(pass), "MatKhau");

                formData.Add(new StringContent(cmbVaiTro.SelectedValue.ToString()!), "IdVaiTro");

                if (FindName("txtLuongCoBan") is TextBox txtLCB)
                    formData.Add(new StringContent(string.IsNullOrWhiteSpace(txtLCB.Text) ? "0" : txtLCB.Text.Trim()), "LuongCoBan");

                if (FindName("cmbTrangThai") is ComboBox cbTrangThai && cbTrangThai.SelectedItem is ComboBoxItem cbi)
                    formData.Add(new StringContent(cbi.Content.ToString()!), "TrangThaiLamViec");

                formData.Add(new StringContent(txtSDT.Text.Trim()), "SoDienThoai");
                formData.Add(new StringContent(txtEmail.Text.Trim()), "Email");
                formData.Add(new StringContent(txtDiaChi.Text.Trim()), "DiaChi");

                if (FindName("dpNgayVaoLam") is DatePicker dp && dp.SelectedDate.HasValue)
                    formData.Add(new StringContent(dp.SelectedDate.Value.ToString("yyyy-MM-dd")), "NgayVaoLam");

                formData.Add(new StringContent(_deleteAvatarRequest.ToString()), "XoaAnhDaiDien");

                if (!string.IsNullOrEmpty(_currentAvatarFilePath))
                {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(_currentAvatarFilePath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    formData.Add(fileContent, "AnhDaiDienUpload", Path.GetFileName(_currentAvatarFilePath));
                }

                HttpResponseMessage response;
                if (isUpdate && _selectedNhanVien != null)
                {
                    response = await httpClient.PutAsync($"api/app/quanly-nhanvien/{_selectedNhanVien.IdNhanVien}", formData);
                }
                else
                {
                    response = await httpClient.PostAsync("api/app/quanly-nhanvien", formData);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnLamMoiForm_Click(null, null);
                    await LoadDanhSachNhanVien();
                }
                else
                {
                    MessageBox.Show($"Lỗi từ Server: {await response.Content.ReadAsStringAsync()}", "Bị Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            await SendSaveRequest(isUpdate: false);
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhanVien == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên từ danh sách để sửa!", "Nhắc nhở");
                return;
            }

            if (AuthService.CurrentUser != null && _selectedNhanVien.IdNhanVien == AuthService.CurrentUser.IdNhanVien)
            {
                MessageBox.Show("Bạn không thể tự sửa thông tin tài khoản đang đăng nhập tại đây!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await SendSaveRequest(isUpdate: true);
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_NHAN_VIEN")) return;
            if (_selectedNhanVien == null) return;

            if (AuthService.CurrentUser != null && _selectedNhanVien.IdNhanVien == AuthService.CurrentUser.IdNhanVien)
            {
                MessageBox.Show("Bạn không thể xóa tài khoản đang đăng nhập!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Xác nhận xóa nhân viên: {_selectedNhanVien.HoTen}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var response = await httpClient.DeleteAsync($"api/app/quanly-nhanvien/{_selectedNhanVien.IdNhanVien}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Xóa thành công!");
                    BtnLamMoiForm_Click(null, null);
                    await LoadDanhSachNhanVien();
                }
                else
                {
                    MessageBox.Show($"Không thể xóa: {await response.Content.ReadAsStringAsync()}", "Lỗi");
                }
            }
        }

        // ========================================================================
        // Navigation Chuyển trang Menu
        // ========================================================================
        private void BtnPhanQuyen_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_PHAN_QUYEN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Phân Quyền!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.NavigationService?.Navigate(new QuanLyPhanQuyenView());
        }

        private void BtnGoToBaoCao_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new BaoCaoNhanSuView());
        }

        private void BtnGoToLichLamViec_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new QuanLyLichLamViecView());
        }

        private void BtnGoToDonXinNghi_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new QuanLyDonXinNghiView());
        }

        private void BtnGoToCaiDat_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new CaiDatNhanSuView());
        }

        private void BtnGoToHieuSuat_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}