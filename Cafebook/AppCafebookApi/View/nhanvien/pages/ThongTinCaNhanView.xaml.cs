using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class ThongTinCaNhanView : Page
    {
        //private static readonly HttpClient httpClient;
        private string? _newAvatarFilePath = null;
        /*
        static ThongTinCaNhanView()
        {
            ApiClient.Instance = new ApiClient.Instance();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl)) ApiClient.Instance.BaseAddress = new Uri(apiUrl);
            else ApiClient.Instance.BaseAddress = new Uri("http://127.0.0.1:5166");
        }
        */
        public ThongTinCaNhanView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_NV", "FULL_QL", "NV_THONG_TIN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Thông Tin Cá Nhân.", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack) this.NavigationService.GoBack();
                return;
            }

            ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (FindName("btnLichSu") is ToggleButton btnLichSu)
            {
                btnLichSu.IsChecked = true;
                BtnTab_Click(btnLichSu, new RoutedEventArgs());
            }

            await LoadDataAsync();
            await LoadLeaveHistoryAsync();
        }

        private void BtnTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedBtn)
            {
                if (FindName("btnLichSu") is ToggleButton b1) b1.IsChecked = false;
                if (FindName("btnXinNghi") is ToggleButton b2) b2.IsChecked = false;
                if (FindName("btnChinhSua") is ToggleButton b3) b3.IsChecked = false;
                if (FindName("btnDoiMatKhau") is ToggleButton b4) b4.IsChecked = false;

                clickedBtn.IsChecked = true;

                if (FindName("MainTabControl") is TabControl tab)
                {
                    if (clickedBtn.Name == "btnLichSu") tab.SelectedIndex = 0;
                    else if (clickedBtn.Name == "btnXinNghi") tab.SelectedIndex = 1;
                    else if (clickedBtn.Name == "btnChinhSua") tab.SelectedIndex = 2;
                    else if (clickedBtn.Name == "btnDoiMatKhau") tab.SelectedIndex = 3;
                }
            }
        }

        private async Task LoadDataAsync()
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<ThongTinCaNhanViewDto>($"api/app/nhanvien/thongtincanhan/me/{idNhanVien}");
                if (response == null) return;

                if (FindName("lblHoTen") is TextBlock txtHoTen) txtHoTen.Text = response.NhanVien.HoTen;
                if (FindName("lblVaiTro") is TextBlock txtVaiTro) txtVaiTro.Text = response.NhanVien.TenVaiTro;
                if (FindName("lblSoDienThoai") is TextBlock txtSdtShow) txtSdtShow.Text = response.NhanVien.SoDienThoai;
                if (FindName("lblEmail") is TextBlock txtEmailShow) txtEmailShow.Text = string.IsNullOrEmpty(response.NhanVien.Email) ? "Chưa cập nhật" : response.NhanVien.Email;
                if (FindName("lblDiaChi") is TextBlock txtDiaChiShow) txtDiaChiShow.Text = string.IsNullOrEmpty(response.NhanVien.DiaChi) ? "Chưa cập nhật" : response.NhanVien.DiaChi;

                if (FindName("txtEditHoTen") is TextBox t1) t1.Text = response.NhanVien.HoTen;
                if (FindName("txtEditSoDienThoai") is TextBox t2) t2.Text = response.NhanVien.SoDienThoai;
                if (FindName("txtEditEmail") is TextBox t3) t3.Text = response.NhanVien.Email;
                if (FindName("txtEditDiaChi") is TextBox t4) t4.Text = response.NhanVien.DiaChi;

                if (FindName("lblThongBaoLich") is TextBlock tLich)
                {
                    if (response.LichLamViecHomNay != null)
                    {
                        tLich.Text = $"Hôm nay bạn có ca làm việc: {response.LichLamViecHomNay.TenCa}";
                        if (FindName("lblThoiGianCa") is TextBlock tTime) tTime.Text = $"⏰ {response.LichLamViecHomNay.GioBatDau:hh\\:mm} - {response.LichLamViecHomNay.GioKetThuc:hh\\:mm}";
                    }
                    else
                    {
                        tLich.Text = "Hôm nay bạn không có lịch làm việc.";
                        if (FindName("lblThoiGianCa") is TextBlock tTime) tTime.Text = "";
                    }
                }

                if (FindName("lblSoLanNghi") is TextBlock txtNghi) txtNghi.Text = response.SoLanXinNghiThangNay.ToString();

                if (FindName("imgAvatar") is System.Windows.Shapes.Ellipse ellipseAvatar && ellipseAvatar.Fill is ImageBrush imgBrush)
                {
                    string baseUrl = AppConfigManager.GetApiServerUrl() ?? "http://127.0.0.1:5166";
                    string fullImgUrl = string.IsNullOrEmpty(response.NhanVien.AnhDaiDien)
                                        ? ""
                                        : $"{baseUrl}{response.NhanVien.AnhDaiDien}";

                    imgBrush.ImageSource = HinhAnhHelper.LoadImage(fullImgUrl, HinhAnhPaths.DefaultAvatar);
                }

                if (FindName("dgLichLamViec") is DataGrid dg) dg.ItemsSource = response.LichLamViecThangNay;
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}"); }
        }

        private async Task LoadLeaveHistoryAsync()
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            try
            {
                var history = await ApiClient.Instance.GetFromJsonAsync<DonXinNghiDto[]>($"api/app/nhanvien/thongtincanhan/leave-history/{idNhanVien}");
                if (FindName("dgLichSuNghi") is DataGrid dg) dg.ItemsSource = history;
            }
            catch { }
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _newAvatarFilePath = openFileDialog.FileName;
                if (FindName("imgAvatar") is System.Windows.Shapes.Ellipse ellipseAvatar && ellipseAvatar.Fill is ImageBrush imgBrush)
                {
                    imgBrush.ImageSource = HinhAnhHelper.LoadImage(_newAvatarFilePath, HinhAnhPaths.DefaultAvatar);
                }
            }
        }

        private async void BtnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CurrentUser == null) { MessageBox.Show("Lỗi phiên đăng nhập."); return; }
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            if (FindName("btnLuuThongTin") is Button btnLuuStart) btnLuuStart.IsEnabled = false;

            try
            {
                if (!string.IsNullOrEmpty(_newAvatarFilePath))
                {
                    using var content = new MultipartFormDataContent();
                    using var fileStream = new FileStream(_newAvatarFilePath, FileMode.Open, FileAccess.Read);
                    using var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    content.Add(streamContent, "avatarFile", Path.GetFileName(_newAvatarFilePath));

                    var resAvt = await ApiClient.Instance.PostAsync($"api/app/nhanvien/thongtincanhan/upload-avatar/{idNhanVien}", content);
                    if (!resAvt.IsSuccessStatusCode) MessageBox.Show("Lỗi khi tải ảnh lên.");
                }

                string name = (FindName("txtEditHoTen") as TextBox)?.Text ?? "";
                string sdt = (FindName("txtEditSoDienThoai") as TextBox)?.Text ?? "";
                string email = (FindName("txtEditEmail") as TextBox)?.Text ?? "";
                string diaChi = (FindName("txtEditDiaChi") as TextBox)?.Text ?? "";

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sdt))
                {
                    MessageBox.Show("Tên và SĐT không được để trống.", "Lỗi");
                    return;
                }

                var req = new CapNhatThongTinDto { HoTen = name, SoDienThoai = sdt, Email = email, DiaChi = diaChi };
                var res = await ApiClient.Instance.PutAsJsonAsync($"api/app/nhanvien/thongtincanhan/update-info/{idNhanVien}", req);

                if (res.IsSuccessStatusCode) MessageBox.Show("Lưu thông tin thành công!");
                else MessageBox.Show("Lưu thông tin thất bại.");

                _newAvatarFilePath = null;
                await LoadDataAsync();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { if (FindName("btnLuuThongTin") is Button btnLuuEnd) btnLuuEnd.IsEnabled = true; }
        }

        private async void BtnGuiDon_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CurrentUser == null) { MessageBox.Show("Lỗi phiên đăng nhập."); return; }
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            string loaiDon = (FindName("cmbLoaiDon") as ComboBox)?.Text ?? "";

            string lyDo = (FindName("txtLyDo") as TextBox)?.Text ?? "";
            DateTime? from = (FindName("dpNgayBatDau") as DatePicker)?.SelectedDate;
            DateTime? to = (FindName("dpNgayKetThuc") as DatePicker)?.SelectedDate;

            if (string.IsNullOrWhiteSpace(loaiDon) || string.IsNullOrWhiteSpace(lyDo) || !from.HasValue || !to.HasValue)
            {
                MessageBox.Show("Vui lòng điền đủ loại đơn, ngày và lý do xin nghỉ.");
                return;
            }

            if (from.Value < DateTime.Today) { MessageBox.Show("Không thể xin nghỉ ngày trong quá khứ."); return; }
            if (to.Value < from.Value) { MessageBox.Show("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu."); return; }

            var req = new DonXinNghiRequestDto { LoaiDon = loaiDon, LyDo = lyDo, NgayBatDau = from.Value, NgayKetThuc = to.Value };

            if (FindName("btnGuiDon") is Button btnGuiStart) btnGuiStart.IsEnabled = false;
            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync($"api/app/nhanvien/thongtincanhan/submit-leave/{idNhanVien}", req);

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Nộp đơn thành công. Đang chờ quản lý duyệt.", "Thành công");
                    if (FindName("txtLyDo") is TextBox t) t.Text = "";
                    await LoadLeaveHistoryAsync();
                }
                else
                {
                    string errorMsg = await res.Content.ReadAsStringAsync();
                    MessageBox.Show(errorMsg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi kết nối: {ex.Message}"); }
            finally { if (FindName("btnGuiDon") is Button btnGuiEnd) btnGuiEnd.IsEnabled = true; }
        }

        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            if (FindName("txtMatKhauCu") is PasswordBox p1 && FindName("txtVisibleMatKhauCu") is TextBox t1)
            { t1.Text = p1.Password; p1.Visibility = Visibility.Collapsed; t1.Visibility = Visibility.Visible; }

            if (FindName("txtMatKhauMoi") is PasswordBox p2 && FindName("txtVisibleMatKhauMoi") is TextBox t2)
            { t2.Text = p2.Password; p2.Visibility = Visibility.Collapsed; t2.Visibility = Visibility.Visible; }

            if (FindName("txtXacNhanMatKhau") is PasswordBox p3 && FindName("txtVisibleXacNhanMatKhau") is TextBox t3)
            { t3.Text = p3.Password; p3.Visibility = Visibility.Collapsed; t3.Visibility = Visibility.Visible; }
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (FindName("txtMatKhauCu") is PasswordBox p1 && FindName("txtVisibleMatKhauCu") is TextBox t1)
            { p1.Password = t1.Text; p1.Visibility = Visibility.Visible; t1.Visibility = Visibility.Collapsed; }

            if (FindName("txtMatKhauMoi") is PasswordBox p2 && FindName("txtVisibleMatKhauMoi") is TextBox t2)
            { p2.Password = t2.Text; p2.Visibility = Visibility.Visible; t2.Visibility = Visibility.Collapsed; }

            if (FindName("txtXacNhanMatKhau") is PasswordBox p3 && FindName("txtVisibleXacNhanMatKhau") is TextBox t3)
            { p3.Password = t3.Text; p3.Visibility = Visibility.Visible; t3.Visibility = Visibility.Collapsed; }
        }

        private void TxtMatKhauCu_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (FindName("txtVisibleMatKhauCu") is TextBox t && FindName("txtMatKhauCu") is PasswordBox p)
            {
                if (t.Text != p.Password) t.Text = p.Password;
            }
        }
        private void TxtVisibleMatKhauCu_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FindName("txtMatKhauCu") is PasswordBox p && FindName("txtVisibleMatKhauCu") is TextBox t)
            {
                if (p.Password != t.Text) p.Password = t.Text;
            }
        }

        private void TxtMatKhauMoi_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (FindName("txtVisibleMatKhauMoi") is TextBox t && FindName("txtMatKhauMoi") is PasswordBox p)
            {
                if (t.Text != p.Password) t.Text = p.Password;
            }
        }
        private void TxtVisibleMatKhauMoi_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FindName("txtMatKhauMoi") is PasswordBox p && FindName("txtVisibleMatKhauMoi") is TextBox t)
            {
                if (p.Password != t.Text) p.Password = t.Text;
            }
        }

        private void TxtXacNhanMatKhau_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (FindName("txtVisibleXacNhanMatKhau") is TextBox t && FindName("txtXacNhanMatKhau") is PasswordBox p)
            {
                if (t.Text != p.Password) t.Text = p.Password;
            }
        }
        private void TxtVisibleXacNhanMatKhau_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FindName("txtXacNhanMatKhau") is PasswordBox p && FindName("txtVisibleXacNhanMatKhau") is TextBox t)
            {
                if (p.Password != t.Text) p.Password = t.Text;
            }
        }

        private async void BtnXacNhanDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CurrentUser == null) { MessageBox.Show("Lỗi phiên đăng nhập."); return; }
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            bool isShow = (FindName("chkShowPassword") as CheckBox)?.IsChecked == true;
            string oldP = isShow ? ((TextBox)FindName("txtVisibleMatKhauCu")).Text : ((PasswordBox)FindName("txtMatKhauCu")).Password;
            string newP = isShow ? ((TextBox)FindName("txtVisibleMatKhauMoi")).Text : ((PasswordBox)FindName("txtMatKhauMoi")).Password;
            string confP = isShow ? ((TextBox)FindName("txtVisibleXacNhanMatKhau")).Text : ((PasswordBox)FindName("txtXacNhanMatKhau")).Password;

            if (string.IsNullOrWhiteSpace(oldP) || string.IsNullOrWhiteSpace(newP))
            {
                MessageBox.Show("Vui lòng nhập đủ mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (newP.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (oldP == newP)
            {
                MessageBox.Show("Mật khẩu mới không được trùng với mật khẩu cũ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (newP != confP)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var req = new DoiMatKhauRequestDto { MatKhauCu = oldP, MatKhauMoi = newP };

            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync($"api/app/nhanvien/thongtincanhan/change-password/{idNhanVien}", req);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (FindName("txtMatKhauCu") is PasswordBox p1) p1.Password = "";
                    if (FindName("txtVisibleMatKhauCu") is TextBox t1) t1.Text = "";
                    if (FindName("txtMatKhauMoi") is PasswordBox p2) p2.Password = "";
                    if (FindName("txtVisibleMatKhauMoi") is TextBox t2) t2.Text = "";
                    if (FindName("txtXacNhanMatKhau") is PasswordBox p3) p3.Password = "";
                    if (FindName("txtVisibleXacNhanMatKhau") is TextBox t3) t3.Text = "";
                }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
        }
    }
}