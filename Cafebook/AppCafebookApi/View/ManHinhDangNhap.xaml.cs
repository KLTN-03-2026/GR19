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

            string? currentUrl = AppConfigManager.GetApiServerUrl();

            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                btnSettings.Visibility = Visibility.Visible;

                MessageBox.Show("Đây là lần đầu chạy ứng dụng. Vui lòng thiết lập địa chỉ Server API để kết nối!", "Yêu cầu thiết lập", MessageBoxButton.OK, MessageBoxImage.Information);

                var settingWindow = new CaiDatServerWindow();
                settingWindow.ShowDialog();

                if (!string.IsNullOrWhiteSpace(AppConfigManager.GetApiServerUrl()))
                {
                    btnSettings.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                btnSettings.Visibility = Visibility.Collapsed;
            }
        }

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

            btnLogin.IsEnabled = false;
            btnLogin.Content = "ĐANG ĐĂNG NHẬP...";

            txtUsername.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtVisiblePassword.IsEnabled = false;
            chkShowPassword.IsEnabled = false;

            bool isRetrying = false;

            try
            {
                var loginResponse = await AuthService.LoginAsync(loginReq);

                if (loginResponse != null)
                {
                    string targetWorkspace = "";

                    bool isSuperManager = loginResponse.TenVaiTro == "Quản lý" && AuthService.CoQuyen("FULL_QL");

                    if (isSuperManager)
                    {
                        var chonKhongGian = new ChonKhongGianWindow();
                        chonKhongGian.Owner = this;

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
                        targetWorkspace = (loginResponse.TenVaiTro == "Quản lý") ? "QuanLy" : "NhanVien";
                    }

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
                            ApiClient.ResetInstance();

                            _countConnectionError = 0;
                            isRetrying = true;
                            BtnLogin_Click(sender, e);
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