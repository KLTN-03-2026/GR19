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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Focus();
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

            btnLogin.IsEnabled = false;
            btnLogin.Content = "ĐANG ĐĂNG NHẬP...";

            try
            {
                var loginResponse = await AuthService.LoginAsync(loginReq);

                // Trong file ManHinhDangNhap.xaml.cs - Hàm BtnLogin_Click
                if (loginResponse != null)
                {
                    string targetWorkspace = "";

                    // ĐIỀU KIỆN ĐẶC BIỆT: Vai trò là "Quản lý" VÀ có quyền "FULL_QL"
                    bool isSuperManager = loginResponse.TenVaiTro == "Quản lý" && AuthService.CoQuyen("FULL_QL");

                    if (isSuperManager)
                    {
                        // Chỉ trường hợp này mới hiện Popup hỏi
                        var chonKhongGian = new ChonKhongGianWindow();
                        bool? result = chonKhongGian.ShowDialog();

                        if (result == true)
                        {
                            targetWorkspace = chonKhongGian.SelectedWorkspace; // "QuanLy" hoặc "NhanVien"
                        }
                        else
                        {
                            // Người dùng tắt popup giữa chừng -> Hủy đăng nhập
                            btnLogin.IsEnabled = true;
                            btnLogin.Content = "ĐĂNG NHẬP";
                            return;
                        }
                    }
                    else
                    {
                        // CÁC TRƯỜNG HỢP CÒN LẠI: TỰ ĐỘNG ĐIỀU HƯỚNG
                        if (loginResponse.TenVaiTro == "Quản lý")
                        {
                            targetWorkspace = "QuanLy";
                        }
                        else
                        {
                            // Là nhân viên hoặc các vai trò khác thì vào thẳng màn hình nhân viên
                            targetWorkspace = "NhanVien";
                        }
                    }

                    // Chuyển sang màn hình Welcome và truyền lệnh điều hướng đã chốt
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
                MessageBox.Show($"Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại mạng hoặc cấu hình API.\nChi tiết: {ex.Message}", "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnLogin.Content = "ĐĂNG NHẬP";
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