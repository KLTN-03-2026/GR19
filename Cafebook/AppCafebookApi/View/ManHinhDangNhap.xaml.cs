using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using AppCafebookApi.Services;
using AppCafebookApi.View.Common;
using CafebookModel.Model.Shared;

namespace AppCafebookApi.View
{
    public partial class ManHinhDangNhap : Window
    {
        private int _countConnectionError = 0;

        public ManHinhDangNhap()
        {
            InitializeComponent();
        }

        // Thêm sự kiện Loaded để kiểm tra URL lúc mới mở form
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Focus();

            // Kiểm tra xem đã có cấu hình API chưa
            string? currentUrl = AppConfigManager.GetApiServerUrl();

            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                // 1. Chưa có cấu hình (Lần đầu chạy) -> Hiện nút và bắt buộc cài đặt
                btnSettings.Visibility = Visibility.Visible;

                MessageBox.Show("Đây là lần đầu chạy ứng dụng. Vui lòng thiết lập địa chỉ Server API để kết nối!", "Yêu cầu thiết lập", MessageBoxButton.OK, MessageBoxImage.Information);

                var settingWindow = new CaiDatServerWindow();
                settingWindow.ShowDialog();

                // 2. Kiểm tra lại ngay sau khi tắt form Cài đặt: Nếu người dùng đã nhập và lưu thành công thì ẩn nút đi
                if (!string.IsNullOrWhiteSpace(AppConfigManager.GetApiServerUrl()))
                {
                    btnSettings.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // 3. Đã có cấu hình từ trước -> Ẩn nút Cài đặt đi cho gọn giao diện
                btnSettings.Visibility = Visibility.Collapsed;
            }
        }

        // Thêm hàm xử lý nút mở Cài đặt (nút này sẽ thêm ở XAML bước 2)
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingWindow = new CaiDatServerWindow();
            settingWindow.ShowDialog();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginReq = new LoginRequest
            {
                Username = txtUsername.Text,
                Password = chkShowPassword.IsChecked == true ? txtVisiblePassword.Text : txtPassword.Password
            };

            if (string.IsNullOrEmpty(loginReq.Username) || string.IsNullOrEmpty(loginReq.Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- 1. KHÓA CÁC ĐIỀU KHIỂN ---
            btnLogin.IsEnabled = false;
            btnLogin.Content = "ĐANG ĐĂNG NHẬP...";

            txtUsername.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtVisiblePassword.IsEnabled = false;
            chkShowPassword.IsEnabled = false;

            bool isRetrying = false; // Biến cờ để tránh mở khóa UI sớm khi đang tự động đăng nhập lại

            try
            {
                var loginResponse = await AuthService.LoginAsync(loginReq);

                if (loginResponse != null)
                {
                    string targetWorkspace = "";

                    // ĐIỀU KIỆN ĐẶC BIỆT: Vai trò là "Quản lý" VÀ có quyền "FULL_QL"
                    bool isSuperManager = loginResponse.TenVaiTro == "Quản lý" && AuthService.CoQuyen("FULL_QL");

                    if (isSuperManager)
                    {
                        var chonKhongGian = new ChonKhongGianWindow();

                        // BỔ SUNG DÒNG NÀY: Đặt màn hình đăng nhập (this) làm chủ của popup
                        chonKhongGian.Owner = this;

                        // Bật popup lên
                        bool? result = chonKhongGian.ShowDialog();

                        if (result == true)
                        {
                            targetWorkspace = chonKhongGian.SelectedWorkspace;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        // CÁC TRƯỜNG HỢP CÒN LẠI: TỰ ĐỘNG ĐIỀU HƯỚNG
                        targetWorkspace = (loginResponse.TenVaiTro == "Quản lý") ? "QuanLy" : "NhanVien";
                    }

                    // Chuyển sang màn hình Welcome
                    WelcomeWindow welcome = new WelcomeWindow(loginResponse, this, targetWorkspace);
                    welcome.Show();

                    _countConnectionError = 0;
                }
                else
                {
                    MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _countConnectionError++;

                if (_countConnectionError >= 3)
                {
                    var result = MessageBox.Show($"Không thể kết nối đến máy chủ.\nBạn có muốn mở bảng Cài đặt Server để kiểm tra lại địa chỉ kết nối không?\n\nChi tiết: {ex.Message}", "Lỗi kết nối", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var settingWindow = new CaiDatServerWindow();
                        bool? isSaved = settingWindow.ShowDialog();

                        if (isSaved == true)
                        {
                            AuthService.ReloadHttpClient();
                            _countConnectionError = 0;

                            isRetrying = true; // Đánh dấu đang retry để finally không mở khóa UI sớm
                            BtnLogin_Click(sender, e); // Gọi lại lệnh đăng nhập
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại mạng hoặc cấu hình API.\nChi tiết: {ex.Message}", "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                // --- 2. MỞ LẠI CÁC ĐIỀU KHIỂN (Chỉ mở khi KHÔNG PHẢI ĐANG TRONG QUÁ TRÌNH RETRY) ---
                if (!isRetrying)
                {
                    btnLogin.IsEnabled = true;
                    btnLogin.Content = "ĐĂNG NHẬP";

                    txtUsername.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                    txtVisiblePassword.IsEnabled = true;
                    chkShowPassword.IsEnabled = true;
                }
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && btnLogin.IsEnabled)
            {
                BtnLogin_Click(sender, e);
                e.Handled = true;
            }
        }

        // --- Logic Hiển thị Mật khẩu ---
        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtVisiblePassword.Text = txtPassword.Password;
            txtVisiblePassword.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtVisiblePassword.Text;
            txtVisiblePassword.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
                txtVisiblePassword.Text = txtPassword.Password;
        }

        private void TxtVisiblePassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (chkShowPassword.IsChecked == false)
                txtPassword.Password = txtVisiblePassword.Text;
        }
    }
}