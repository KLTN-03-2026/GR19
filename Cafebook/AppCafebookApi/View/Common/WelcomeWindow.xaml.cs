using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CafebookModel.Model.Shared;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using AppCafebookApi.View.quanly;
using AppCafebookApi.View.nhanvien;

namespace AppCafebookApi.View.Common
{
    public partial class WelcomeWindow : Window
    {
        private readonly LoginResponse? _user;
        private Window? _parentWindow;
        private string _workspace;
        private DispatcherTimer? _timer;

        public WelcomeWindow()
        {
            InitializeComponent();
            _workspace = "NhanVien"; // Fallback an toàn
        }

        // Khởi tạo nhận 3 biến: Dữ liệu user, Cửa sổ Login cũ, và Lệnh điều hướng
        public WelcomeWindow(LoginResponse user, Window parentWindow, string workspace)
        {
            InitializeComponent();
            _user = user;
            _parentWindow = parentWindow;
            _workspace = workspace;

            // Ẩn màn hình đăng nhập ngay lập tức để tạo cảm giác mượt mà
            if (_parentWindow != null)
            {
                _parentWindow.Hide();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_user != null)
            {
                txtUserGreeting.Text = $"Xin chào, {_user.HoTen}";

                // Tải ảnh đại diện
                BitmapImage avatar = HinhAnhHelper.LoadImage(
                    _user.AnhDaiDien ?? string.Empty,
                    HinhAnhPaths.DefaultAvatar
                );

                if (avatar != null)
                {
                    imgAvatar.Fill = new ImageBrush(avatar)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }

            // Đếm ngược 2.5 giây cho người dùng xem hiệu ứng Welcome
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2500)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer?.Stop();

            // Đóng cửa sổ Đăng Nhập hoàn toàn để giải phóng RAM
            if (_parentWindow != null)
            {
                _parentWindow.Close();
            }

            // THỰC THI CHUYỂN HƯỚNG DỰA VÀO LỆNH TỪ MÀN HÌNH ĐĂNG NHẬP
            if (_workspace == "QuanLy")
            {
                new ManHinhQuanly().Show();
            }
            else
            {
                new ManHinhNhanVien().Show();
            }

            // Đóng Welcome Window
            this.Close();
        }
    }
}