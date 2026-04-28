using AppCafebookApi.Services;
using AppCafebookApi.View.Common;
using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class GiaoHangView : Page
    {
        private bool _isLoading = false;
        private DispatcherTimer _searchTimer;
        private DispatcherTimer _autoRefreshTimer;

        public GiaoHangView()
        {
            InitializeComponent();
            dpNgayLoc.SelectedDate = DateTime.Today; 

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchTimer.Tick += async (s, e) => { _searchTimer.Stop(); await LoadDataAsync(false); };

            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoRefreshTimer.Tick += async (s, e) => { await LoadDataAsync(true); };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_GIAO_HANG"))
            {
                MessageBox.Show("Bạn không có quyền truy cập.", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService?.CanGoBack == true) this.NavigationService.GoBack();
                return;
            }

            ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            ApplyPermissions();

            await LoadDataAsync(false);
            _autoRefreshTimer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e) { _autoRefreshTimer.Stop(); }

        private void ApplyPermissions()
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_GIAO_HANG_DUYET"))
            {
                if (FindName("btnConfirmAll") is Button btnDuyet) btnDuyet.Visibility = Visibility.Collapsed;
            }
        }

        private string GetSelectedTabStatus()
        {
            if (FindName("pnlTabs") is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is RadioButton rb && rb.IsChecked == true)
                    {
                        return rb.Content?.ToString() ?? "Tất cả";
                    }
                }
            }
            return "Chờ xác nhận";
        }

        private async Task LoadDataAsync(bool isBackground)
        {
            if (AuthService.CurrentUser == null) return;
            if (_isLoading && !isBackground) return;

            if (!isBackground) LoadingOverlay.Visibility = Visibility.Visible;
            _isLoading = true;

            try
            {
                string searchQuery = txtSearch.Text;
                string statusQuery = GetSelectedTabStatus();
                string dateQuery = dpNgayLoc.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

                var queryParams = new List<string>
                {
                    $"status={Uri.EscapeDataString(statusQuery)}",
                    $"date={dateQuery}"
                };

                if (!string.IsNullOrWhiteSpace(searchQuery))
                    queryParams.Add($"search={Uri.EscapeDataString(searchQuery)}");

                string queryString = string.Join("&", queryParams);
                var response = await ApiClient.Instance.GetFromJsonAsync<GiaoHangViewDto>($"api/app/nhanvien/giaohang/load?{queryString}");

                if (response != null)
                {
                    dgGiaoHang.ItemsSource = response.DonGiaoHang;
                    if (this.FindResource("ShippersSource") is CollectionViewSource shippersSource)
                        shippersSource.Source = response.NguoiGiaoHangSanSang;
                }
            }
            catch (Exception ex) { if (!isBackground) MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally
            {
                _isLoading = false;
                if (!isBackground) LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            _searchTimer.Stop(); _searchTimer.Start();
        }

        private async void TabStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded) return;
            _searchTimer?.Stop();
            await LoadDataAsync(false);
        }

        private async void DpNgayLoc_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            await LoadDataAsync(false);
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            dpNgayLoc.SelectedDate = DateTime.Today;
            if (FindName("rbChoXacNhan") is RadioButton rb) rb.IsChecked = true;
            await LoadDataAsync(false);
        }

        private async void BtnConfirmAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận duyệt tất cả đơn sang Bếp?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            try
            {
                var response = await ApiClient.Instance.PostAsync("api/app/nhanvien/giaohang/confirm-all-pending", null);
                if (response.IsSuccessStatusCode)
                {
                    if (FindName("rbDangChuanBi") is RadioButton rb) rb.IsChecked = true;
                    await LoadDataAsync(false);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void BtnChuyenCheBien_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (MessageBox.Show($"Nhận đơn {id} và In phiếu?", "Nhận đơn", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    bool ok = await UpdateOrderAsync(id, new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = "Đang chuẩn bị" });

                    if (ok)
                    {
                        if (FindName("rbDangChuanBi") is RadioButton rb) rb.IsChecked = true;
                        await ShowPrintPreviewAsync(id);
                    }
                }
            }
        }

        private async void BtnHuyDon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (MessageBox.Show($"HỦY đơn {id}?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await UpdateOrderAsync(id, new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = "Đã hủy" });
                }
            }
        }

        private async void CmbNguoiGiaoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            var cb = sender as ComboBox;
            if (cb?.DataContext is GiaoHangItemDto item && cb.IsDropDownOpen)
            {
                var dto = new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = item.TrangThaiGiaoHang, IdNguoiGiaoHang = (int?)cb.SelectedValue };
                bool ok = await UpdateOrderAsync(item.IdHoaDon, dto);
                if (ok) MessageBox.Show("Đã điều phối Shipper. Hệ thống đã gửi thông báo đến nhân viên.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id) await ShowPrintPreviewAsync(id);
        }

        private void BtnXemAnh_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string imageUrl && !string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    string fullUrl = imageUrl;
                    if (imageUrl.StartsWith("/"))
                    {
                        string baseUrl = ApiClient.Instance.BaseAddress?.ToString()?.TrimEnd('/') ?? string.Empty; fullUrl = $"{baseUrl}{imageUrl}";
                    }

                    var popup = new Window
                    {
                        Title = "Ảnh Xác Nhận Giao Hàng",
                        Width = 450,
                        Height = 600,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Background = new SolidColorBrush(Colors.Black),
                        Content = new Image
                        {
                            Source = new BitmapImage(new Uri(fullUrl, UriKind.Absolute)),
                            Stretch = Stretch.Uniform
                        }
                    };
                    popup.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể tải ảnh. Lỗi: {ex.Message}\nURL: {imageUrl}", "Lỗi tải ảnh", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Chưa có ảnh giao hàng cho đơn này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task<bool> UpdateOrderAsync(int id, GiaoHangUpdateRequestDto dto)
        {
            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync($"api/app/nhanvien/giaohang/update/{id}", dto);
                if (res.IsSuccessStatusCode) { await LoadDataAsync(false); return true; }
                return false;
            }
            catch { return false; }
        }

        private async Task ShowPrintPreviewAsync(int idHoaDon)
        {
            try
            {
                var printData = await ApiClient.Instance.GetFromJsonAsync<PhieuGoiMonPrintDto>($"api/app/nhanvien/giaohang/print-data/{idHoaDon}");
                if (printData != null) new PhieuGiaoHangPreviewWindow(printData).ShowDialog();
            }
            catch { }
        }
    }
}