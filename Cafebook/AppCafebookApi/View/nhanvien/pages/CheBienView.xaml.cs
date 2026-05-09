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
using AppCafebookApi.View.Common; 

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class CheBienView : Page
    {
        private DispatcherTimer _refreshTimer;

        private bool _isDataLoaded = false;

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
            if (_isDataLoaded) return;

            if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_NV", "NV_CHE_BIEN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Màn hình chế biến!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack) this.NavigationService.GoBack();
                return;
            }

            if (ApiClient.Instance.BaseAddress == null)
            {
                MessageBox.Show("Hệ thống chưa được cấu hình URL Server.", "Thiếu cấu hình", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                await LoadDataAsync();

                _refreshTimer.Start();

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tại module Chế biến: {ex.Message}");
            }
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
                var items = await ApiClient.Instance.GetFromJsonAsync<List<CheBienItemDto>>("api/app/nhanvien/chebien/load");
                if (items != null)
                {
                    if (FindName("lblLastUpdated") is TextBlock lblUpdate)
                    {
                        lblUpdate.Text = $"(Cập nhật lúc {DateTime.Now:HH:mm:ss})";
                    }

                    var bepItems = items.Where(i => string.Equals(i.NhomIn, "Bếp", StringComparison.OrdinalIgnoreCase)).ToList();
                    var phaCheItems = items.Where(i => string.Equals(i.NhomIn, "Pha chế", StringComparison.OrdinalIgnoreCase)).ToList();

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
                var response = await ApiClient.Instance.PutAsJsonAsync($"api/app/nhanvien/chebien/start/{item.IdTrangThaiCheBien}", new { });
                if (response.IsSuccessStatusCode)
                {
                    await LoadDataAsync();
                    var temWindow = new TemDanPreviewWindow(item);
                    temWindow.ShowDialog();
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
                var response = await ApiClient.Instance.PutAsJsonAsync($"api/app/nhanvien/chebien/complete/{item.IdTrangThaiCheBien}", new { });
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
                var congThucItems = await ApiClient.Instance.GetFromJsonAsync<List<CongThucItemDto>>($"api/app/nhanvien/chebien/congthuc/{item.IdSanPham}");

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

        private List<CheBienItemDto> _allLichSuItems = new List<CheBienItemDto>();

        private async void BtnLichSu_Click(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop(); 

            if (FindName("MainTabs") is TabControl tabs) tabs.Visibility = Visibility.Collapsed;
            if (FindName("panelLichSu") is Grid panel) panel.Visibility = Visibility.Visible;

            ShowLoading(true);
            try
            {
                var items = await ApiClient.Instance.GetFromJsonAsync<List<CheBienItemDto>>("api/app/nhanvien/chebien/history");
                if (items != null)
                {
                    _allLichSuItems = items;
                    ApplyLichSuFilter(); 
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

        private async void BtnDongLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("panelLichSu") is Grid panel) panel.Visibility = Visibility.Collapsed;
            if (FindName("MainTabs") is TabControl tabs) tabs.Visibility = Visibility.Visible;
            if (FindName("txtTimKiemLichSu") is TextBox txt) txt.Text = string.Empty;

            _refreshTimer.Start(); 
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

        private void BtnInLai_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CheBienItemDto;
            if (item != null)
            {
                var temWindow = new TemDanPreviewWindow(item);
                temWindow.ShowDialog();
            }
        }
    }
}