using AppCafebookApi.Services;
using AppCafebookApi.View.nhanvien.pages;
using CafebookModel.Utils;
using System;
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
//using CafebookModel.Model.ModelApp.NhanVien;

namespace AppCafebookApi.View.nhanvien
{
    public partial class ManHinhNhanVien : Window
    {
        // --- KHAI BÁO BIẾN ---
        private ToggleButton? currentNavButton;
        private DispatcherTimer _sidebarTimer;
        private static readonly HttpClient httpClient;

        public static string CurrentTrangThai { get; set; } = "KhongCoCa";

        // Tích hợp URL động
        static ManHinhNhanVien()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };
        }

        public ManHinhNhanVien()
        {
            InitializeComponent();

            _sidebarTimer = new DispatcherTimer();
            _sidebarTimer.Interval = TimeSpan.FromSeconds(10);
            //_sidebarTimer.Tick += async (s, e) => await CheckTrangThaiChamCongAsync();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = AuthService.CurrentUser;
            if (currentUser != null)
            {
                // Sử dụng đúng tên txtUserName và AvatarBorder của file XAML
                txtUserName.Text = currentUser.HoTen;
                txtUserRole.Text = currentUser.TenVaiTro;

                BitmapImage avatar = HinhAnhHelper.LoadImage(currentUser.AnhDaiDien ?? string.Empty, HinhAnhPaths.DefaultAvatar);
                AvatarBorder.Background = new ImageBrush(avatar) { Stretch = Stretch.UniformToFill };
            }

            // Phân quyền
            ApplyPermissions();

            // Mở trang đầu tiên (nếu có quyền mở Sơ đồ bàn, thì mở nó mặc định)
            if (btnSoDoBan.Visibility == Visibility.Visible)
            {
                btnSoDoBan.IsChecked = true;
                BtnSoDoBan_Click(btnSoDoBan, new RoutedEventArgs());
            }

            // Kích hoạt theo dõi nhân viên
           // await CheckTrangThaiChamCongAsync();
            _sidebarTimer.Start();
        }

        // =================================================================================
        // HÀM PHÂN QUYỀN 
        // =================================================================================
        private void ApplyPermissions()
        {
            bool isFullControl = AuthService.CoQuyen("FULL_NV", "FULL_QL");

            btnSoDoBan.Visibility = (isFullControl || AuthService.CoQuyen("NV_SO_DO_BAN")) ? Visibility.Visible : Visibility.Collapsed;
            btnDatBan.Visibility = (isFullControl || AuthService.CoQuyen("NV_DAT_BAN")) ? Visibility.Visible : Visibility.Collapsed;
            btnCheBien.Visibility = (isFullControl || AuthService.CoQuyen("NV_CHE_BIEN")) ? Visibility.Visible : Visibility.Collapsed;
            btnThueSach.Visibility = (isFullControl || AuthService.CoQuyen("NV_THUE_SACH")) ? Visibility.Visible : Visibility.Collapsed;
            btnGiaoHang.Visibility = (isFullControl || AuthService.CoQuyen("NV_GIAO_HANG")) ? Visibility.Visible : Visibility.Collapsed;

            btnThongTinCaNhan.Visibility = (isFullControl || AuthService.CoQuyen("NV_THONG_TIN")) ? Visibility.Visible : Visibility.Collapsed;
            btnChamCong.Visibility = (isFullControl || AuthService.CoQuyen("NV_CHAM_CONG")) ? Visibility.Visible : Visibility.Collapsed;
            btnLichLamViecCuaToi.Visibility = (isFullControl || AuthService.CoQuyen("NV_LICH_LAM_VIEC")) ? Visibility.Visible : Visibility.Collapsed;
            btnPhieuLuongCuaToi.Visibility = (isFullControl || AuthService.CoQuyen("NV_PHIEU_LUONG")) ? Visibility.Visible : Visibility.Collapsed;
        }

        // =================================================================================
        // XỬ LÝ CLICK TỪNG NÚT THEO XAML
        // =================================================================================
        private void HandleNavigation(ToggleButton clickedBtn, Page pageTarget, params string[] permissions)
        {
            if (clickedBtn == currentNavButton)
            {
                clickedBtn.IsChecked = true;
                return;
            }

            if (AuthService.CoQuyen("FULL_NV", "FULL_QL") || AuthService.CoQuyen(permissions))
            {
                if (currentNavButton != null) currentNavButton.IsChecked = false;
                currentNavButton = clickedBtn;
                currentNavButton.IsChecked = true;
                MainFrame.Navigate(pageTarget);
            }
            else
            {
                clickedBtn.IsChecked = false;
                if (currentNavButton != null) currentNavButton.IsChecked = true;
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSoDoBan_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnSoDoBan, new SoDoBanView(), "NV_SO_DO_BAN");
        private void BtnDatBan_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnDatBan, new DatBanView(), "NV_DAT_BAN");
        private void BtnCheBien_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnCheBien, new CheBienView(), "NV_CHE_BIEN");
        private void BtnThueSach_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnThueSach, new ThueSachView(), "NV_THUE_SACH");
        private void BtnGiaoHang_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnGiaoHang, new GiaoHangView(), "NV_GIAO_HANG");

        private void BtnThongTinCaNhan_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnThongTinCaNhan, new ThongTinCaNhanView(), "NV_THONG_TIN");
        private void BtnChamCong_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnChamCong, new ChamCongView(), "NV_CHAM_CONG");
        private void BtnLichLamViecCuaToi_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnLichLamViecCuaToi, new LichLamViecView(), "NV_LICH_LAM_VIEC");
        private void BtnPhieuLuongCuaToi_Click(object sender, RoutedEventArgs e) => HandleNavigation(btnPhieuLuongCuaToi, new PhieuLuongView(), "NV_PHIEU_LUONG");

        // =================================================================================
        // HÀM NGHIỆP VỤ NHÂN VIÊN VÀ ĐĂNG XUẤT (ĐÃ ĐÓNG BĂNG ĐỂ TEST)
        // =================================================================================

        /* TẠM THỜI ĐÓNG BĂNG API CHẤM CÔNG
        private async Task CheckTrangThaiChamCongAsync()
        {
            if (AuthService.CurrentUser == null) return;
            try
            {
                int idNhanVien = AuthService.CurrentUser.IdNhanVien;
                var response = await httpClient.GetAsync($"api/app/chamcong/dashboard/{idNhanVien}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ChamCongDashboardDto>();
                    if (data != null)
                    {
                        CurrentTrangThai = data.TrangThaiHienTai;
                        UpdateSidebarStatusUI();
                    }
                }
            }
            catch
            {
                CurrentTrangThai = "LoiDongBo";
                UpdateSidebarStatusUI();
            }
        }

        private void UpdateSidebarStatusUI()
        {
            // Logic cập nhật giao diện trạng thái chấm công...
        }

        public static string GetCurrentCheckInStatus() => CurrentTrangThai;
        */

        private void BtnThongBao_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Thông báo đang được bảo trì!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // HÀM ĐĂNG XUẤT CƠ BẢN (DÙNG ĐỂ TEST)
        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            /* ĐÃ BỎ RÀNG BUỘC TRẢ CA
            if (GetCurrentCheckInStatus() == "DaChamCong")
            {
                MessageBox.Show("Bạn chưa trả ca. Vui lòng nhấn \"TRẢ CA\" trước khi đăng xuất.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            */

            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (_sidebarTimer != null)
                {
                    _sidebarTimer.Stop(); // Dừng timer nếu nó đang chạy
                }

                AuthService.Logout();
                new ManHinhDangNhap().Show();
                this.Close();
            }
        }
    }
}