using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Http.Headers;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;
using System.Windows.Shapes;
using System.Windows.Input;

namespace AppCafebookApi.View.quanly.pages
{
    // Model ảo hỗ trợ cho DataGrid hiển thị Duyệt ca
    public class PendingShiftModel
    {
        public string NgayLamStr { get; set; } = string.Empty;
        public string TenCa { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public int IdLichLamViec { get; set; }
    }

    public partial class QuanLyLichLamViecView : Page
    {
        //private static readonly HttpClient httpClient;
        private const double PIXELS_PER_HOUR = 60.0;

        private QuanLyLichLamViec_CaiDatDto? _caiDat;
        private List<QuanLyLichLamViec_ItemDto> _lichData = new();
        private List<QuanLyLichLamViec_CaDto> _caList = new();
        private List<QuanLyVaiTroLookupDto> _vaiTroList = new();
        private List<QuanLyNhanVienLookupDto> _allNhanVienList = new();

        private DateTime _currentStartDate;
        private int _soNgayHienThi = 7;
        private DateTime? _clickedDate = null;
        private int _editNhuCauId = 0;
        private QuanLyNhanVienLookupDto? _pendingAssignNhanVien = null;
        /*
        static QuanLyLichLamViecView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }
        */
        public QuanLyLichLamViecView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            dpTuNgay.SelectedDate = LayNgayDauTuan(DateTime.Now);
            await LoadMasterData();
            await RefreshDataAndDraw();
        }

        private DateTime LayNgayDauTuan(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private async Task LoadMasterData()
        {
            try
            {
                _caList = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyLichLamViec_CaDto>>("api/app/quanly-lichlamviec/ca-lam-viec") ?? new();
                cmbPopupCaNhuCau.ItemsSource = _caList;
                cmbPopupCaGiao.ItemsSource = _caList;
                dgCaLamViec.ItemsSource = _caList;

                _vaiTroList = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyVaiTroLookupDto>>("api/app/quanly-lichlamviec/vaitro") ?? new();
                cmbPopupVaiTro.ItemsSource = _vaiTroList;

                _allNhanVienList = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyNhanVienLookupDto>>("api/app/quanly-lichlamviec/nhanvien") ?? new();
                lbNhanVien.ItemsSource = _allNhanVienList;

                // --- BẮT ĐẦU: Nạp dữ liệu cho ComboBox Lọc Nhân viên (Cột bên trái) ---
                var filterList = new List<RoleLookupDto> { new RoleLookupDto { Id = 0, Name = "Tất cả vai trò" } };
                filterList.AddRange(_vaiTroList.Select(v => new RoleLookupDto { Id = v.IdVaiTro, Name = v.TenVaiTro }));

                cmbFilterVaiTroNV.ItemsSource = filterList;
                cmbFilterVaiTroNV.SelectedIndex = 0; // Mặc định chọn "Tất cả"
                // --- KẾT THÚC ---
            }
            catch { }
        }

        private async Task RefreshDataAndDraw()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                if (_caiDat == null)
                    _caiDat = await ApiClient.Instance.GetFromJsonAsync<QuanLyLichLamViec_CaiDatDto>("api/app/quanly-lichlamviec/cai-dat");

                _currentStartDate = dpTuNgay.SelectedDate ?? DateTime.Today;
                DateTime endDate = _currentStartDate.AddDays(_soNgayHienThi - 1);

                string url = $"api/app/quanly-lichlamviec/data?fromDate={_currentStartDate:yyyy-MM-dd}&toDate={endDate:yyyy-MM-dd}";
                _lichData = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyLichLamViec_ItemDto>>(url) ?? new();

                DrawGridAndSchedules();
            }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        // =======================================================
        // VẼ LƯỚI & KHỐI NHU CẦU / ĐĂNG KÝ TRÊN LỊCH
        // =======================================================
        private void DrawGridAndSchedules()
        {
            if (_caiDat == null) return;

            gridDayHeaders.ColumnDefinitions.Clear();
            gridDayHeaders.Children.Clear();
            gridTimeAxis.RowDefinitions.Clear();
            gridTimeAxis.Children.Clear();
            canvasLich.Children.Clear();

            int startHour = _caiDat.GioMoCua.Hours;
            int totalHours = _caiDat.GioDongCua.Hours - startHour;
            if (totalHours <= 0) totalHours = 24;

            double totalHeight = totalHours * PIXELS_PER_HOUR;
            gridTimeAxis.Height = totalHeight;
            canvasLich.Height = totalHeight;

            for (int i = 0; i <= totalHours; i++)
            {
                gridTimeAxis.RowDefinitions.Add(new RowDefinition { Height = new GridLength(PIXELS_PER_HOUR) });
                var txtHour = new TextBlock { Text = $"{startHour + i}:00", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, -8, 0, 0), Foreground = Brushes.Gray, FontSize = 11 };
                Grid.SetRow(txtHour, i);
                gridTimeAxis.Children.Add(txtHour);

                var line = new Line { X1 = 0, Y1 = i * PIXELS_PER_HOUR, X2 = 3000, Y2 = i * PIXELS_PER_HOUR, Stroke = new SolidColorBrush(Color.FromRgb(230, 230, 230)), StrokeThickness = 1 };
                canvasLich.Children.Add(line);
            }

            var gridColumns = new Grid { Width = double.NaN, Height = totalHeight };
            gridColumns.SetBinding(WidthProperty, new System.Windows.Data.Binding("ActualWidth") { Source = gridDayHeaders });

            for (int i = 0; i < _soNgayHienThi; i++)
            {
                DateTime currentDate = _currentStartDate.AddDays(i);
                gridDayHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gridColumns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // UI: Nút Tạo Slot & Nút Copy Tuần
                var headerPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 5, 0, 5) };
                headerPanel.Children.Add(new TextBlock { Text = currentDate.ToString("dd/MM - dddd"), FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });

                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 5, 0, 0) };

                var btnAddSlot = new Button { Content = "+ Tạo Slot", Background = Brushes.Transparent, Foreground = Brushes.DarkGreen, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, FontSize = 11, Padding = new Thickness(5, 2, 5, 2) };
                btnAddSlot.Click += (s, e) => {
                    _clickedDate = currentDate;
                    OpenNhuCauPopup(null);
                };
                btnPanel.Children.Add(btnAddSlot);

                var btnCopyWeek = new Button { Content = "📋 Copy Tuần", Background = Brushes.Transparent, Foreground = Brushes.DarkBlue, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, FontSize = 11, Padding = new Thickness(5, 2, 5, 2) };
                btnCopyWeek.Click += async (s, e) => {
                    var msg = $"Áp dụng lịch của ngày {currentDate:dd/MM} cho TẤT CẢ CÁC NGÀY KHÁC TRONG TUẦN?\n\nCảnh báo: Toàn bộ lịch cũ của các ngày khác sẽ bị xóa sạch!";
                    if (MessageBox.Show(msg, "Xác nhận chép lịch", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        LoadingOverlay.Visibility = Visibility.Visible;
                        var res = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-lichlamviec/copy-tuan", new { SourceDate = currentDate });
                        if (res.IsSuccessStatusCode) await RefreshDataAndDraw();
                        else MessageBox.Show("Lỗi: " + await res.Content.ReadAsStringAsync());
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                };
                btnPanel.Children.Add(btnCopyWeek);

                headerPanel.Children.Add(btnPanel);

                var colBorder = new Border { BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)), BorderThickness = new Thickness(0, 0, 1, 0) };
                Grid.SetColumn(colBorder, i);
                gridColumns.Children.Add(colBorder);

                Grid.SetColumn(headerPanel, i);
                gridDayHeaders.Children.Add(headerPanel);
            }
            canvasLich.Children.Add(gridColumns);

            var shiftsByDay = _lichData.GroupBy(l => (l.NgayLam.Date - _currentStartDate.Date).Days);

            foreach (var dayGroup in shiftsByDay)
            {
                int dayIndex = dayGroup.Key;
                if (dayIndex < 0 || dayIndex >= _soNgayHienThi) continue;

                var shiftsToday = dayGroup.OrderBy(s => s.GioBatDau).ToList();
                var overlapGroups = new List<List<QuanLyLichLamViec_ItemDto>>();

                // Thuật toán chia cột nếu ca làm trùng giờ
                foreach (var shift in shiftsToday)
                {
                    bool placed = false;
                    foreach (var group in overlapGroups)
                    {
                        if (group.Any(s => s.GioBatDau < shift.GioKetThuc && s.GioKetThuc > shift.GioBatDau))
                        {
                            group.Add(shift);
                            placed = true; break;
                        }
                    }
                    if (!placed) overlapGroups.Add(new List<QuanLyLichLamViec_ItemDto> { shift });
                }

                foreach (var group in overlapGroups)
                {
                    int overlapCount = group.Count;
                    var sortedGroup = group.OrderBy(s => s.LoaiYeuCau == "Part-time" ? 1 : 0).ToList();

                    for (int i = 0; i < overlapCount; i++)
                    {
                        var nhuCau = sortedGroup[i];
                        double yPos = Math.Max(0, (nhuCau.GioBatDau - _caiDat.GioMoCua).TotalHours * PIXELS_PER_HOUR);
                        double blockHeight = (nhuCau.GioKetThuc - nhuCau.GioBatDau).TotalHours * PIXELS_PER_HOUR;

                        double widthPercentage = 1.0 / overlapCount;

                        var blockContainer = new Grid();
                        blockContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(i * widthPercentage, GridUnitType.Star) });
                        blockContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(widthPercentage, GridUnitType.Star) });
                        blockContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength((overlapCount - 1 - i) * widthPercentage, GridUnitType.Star) });

                        var outerBorder = new Border
                        {
                            BorderBrush = new SolidColorBrush(Color.FromRgb(66, 165, 245)),
                            BorderThickness = new Thickness(2),
                            CornerRadius = new CornerRadius(6),
                            Margin = new Thickness(1, yPos, 1, 0),
                            Height = blockHeight - 2,
                            VerticalAlignment = VerticalAlignment.Top,
                            Background = new SolidColorBrush(Color.FromArgb(20, 66, 165, 245)),
                            Cursor = Cursors.Hand
                        };

                        var innerPanel = new StackPanel { Margin = new Thickness(3) };
                        innerPanel.Children.Add(new TextBlock { Text = $"{nhuCau.TenVaiTroYeuCau} ({nhuCau.LoaiYeuCau})", FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, FontSize = 11 });

                        string infoText = $"Cần: {nhuCau.SoLuongCan} | Ca: {nhuCau.TenCa}";
                        if (!string.IsNullOrEmpty(nhuCau.GhiChu)) infoText += $"\nGC: {nhuCau.GhiChu}";
                        innerPanel.Children.Add(new TextBlock { Text = infoText, FontSize = 10, Foreground = Brushes.Black, TextWrapping = TextWrapping.Wrap });

                        foreach (var nv in nhuCau.NhanViens)
                        {
                            var nvBlock = new Border
                            {
                                CornerRadius = new CornerRadius(3),
                                Margin = new Thickness(0, 2, 0, 0),
                                Padding = new Thickness(4, 2, 4, 2),
                                Background = nv.TrangThai == "Đã duyệt" ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) : new SolidColorBrush(Color.FromRgb(255, 193, 7))
                            };
                            // Tạo text hiển thị Tên + Nhiệm vụ
                            string nvText = nv.TenNhanVien;
                            if (!string.IsNullOrEmpty(nv.GhiChu)) nvText += $" ({nv.GhiChu})";

                            nvBlock.Child = new TextBlock { Text = nvText, Foreground = Brushes.White, FontSize = 10, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
                            // Click đúp vào user trên lịch để duyệt nhanh (Duyệt nhanh)
                            nvBlock.PreviewMouseLeftButtonDown += async (s, e) => {
                                e.Handled = true;
                                if (e.ClickCount == 2)
                                {
                                    if (nv.TrangThai == "Chờ duyệt" && MessageBox.Show($"Duyệt cho {nv.TenNhanVien} làm ca này?", "Duyệt nhanh", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                    {
                                        await ApiClient.Instance.PutAsync($"api/app/quanly-lichlamviec/duyet-ca/{nv.IdLichLamViec}", null);
                                        await RefreshDataAndDraw();
                                    }
                                }
                            };
                            innerPanel.Children.Add(nvBlock);
                        }

                        outerBorder.Child = innerPanel;
                        // Click đúp vào Block để mở Popup Nhu cầu
                        outerBorder.PreviewMouseLeftButtonDown += (s, e) =>
                        {
                            if (e.ClickCount == 2)
                            {
                                e.Handled = true;
                                OpenNhuCauPopup(nhuCau);
                            }
                        };

                        Grid.SetColumn(outerBorder, 1);
                        blockContainer.Children.Add(outerBorder);
                        Grid.SetColumn(blockContainer, dayIndex);
                        gridColumns.Children.Add(blockContainer);
                    }
                }
            }
        }

        // =======================================================
        // POPUP DUYỆT CA ĐĂNG KÝ HÀNG LOẠT
        // =======================================================
        private void BtnMoPopupDuyet_Click(object sender, RoutedEventArgs e)
        {
            var pendingList = _lichData.SelectMany(nc => nc.NhanViens.Select(nv => new PendingShiftModel
            {
                NgayLamStr = nc.NgayLam.ToString("dd/MM"),
                TenCa = nc.TenCa,
                TenNhanVien = nv.TenNhanVien,
                TenVaiTro = nv.TenVaiTro,
                IdLichLamViec = nv.IdLichLamViec
            })).Where(x => _lichData.Any(c => c.NhanViens.Any(n => n.IdLichLamViec == x.IdLichLamViec && n.TrangThai == "Chờ duyệt"))).ToList();

            dgChoDuyet.ItemsSource = pendingList;
            popupDuyetCa.Visibility = Visibility.Visible;
        }

        private void BtnClosePopupDuyet_Click(object sender, RoutedEventArgs e) => popupDuyetCa.Visibility = Visibility.Collapsed;

        private async void BtnDuyetCaGrid_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idLich)
            {
                await ApiClient.Instance.PutAsync($"api/app/quanly-lichlamviec/duyet-ca/{idLich}", null);
                await RefreshDataAndDraw();

                // SỬA Ở ĐÂY: Truyền lại sender và e thay vì (null, null)
                BtnMoPopupDuyet_Click(sender, e);
            }
        }

        // =======================================================
        // POPUP TẠO / SỬA NHU CẦU & QUẢN LÝ SLOT
        // =======================================================
        private void CanvasLich_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && (e.OriginalSource is Canvas || e.OriginalSource is Grid))
            {
                e.Handled = true;
                double mouseX = e.GetPosition(canvasLich).X;
                int colIndex = (int)(mouseX / (canvasLich.ActualWidth / _soNgayHienThi));
                if (colIndex >= 0 && colIndex < _soNgayHienThi)
                {
                    _clickedDate = _currentStartDate.AddDays(colIndex);
                    OpenNhuCauPopup(null);
                }
            }
        }

        private void OpenNhuCauPopup(QuanLyLichLamViec_ItemDto? existingNhuCau)
        {
            if (existingNhuCau == null)
            {
                _editNhuCauId = 0;
                txtNhuCauInfo.Text = $"TẠO MỚI | Ngày: {_clickedDate!.Value:dd/MM/yyyy}";
                cmbPopupVaiTro.SelectedIndex = -1;
                cmbPopupCaNhuCau.SelectedIndex = -1;
                cmbPopupLoai.SelectedIndex = 0;
                txtPopupSoLuong.Text = "1";
                txtPopupGhiChu.Text = "";
                btnDeleteNhuCau.Visibility = Visibility.Collapsed;

                // Ẩn vùng gán nhân viên khi tạo mới (chưa lưu vào DB nên không gán được)
                gridGanNhanVien.Visibility = Visibility.Collapsed;
                gridDsNhanVienSlot.Visibility = Visibility.Collapsed;
                lbNhanVienTrongSlot.ItemsSource = null;
            }
            else
            {
                _editNhuCauId = existingNhuCau.IdNhuCau;
                _clickedDate = existingNhuCau.NgayLam;
                txtNhuCauInfo.Text = $"CHỈNH SỬA | Ngày: {existingNhuCau.NgayLam:dd/MM/yyyy}";
                cmbPopupCaNhuCau.SelectedValue = existingNhuCau.IdCa;

                var vaitro = _vaiTroList.FirstOrDefault(v => v.TenVaiTro == existingNhuCau.TenVaiTroYeuCau);
                if (vaitro != null) cmbPopupVaiTro.SelectedValue = vaitro.IdVaiTro;

                cmbPopupLoai.Text = existingNhuCau.LoaiYeuCau;
                txtPopupSoLuong.Text = existingNhuCau.SoLuongCan.ToString();
                txtPopupGhiChu.Text = existingNhuCau.GhiChu;
                btnDeleteNhuCau.Visibility = Visibility.Visible;

                // ========================================================
                // [ĐÃ SỬA] Lọc NV theo Vai trò VÀ Loại bỏ những người đã gán
                // ========================================================
                // 1. Lấy danh sách ID của những nhân viên ĐÃ CÓ trong slot này
                var danhSachIdDaGan = existingNhuCau.NhanViens != null
                                      ? existingNhuCau.NhanViens.Select(nv => nv.IdNhanVien).ToList()
                                      : new List<int>();

                // 2. Lọc danh sách tổng: Cùng vai trò VÀ Không nằm trong danh sách đã gán
                var danhSachPhuHop = _allNhanVienList
                    .Where(nv => nv.TenVaiTro == existingNhuCau.TenVaiTroYeuCau && !danhSachIdDaGan.Contains(nv.IdNhanVien))
                    .ToList();

                cmbPopupAssignNV.ItemsSource = danhSachPhuHop;
                cmbPopupAssignNV.SelectedIndex = -1;
                gridGanNhanVien.Visibility = Visibility.Visible;
                // ========================================================

                // Tải danh sách NV đang trong ca
                if (existingNhuCau.NhanViens != null && existingNhuCau.NhanViens.Any())
                {
                    gridDsNhanVienSlot.Visibility = Visibility.Visible;
                    lbNhanVienTrongSlot.ItemsSource = existingNhuCau.NhanViens;
                }
                else
                {
                    gridDsNhanVienSlot.Visibility = Visibility.Collapsed;
                    lbNhanVienTrongSlot.ItemsSource = null;
                }
            }
            popupNhuCau.Visibility = Visibility.Visible;
        }

        private async void BtnPopupNhuCauSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPopupVaiTro.SelectedValue == null || cmbPopupCaNhuCau.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Vị trí và Ca làm!"); return;
            }
            if (!int.TryParse(txtPopupSoLuong.Text, out int sl) || sl <= 0)
            {
                MessageBox.Show("Số lượng phải là số lớn hơn 0!"); return;
            }

            var dto = new QuanLyLichLamViec_NhuCauSaveDto
            {
                NgayLam = _clickedDate!.Value,
                IdCa = (int)cmbPopupCaNhuCau.SelectedValue,
                IdVaiTro = (int)cmbPopupVaiTro.SelectedValue,
                SoLuongCan = sl,
                LoaiYeuCau = cmbPopupLoai.Text,
                GhiChu = txtPopupGhiChu.Text
            };

            popupNhuCau.Visibility = Visibility.Collapsed;
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage response;
                if (_editNhuCauId == 0) response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-lichlamviec/nhucau", dto);
                else response = await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-lichlamviec/nhucau/{_editNhuCauId}", dto);

                if (response.IsSuccessStatusCode) await RefreshDataAndDraw();
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
            }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnDeleteNhuCau_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xóa hoàn toàn Khung làm việc này cùng toàn bộ nhân viên bên trong?", "Xác nhận xóa Slot", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                popupNhuCau.Visibility = Visibility.Collapsed;
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-lichlamviec/nhucau/{_editNhuCauId}");
                    if (res.IsSuccessStatusCode) await RefreshDataAndDraw();
                    else MessageBox.Show(await res.Content.ReadAsStringAsync());
                }
                finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
            }
        }

        private async void BtnThemNVVaoSlot_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPopupAssignNV.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn/tìm nhân viên cần gán!"); return;
            }

            var dto = new QuanLyLichLamViec_AssignDto
            {
                IdNhanVien = (int)cmbPopupAssignNV.SelectedValue,
                IdCa = (int)cmbPopupCaNhuCau.SelectedValue,
                NgayLam = _clickedDate!.Value,
                TrangThai = "Đã duyệt",
                GhiChu = txtPopupAssignGhiChu.Text.Trim() // Đẩy ghi chú xuống API
            };

            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-lichlamviec/assign", dto);
                if (response.IsSuccessStatusCode)
                {
                    await RefreshDataAndDraw();

                    var updatedNhuCau = _lichData.FirstOrDefault(nc => nc.IdNhuCau == _editNhuCauId);
                    if (updatedNhuCau != null && updatedNhuCau.NhanViens.Any())
                    {
                        gridDsNhanVienSlot.Visibility = Visibility.Visible;
                        lbNhanVienTrongSlot.ItemsSource = updatedNhuCau.NhanViens;
                    }
                    cmbPopupAssignNV.SelectedIndex = -1;
                    txtPopupAssignGhiChu.Clear(); // Xóa trắng ô ghi chú
                }
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}", "Không thể xếp ca");
            }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoaNVTrongSlot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idLich)
            {
                if (MessageBox.Show("Rút nhân viên này khỏi ca làm?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                    try
                    {
                        var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-lichlamviec/xoa-ca/{idLich}");
                        if (res.IsSuccessStatusCode)
                        {
                            await RefreshDataAndDraw();

                            var updatedNhuCau = _lichData.FirstOrDefault(nc => nc.IdNhuCau == _editNhuCauId);
                            if (updatedNhuCau != null && updatedNhuCau.NhanViens.Any())
                            {
                                lbNhanVienTrongSlot.ItemsSource = updatedNhuCau.NhanViens;
                            }
                            else
                            {
                                gridDsNhanVienSlot.Visibility = Visibility.Collapsed;
                                lbNhanVienTrongSlot.ItemsSource = null;
                            }
                        }
                    }
                    finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
                }
            }
        }

        private void BtnPopupNhuCauCancel_Click(object sender, RoutedEventArgs e) => popupNhuCau.Visibility = Visibility.Collapsed;


        // =======================================================
        // GÁN NHÂN VIÊN BẰNG CÁCH KÉO THẢ
        // =======================================================
        private void LbNhanVien_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem is QuanLyNhanVienLookupDto nv)
                DragDrop.DoDragDrop(lb, nv, DragDropEffects.Copy);
        }

        private void CanvasLich_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(QuanLyNhanVienLookupDto)))
            {
                var nv = e.Data.GetData(typeof(QuanLyNhanVienLookupDto)) as QuanLyNhanVienLookupDto;
                double mouseX = e.GetPosition(canvasLich).X;
                int colIndex = (int)(mouseX / (canvasLich.ActualWidth / _soNgayHienThi));

                if (colIndex >= 0 && colIndex < _soNgayHienThi)
                {
                    _clickedDate = _currentStartDate.AddDays(colIndex);
                    _pendingAssignNhanVien = nv;

                    txtAssignInfo.Text = $"Ngày: {_clickedDate.Value:dd/MM/yyyy}\nNhân viên: {nv!.HoTen}";
                    cmbPopupCaGiao.SelectedIndex = -1;

                    if (_caList != null && _caList.Any())
                        cmbPopupCaGiao.ItemsSource = _caList;

                    popupAssign.Visibility = Visibility.Visible;
                }
            }
        }

        private async void BtnPopupAssignSave_Click(object sender, RoutedEventArgs e)
        {
            if (_clickedDate == null || cmbPopupCaGiao.SelectedValue == null || _pendingAssignNhanVien == null)
            {
                MessageBox.Show("Vui lòng chọn ca làm!"); return;
            }

            var dto = new QuanLyLichLamViec_AssignDto { IdNhanVien = _pendingAssignNhanVien.IdNhanVien, IdCa = (int)cmbPopupCaGiao.SelectedValue, NgayLam = _clickedDate.Value, TrangThai = "Đã duyệt" };
            popupAssign.Visibility = Visibility.Collapsed;
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                var response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-lichlamviec/assign", dto);
                if (response.IsSuccessStatusCode) await RefreshDataAndDraw();
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}", "Bị Từ Chối");
            }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private void BtnPopupAssignCancel_Click(object sender, RoutedEventArgs e) => popupAssign.Visibility = Visibility.Collapsed;


        // =======================================================
        // QUẢN LÝ CA LÀM MẪU
        // =======================================================
        private async void BtnQuanLyCa_Click(object sender, RoutedEventArgs e)
        {
            await LoadMasterData();
            popupQuanLyCa.Visibility = Visibility.Visible;
        }

        private void BtnCloseQuanLyCa_Click(object sender, RoutedEventArgs e) => popupQuanLyCa.Visibility = Visibility.Collapsed;

        private async void BtnAddCa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddTenCa.Text) || string.IsNullOrWhiteSpace(txtAddGioBD.Text) || string.IsNullOrWhiteSpace(txtAddGioKT.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ tên và khung giờ (VD: 07:00)."); return;
            }

            if (!TimeSpan.TryParse(txtAddGioBD.Text, out TimeSpan bd) || !TimeSpan.TryParse(txtAddGioKT.Text, out TimeSpan kt))
            {
                MessageBox.Show("Định dạng giờ không hợp lệ. Vui lòng nhập HH:mm"); return;
            }

            var dto = new QuanLyLichLamViec_CaDto { TenCa = txtAddTenCa.Text, GioBatDau = bd, GioKetThuc = kt };
            var response = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-lichlamviec/ca-lam-viec", dto);

            if (response.IsSuccessStatusCode)
            {
                txtAddTenCa.Clear(); txtAddGioBD.Clear(); txtAddGioKT.Clear();
                await LoadMasterData();
            }
            else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
        }

        private async void BtnDeleteCa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idCa)
            {
                if (MessageBox.Show("Xóa ca làm này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var response = await ApiClient.Instance.DeleteAsync($"api/app/quanly-lichlamviec/ca-lam-viec/{idCa}");
                    if (response.IsSuccessStatusCode) await LoadMasterData();
                    else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }


        // =======================================================
        // TÌM KIẾM & CHUYỂN NGÀY
        // =======================================================
        /*
        private void TxtSearchNV_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearchNV.Text.Trim().ToLower();
            lbNhanVien.ItemsSource = string.IsNullOrEmpty(search) ? _allNhanVienList : _allNhanVienList.Where(n => n.HoTen.ToLower().Contains(search) || n.TenVaiTro.ToLower().Contains(search)).ToList();
        }
        */
        // =======================================================
        // TÌM KIẾM & LỌC DANH SÁCH NHÂN VIÊN (CỘT BÊN TRÁI)
        // =======================================================
        private void CmbFilterVaiTroNV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyNhanVienFilter();
        }

        private void TxtSearchNV_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyNhanVienFilter();
        }

        private void ApplyNhanVienFilter()
        {
            if (_allNhanVienList == null) return;

            var query = _allNhanVienList.AsEnumerable();

            // 1. Lọc theo Text Search
            string search = txtSearchNV.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n => n.HoTen.ToLower().Contains(search) || n.TenVaiTro.ToLower().Contains(search));
            }

            // 2. Lọc theo ComboBox Vai trò
            if (cmbFilterVaiTroNV.SelectedItem is RoleLookupDto selectedRole && selectedRole.Id > 0)
            {
                query = query.Where(n => n.TenVaiTro == selectedRole.Name);
            }

            lbNhanVien.ItemsSource = query.ToList();
        }
        private async void DpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { if (this.IsLoaded) await RefreshDataAndDraw(); }

        private async void CmbViewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            _soNgayHienThi = cmbViewMode.SelectedIndex == 0 ? 7 : 1;
            if (_soNgayHienThi == 7 && dpTuNgay.SelectedDate.HasValue) dpTuNgay.SelectedDate = LayNgayDauTuan(dpTuNgay.SelectedDate.Value);
            await RefreshDataAndDraw();
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService?.CanGoBack == true) this.NavigationService.GoBack();
        }
    }
}