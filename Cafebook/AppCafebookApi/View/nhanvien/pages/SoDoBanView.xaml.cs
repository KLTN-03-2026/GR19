using AppCafebookApi.Services;
using AppCafebookApi.View.common;
using CafebookModel.Model.ModelApp;
using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class SoDoBanView : Page
    {
        private int? _idBanToHighlight = null;
        private bool _isDataLoaded = false;
        private DispatcherTimer _autoRefreshTimer;
        private class CreateOrderResponseDto
        {
            [System.Text.Json.Serialization.JsonPropertyName("idHoaDon")]
            public int idHoaDon { get; set; }
        }
        private enum SelectionMode { None, ChuyenBan, GopBan }

        private BanSoDoDto? _selectedBan = null;
        public ObservableCollection<BanSoDoDto> DisplayedTables { get; set; } = new ObservableCollection<BanSoDoDto>();
        private List<BanSoDoDto> _allTablesCache = new List<BanSoDoDto>();
        private List<KhuVucDto> _khuVucCache = new List<KhuVucDto>();
        private SelectionMode _currentMode = SelectionMode.None;

        public SoDoBanView()
        {
            InitializeComponent();
            this.DataContext = this;
            icBan.ItemsSource = DisplayedTables;
            _autoRefreshTimer = new DispatcherTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(5);
            _autoRefreshTimer.Tick += async (s, e) => await BackgroundRefreshAsync();
            _autoRefreshTimer.Start();
        }

        public SoDoBanView(int idBan)
        {
            InitializeComponent();
            this.DataContext = this;
            icBan.ItemsSource = DisplayedTables;
            _idBanToHighlight = idBan;
            _autoRefreshTimer = new DispatcherTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(60);
            _autoRefreshTimer.Tick += async (s, e) => await BackgroundRefreshAsync();
        }

        #region Tải Dữ Liệu và Lọc Khu Vực
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_NV", "NV_SO_DO_BAN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Sơ Đồ Bàn!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack)
                    this.NavigationService.GoBack();
                return;
            }

            await Task.Delay(450);

            if (!this.IsLoaded) return;

            if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
            {
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            ApplyPermissions();

            if (ApiClient.Instance.BaseAddress == null)
            {
                MessageBox.Show("Hệ thống chưa được cấu hình URL Server. Vui lòng kiểm tra file AppConfig.json!", "Thiếu cấu hình", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                MainPanel.Opacity = 0.5; 
                await ReloadDataAsync();
                MainPanel.Opacity = 1.0;

                HandleHighlightBan();

                _isDataLoaded = true; 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp sơ đồ: {ex.Message}", "Lỗi hệ thống");
            }
        }

        private void HandleHighlightBan()
        {
            if (_idBanToHighlight.HasValue)
            {
                var banToSelect = _allTablesCache.FirstOrDefault(b => b.IdBan == _idBanToHighlight.Value);
                if (banToSelect != null)
                {
                    if (!DisplayedTables.Any(b => b.IdBan == banToSelect.IdBan))
                    {
                        btnKhuVucAll.IsChecked = true;
                        UncheckOtherKhuVucButtons(btnKhuVucAll);
                        ApplyTableFilter(null);
                        icBan.UpdateLayout();
                    }

                    var banContainer = icBan.ItemContainerGenerator.ContainerFromItem(banToSelect) as FrameworkElement;
                    if (banContainer != null)
                    {
                        banContainer.BringIntoView();
                    }
                    ShowPanelForBan(banToSelect);
                }
                _idBanToHighlight = null;
            }
        }

        private void ApplyPermissions()
        {
            if (FindName("btnGoiMon") is Button btnGoiMon)
            {
                btnGoiMon.Visibility = AuthService.CoQuyen("FULL_ADMIN", "FULL_NV", "NV_GOI_MON", "NV_THANH_TOAN")
                                        ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task ReloadDataAsync()
        {
            // 1. Ghi nhớ khu vực đang được chọn để phục hồi sau khi load
            var selectedKhuVucBtn = FindCheckedKhuVucButton();
            int? selectedKhuVucId = null;
            if (selectedKhuVucBtn != null && selectedKhuVucBtn != btnKhuVucAll && selectedKhuVucBtn.DataContext is KhuVucDto dto)
            {
                selectedKhuVucId = dto.IdKhuVuc;
            }

            panelChuaChon.Visibility = Visibility.Visible;
            panelDaChon.Visibility = Visibility.Collapsed;

            // 2. Kiểm tra dữ liệu trong RAM (GlobalDataCache)
            bool needsKhuVucApi = true;
            if (GlobalDataCache.KhuVucCache != null && GlobalDataCache.KhuVucCache.Count > 0)
            {
                _khuVucCache = GlobalDataCache.KhuVucCache;
                if (FindName("icKhuVuc") is ItemsControl icKhuVuc)
                    icKhuVuc.ItemsSource = _khuVucCache;
                needsKhuVucApi = false; // Đã có trong RAM, không cần gọi API
            }

            bool needsBanApi = true;
            if (GlobalDataCache.BanCache != null && GlobalDataCache.BanCache.Count > 0)
            {
                _allTablesCache = GlobalDataCache.BanCache;
                needsBanApi = false; // Đã có trong RAM, không cần gọi API
            }

            // 3. Nếu RAM trống (trường hợp hiếm khi bỏ qua WelcomeWindow), dự phòng gọi API
            var tasks = new List<Task>();
            if (needsKhuVucApi) tasks.Add(LoadKhuVucSidebarAsync());
            if (needsBanApi) tasks.Add(LoadTablesAsync());

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            // 4. Vẽ bàn ra giao diện (sử dụng dữ liệu vừa lấy từ RAM hoặc API)
            ApplyTableFilter(selectedKhuVucId);

            // 5. Phục hồi trạng thái nút Khu Vực
            if (selectedKhuVucBtn != null)
                selectedKhuVucBtn.IsChecked = true;
            else if (btnKhuVucAll != null)
                btnKhuVucAll.IsChecked = true;

            // 6. KÍCH HOẠT ĐỒNG BỘ NGẦM (SMART UPDATE)
            // Nếu vừa lấy dữ liệu từ RAM, ta "bắn" một lệnh cập nhật ngầm ngay lập tức 
            // để kiểm tra xem trong lúc load RAM, có bàn nào thay đổi trạng thái không.
            if (!needsBanApi)
            {
                _ = BackgroundRefreshAsync();
            }
        }

        private async Task LoadKhuVucSidebarAsync()
        {
            try
            {
                string apiRoute = "api/app/sodoban/khuvuc-list";

                _khuVucCache = (await ApiClient.Instance.GetFromJsonAsync<List<KhuVucDto>>(apiRoute))
                                 ?? new List<KhuVucDto>();

                if (FindName("icKhuVuc") is ItemsControl icKhuVuc)
                    icKhuVuc.ItemsSource = _khuVucCache;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải khu vực: {ex.Message}", "Lỗi API");
            }
        }

        private async Task LoadTablesAsync()
        {
            try
            {
                var tables = await ApiClient.Instance.GetFromJsonAsync<List<BanSoDoDto>>("api/app/sodoban/tables");
                if (tables != null)
                {
                    _allTablesCache = tables;
                    UpdateDisplay(null);
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Lỗi tải khu vực: {ex.Message}", "Lỗi API");

            }
        }

        private void UpdateDisplay(int? khuVucId)
        {
            var filtered = khuVucId == null ? _allTablesCache : _allTablesCache.Where(b => b.IdKhuVuc == khuVucId).ToList();
            DisplayedTables.Clear();
            foreach (var table in filtered) DisplayedTables.Add(table); // Cập nhật mượt mà[cite: 5]
        }

        private void BtnKhuVuc_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as ToggleButton;
            if (clickedButton == null) return;
            UncheckOtherKhuVucButtons(clickedButton);
            int? khuVucId = null;
            if (clickedButton != btnKhuVucAll && clickedButton.DataContext is KhuVucDto selectedKhuVuc)
            {
                khuVucId = selectedKhuVuc.IdKhuVuc;
            }
            ApplyTableFilter(khuVucId);
        }

        private void ApplyTableFilter(int? khuVucId)
        {
            var filtered = khuVucId == null
                ? _allTablesCache
                : _allTablesCache.Where(ban => ban.IdKhuVuc == khuVucId).ToList();
            DisplayedTables.Clear();
            foreach (var ban in filtered)
            {
                DisplayedTables.Add(ban);
            }
        }

        private void UncheckOtherKhuVucButtons(ToggleButton? exception)
        {
            if (btnKhuVucAll != null && btnKhuVucAll != exception)
            {
                btnKhuVucAll.IsChecked = false;
            }
            foreach (var item in icKhuVuc.Items)
            {
                var container = icKhuVuc.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container == null) continue;
                var toggleButton = FindVisualChild<ToggleButton>(container);
                if (toggleButton != null && toggleButton != exception)
                {
                    toggleButton.IsChecked = false;
                }
            }
        }

        private T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
        {
            if (obj == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null)
                {
                    if (child is T)
                        return (T)child;
                    else
                    {
                        T? childOfChild = FindVisualChild<T>(child);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
            }
            return null;
        }

        private ToggleButton? FindCheckedKhuVucButton()
        {
            if (btnKhuVucAll != null && btnKhuVucAll.IsChecked == true) return btnKhuVucAll;
            foreach (var item in icKhuVuc.Items)
            {
                var container = icKhuVuc.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container == null) continue;
                var toggleButton = FindVisualChild<ToggleButton>(container);
                if (toggleButton != null && toggleButton.IsChecked == true)
                {
                    return toggleButton;
                }
            }
            return btnKhuVucAll;
        }

        #endregion

        // === LOGIC XỬ LÝ CHÍNH ===

        private void BtnDonMoi_Click(object sender, RoutedEventArgs e)
        {
            var virtualBan = new BanSoDoDto
            {
                IdBan = -1,
                SoBan = "Tại Quầy",
                TrangThai = "Trống",
                IdKhuVuc = null,
                IdHoaDonHienTai = null,
                TongTienHienTai = 0
            };
            UncheckOtherKhuVucButtons(null);
            _selectedBan = null;
            ShowPanelForBan(virtualBan);
        }

        private async void BtnBan_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var clickedBan = button?.DataContext as BanSoDoDto;
            if (clickedBan == null) return;

            if (_currentMode != SelectionMode.None)
            {
                await HandleTableSelectionAsync(clickedBan);
            }
            else
            {
                UncheckOtherKhuVucButtons(null);
                ShowPanelForBan(clickedBan);
            }
        }

        private void ShowPanelForBan(BanSoDoDto ban)
        {
            _selectedBan = ban;

            panelChuaChon.Visibility = Visibility.Collapsed;
            panelChonBan.Visibility = Visibility.Collapsed;
            panelDaChon.Visibility = Visibility.Visible;

            runSoBan.Text = _selectedBan.SoBan;
            runTrangThai.Text = _selectedBan.TrangThai;

            if (!string.IsNullOrEmpty(_selectedBan.ThongTinDatBan) && _selectedBan.TrangThai == "Trống")
            {
                tbThongTinDatBan.Text = _selectedBan.ThongTinDatBan;
                tbThongTinDatBan.Visibility = Visibility.Visible;
            }
            else
            {
                tbThongTinDatBan.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(_selectedBan.GhiChu))
            {
                tbGhiChu.Text = $"Ghi chú: {_selectedBan.GhiChu}";
                tbGhiChu.Visibility = Visibility.Visible;
            }
            else
            {
                tbGhiChu.Visibility = Visibility.Collapsed;
            }

            switch (_selectedBan.TrangThai)
            {
                case "Trống":
                    btnGoiMon.Content = "Tạo Hóa Đơn Mới";
                    btnGoiMon.IsEnabled = true;
                    btnChuyenBan.IsEnabled = false;
                    btnGopBan.IsEnabled = false;
                    btnBaoCaoSuCo.IsEnabled = (_selectedBan.IdBan > 0);
                    tbTongTienWrapper.Visibility = Visibility.Collapsed;
                    break;
                case "Có khách":
                    btnGoiMon.Content = "Gọi Món / Thanh Toán";
                    btnGoiMon.IsEnabled = true;
                    btnChuyenBan.IsEnabled = true;
                    btnGopBan.IsEnabled = true;
                    btnBaoCaoSuCo.IsEnabled = false;
                    tbTongTienWrapper.Visibility = Visibility.Visible;
                    runTongTien.Text = _selectedBan.TongTienHienTai.ToString("N0") + " đ";
                    break;
                case "Đã đặt":
                    btnGoiMon.Content = "Khách đặt (Mở Hóa Đơn)";
                    btnGoiMon.IsEnabled = true;
                    btnChuyenBan.IsEnabled = false;
                    btnGopBan.IsEnabled = false;
                    btnBaoCaoSuCo.IsEnabled = true;
                    tbTongTienWrapper.Visibility = Visibility.Collapsed;
                    break;
                case "Bảo trì":
                case "Tạm ngưng":
                    btnGoiMon.Content = "BÀN ĐANG BẢO TRÌ";
                    btnGoiMon.IsEnabled = false;
                    btnChuyenBan.IsEnabled = false;
                    btnGopBan.IsEnabled = false;
                    btnBaoCaoSuCo.IsEnabled = false;
                    tbTongTienWrapper.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void BtnGoiMon_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBan == null) return;

            // Nếu bàn đã có khách, chuyển thẳng sang màn hình gọi món
            if (_selectedBan.TrangThai == "Có khách")
            {
                int? idHoaDon = _selectedBan.IdHoaDonHienTai;
                if (idHoaDon.HasValue)
                {
                    this.NavigationService?.Navigate(new GoiMonView(idHoaDon.Value));
                }
                return;
            }

            if (AuthService.CurrentUser == null)
            {
                MessageBox.Show("Lỗi: Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Lỗi Phiên");
                return;
            }

            // Vô hiệu hóa nút trong lúc chờ API để tránh user click double tạo ra 2 hóa đơn
            btnGoiMon.IsEnabled = false;
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;
            int? idHoaDonMoi = null;

            try
            {
                HttpResponseMessage response;
                if (_selectedBan.IdBan > 0)
                {
                    response = await ApiClient.Instance.PostAsJsonAsync($"api/app/sodoban/createorder/{_selectedBan.IdBan}/{idNhanVien}", new { });
                }
                else
                {
                    string loaiHoaDon = (_selectedBan.IdBan == -1) ? "Tại quán" : "Mang về";
                    response = await ApiClient.Instance.PostAsJsonAsync($"api/app/sodoban/createorder-no-table/{idNhanVien}", loaiHoaDon);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CreateOrderResponseDto>();
                    if (result != null && result.idHoaDon > 0)
                    {
                        idHoaDonMoi = result.idHoaDon;
                    }

                    // XÓA: await ReloadDataAsync(); <--- Nguyên nhân gây giật lag
                    // Không cần tải lại sơ đồ vì chúng ta sắp chuyển trang ngay lập tức
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi tạo hóa đơn");
                    btnGoiMon.IsEnabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi API: {ex.Message}", "Lỗi");
                btnGoiMon.IsEnabled = true;
                return;
            }

            // Chuyển trang ngay sau khi có ID hóa đơn
            if (idHoaDonMoi.HasValue)
            {
                this.NavigationService?.Navigate(new GoiMonView(idHoaDonMoi.Value));

                if (_selectedBan.IdBan <= 0)
                {
                    ResetForm();
                }
            }
            else
            {
                btnGoiMon.IsEnabled = true; // Phục hồi nút nếu có lỗi logic
            }
        }

        private async void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Opacity = 0.5;
            await ReloadDataAsync();
            MainPanel.Opacity = 1.0;
        }

        private async void BtnBaoCaoSuCo_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBan == null) return;
            if (AuthService.CurrentUser == null)
            {
                MessageBox.Show("Lỗi: Phiên đăng nhập đã hết hạn.", "Lỗi Phiên");
                return;
            }
            int idNhanVien = AuthService.CurrentUser.IdNhanVien;
            var inputDialog = new InputDialogWindow("Báo cáo sự cố", $"Vui lòng mô tả sự cố cho bàn {_selectedBan.SoBan}:");
            if (inputDialog.ShowDialog() == true)
            {
                string ghiChu = inputDialog.InputText;
                if (string.IsNullOrWhiteSpace(ghiChu)) return;
                try
                {
                    var request = new BaoCaoSuCoRequestDto { GhiChuSuCo = ghiChu };
                    var response = await ApiClient.Instance.PostAsJsonAsync($"api/app/sodoban/reportproblem/{_selectedBan.IdBan}/{idNhanVien}", request);
                    if (response.IsSuccessStatusCode)
                    {
                        await ReloadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi báo cáo");
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi API: {ex.Message}", "Lỗi"); }
            }
        }

        private void StartSelectionMode(SelectionMode mode, string instructionText)
        {
            _currentMode = mode;
            selectionText.Text = instructionText;

            panelChuaChon.Visibility = Visibility.Collapsed;
            panelDaChon.Visibility = Visibility.Collapsed;
            panelChonBan.Visibility = Visibility.Visible;
        }

        private void BtnCancelSelect_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void BtnChuyenBan_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBan == null) return;
            StartSelectionMode(SelectionMode.ChuyenBan,
                $"Đang Chuyển [Bàn {_selectedBan.SoBan}]\n" +
                $"Vui lòng chọn một [Bàn Trống] làm Bàn Đích.");
        }

        private void BtnGopBan_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBan == null) return;
            StartSelectionMode(SelectionMode.GopBan,
                $"Đang Gộp [Bàn {_selectedBan.SoBan}]\n" +
                $"Vui lòng chọn một [Bàn Có Khách] khác làm Bàn Đích.");
        }

        private async Task HandleTableSelectionAsync(BanSoDoDto targetBan)
        {
            if (_selectedBan == null)
            {
                ResetForm();
                return;
            }
            int? idHoaDonNguon = _selectedBan.IdHoaDonHienTai;
            if (!idHoaDonNguon.HasValue)
            {
                MessageBox.Show("Bàn nguồn không có hóa đơn để thao tác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetForm();
                return;
            }

            try
            {
                HttpResponseMessage response;
                BanActionRequestDto request = new BanActionRequestDto
                {
                    IdHoaDonNguon = idHoaDonNguon.Value
                };

                if (_currentMode == SelectionMode.ChuyenBan)
                {
                    if (targetBan.TrangThai != "Trống")
                    {
                        MessageBox.Show($"Bàn đích [{targetBan.SoBan}] phải là [Bàn Trống].", "Chọn sai bàn", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    request.IdBanDich = targetBan.IdBan;
                    response = await ApiClient.Instance.PostAsJsonAsync("api/app/sodoban/move-table", request);
                }
                else
                {
                    if (targetBan.TrangThai != "Có khách")
                    {
                        MessageBox.Show($"Bàn đích [{targetBan.SoBan}] phải là [Bàn Có Khách].", "Chọn sai bàn", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (targetBan.IdBan == _selectedBan.IdBan)
                    {
                        MessageBox.Show("Không thể gộp bàn vào chính nó.", "Chọn sai bàn", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    request.IdHoaDonDich = targetBan.IdHoaDonHienTai;
                    response = await ApiClient.Instance.PostAsJsonAsync("api/app/sodoban/merge-table", request);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        _currentMode == SelectionMode.ChuyenBan ? "Chuyển bàn thành công!" : "Gộp bàn thành công!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    await ReloadDataAsync();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Thao tác thất bại: {error}", "Lỗi API", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ResetForm();
            }
        }

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                var newTables = await ApiClient.Instance.GetFromJsonAsync<List<BanSoDoDto>>("api/app/sodoban/tables");
                if (newTables == null) return;

                GlobalDataCache.BanCache = newTables;
                _allTablesCache = newTables;

                var selectedKhuVucBtn = FindCheckedKhuVucButton();
                int? selectedKhuVucId = (selectedKhuVucBtn != null && selectedKhuVucBtn != btnKhuVucAll && selectedKhuVucBtn.DataContext is KhuVucDto dto) ? dto.IdKhuVuc : null;

                var filteredTables = selectedKhuVucId == null ? _allTablesCache : _allTablesCache.Where(b => b.IdKhuVuc == selectedKhuVucId).ToList();

                if (DisplayedTables.Count != filteredTables.Count)
                {
                    DisplayedTables.Clear();
                    foreach (var table in filteredTables) DisplayedTables.Add(table);

                    if (_selectedBan != null)
                    {
                        var updatedSelectedBan = DisplayedTables.FirstOrDefault(b => b.IdBan == _selectedBan.IdBan);
                        if (updatedSelectedBan != null) ShowPanelForBan(updatedSelectedBan);
                        else ResetForm();
                    }
                    return;
                }

                bool isSelectedTableChanged = false; 

                for (int i = 0; i < DisplayedTables.Count; i++)
                {
                    var oldBan = DisplayedTables[i];
                    var newBan = filteredTables.FirstOrDefault(b => b.IdBan == oldBan.IdBan);

                    if (newBan != null)
                    {
                        bool isChanged = oldBan.TrangThai != newBan.TrangThai ||
                                         oldBan.TongTienHienTai != newBan.TongTienHienTai ||
                                         oldBan.IdHoaDonHienTai != newBan.IdHoaDonHienTai ||
                                         oldBan.ThongTinDatBan != newBan.ThongTinDatBan ||
                                         oldBan.GhiChu != newBan.GhiChu;

                        if (isChanged)
                        {
                            oldBan.TrangThai = newBan.TrangThai;
                            oldBan.TongTienHienTai = newBan.TongTienHienTai;
                            oldBan.IdHoaDonHienTai = newBan.IdHoaDonHienTai;
                            oldBan.ThongTinDatBan = newBan.ThongTinDatBan;
                            oldBan.GhiChu = newBan.GhiChu;
                            oldBan.IdKhuVuc = newBan.IdKhuVuc;

                            if (_selectedBan != null && _selectedBan.IdBan == oldBan.IdBan)
                            {
                                isSelectedTableChanged = true;
                            }
                        }
                    }
                }

                if (isSelectedTableChanged && _selectedBan != null)
                {
                    ShowPanelForBan(_selectedBan);
                }
            }
            catch
            {
            }
        }

        private async void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true) 
            {
                _autoRefreshTimer?.Start(); 
                if (_isDataLoaded)
                {
                    await BackgroundRefreshAsync();
                }
            }
            else 
            {
                _autoRefreshTimer?.Stop();
            }
        }

        private void ResetForm()
        {
            _selectedBan = null;

            panelDaChon.Visibility = Visibility.Collapsed;
            panelChonBan.Visibility = Visibility.Collapsed;
            panelChuaChon.Visibility = Visibility.Visible;

            _currentMode = SelectionMode.None;

            if (btnKhuVucAll != null)
            {
                btnKhuVucAll.IsChecked = true;
                UncheckOtherKhuVucButtons(btnKhuVucAll);
            }
            ApplyTableFilter(null);
        }
    }
}