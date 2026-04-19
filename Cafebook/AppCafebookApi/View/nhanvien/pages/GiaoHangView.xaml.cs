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
using System.Net.Http;
using System.Windows.Threading;
using System.Net.Http.Headers;

namespace AppCafebookApi.View.nhanvien.pages
{
    public partial class GiaoHangView : Page
    {
        private static readonly HttpClient httpClient;
        private bool _isLoading = false;
        private DispatcherTimer _searchTimer;
        private DispatcherTimer _autoRefreshTimer;

        static GiaoHangView()
        {
            httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl)) httpClient.BaseAddress = new Uri(apiUrl);
            else httpClient.BaseAddress = new Uri("http://127.0.0.1:5166");
        }

        public GiaoHangView()
        {
            InitializeComponent();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchTimer.Tick += async (s, e) => { _searchTimer.Stop(); await LoadDataAsync(false); };

            _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoRefreshTimer.Tick += async (s, e) => { await LoadDataAsync(true); };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_GIAO_HANG"))
            {
                MessageBox.Show("Bạn không có quyền truy cập Quản lý Giao Hàng.", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                if (this.NavigationService != null && this.NavigationService.CanGoBack) this.NavigationService.GoBack();
                return;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            ApplyPermissions();

            await LoadDataAsync(false);
            _autoRefreshTimer.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer.Stop();
        }

        private void ApplyPermissions()
        {
            if (!AuthService.CoQuyen("FULL_QL", "FULL_NV", "NV_GIAO_HANG_DUYET"))
            {
                if (FindName("btnConfirmAll") is Button btnDuyet) btnDuyet.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadDataAsync(bool isBackground)
        {
            if (_isLoading && !isBackground) return;
            if (!isBackground) _isLoading = true;

            try
            {
                string searchQuery = "";
                string statusQuery = "Chờ xác nhận";

                if (FindName("txtSearch") is TextBox txt)
                {
                    searchQuery = txt.Text;
                    if (searchQuery.Contains("Tìm theo")) searchQuery = "";
                }

                if (FindName("cmbStatusFilter") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item)
                {
                    statusQuery = item.Content?.ToString() ?? "Chờ xác nhận";
                }

                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchQuery)) queryParams.Add($"search={Uri.EscapeDataString(searchQuery)}");

                queryParams.Add($"status={Uri.EscapeDataString(statusQuery)}");
                string queryString = string.Join("&", queryParams);

                var response = await httpClient.GetFromJsonAsync<GiaoHangViewDto>($"api/app/nhanvien/giaohang/load?{queryString}");

                if (response != null)
                {
                    if (FindName("dgGiaoHang") is DataGrid dg) dg.ItemsSource = response.DonGiaoHang;
                    var shippersSource = (CollectionViewSource)this.FindResource("ShippersSource");
                    if (shippersSource != null) shippersSource.Source = response.NguoiGiaoHangSanSang;
                }
            }
            catch (Exception ex) { if (!isBackground) MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}"); }
            finally { if (!isBackground) _isLoading = false; }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            _searchTimer.Stop(); _searchTimer.Start();
        }

        private async void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            _searchTimer.Stop();
            await LoadDataAsync(false);
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("txtSearch") is TextBox txt) txt.Text = "";
            if (FindName("cmbStatusFilter") is ComboBox cmb) cmb.SelectedIndex = 1;
            await LoadDataAsync(false);
        }

        private async void BtnConfirmAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận chuyển TẤT CẢ đơn 'Chờ xác nhận' sang Bếp?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            try
            {
                var response = await httpClient.PostAsync("api/app/nhanvien/giaohang/confirm-all-pending", null);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Chuyển đơn thành công. Hệ thống đã tự động gửi Mail thông báo đến khách hàng.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (FindName("cmbStatusFilter") is ComboBox cmb) cmb.SelectedIndex = 2;
                    await LoadDataAsync(false);
                }
                else { MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}"); }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi kết nối: {ex.Message}"); }
        }

        private async void BtnChuyenCheBien_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idHoaDon)
            {
                if (MessageBox.Show($"Chuyển đơn HĐ{idHoaDon:D6} sang Bếp?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                var dto = new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = "Đang chuẩn bị" };
                await UpdateOrderAsync(idHoaDon, dto);
            }
        }

        private async void BtnHuyDon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idHoaDon)
            {
                if (MessageBox.Show($"HỦY đơn HĐ{idHoaDon:D6}? Không thể hoàn tác hành động này.", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
                var dto = new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = "Hủy" };
                await UpdateOrderAsync(idHoaDon, dto);
            }
        }

        private async void CmbNguoiGiaoHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            var comboBox = sender as ComboBox;
            var item = comboBox?.DataContext as GiaoHangItemDto;

            // ĐÃ VÁ LỖI CS8602: Thêm "comboBox == null" để chống Crash an toàn tuyệt đối
            if (item == null || comboBox == null || !comboBox.IsDropDownOpen) return;

            var newShipperId = (int?)comboBox.SelectedValue;
            string newStatus = item.TrangThaiGiaoHang ?? "";

            if ((item.TrangThaiGiaoHang == "Chờ lấy hàng" || item.TrangThaiGiaoHang == "Đang chuẩn bị") && newShipperId.HasValue)
            {
                newStatus = "Đang giao";
            }

            var dto = new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = newStatus, IdNguoiGiaoHang = newShipperId };
            await UpdateOrderAsync(item.IdHoaDon, dto);
        }

        private async void BtnHoanThanh_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idHoaDon)
            {
                if (MessageBox.Show($"Xác nhận Shipper đã giao thành công và thu tiền đơn HĐ{idHoaDon:D6}?", "Hoàn tất đơn", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No) return;
                var dto = new GiaoHangUpdateRequestDto { TrangThaiGiaoHang = "Hoàn thành" };
                await UpdateOrderAsync(idHoaDon, dto);
            }
        }

        private async Task UpdateOrderAsync(int idHoaDon, GiaoHangUpdateRequestDto dto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"api/app/nhanvien/giaohang/update/{idHoaDon}", dto);
                if (!response.IsSuccessStatusCode) { MessageBox.Show($"Thất bại: {await response.Content.ReadAsStringAsync()}"); }
                await LoadDataAsync(false);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi kết nối: {ex.Message}"); }
        }

        private async void BtnInPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idHoaDon)
            {
                try
                {
                    var printData = await httpClient.GetFromJsonAsync<PhieuGoiMonPrintDto>($"api/app/nhanvien/giaohang/print-data/{idHoaDon}");
                    if (printData != null)
                    {
                        var printWindow = new AppCafebookApi.View.Common.PhieuGiaoHangPreviewWindow(printData);
                        printWindow.ShowDialog();
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi in phiếu: {ex.Message}"); }
            }
        }
    }
}