using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class CheBienView : Page
    {
        private static readonly HttpClient _httpClient;
        private DispatcherTimer _refreshTimer;

        static CheBienView()
        {
            _httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                _httpClient.BaseAddress = new Uri(apiUrl);
            }
        }

        public CheBienView()
        {
            InitializeComponent();

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _refreshTimer.Tick += async (s, e) => await LoadDataAsync();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_CHE_BIEN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Màn hình chế biến!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack) this.NavigationService.GoBack();
                return;
            }

            if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (_httpClient.BaseAddress == null)
            {
                MessageBox.Show("Hệ thống chưa được cấu hình URL Server.", "Thiếu cấu hình", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await LoadDataAsync();
            _refreshTimer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop();
        }

        private void ShowLoading(bool isShow)
        {
            if (FindName("LoadingOverlay") is Border loading)
            {
                loading.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async Task LoadDataAsync()
        {
            ShowLoading(true);
            try
            {
                var items = await _httpClient.GetFromJsonAsync<List<CheBienItemDto>>("api/app/nhanvien/chebien/load");
                if (items != null)
                {
                    if (FindName("lblLastUpdated") is TextBlock lblUpdate)
                    {
                        lblUpdate.Text = $"(Cập nhật lúc {DateTime.Now:HH:mm:ss})";
                    }

                    var bepItems = items.Where(i => i.NhomIn == "Bếp").ToList();
                    var phaCheItems = items.Where(i => i.NhomIn == "Pha chế").ToList();

                    if (FindName("icBep") is ItemsControl icBep)
                        icBep.ItemsSource = bepItems;

                    if (FindName("icPhaChe") is ItemsControl icPhaChe)
                        icPhaChe.ItemsSource = phaCheItems;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi làm mới dữ liệu bếp: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void BtnStartItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CheBienItemDto;
            if (item == null) return;

            ShowLoading(true);
            try
            {
                // SỬA LỖI: Dùng PutAsJsonAsync gửi {} thay vì PutAsync gửi null để tránh lỗi 415
                var response = await _httpClient.PutAsJsonAsync($"api/app/nhanvien/chebien/start/{item.IdTrangThaiCheBien}", new { });
                if (response.IsSuccessStatusCode)
                {
                    await LoadDataAsync();
                }
                else MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API"); }
            finally { ShowLoading(false); }
        }

        private async void BtnCompleteItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CheBienItemDto;
            if (item == null) return;

            ShowLoading(true);
            try
            {
                // SỬA LỖI: Dùng PutAsJsonAsync gửi {} thay vì PutAsync gửi null để tránh lỗi 415
                var response = await _httpClient.PutAsJsonAsync($"api/app/nhanvien/chebien/complete/{item.IdTrangThaiCheBien}", new { });
                if (response.IsSuccessStatusCode)
                {
                    await LoadDataAsync();
                }
                else MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi API"); }
            finally { ShowLoading(false); }
        }

        private async void Border_CongThuc_Click(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as Border)?.DataContext as CheBienItemDto;
            if (item == null) return;

            _refreshTimer.Stop();
            ShowLoading(true);

            try
            {
                var congThucItems = await _httpClient.GetFromJsonAsync<List<CongThucItemDto>>($"api/app/nhanvien/chebien/congthuc/{item.IdSanPham}");

                if (FindName("lblCongThucTenMon") is TextBlock lbl) lbl.Text = $"Công thức: {item.TenMon}";
                if (FindName("lvCongThuc") is ListView lv) lv.ItemsSource = congThucItems;

                if (FindName("CongThucOverlay") is Grid overlay)
                {
                    overlay.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải công thức: {ex.Message}", "Lỗi API");
                _refreshTimer.Start();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // =========================================================================
        // LOGIC XỬ LÝ LỊCH SỬ CHẾ BIẾN (Tuân thủ FindName Protection)
        // =========================================================================
        private List<CheBienItemDto> _allLichSuItems = new List<CheBienItemDto>();

        private async void BtnLichSu_Click(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop(); // Dừng tải dữ liệu bếp liên tục

            // Dùng FindName để ẩn Tab và hiện Lịch Sử
            if (FindName("MainTabs") is TabControl tabs) tabs.Visibility = Visibility.Collapsed;
            if (FindName("panelLichSu") is Grid panel) panel.Visibility = Visibility.Visible;

            ShowLoading(true);
            try
            {
                var items = await _httpClient.GetFromJsonAsync<List<CheBienItemDto>>("api/app/nhanvien/chebien/history");
                if (items != null)
                {
                    _allLichSuItems = items;
                    ApplyLichSuFilter(); // Gọi hàm lọc để hiển thị
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch sử: {ex.Message}", "Lỗi API", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ĐÃ SỬA: Thêm từ khóa 'async' vào hàm
        private async void BtnDongLichSu_Click(object sender, RoutedEventArgs e)
        {
            // Dùng FindName để ẩn Lịch sử và hiện lại Tab Bếp
            if (FindName("panelLichSu") is Grid panel) panel.Visibility = Visibility.Collapsed;
            if (FindName("MainTabs") is TabControl tabs) tabs.Visibility = Visibility.Visible;

            // Xóa ô tìm kiếm
            if (FindName("txtTimKiemLichSu") is TextBox txt) txt.Text = string.Empty;

            _refreshTimer.Start(); // Tiếp tục tải dữ liệu bếp

            // ĐÃ SỬA: Thêm từ khóa 'await' để xử lý triệt để cảnh báo CS4014
            await LoadDataAsync();
        }

        private void TxtTimKiemLichSu_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyLichSuFilter();
        }

        private void ApplyLichSuFilter()
        {
            if (FindName("dgLichSu") is DataGrid dg && FindName("txtTimKiemLichSu") is TextBox txt)
            {
                string keyword = txt.Text.Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(keyword))
                {
                    dg.ItemsSource = _allLichSuItems;
                }
                else
                {
                    // Lọc theo Tên món, Số bàn hoặc Khu vực
                    var filtered = _allLichSuItems.Where(i =>
                        (i.TenMon != null && i.TenMon.ToLowerInvariant().Contains(keyword)) ||
                        (i.SoBan != null && i.SoBan.ToLowerInvariant().Contains(keyword)) ||
                        (i.NhomIn != null && i.NhomIn.ToLowerInvariant().Contains(keyword))
                    ).ToList();

                    dg.ItemsSource = filtered;
                }
            }
        }

        private void BtnCloseCongThuc_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("CongThucOverlay") is Grid overlay) overlay.Visibility = Visibility.Collapsed;
            if (FindName("lvCongThuc") is ListView lv) lv.ItemsSource = null;

            _refreshTimer.Start();
        }
    }
}