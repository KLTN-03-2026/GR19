using AppCafebookApi.Services;
using AppCafebookApi.View.nhanvien.pages;
using AppCafebookApi.View.quanly.pages;
using CafebookModel.Model.Shared;
using CafebookModel.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CafebookModel.Model.ModelApp.NhanVien;

namespace AppCafebookApi.View.nhanvien
{
    public partial class ManHinhNhanVien : Window
    {
        private ToggleButton? currentNavButton;
        //private static readonly HttpClient httpClient;
        private DispatcherTimer _notificationTimer;
        private Page? _targetPageForNotification = null;
        /*
        static ManHinhNhanVien()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }
        */
        public ManHinhNhanVien() 
        {
            InitializeComponent();

            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
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
                txtUserName.Text = currentUser.HoTen;
                txtUserRole.Text = currentUser.TenVaiTro;

                // Xử lý ảnh đại diện 
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
                    AvatarBorder.Child.Visibility = string.IsNullOrEmpty(currentUser.AnhDaiDien) ? Visibility.Visible : Visibility.Collapsed;
            }

            ApplyPermissions();

            // Mở trang mặc định (Sơ đồ bàn) nếu có quyền
            if (btnSoDoBan.Visibility == Visibility.Visible)
            {
                UpdateSelectedButton(btnSoDoBan);
                MainFrame.Navigate(new SoDoBanView());
            }

            await CheckNotificationsAsync();
            await CheckChamCongStatusAsync();
            _notificationTimer.Start();
        }

        // =================================================================================
        // HÀM BỔ SUNG: CẬP NHẬT TRẠNG THÁI CHẤM CÔNG TỪ PAGE CON
        // =================================================================================
        private async Task CheckChamCongStatusAsync()
        {
            if (AuthService.CurrentUser == null) return;

            // Chỉ kiểm tra nếu user có quyền chấm công
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHAM_CONG", "QL_CHAM_CONG"))
                return;

            try
            {
                // Gọi API lấy trạng thái
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

                // Đổi màu tùy theo trạng thái
                if (statusText.StartsWith("Đang làm"))
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745"));
                else if (statusText == "Lỗi đồng bộ" || statusText == "Chưa chấm công")
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc3545"));
                else
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            }
        }

        private void ApplyPermissions()
        {
            // Kiểm tra quyền quản trị hoặc toàn quyền nhân viên 
            bool isFull = AuthService.CoQuyen("FULL_QL", "FULL_NV");

            // Nhóm Vận hành POS
            btnSoDoBan.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_SO_DO_BAN", "NV_GOI_MON", "NV_THANH_TOAN")) ? Visibility.Visible : Visibility.Collapsed;
            btnDatBan.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_DAT_BAN")) ? Visibility.Visible : Visibility.Collapsed;
            btnCheBien.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHE_BIEN")) ? Visibility.Visible : Visibility.Collapsed;
            btnThueSach.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH")) ? Visibility.Visible : Visibility.Collapsed;
            btnGiaoHang.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_GIAO_HANG")) ? Visibility.Visible : Visibility.Collapsed;
            // Nhóm Cá nhân
            btnThongTinCaNhan.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THONG_TIN")) ? Visibility.Visible : Visibility.Collapsed;
            btnChamCong.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHAM_CONG")) ? Visibility.Visible : Visibility.Collapsed;
            btnLichLamViecCuaToi.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_LICH_LAM_VIEC")) ? Visibility.Visible : Visibility.Collapsed;
            btnPhieuLuongCuaToi.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_PHIEU_LUONG")) ? Visibility.Visible : Visibility.Collapsed;

            // Nút Thông báo ở Footer
            btnThongBao.Visibility = (isFull || AuthService.CoQuyen("FULL_QL", "FULL_NV", "CM_THONG_BAO")) ? Visibility.Visible : Visibility.Collapsed;
        }

        // =================================================================================
        // XỬ LÝ SỰ KIỆN CLICK SIDEBAR 
        // =================================================================================
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as ToggleButton;
            if (clickedButton == null) return;
            /*
            if (clickedButton == currentNavButton)
            {
                clickedButton.IsChecked = true;
                return;
            }
            */
            Page? pageToNavigate = null;
            bool hasPermission = false;

            if (clickedButton == btnSoDoBan)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_SO_DO_BAN", "NV_GOI_MON", "NV_THANH_TOAN");
                pageToNavigate = new SoDoBanView();
            }
            else if (clickedButton == btnDatBan)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_DAT_BAN");
                pageToNavigate = new DatBanView();
            }
            else if (clickedButton == btnCheBien)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHE_BIEN");
                pageToNavigate = new CheBienView();
            }
            else if (clickedButton == btnThueSach)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_THUE_SACH");
                pageToNavigate = new ThueSachView();
            }
            else if (clickedButton == btnGiaoHang)
            {
                hasPermission = AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_GIAO_HANG");
                pageToNavigate = new GiaoHangView();
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
            if (currentNavButton != null) currentNavButton.IsChecked = false;
            currentNavButton = newButton;
            currentNavButton.IsChecked = true;
        }

        // =================================================================================
        // HỆ THỐNG THÔNG BÁO (Đồng bộ với Quản lý) 
        // =================================================================================

        private void BtnThongBao_Click(object sender, RoutedEventArgs e) => popThongBao.IsOpen = !popThongBao.IsOpen;

        private async Task CheckNotificationsAsync()
        {
            if (AuthService.CurrentUser == null) return;
            try
            {
                // 1. Quét danh sách các quyền nghiệp vụ hiện tại
                var myRoles = new List<string>();
                if (AuthService.CoQuyen("FULL_QL")) myRoles.Add("FULL_QL");
                if (AuthService.CoQuyen("FULL_NV")) myRoles.Add("FULL_NV");
                if (AuthService.CoQuyen("NV_DAT_BAN")) myRoles.Add("NV_DAT_BAN");
                if (AuthService.CoQuyen("NV_CHE_BIEN")) myRoles.Add("NV_CHE_BIEN");
                if (AuthService.CoQuyen("NV_GOI_MON")) myRoles.Add("NV_GOI_MON");
                if (AuthService.CoQuyen("NV_GIAO_HANG")) myRoles.Add("NV_GIAO_HANG");

                string roles = string.Join(",", myRoles);
                string rName = Uri.EscapeDataString(AuthService.CurrentUser.TenVaiTro ?? "");

                // 2. PHẢI CÓ THAM SỐ roleName
                string url = $"api/shared/thongbao/my-notifications?userId={AuthService.CurrentUser.IdNhanVien}&userRoles={roles}&roleName={rName}";

                var response = await ApiClient.Instance.GetFromJsonAsync<SharedThongBaoResponseDto>(url);

                if (response != null)
                {
                    lstThongBao.ItemsSource = response.Notifications;
                    if (response.UnreadCount > 0)
                    {
                        BadgeThongBao.Visibility = Visibility.Visible;
                        lblSoThongBao.Text = response.UnreadCount > 99 ? "99+" : response.UnreadCount.ToString();
                    }
                    else BadgeThongBao.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Lỗi load thông báo NV: {ex.Message}"); }
        }

        private async void ThongBaoItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SharedThongBaoItemDto tb)
            {
                // Đánh dấu đã xem nếu không phải thông báo thủ công từ Admin 
                bool isManual = tb.LoaiThongBao == "ThongBaoNhanVien" || tb.LoaiThongBao == "ThongBaoToanNhanVien" || tb.LoaiThongBao == "ThongBaoQuanLy";
                if (!tb.DaXem && !isManual)
                {
                    try
                    {
                        await ApiClient.Instance.PostAsync($"api/shared/thongbao/mark-as-read/{tb.IdThongBao}", null);
                        await CheckNotificationsAsync();
                    }
                    catch { }
                }

                popThongBao.IsOpen = false;
                lblDetailSender.Text = $"Người gửi: {tb.TenNhanVienTao}";
                lblDetailTime.Text = $"Thời gian: {tb.ThoiGianTao:dd/MM/yyyy HH:mm}";
                txtDetailContent.Text = tb.NoiDung;

                // XÁC ĐỊNH TRANG ĐIỀU HƯỚNG DÀNH CHO NHÂN VIÊN
                _targetPageForNotification = tb.LoaiThongBao switch
                {
                    "DatBan" => new DatBanView(),
                    "PhieuGoiMon" => new CheBienView(),
                    "DonHangMoi" => new GiaoHangView(),
                    "ThongBaoNhanVien" or "ThongBaoToanNhanVien" => null,
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
                // Cập nhật trạng thái nút Sidebar tương ứng
                if (_targetPageForNotification is DatBanView) UpdateSelectedButton(btnDatBan);
                else if (_targetPageForNotification is CheBienView) UpdateSelectedButton(btnCheBien);
                else if (_targetPageForNotification is GiaoHangView) UpdateSelectedButton(btnGiaoHang);

                MainFrame.Navigate(_targetPageForNotification);
            }
        }

        private void BtnDetailClose_Click(object sender, RoutedEventArgs e) => DetailThongBaoOverlay.Visibility = Visibility.Collapsed;

        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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