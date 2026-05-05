using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Media;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyCaiDatView : Page
    {
        private List<QuanLyCaiDatDto> _allSettings = new();
        private ObservableCollection<QuanLyCaiDatDto> _viewSettings = new();
        private DispatcherTimer _notificationTimer;

        private bool _isDataLoaded = false;

        public QuanLyCaiDatView()
        {
            InitializeComponent();
            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _notificationTimer.Tick += (s, e) => { NotificationBorder.Visibility = Visibility.Collapsed; _notificationTimer.Stop(); };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL") && !AuthService.CoQuyen("QL_CAI_DAT"))
            {
                MessageBox.Show("Bạn không có quyền truy cập trang Cài đặt!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;
            try
            {
                await LoadDataAsync();

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải cài đặt: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                // 1. KIỂM TRA RAM (Hiển thị ngay lập tức toàn bộ cấu hình)
                if (GlobalDataCache.QL_CaiDatCache != null && GlobalDataCache.QL_CaiDatCache.Count > 0)
                {
                    _allSettings = GlobalDataCache.QL_CaiDatCache;
                    ApplyFilter();

                    // 2. Kích hoạt cập nhật ngầm API
                    _ = BackgroundRefreshAsync();
                    return;
                }

                // 3. Dự phòng (Fallback): Nếu RAM trống, gọi tải API trực tiếp
                await FetchApiAndSetupUI();
            }
            catch (Exception ex) { ShowNotification("Lỗi tải dữ liệu: " + ex.Message, true); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (ĐỒNG BỘ NGẦM)
        // ==========================================

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                // Gọi ngầm lấy dữ liệu cấu hình mới nhất
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyCaiDatDto>>("api/app/quanly-caidat/all");
                if (res != null)
                {
                    // Nạp vào RAM
                    GlobalDataCache.QL_CaiDatCache = res;
                    _allSettings = res;

                    // Cập nhật lại List UI mà không làm gián đoạn thao tác của user
                    ApplyFilter();
                }
            }
            catch { /* Lỗi mạng thì im lặng bỏ qua, người dùng vẫn xem được cấu hình cũ trên RAM */ }
        }

        private async Task FetchApiAndSetupUI()
        {
            var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyCaiDatDto>>("api/app/quanly-caidat/all");
            if (res != null)
            {
                GlobalDataCache.QL_CaiDatCache = res;
                _allSettings = res;
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.ToLower().Trim();
            var filtered = string.IsNullOrEmpty(keyword)
                ? _allSettings
                : _allSettings.Where(x => x.TenCaiDat.ToLower().Contains(keyword) || (x.MoTa?.ToLower().Contains(keyword) ?? false)).ToList();

            _viewSettings = new ObservableCollection<QuanLyCaiDatDto>(filtered);
            lvCaiDat.ItemsSource = _viewSettings;

            // Thiết lập Grouping
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvCaiDat.ItemsSource);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("Nhom"));
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is QuanLyCaiDatDto item)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    var response = await ApiClient.Instance.PutAsJsonAsync("api/app/quanly-caidat/update-single", item);
                    if (response.IsSuccessStatusCode) ShowNotification($"Đã lưu: {item.TenCaiDat}");
                    else ShowNotification("Lỗi khi lưu: " + await response.Content.ReadAsStringAsync(), true);
                }
                catch (Exception ex) { ShowNotification("Lỗi kết nối: " + ex.Message, true); }
                finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
            }
        }

        private void ShowNotification(string message, bool isError = false)
        {
            NotificationText.Text = message;
            NotificationBorder.Background = (SolidColorBrush)FindResource(isError ? "ErrorBrush" : "SuccessBrush");
            NotificationBorder.Visibility = Visibility.Visible;
            _notificationTimer.Stop();
            _notificationTimer.Start();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}