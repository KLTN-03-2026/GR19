using AppCafebookApi.Services;
using AppCafebookApi.View;
using AppCafebookApi.View.quanly.pages;
using CafebookModel.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AppCafebookApi.View.quanly
{
    public partial class ManHinhQuanly : Window
    {
        private ToggleButton? currentNavButton;
        private static readonly HttpClient httpClient;

        // Tạm thời comment lại Timer của Thông báo
        // private DispatcherTimer _notificationTimer;

        // 1. TÍCH HỢP ĐỌC URL API ĐỘNG TỪ APPCONFIG
        static ManHinhQuanly()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };
        }

        public ManHinhQuanly()
        {
            InitializeComponent();

            /* Tạm thời vô hiệu hóa chức năng Thông báo
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _notificationTimer.Tick += _notificationTimer_Tick;
            */
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin user từ AuthService
            var currentUser = AuthService.CurrentUser;
            if (currentUser != null)
            {
                txtAdminName.Text = currentUser.HoTen;
                txtUserRole.Text = currentUser.TenVaiTro;

                // --- BẮT ĐẦU LOGIC CHUẨN TỪ MÀN HÌNH COMMON ---
                string avatarPath = currentUser.AnhDaiDien ?? string.Empty;

                if (!string.IsNullOrEmpty(avatarPath) && !avatarPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string baseUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";

                    if (!avatarPath.Contains("/"))
                    {
                        avatarPath = $"{HinhAnhPaths.UrlAvatarNV}/{avatarPath}";
                    }

                    avatarPath = $"{baseUrl.TrimEnd('/')}/{avatarPath.TrimStart('/')}";
                }

                // Gọi helper truyền URL chuẩn
                BitmapImage avatarImage = HinhAnhHelper.LoadImage(
                    avatarPath,
                    HinhAnhPaths.DefaultAvatar
                );
                // --- KẾT THÚC LOGIC CHUẨN ---

                // Đổ ảnh vào Border
                AvatarBorder.Background = new ImageBrush(avatarImage)
                {
                    Stretch = Stretch.UniformToFill
                };

                // Xử lý ẩn/hiện cái Icon mặc định (nằm dưới lớp ảnh)
                if (AvatarBorder.Child != null)
                {
                    // Nếu có đường dẫn ảnh thật -> Ẩn icon đi
                    if (!string.IsNullOrEmpty(currentUser.AnhDaiDien))
                        AvatarBorder.Child.Visibility = Visibility.Collapsed;
                    // Nếu không có -> Hiện icon
                    else
                        AvatarBorder.Child.Visibility = Visibility.Visible;
                }
            }

            // 2. CHẠY HÀM ẨN/HIỆN MENU (PHÂN QUYỀN CẤP 1)
            ApplyPermissions();

            // Mở trang Tổng quan đầu tiên nếu có quyền
            if (btnTongQuan.Visibility == Visibility.Visible)
            {
                btnTongQuan.IsChecked = true;
                UpdateSelectedButton(btnTongQuan);
                MainFrame.Navigate(new QuanLyTongQuanView());
            }

            /* Tạm thời vô hiệu hóa chức năng Thông báo
            await CheckNotificationsAsync();
            _notificationTimer.Start();
            */
        }

        // =================================================================================
        // HÀM PHÂN QUYỀN CẤP 1: MỞ CỬA CÁC MODULE CHÍNH
        // =================================================================================
        private void ApplyPermissions()
        {
            // Nếu là Quản trị viên -> Bật hết các nút trên menu
            if (AuthService.CoQuyen("FULL_QL"))
            {
                btnTongQuan.Visibility = Visibility.Visible;
                btnBan.Visibility = Visibility.Visible;
                btnSanPham.Visibility = Visibility.Visible;
                btnKho.Visibility = Visibility.Visible;
                btnDonHang.Visibility = Visibility.Visible;
                btnSach.Visibility = Visibility.Visible;
                btnLuong.Visibility = Visibility.Visible;
                btnNhanSu.Visibility = Visibility.Visible;
                btnKhachHang.Visibility = Visibility.Visible;
                return;
            }

            // GOM NHÓM QUYỀN: Kiểm tra mảng quyền bằng hàm CoQuyen
            btnTongQuan.Visibility = AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN", "QL_BAO_CAO_TON_KHO_SACH", "QL_BAO_CAO_TON_KHO_NL", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_BAO_CAO_DOANH_THU", "CM_CAI_DAT") ? Visibility.Visible : Visibility.Collapsed;

            btnBan.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAN", "QL_SU_CO_BAN", "QL_KHU_VUC") ? Visibility.Visible : Visibility.Collapsed;

            btnSanPham.Visibility = AuthService.CoQuyen("FULL_QL", "QL_SAN_PHAM", "QL_DANH_MUC", "QL_DINH_LUONG") ? Visibility.Visible : Visibility.Collapsed;

            btnKho.Visibility = AuthService.CoQuyen("FULL_QL", "QL_TON_KHO", "QL_NGUYEN_LIEU", "QL_NHAP_KHO", "QL_XUAT_HUY", "QL_KIEM_KHO", "QL_NHA_CUNG_CAP", "QL_DON_VI_CHUYEN_DOI") ? Visibility.Visible : Visibility.Collapsed;

            btnDonHang.Visibility = AuthService.CoQuyen("FULL_QL", "QL_DON_HANG", "QL_PHU_THU", "QL_NGUOI_GIAO_HANG") ? Visibility.Visible : Visibility.Collapsed;

            btnSach.Visibility = AuthService.CoQuyen("FULL_QL", "QL_SACH", "QL_DANH_MUC_SACH", "QL_LICH_SU_THUE_SACH") ? Visibility.Visible : Visibility.Collapsed;

            btnLuong.Visibility = AuthService.CoQuyen("FULL_QL", "QL_LUONG", "QL_PHAT_LUONG", "QL_CHAM_CONG", "QL_THUONG_PHAT") ? Visibility.Visible : Visibility.Collapsed;

            btnNhanSu.Visibility = AuthService.CoQuyen("FULL_QL", "QL_NHAN_VIEN", "QL_PHAN_QUYEN", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_LICH_LAM_VIEC", "QL_DON_XIN_NGHI", "QL_CAI_DAT_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;

            btnKhachHang.Visibility = AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG", "QL_KHUYEN_MAI") ? Visibility.Visible : Visibility.Collapsed;
        }

        // =================================================================================
        // XỬ LÝ CLICK MENU & ĐIỀU HƯỚNG (BẢO VỆ LỚP 2)
        // =================================================================================
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as ToggleButton;
            if (clickedButton == null) return;

            if (clickedButton == currentNavButton)
            {
                clickedButton.IsChecked = true;
                return;
            }

            Page? pageToNavigate = null;
            bool hasPermission = false;

            if (clickedButton == btnTongQuan)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN", "QL_BAO_CAO_TON_KHO_SACH", "QL_BAO_CAO_TON_KHO_NL", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_BAO_CAO_DOANH_THU", "CM_CAI_DAT");
                pageToNavigate = new QuanLyTongQuanView();
            }
            else if (clickedButton == btnBan)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_BAN", "QL_SU_CO_BAN", "QL_KHU_VUC");
                pageToNavigate = new QuanLyBanView();
            }
            else if (clickedButton == btnSanPham)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_SAN_PHAM", "QL_DANH_MUC", "QL_DINH_LUONG");
                pageToNavigate = new QuanLySanPhamView();
            }
            else if (clickedButton == btnKho)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_TON_KHO", "QL_NGUYEN_LIEU", "QL_NHAP_KHO", "QL_XUAT_HUY", "QL_KIEM_KHO", "QL_NHA_CUNG_CAP", "QL_DON_VI_CHUYEN_DOI");
                pageToNavigate = new QuanLyTonKhoView();
            }
            else if (clickedButton == btnDonHang)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_DON_HANG", "QL_PHU_THU", "QL_NGUOI_GIAO_HANG");
                pageToNavigate = new QuanLyDonHangView();
            }
            else if (clickedButton == btnSach)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_SACH", "QL_DANH_MUC_SACH", "QL_LICH_SU_THUE_SACH");
                pageToNavigate = new QuanLySachView();
            }
            else if (clickedButton == btnLuong)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_LUONG", "QL_PHAT_LUONG", "QL_CHAM_CONG", "QL_THUONG_PHAT");
                pageToNavigate = new QuanLyLuongView();
            }
            else if (clickedButton == btnNhanSu)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_NHAN_VIEN", "QL_PHAN_QUYEN", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_LICH_LAM_VIEC", "QL_DON_XIN_NGHI", "QL_CAI_DAT_NHAN_SU");
                pageToNavigate = new QuanLyNhanVienView();
            }
            else if (clickedButton == btnKhachHang)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG", "QL_KHUYEN_MAI");
                pageToNavigate = new QuanLyKhachHangView();
            }

            if (hasPermission && pageToNavigate != null)
            {
                UpdateSelectedButton(clickedButton);
                MainFrame.Navigate(pageToNavigate);
            }
            else
            {
                clickedButton.IsChecked = false;
                if (currentNavButton != null) currentNavButton.IsChecked = true;

                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateSelectedButton(ToggleButton newButton)
        {
            if (currentNavButton != null && currentNavButton != newButton)
            {
                currentNavButton.IsChecked = false;
            }
            currentNavButton = newButton;
            currentNavButton.IsChecked = true;
        }

        // =================================================================================
        // HỆ THỐNG THÔNG BÁO (ĐANG TẠM KHÓA) & LOGOUT
        // =================================================================================

        private void BtnThongBao_Click(object sender, RoutedEventArgs e)
        {
            // Tạm thời vô hiệu hóa để tập trung code các chức năng khác
            MessageBox.Show("Chức năng Thông báo đang được bảo trì!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /* TẠM THỜI KHÓA TOÀN BỘ LOGIC THÔNG BÁO VÀ API 
        private async void _notificationTimer_Tick(object? sender, EventArgs e)
        {
            await CheckNotificationsAsync();
        }

        private async Task CheckNotificationsAsync()
        {
            // Logic API check unread count...
        }

        private void ThongBaoItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Logic xử lý click thông báo...
        }
        */

        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                AuthService.Logout();
                new ManHinhDangNhap().Show();
                this.Close();
            }
        }
    }
}