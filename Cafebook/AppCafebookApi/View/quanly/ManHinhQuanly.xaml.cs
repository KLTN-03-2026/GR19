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
using AppCafebookApi.View.nhanvien.pages;
using CafebookModel.Model.ModelApp.NhanVien;
using System.Threading;

namespace AppCafebookApi.View.quanly
{
    public partial class ManHinhQuanly : Window
    {
        private ToggleButton? currentNavButton;
        private DispatcherTimer _notificationTimer;
        private Page? _targetPageForNotification = null; 
        private int _lastLatestNotificationId = 0;
        private CancellationTokenSource? _toastCts; 

        public ManHinhQuanly()
        {
            InitializeComponent();
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _notificationTimer.Tick += async (s, e) =>
            {
                await CheckNotificationsAsync();
                await CheckChamCongStatusAsync(); 
            };
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
            await CheckChamCongStatusAsync(); 
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

                btnThongTinCaNhan.Visibility = Visibility.Visible;
                btnChamCong.Visibility = Visibility.Visible;
                btnLichLamViecCuaToi.Visibility = Visibility.Visible;
                btnPhieuLuongCuaToi.Visibility = Visibility.Visible;

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

            btnThongTinCaNhan.Visibility = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THONG_TIN") ? Visibility.Visible : Visibility.Collapsed;
            btnChamCong.Visibility = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHAM_CONG") ? Visibility.Visible : Visibility.Collapsed;
            btnLichLamViecCuaToi.Visibility = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_LICH_LAM_VIEC") ? Visibility.Visible : Visibility.Collapsed;
            btnPhieuLuongCuaToi.Visibility = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_PHIEU_LUONG") ? Visibility.Visible : Visibility.Collapsed;

            btnThongBao.Visibility = AuthService.CoQuyen("FULL_QL", "CM_THONG_BAO") ? Visibility.Visible : Visibility.Collapsed;

            btnQuanLyThongBao.Visibility = AuthService.CoQuyen("FULL_QL", "QL_THONG_BAO") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task CheckChamCongStatusAsync()
        {
            if (AuthService.CurrentUser == null) return;

            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHAM_CONG", "QL_CHAM_CONG"))
                return;

            try
            {
                string url = $"api/app/chamcong/status/{AuthService.CurrentUser.IdNhanVien}";
                var response = await ApiClient.Instance.GetFromJsonAsync<ChamCongDashboardDto>(url);

                if (response != null)
                {
                    if (response.DangTrongCa)
                    {
                        if (response.LanVaoGanNhat.HasValue)
                        {
                            var totalSpan = TimeSpan.FromHours((double)response.TongGioLamHienTai) + (DateTime.Now - response.LanVaoGanNhat.Value);
                            UpdateSidebarStatus($"Đang làm ({(int)totalSpan.TotalHours:D2}:{totalSpan.Minutes:D2})");
                        }
                        else UpdateSidebarStatus("Đang làm việc");
                    }
                    else if (response.TrangThai == "KhongCoCa") UpdateSidebarStatus("Không có ca");
                    else if (response.TrangThai == "ChuaDenGio") UpdateSidebarStatus("Sắp tới ca");
                    else if (response.TrangThai == "ChoVaoCa") UpdateSidebarStatus("Chưa chấm công");
                    else if (response.TrangThai == "DaHoanThanh") UpdateSidebarStatus("Hoàn thành ca");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi load trạng thái chấm công: {ex.Message}");
                UpdateSidebarStatus("Lỗi đồng bộ");
            }
        }

        public void UpdateSidebarStatus(string statusText)
        {
            if (FindName("lblSidebarStatus") is TextBlock lblStatus)
            {
                lblStatus.Text = statusText;

                if (statusText.StartsWith("Đang làm"))
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745"));
                else if (statusText == "Lỗi đồng bộ" || statusText == "Chưa chấm công")
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc3545"));
                else
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            }
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as ToggleButton;
            if (clickedButton == null) return;
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
            else if (clickedButton == btnThongTinCaNhan)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THONG_TIN");
                pageToNavigate = new ThongTinCaNhanView();
            }
            else if (clickedButton == btnChamCong)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHAM_CONG");
                pageToNavigate = new ChamCongView();
            }
            else if (clickedButton == btnLichLamViecCuaToi)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_LICH_LAM_VIEC");
                pageToNavigate = new LichLamViecView();
            }
            else if (clickedButton == btnPhieuLuongCuaToi)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_PHIEU_LUONG");
                pageToNavigate = new PhieuLuongView();
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

        private void BtnThongBao_Click(object sender, RoutedEventArgs e)
        {
            popThongBao.IsOpen = !popThongBao.IsOpen;
        }

        private async Task CheckNotificationsAsync()
        {
            if (AuthService.CurrentUser == null) return;

            try
            {
                var myRoles = new List<string>();
                if (AuthService.CoQuyen("FULL_QL")) myRoles.Add("FULL_QL");
                if (AuthService.CoQuyen("QL_BAN")) myRoles.Add("QL_BAN");
                if (AuthService.CoQuyen("QL_SU_CO_BAN")) myRoles.Add("QL_SU_CO_BAN");
                if (AuthService.CoQuyen("QL_DON_HANG")) myRoles.Add("QL_DON_HANG");
                if (AuthService.CoQuyen("QL_TON_KHO")) myRoles.Add("QL_TON_KHO");
                if (AuthService.CoQuyen("QL_DON_XIN_NGHI")) myRoles.Add("QL_DON_XIN_NGHI");
                if (AuthService.CoQuyen("QL_LICH_LAM_VIEC")) myRoles.Add("QL_LICH_LAM_VIEC");

                string roles = string.Join(",", myRoles);
                string rName = Uri.EscapeDataString(AuthService.CurrentUser.TenVaiTro ?? "");

                string url = $"api/shared/thongbao/my-notifications?userId={AuthService.CurrentUser.IdNhanVien}&userRoles={roles}&roleName={rName}";

                var response = await ApiClient.Instance.GetFromJsonAsync<SharedThongBaoResponseDto>(url);

                if (response != null && response.Notifications != null)
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

                    if (response.Notifications.Any())
                    {
                        var maxIdInList = response.Notifications.Max(n => n.IdThongBao);

                        if (_lastLatestNotificationId != 0 && maxIdInList > _lastLatestNotificationId)
                        {
                            var newNotifs = response.Notifications.Where(n => n.IdThongBao > _lastLatestNotificationId).ToList();
                            var newCount = newNotifs.Count;

                            var trulyLatestNotif = newNotifs.OrderByDescending(n => n.IdThongBao).First();

                            string msg = newCount == 1 ? trulyLatestNotif.NoiDung : $"Bạn vừa có {newCount} thông báo mới!";
                            ShowToastNotification(msg);
                        }

                        _lastLatestNotificationId = Math.Max(_lastLatestNotificationId, maxIdInList);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Lỗi load thông báo: {ex.Message}"); }
        }

        private async void ShowToastNotification(string message)
        {
            _toastCts?.Cancel();
            _toastCts = new CancellationTokenSource();
            var token = _toastCts.Token;

            txtToastMessage.Text = message;

            var showStoryboard = new System.Windows.Media.Animation.Storyboard();

            var slideUp = new System.Windows.Media.Animation.ThicknessAnimation
            {
                From = new Thickness(0, 0, -300, 140),
                To = new Thickness(0, 0, 10, 140),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
            };
            System.Windows.Media.Animation.Storyboard.SetTarget(slideUp, ToastNotification);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(slideUp, new PropertyPath("Margin"));

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            System.Windows.Media.Animation.Storyboard.SetTarget(fadeIn, ToastNotification);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));

            showStoryboard.Children.Add(slideUp);
            showStoryboard.Children.Add(fadeIn);
            showStoryboard.Begin();

            try
            {
                await Task.Delay(3500, token);

                var hideStoryboard = new System.Windows.Media.Animation.Storyboard();
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.4)
                };
                System.Windows.Media.Animation.Storyboard.SetTarget(fadeOut, ToastNotification);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

                hideStoryboard.Children.Add(fadeOut);
                hideStoryboard.Begin();
            }
            catch (TaskCanceledException)
            {
                // Có thông báo mới xen vào, tiến trình ẩn bị hủy -> Không làm gì cả, để Toast tiếp tục hiện
            }
        }

        private async void ThongBaoItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SharedThongBaoItemDto tb)
            {
                bool isManualAdminNotice = tb.LoaiThongBao == "ThongBaoQuanLy" || tb.LoaiThongBao == "ThongBaoNhanVien" || tb.LoaiThongBao == "ThongBaoToanNhanVien";

                if (!tb.DaXem && !isManualAdminNotice)
                {
                    try
                    {
                        await ApiClient.Instance.PostAsync($"api/shared/thongbao/mark-as-read/{tb.IdThongBao}", null);
                        tb.DaXem = true;
                        await CheckNotificationsAsync();
                    }
                    catch { }
                }

                popThongBao.IsOpen = false;

                lblDetailSender.Text = $"Người gửi: {tb.TenNhanVienTao}";
                lblDetailTime.Text = $"Gửi lúc: {tb.ThoiGianTao:dd/MM/yyyy HH:mm}";
                txtDetailContent.Text = tb.NoiDung;

                _targetPageForNotification = tb.LoaiThongBao switch
                {
                    "SuCoBan" => new QuanLySuCoBanView(),
                    "HetHang" or "CanhBaoKho" or "Kho" => new QuanLyTonKhoView(),
                    "DangKyLichMoi" => new QuanLyLichLamViecView(),
                    "DonXinNghi" => new QuanLyDonXinNghiView(),
                    "DonHangMoi" => new QuanLyDonHangView(),
                    "ThongBaoQuanLy" or "ThongBaoNhanVien" or "ThongBaoToanNhanVien" => null,
                    _ => null
                };

                btnDetailGoTo.Visibility = _targetPageForNotification != null ? Visibility.Visible : Visibility.Collapsed;
                DetailThongBaoOverlay.Visibility = Visibility.Visible;
            }
        }

        private void BtnDetailGoTo_Click(object sender, RoutedEventArgs e)
        {
            DetailThongBaoOverlay.Visibility = Visibility.Collapsed;
            if (_targetPageForNotification != null)
            {
                if (_targetPageForNotification is QuanLyBanView) UpdateSelectedButton(btnBan);
                else if (_targetPageForNotification is QuanLyTonKhoView) UpdateSelectedButton(btnKho);
                else if (_targetPageForNotification is QuanLyDonHangView) UpdateSelectedButton(btnDonHang);
                else if (_targetPageForNotification is QuanLyLichLamViecView || _targetPageForNotification is QuanLyDonXinNghiView) UpdateSelectedButton(btnNhanSu);

                MainFrame.Navigate(_targetPageForNotification);
            }
        }

        private void BtnDetailClose_Click(object sender, RoutedEventArgs e)
        {
            DetailThongBaoOverlay.Visibility = Visibility.Collapsed;
        }
        private void BtnMoQuanLyThongBao_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_THONG_BAO"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng Quản lý Thông báo!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            popThongBao.IsOpen = false;
            MainFrame.Navigate(new QuanLyThongBaoView());
        }

        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _notificationTimer?.Stop();

                if (MainFrame != null)
                {
                    MainFrame.Content = null;

                    while (MainFrame.NavigationService.CanGoBack)
                    {
                        MainFrame.NavigationService.RemoveBackEntry();
                    }
                }

                AuthService.Logout();

                new ManHinhDangNhap().Show();
                this.Close();
            }
        }
    }
}