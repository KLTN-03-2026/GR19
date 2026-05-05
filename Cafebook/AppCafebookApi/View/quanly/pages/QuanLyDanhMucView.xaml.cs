using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyDanhMucView : Page
    {
        private List<QuanLyDanhMucGridDto> _dataList = new();
        private QuanLyDanhMucGridDto? _selectedItem;
        private bool _isAdding = false;

        private bool _isDataLoaded = false;

        public QuanLyDanhMucView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL") && !AuthService.CoQuyen("QL_DANH_MUC"))
            {
                MessageBox.Show("Từ chối truy cập module Danh mục!");
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                ApplyPermissions();

                await LoadDataAsync();

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải danh mục: {ex.Message}");
            }
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_DANH_MUC");
            if (FindName("btnThemMoi") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                // 1. KIỂM TRA RAM (Hiển thị ngay lập tức không có độ trễ)
                if (GlobalDataCache.QL_DanhMucCache != null && GlobalDataCache.QL_DanhMucCache.Count > 0)
                {
                    _dataList = GlobalDataCache.QL_DanhMucCache;
                    FilterData();

                    // 2. Kích hoạt cập nhật ngầm API
                    _ = BackgroundRefreshAsync();
                    return;
                }

                // 3. Dự phòng (Fallback): Nếu RAM trống do lỗi, tải trực tiếp từ API
                await FetchApiAndSetupUI();
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (ĐỒNG BỘ NGẦM)
        // ==========================================

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                // Gọi ngầm lấy dữ liệu danh mục mới nhất
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDanhMucGridDto>>("api/app/quanly-danhmuc");
                if (res != null)
                {
                    // Nạp vào RAM để các trang khác (Sản phẩm) dùng chung
                    GlobalDataCache.QL_DanhMucCache = res;
                    _dataList = res;

                    // Ghi nhớ dòng đang chọn để không làm gián đoạn thao tác
                    int? currentSelectedId = _selectedItem?.IdDanhMuc;

                    // Vẽ lại giao diện ngầm
                    FilterData();

                    // Phục hồi lại dòng đang chọn (nếu có)
                    if (currentSelectedId.HasValue && FindName("dgDanhMuc") is DataGrid dg)
                    {
                        var itemToSelect = _dataList.FirstOrDefault(x => x.IdDanhMuc == currentSelectedId);
                        if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                    }
                }
            }
            catch { /* Lỗi mạng thì bỏ qua, giữ nguyên UI cũ trên RAM */ }
        }

        private async Task FetchApiAndSetupUI()
        {
            var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDanhMucGridDto>>("api/app/quanly-danhmuc");
            if (res != null)
            {
                GlobalDataCache.QL_DanhMucCache = res;
                _dataList = res;
                FilterData();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();
        private void FilterData() { if (FindName("dgDanhMuc") is DataGrid dg) { string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? ""; dg.ItemsSource = string.IsNullOrEmpty(k) ? _dataList : _dataList.Where(x => x.TenDanhMuc.ToLower().Contains(k)).ToList(); } }

        private void DgDanhMuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDanhMuc") is DataGrid dg && dg.SelectedItem is QuanLyDanhMucGridDto item) { _selectedItem = item; _isAdding = false; if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true; if (FindName("lblTitle") is TextBlock title) title.Text = "Chi tiết Danh mục"; if (FindName("txtTenDanhMuc") is TextBox t1) t1.Text = item.TenDanhMuc; }
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e) { _selectedItem = new QuanLyDanhMucGridDto(); _isAdding = true; if (FindName("dgDanhMuc") is DataGrid dg) dg.SelectedItem = null; if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true; if (FindName("lblTitle") is TextBlock title) title.Text = "Thêm mới Danh mục"; if (FindName("txtTenDanhMuc") is TextBox t1) t1.Text = ""; }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DANH_MUC") || _selectedItem == null) return;
            string ten = (FindName("txtTenDanhMuc") as TextBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(ten)) { MessageBox.Show("Nhập tên danh mục!"); return; }
            var dto = new QuanLyDanhMucSaveDto { TenDanhMuc = ten };
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try { var res = _isAdding ? await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-danhmuc", dto) : await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-danhmuc/{_selectedItem.IdDanhMuc}", dto); if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu thành công!"); await LoadDataAsync(); } }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e) { if (!AuthService.CoQuyen("QL_DANH_MUC") || _selectedItem == null || _isAdding) return; if (MessageBox.Show("Xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-danhmuc/{_selectedItem.IdDanhMuc}"); if (res.IsSuccessStatusCode) await LoadDataAsync(); } }
        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}