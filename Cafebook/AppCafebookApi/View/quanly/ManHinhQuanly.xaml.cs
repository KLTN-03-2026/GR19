using AppCafebookApi.Services;
using AppCafebookApi.View;
using AppCafebookApi.View.quanly.pages;
using CafebookModel.Utils;
using CafebookModel.Model.Shared;
using System;
using System.Collections.Generic;
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
        private DispatcherTimer _notificationTimer;
        private Page? _targetPageForNotification = null; // Lưu trang đích khi click thông báo

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
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _notificationTimer.Tick += async (s, e) => await CheckNotificationsAsync();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = AuthService.CurrentUser;
            if (currentUser != null)
            {
                txtAdminName.Text = currentUser.HoTen;
                txtUserRole.Text = currentUser.TenVaiTro;

                string avatarPath = currentUser.AnhDaiDien ?? string.Empty;
                if (!string.IsNullOrEmpty(avatarPath) && !avatarPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string baseUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
                    if (!avatarPath.Contains("/")) avatarPath = $"{HinhAnhPaths.UrlAvatarNV}/{avatarPath}";
                    avatarPath = $"{baseUrl.TrimEnd('/')}/{avatarPath.TrimStart('/')}";
                }

                BitmapImage avatarImage = HinhAnhHelper.LoadImage(avatarPath, HinhAnhPaths.DefaultAvatar);
                AvatarBorder.Background = new ImageBrush(avatarImage) { Stretch = Stretch.UniformToFill };

                if (AvatarBorder.Child != null)
                {
                    AvatarBorder.Child.Visibility = string.IsNullOrEmpty(currentUser.AnhDaiDien) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            ApplyPermissions();

            if (btnTongQuan.Visibility == Visibility.Visible)
            {
                btnTongQuan.IsChecked = true;
                UpdateSelectedButton(btnTongQuan);
                MainFrame.Navigate(new QuanLyTongQuanView());
            }

            await CheckNotificationsAsync();
            _notificationTimer.Start();
        }

        private void ApplyPermissions()
        {
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

                // Hiển thị cả 2 nút thông báo
                btnThongBao.Visibility = Visibility.Visible;
                btnQuanLyThongBao.Visibility = Visibility.Visible;
                return;
            }

            btnTongQuan.Visibility = AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN", "QL_BAO_CAO_TON_KHO_SACH", "QL_BAO_CAO_TON_KHO_NL", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_BAO_CAO_DOANH_THU", "CM_CAI_DAT", "CM_NHAT_KY_HE_THONG") ? Visibility.Visible : Visibility.Collapsed;
            btnBan.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAN", "QL_SU_CO_BAN", "QL_KHU_VUC") ? Visibility.Visible : Visibility.Collapsed;
            btnSanPham.Visibility = AuthService.CoQuyen("FULL_QL", "QL_SAN_PHAM", "QL_DANH_MUC", "QL_DINH_LUONG") ? Visibility.Visible : Visibility.Collapsed;
            btnKho.Visibility = AuthService.CoQuyen("FULL_QL", "QL_TON_KHO", "QL_NGUYEN_LIEU", "QL_NHAP_KHO", "QL_XUAT_HUY", "QL_KIEM_KHO", "QL_NHA_CUNG_CAP", "QL_DON_VI_CHUYEN_DOI") ? Visibility.Visible : Visibility.Collapsed;
            btnDonHang.Visibility = AuthService.CoQuyen("FULL_QL", "QL_DON_HANG", "QL_PHU_THU", "QL_NGUOI_GIAO_HANG") ? Visibility.Visible : Visibility.Collapsed;
            btnSach.Visibility = AuthService.CoQuyen("FULL_QL", "QL_SACH", "QL_DANH_MUC_SACH", "QL_LICH_SU_THUE_SACH") ? Visibility.Visible : Visibility.Collapsed;
            btnLuong.Visibility = AuthService.CoQuyen("FULL_QL", "QL_LUONG", "QL_PHAT_LUONG", "QL_CHAM_CONG", "QL_THUONG_PHAT") ? Visibility.Visible : Visibility.Collapsed;
            btnNhanSu.Visibility = AuthService.CoQuyen("FULL_QL", "QL_NHAN_VIEN", "QL_PHAN_QUYEN", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_LICH_LAM_VIEC", "QL_DON_XIN_NGHI", "QL_CAI_DAT_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
            btnKhachHang.Visibility = AuthService.CoQuyen("FULL_QL", "QL_KHACH_HANG", "QL_KHUYEN_MAI") ? Visibility.Visible : Visibility.Collapsed;

            // QUYỀN THÔNG BÁO MỚI
            // Nút cái chuông ngoài màn hình chính
            btnThongBao.Visibility = AuthService.CoQuyen("FULL_QL", "CM_THONG_BAO") ? Visibility.Visible : Visibility.Collapsed;

            // Nút "Tới màn hình Quản lý Thông báo" ở dưới cùng của Popup
            btnQuanLyThongBao.Visibility = AuthService.CoQuyen("FULL_QL", "QL_THONG_BAO") ? Visibility.Visible : Visibility.Collapsed;
        }

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
                hasPermission = AuthService.CoQuyen("FULL_QL", "QL_TONG_QUAN", "QL_BAO_CAO_TON_KHO_SACH", "QL_BAO_CAO_TON_KHO_NL", "QL_BAO_CAO_NHAN_SU", "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", "QL_BAO_CAO_DOANH_THU", "CM_CAI_DAT", "CM_NHAT_KY_HE_THONG");
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
        // HỆ THỐNG THÔNG BÁO 
        // =================================================================================

        private void BtnThongBao_Click(object sender, RoutedEventArgs e)
        {
            popThongBao.IsOpen = !popThongBao.IsOpen;
        }

        private async Task CheckNotificationsAsync()
        {
            if (AuthService.CurrentUser == null) return;

            try
            {
                string roles = AuthService.CoQuyen("FULL_QL", "FULL_QL", "QL_THONG_BAO") ? "FULL_QL" : "NV_PHUC_VU";

                string url = $"api/shared/thongbao/my-notifications?userId={AuthService.CurrentUser.IdNhanVien}&userRoles={roles}";
                var response = await httpClient.GetFromJsonAsync<SharedThongBaoResponseDto>(url);

                if (response != null)
                {
                    lstThongBao.ItemsSource = response.Notifications;

                    if (response.UnreadCount > 0)
                    {
                        BadgeThongBao.Visibility = Visibility.Visible;
                        txtBadgeCount.Text = response.UnreadCount > 99 ? "99+" : response.UnreadCount.ToString();
                    }
                    else
                    {
                        BadgeThongBao.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi load thông báo: {ex.Message}");
            }
        }

        private async void ThongBaoItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SharedThongBaoItemDto tb)
            {
                // ĐÃ FIX: Không chuyển thành Đã xem nếu là thông báo Thủ công từ Quản lý
                bool isManualAdminNotice = tb.LoaiThongBao == "ThongBaoQuanLy" || tb.LoaiThongBao == "ThongBaoNhanVien" || tb.LoaiThongBao == "ThongBaoToanNhanVien";

                if (!tb.DaXem && !isManualAdminNotice)
                {
                    try
                    {
                        await httpClient.PostAsync($"api/shared/thongbao/mark-as-read/{tb.IdThongBao}", null);
                        tb.DaXem = true;
                        await CheckNotificationsAsync();
                    }
                    catch { }
                }

                popThongBao.IsOpen = false;

                // Chuẩn bị dữ liệu hiển thị lên Popup Chi tiết
                lblDetailSender.Text = $"Người gửi: {tb.TenNhanVienTao}";
                lblDetailTime.Text = $"Gửi lúc: {tb.ThoiGianTao:dd/MM/yyyy HH:mm}";
                txtDetailContent.Text = tb.NoiDung;

                _targetPageForNotification = null;

                // Xác định trang liên kết
                if (tb.LoaiThongBao == "SuCoBan" || tb.LoaiThongBao == "DatBan")
                    _targetPageForNotification = new QuanLyBanView();
                else if (tb.LoaiThongBao == "HetHang" || tb.LoaiThongBao == "CanhBaoKho" || tb.LoaiThongBao == "Kho")
                    _targetPageForNotification = new QuanLyTonKhoView();
                else if (tb.LoaiThongBao == "DonXinNghi")
                    _targetPageForNotification = new QuanLyNhanVienView();

                // Ẩn/Hiện nút "Đi tới trang"
                btnDetailGoTo.Visibility = _targetPageForNotification != null ? Visibility.Visible : Visibility.Collapsed;

                // Mở khung Overlay chi tiết cực đẹp
                DetailThongBaoOverlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnDetailClose_Click(object sender, RoutedEventArgs e)
        {
            DetailThongBaoOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnDetailGoTo_Click(object sender, RoutedEventArgs e)
        {
            DetailThongBaoOverlay.Visibility = Visibility.Collapsed;
            if (_targetPageForNotification != null)
            {
                MainFrame.Navigate(_targetPageForNotification);
            }
        }

        private void BtnMoQuanLyThongBao_Click(object sender, RoutedEventArgs e)
        {
            // Bảo vệ lớp 2: Chặn ngay nếu không có quyền
            if (!AuthService.CoQuyen("FULL_QL", "QL_THONG_BAO"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng Quản lý Thông báo!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            popThongBao.IsOpen = false;
            MainFrame.Navigate(new QuanLyThongBaoView());
        }

        // =================================================================================
        // ĐĂNG XUẤT
        // =================================================================================
        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _notificationTimer.Stop();
                AuthService.Logout();
                new ManHinhDangNhap().Show();
                this.Close();
            }
        }
    }
}