using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class ChamCongView : Page
    {
        private DispatcherTimer _timerClock;
        private DispatcherTimer _timerWork;

        private DateTime? _gioVaoHienTai;
        private decimal _tongGioDaLamCache = 0;

        // Bảng màu cho Trạng thái chuẩn Material
        private readonly Brush _colorSuccess = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
        private readonly Brush _colorSuccessBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));

        private readonly Brush _colorDanger = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
        private readonly Brush _colorDangerBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));

        private readonly Brush _colorWarning = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F57F17"));
        private readonly Brush _colorWarningBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDE7"));

        private readonly Brush _colorInfo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D27D2D"));
        private readonly Brush _colorInfoBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8E1"));

        private readonly Brush _colorGray = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
        private readonly Brush _colorGrayBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));

        public ChamCongView()
        {
            InitializeComponent();

            _timerClock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timerClock.Tick += _timerClock_Tick;

            _timerWork = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timerWork.Tick += _timerWork_Tick;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // BẢO MẬT LỚP 2: KIỂM TRA QUYỀN
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHAM_CONG"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng Chấm công.", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack) this.NavigationService.GoBack();
                return;
            }

            _timerClock.Start();
            await LoadStatusAsync();

            if (FindName("dpChonThang") is DatePicker dp) dp.SelectedDate = DateTime.Now;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timerClock.Stop();
            _timerWork.Stop();
        }
        private void BtnXemLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("viewDiemDanh") is Grid vDiemDanh) vDiemDanh.Visibility = Visibility.Collapsed;
            if (FindName("viewLichSu") is Grid vLichSu) vLichSu.Visibility = Visibility.Visible;
            if (FindName("txtHeaderTitle") is TextBlock txtTitle) txtTitle.Text = "Lịch Sử Chấm Công";

            if (FindName("btnXemLichSu") is Button btnLichSu) btnLichSu.Visibility = Visibility.Collapsed;
            if (FindName("btnQuayLai") is Button btnBack) btnBack.Visibility = Visibility.Visible;

            if (FindName("dpChonThang") is DatePicker dp && dp.SelectedDate.HasValue)
            {
                _ = LoadLichSuAsync(dp.SelectedDate.Value.Month, dp.SelectedDate.Value.Year);
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {

            if (FindName("viewLichSu") is Grid vLichSu) vLichSu.Visibility = Visibility.Collapsed;
            if (FindName("viewDiemDanh") is Grid vDiemDanh) vDiemDanh.Visibility = Visibility.Visible;
            if (FindName("txtHeaderTitle") is TextBlock txtTitle) txtTitle.Text = "Chấm Công Nhân Viên";

            if (FindName("btnQuayLai") is Button btnBack) btnBack.Visibility = Visibility.Collapsed;
            if (FindName("btnXemLichSu") is Button btnLichSu) btnLichSu.Visibility = Visibility.Visible;
        }

        private void _timerClock_Tick(object? sender, EventArgs e)
        {
            if (FindName("txtClock") is TextBlock txtC) txtC.Text = DateTime.Now.ToString("HH:mm:ss");
            if (FindName("txtDate") is TextBlock txtD) txtD.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void _timerWork_Tick(object? sender, EventArgs e)
        {
            TimeSpan currentSession = TimeSpan.Zero;
            if (_gioVaoHienTai.HasValue)
            {
                currentSession = DateTime.Now - _gioVaoHienTai.Value;
            }

            TimeSpan totalSpan = TimeSpan.FromHours((double)_tongGioDaLamCache) + currentSession;

            if (FindName("txtThoiGianLam") is TextBlock txtT)
                txtT.Text = $"{(int)totalSpan.TotalHours:D2}:{totalSpan.Minutes:D2}:{totalSpan.Seconds:D2}";

            if (_gioVaoHienTai.HasValue)
                UpdateSidebarStatus($"Đang làm ({(int)totalSpan.TotalHours:D2}:{totalSpan.Minutes:D2})");
        }

        private void UpdateSidebarStatus(string status)
        {
            if (Window.GetWindow(this) is ManHinhNhanVien mainWindow)
            {
                mainWindow.UpdateSidebarStatus(status);
            }
        }

        private async Task LoadStatusAsync()
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            if (FindName("brdLoadingBtn") is Border brdLoad) brdLoad.Visibility = Visibility.Visible;
            if (FindName("btnVaoCa") is Button btnVao) btnVao.Visibility = Visibility.Collapsed;
            if (FindName("btnRaCa") is Button btnRa) btnRa.Visibility = Visibility.Collapsed;

            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<ChamCongDashboardDto>($"api/app/chamcong/status/{idNhanVien}");
                if (response != null) UpdateUI(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadStatusAsync Error]: {ex.Message}");
                SetStatusBadge("Lỗi kết nối", _colorDanger, _colorDangerBg);
                UpdateSidebarStatus("Lỗi đồng bộ");
            }
            finally
            {
                if (FindName("brdLoadingBtn") is Border brdLoadEnd) brdLoadEnd.Visibility = Visibility.Collapsed;
            }
        }

        private void SetStatusBadge(string text, Brush textBrush, Brush bgBrush)
        {
            if (FindName("txtTrangThaiCa") is TextBlock txt)
            {
                txt.Text = text;
                txt.Foreground = textBrush;
            }
            if (FindName("brdTrangThaiCa") is Border brd)
            {
                brd.Background = bgBrush;
            }
        }

        private void UpdateUI(ChamCongDashboardDto dto)
        {
            if (FindName("txtNhanVien") is TextBlock txtNv) txtNv.Text = $"Nhân viên: {dto.TenNhanVien}";

            if (dto.TenCa != null)
            {
                if (FindName("txtTenCa") is TextBlock txtTc) txtTc.Text = $"Ca hiện tại: {dto.TenCa}";
                if (FindName("txtThoiGianCa") is TextBlock txtTgc) txtTgc.Text = $"Quy định: {dto.GioBatDauCa:hh\\:mm} - {dto.GioKetThucCa:hh\\:mm}";
            }
            else
            {
                if (FindName("txtTenCa") is TextBlock txtTc) txtTc.Text = "Không có ca làm việc";
                if (FindName("txtThoiGianCa") is TextBlock txtTgc) txtTgc.Text = "";
            }

            if (FindName("txtGioVao") is TextBlock txtGv) txtGv.Text = dto.LanVaoGanNhat?.ToString("HH:mm") ?? "--:--";
            if (FindName("txtGioRa") is TextBlock txtGr) txtGr.Text = dto.LanRaGanNhat?.ToString("HH:mm") ?? "--:--";

            _tongGioDaLamCache = dto.TongGioLamHienTai;
            _timerWork.Stop();
            _gioVaoHienTai = null;

            // Đặt lại mặc định cho Nút Vào Ca
            if (FindName("btnVaoCa") is Button btnVaoDefault)
            {
                btnVaoDefault.IsEnabled = true;
                btnVaoDefault.Content = "VÀO CA BẮT ĐẦU LÀM";
            }

            if (dto.TrangThai == "KhongCoCa")
            {
                SetStatusBadge("Ngoài ca làm việc", _colorGray, _colorGrayBg);
                UpdateSidebarStatus("Không có ca");
            }
            // [BỔ SUNG]: TRẠNG THÁI CHƯA ĐẾN GIỜ -> HIỆN NÚT NHƯNG BỊ MỜ KHÔNG CHO BẤM
            else if (dto.TrangThai == "ChuaDenGio")
            {
                SetStatusBadge("CHƯA ĐẾN GIỜ VÀO CA", _colorGray, _colorGrayBg);
                UpdateSidebarStatus("Sắp tới ca");

                if (FindName("btnVaoCa") is Button btnVao)
                {
                    btnVao.Visibility = Visibility.Visible;
                    btnVao.IsEnabled = false; // Khóa nút
                    var gioMoMoChamCong = dto.GioBatDauCa?.Subtract(TimeSpan.FromMinutes(30));
                    btnVao.Content = $"MỞ CHẤM CÔNG LÚC {gioMoMoChamCong:hh\\:mm}";
                }
                if (FindName("btnRaCa") is Button btnRa) btnRa.Visibility = Visibility.Collapsed;
            }
            else if (dto.TrangThai == "ChoVaoCa")
            {
                SetStatusBadge("ĐÃ MỞ CHẤM CÔNG", _colorWarning, _colorWarningBg);
                UpdateSidebarStatus("Chưa chấm công");
                if (FindName("btnVaoCa") is Button btnVao) btnVao.Visibility = Visibility.Visible;
                if (FindName("btnRaCa") is Button btnRa) btnRa.Visibility = Visibility.Collapsed;
            }
            else if (dto.DangTrongCa)
            {
                SetStatusBadge("Đang làm việc", _colorSuccess, _colorSuccessBg);
                if (FindName("btnVaoCa") is Button btnVao) btnVao.Visibility = Visibility.Collapsed;
                if (FindName("btnRaCa") is Button btnRa) btnRa.Visibility = Visibility.Visible;
                if (FindName("txtGioRa") is TextBlock txtGr2) txtGr2.Text = "Đang chạy...";

                _gioVaoHienTai = dto.LanVaoGanNhat;
                _timerWork.Start();
            }
            else 
            {
                SetStatusBadge("Hoàn thành mọi ca", _colorSuccess, _colorSuccessBg);
                if (FindName("btnVaoCa") is Button btnVao) btnVao.Visibility = Visibility.Collapsed;
                if (FindName("btnRaCa") is Button btnRa) btnRa.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnVaoCa_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            try
            {
                var response = await ApiClient.Instance.PostAsync($"api/app/chamcong/clock-in/{idNhanVien}", null);
                if (response.IsSuccessStatusCode)
                {
                    await LoadStatusAsync();
                }
                else MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi kết nối: {ex.Message}"); }
        }

        private async void BtnRaCa_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            try
            {
                var response = await ApiClient.Instance.PostAsync($"api/app/chamcong/clock-out/{idNhanVien}", null);
                if (response.IsSuccessStatusCode)
                {
                    await LoadStatusAsync();
                }
                else MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi kết nối: {ex.Message}"); }
        }

        private async void dpChonThang_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dpChonThang") is DatePicker dp && dp.SelectedDate.HasValue)
                await LoadLichSuAsync(dp.SelectedDate.Value.Month, dp.SelectedDate.Value.Year);
        }

        private async Task LoadLichSuAsync(int month, int year)
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<LichSuChamCongPageDto>($"api/app/chamcong/lich-su/{idNhanVien}?thang={month}&nam={year}");

                if (response != null)
                {
                    if (FindName("dgLichSu") is DataGrid dg) dg.ItemsSource = response.LichSuChamCong;
                    if (FindName("txtTongGio") is TextBlock txtTg) txtTg.Text = $"{response.ThongKe.TongGioLam:N2} giờ";
                    if (FindName("txtDiTre") is TextBlock txtDt) txtDt.Text = $"{response.ThongKe.SoLanDiTre} lần";
                    if (FindName("txtVeSom") is TextBlock txtVs) txtVs.Text = $"{response.ThongKe.SoLanVeSom} lần";
                }
            }
            catch { /* Im lặng nếu lỗi mạng */ }
        }
    }
}