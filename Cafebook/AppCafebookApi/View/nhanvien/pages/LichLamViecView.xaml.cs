using System.Net.Http.Headers;
using System.Net.Http.Json;
using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class LichLamViecView : Page
    {
        //private static readonly HttpClient httpClient;
        private const double PIXELS_PER_HOUR = 60.0; // 1 giờ = 60 pixels
        private LichLamViec_ConfigDto? _config;

        // Trạng thái hiển thị
        private DateTime _ngayBatDauHienThi;
        private int _soNgayHienThi = 7; // Mặc định xem 1 tuần
        private List<LichLamViec_CaNhanDto> _currentData = new();
        /*
        static LichLamViecView()
        {
            httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166") };
        }
        */
        public LichLamViecView()
        {
            InitializeComponent();
            _ngayBatDauHienThi = LayNgayDauTuan(DateTime.Today);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // BẢO MẬT LỚP 2: KIỂM TRA QUYỀN
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_LICH_LAM_VIEC"))
            {
                MessageBox.Show("Bạn không có quyền xem mục này!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            await LoadConfigAsync();
            await RefreshScheduleAsync();
        }

        private async Task LoadConfigAsync()
        {
            try
            {
                _config = await ApiClient.Instance.GetFromJsonAsync<LichLamViec_ConfigDto>("api/app/nhanvien/lichlamviec/config");
            }
            catch
            {
                // Fallback nếu không có cấu hình từ Server
                _config = new LichLamViec_ConfigDto { GioMoCua = new TimeSpan(6, 0, 0), GioDongCua = new TimeSpan(23, 0, 0) };
            }
        }

        private async Task RefreshScheduleAsync()
        {
            if (AuthService.CurrentUser == null) return;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;

            try
            {
                var denNgay = _ngayBatDauHienThi.AddDays(_soNgayHienThi - 1);

                // Cập nhật Text Toolbar
                if (FindName("txtKhungThoiGian") is TextBlock txtKhung)
                {
                    if (_soNgayHienThi == 1) txtKhung.Text = $"Ngày {_ngayBatDauHienThi:dd/MM/yyyy}";
                    else txtKhung.Text = $"Từ {_ngayBatDauHienThi:dd/MM} đến {denNgay:dd/MM/yyyy}";
                }

                var url = $"api/app/nhanvien/lichlamviec/my-schedule/{idNhanVien}?tuNgay={_ngayBatDauHienThi:yyyy-MM-dd}&denNgay={denNgay:yyyy-MM-dd}";
                var data = await ApiClient.Instance.GetFromJsonAsync<List<LichLamViec_CaNhanDto>>(url);

                _currentData = data ?? new List<LichLamViec_CaNhanDto>();

                // Gọi vẽ lịch
                VeBangLich();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed;
            }
        }

        // Sự kiện xảy ra khi CanvasSchedule thay đổi kích thước (Phóng to, thu nhỏ Window)
        private void CanvasSchedule_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VeBangLich(); // Tự động vẽ lại cho vừa khít
        }

        private void VeBangLich()
        {
            if (_config == null) return;

            var canvasTime = FindName("canvasTime") as Canvas;
            var canvasSchedule = FindName("canvasSchedule") as Canvas;
            var gridHeaderDays = FindName("gridHeaderDays") as Grid;

            if (canvasTime == null || canvasSchedule == null || gridHeaderDays == null) return;

            // Clear cũ
            canvasTime.Children.Clear();
            canvasSchedule.Children.Clear();
            gridHeaderDays.Children.Clear();
            gridHeaderDays.ColumnDefinitions.Clear();

            double startH = Math.Floor(_config.GioMoCua.TotalHours);
            double endH = Math.Ceiling(_config.GioDongCua.TotalHours);
            if (endH <= startH) endH = 24;

            double canvasWidth = canvasSchedule.ActualWidth;
            if (canvasWidth <= 0) return; // Chưa render xong

            double colWidth = canvasWidth / _soNgayHienThi;
            double canvasHeight = (endH - startH) * PIXELS_PER_HOUR;

            canvasTime.Height = canvasHeight;
            canvasSchedule.Height = canvasHeight;

            // ========================================================
            // 1. VẼ HEADER THỨ/NGÀY Ở TRÊN CÙNG
            // ========================================================
            for (int i = 0; i < _soNgayHienThi; i++)
            {
                gridHeaderDays.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                DateTime date = _ngayBatDauHienThi.AddDays(i);
                bool isToday = date.Date == DateTime.Today;

                Border headerBorder = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Background = isToday ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")) : Brushes.Transparent
                };

                string dayName = isToday ? "HÔM NAY" : GetVietnameseDayOfWeek(date.DayOfWeek);

                TextBlock tbHeader = new TextBlock
                {
                    Text = $"{dayName}\n{date:dd/MM}",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = isToday ? FontWeights.Black : FontWeights.Bold,
                    Foreground = isToday ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D27D2D")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"))
                };

                headerBorder.Child = tbHeader;
                Grid.SetColumn(headerBorder, i);
                gridHeaderDays.Children.Add(headerBorder);
            }

            // ========================================================
            // 2. VẼ TRỤC THỜI GIAN (CỘT BÊN TRÁI) VÀ ĐƯỜNG KẺ NGANG
            // ========================================================
            for (double h = startH; h <= endH; h++)
            {
                double y = (h - startH) * PIXELS_PER_HOUR;

                // Label Giờ
                TextBlock tbTime = new TextBlock
                {
                    Text = $"{h:00}:00",
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E")),
                    Width = 55,
                    TextAlignment = TextAlignment.Right
                };
                Canvas.SetTop(tbTime, y - 9); // Dịch lên một chút để căn giữa đường kẻ
                Canvas.SetLeft(tbTime, 0);
                canvasTime.Children.Add(tbTime);

                // Đường kẻ ngang qua lịch
                Line lineH = new Line
                {
                    X1 = 0,
                    X2 = canvasWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 2 } // Nét đứt
                };
                canvasSchedule.Children.Add(lineH);
            }

            // VẼ ĐƯỜNG KẺ DỌC (Chia cột ngày)
            for (int i = 1; i <= _soNgayHienThi; i++)
            {
                double x = i * colWidth;
                Line lineV = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = canvasHeight,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                    StrokeThickness = 1
                };
                canvasSchedule.Children.Add(lineV);
            }

            // ========================================================
            // 3. VẼ CÁC CA LÀM VIỆC (BLOCKS)
            // ========================================================
            foreach (var item in _currentData)
            {
                int dayOffset = (item.NgayLam.Date - _ngayBatDauHienThi.Date).Days;
                if (dayOffset < 0 || dayOffset >= _soNgayHienThi) continue; // Bỏ qua nếu nằm ngoài khung hiển thị

                double x = dayOffset * colWidth;
                double y = (item.GioBatDau.TotalHours - startH) * PIXELS_PER_HOUR;
                double height = (item.GioKetThuc.TotalHours - item.GioBatDau.TotalHours) * PIXELS_PER_HOUR;

                bool isApproved = item.TrangThai == "Đã duyệt";

                Border shiftBlock = new Border
                {
                    Width = colWidth - 8, // Trừ hao lề trái phải
                    Height = height - 4,  // Trừ hao lề trên dưới
                    Background = isApproved ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D27D2D")) : Brushes.Gray,
                    CornerRadius = new CornerRadius(6),
                    ToolTip = $"Ca: {item.TenCa}\nTrạng thái: {item.TrangThai}\nGhi chú: {(string.IsNullOrEmpty(item.GhiChu) ? "Không" : item.GhiChu)}"
                };

                TextBlock textBlock = new TextBlock
                {
                    Text = $"{item.TenCa}\n{item.GioBatDau:hh\\:mm} - {item.GioKetThuc:hh\\:mm}",
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                shiftBlock.Child = textBlock;

                Canvas.SetLeft(shiftBlock, x + 4);
                Canvas.SetTop(shiftBlock, y + 2);
                canvasSchedule.Children.Add(shiftBlock);
            }
        }

        // ========================================================
        // CÁC HÀM TIỆN ÍCH & SỰ KIỆN CLICK
        // ========================================================
        private string GetVietnameseDayOfWeek(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => ""
            };
        }

        private DateTime LayNgayDauTuan(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }


        private async void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            _ngayBatDauHienThi = _ngayBatDauHienThi.AddDays(_soNgayHienThi == 1 ? -1 : -7);
            await RefreshScheduleAsync();
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _ngayBatDauHienThi = _ngayBatDauHienThi.AddDays(_soNgayHienThi == 1 ? 1 : 7);
            await RefreshScheduleAsync();
        }

        private async void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            // CHỈ XEM HÔM NAY
            _soNgayHienThi = 1;
            _ngayBatDauHienThi = DateTime.Today;
            await RefreshScheduleAsync();
        }

        private async void BtnThisWeek_Click(object sender, RoutedEventArgs e)
        {
            // XEM CẢ TUẦN
            _soNgayHienThi = 7;
            _ngayBatDauHienThi = LayNgayDauTuan(DateTime.Today);
            await RefreshScheduleAsync();
        }
    }
}