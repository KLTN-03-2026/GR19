using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http.Headers;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyPhanQuyenView : Page
    {
        private static readonly HttpClient httpClient;

        // Class Wrapper dùng để Binding Checkbox lên giao diện (Thêm INotifyPropertyChanged)
        public class QuyenWrapper : INotifyPropertyChanged
        {
            public string IdQuyen { get; set; } = string.Empty;
            public string TenQuyen { get; set; } = string.Empty;

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            private Visibility _visibility = Visibility.Visible;
            public Visibility Visibility
            {
                get => _visibility;
                set { _visibility = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class NhomQuyenWrapper : INotifyPropertyChanged
        {
            public string NhomName { get; set; } = string.Empty;
            public List<QuyenWrapper> Quyens { get; set; } = new List<QuyenWrapper>();

            private Visibility _visibility = Visibility.Visible;
            public Visibility Visibility
            {
                get => _visibility;
                set { _visibility = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<PhanQuyen_NhanVienDto> _allNhanViens = new List<PhanQuyen_NhanVienDto>();
        private List<NhomQuyenWrapper> _danhSachNhomQuyen = new List<NhomQuyenWrapper>();
        private PhanQuyen_NhanVienDto? _selectedNhanVien = null;
        private string _currentRoleScope = string.Empty;
        static QuanLyPhanQuyenView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }

        public QuanLyPhanQuyenView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            if (!AuthService.CoQuyen("FULL_QL", "QL_PHAN_QUYEN"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var loadingOverlay = this.FindName("LoadingOverlay") as Border;
            if (loadingOverlay != null) loadingOverlay.Visibility = Visibility.Visible;

            try
            {
                var nhanViens = await httpClient.GetFromJsonAsync<List<PhanQuyen_NhanVienDto>>("api/app/quanly-phanquyen/nhanvien");
                if (nhanViens != null)
                {
                    _allNhanViens = nhanViens;
                    var roles = new List<string> { "Tất cả" };
                    roles.AddRange(_allNhanViens.Select(n => n.TenVaiTro).Distinct());

                    var cmbRoleFilter = this.FindName("cmbRoleFilter") as ComboBox;
                    if (cmbRoleFilter != null)
                    {
                        cmbRoleFilter.ItemsSource = roles;
                        cmbRoleFilter.SelectedIndex = 0;
                    }
                }

                var allQuyen = await httpClient.GetFromJsonAsync<List<PhanQuyen_QuyenDto>>("api/app/quanly-phanquyen/quyen");
                if (allQuyen != null)
                {
                    _danhSachNhomQuyen = allQuyen
                        .GroupBy(q => q.NhomQuyen)
                        .Select(g => new NhomQuyenWrapper
                        {
                            NhomName = g.Key,
                            Quyens = g.Select(q => new QuyenWrapper { IdQuyen = q.IdQuyen, TenQuyen = q.TenQuyen, IsSelected = false }).ToList()
                        }).ToList();

                    var icNhomQuyen = this.FindName("icNhomQuyen") as ItemsControl;
                    if (icNhomQuyen != null) icNhomQuyen.ItemsSource = _danhSachNhomQuyen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
            finally
            {
                if (loadingOverlay != null) loadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================================
        // XỬ LÝ LỌC NHÂN VIÊN
        // ==========================================================
        private void TxtSearchNhanVien_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilterNhanVien();
        private void CmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilterNhanVien();

        private void ApplyFilterNhanVien()
        {
            var txtSearchNhanVien = this.FindName("txtSearchNhanVien") as TextBox;
            var cmbRoleFilter = this.FindName("cmbRoleFilter") as ComboBox;
            var dgNhanVien = this.FindName("dgNhanVien") as DataGrid;

            if (txtSearchNhanVien == null || cmbRoleFilter == null || dgNhanVien == null || _allNhanViens == null) return;

            var filteredList = _allNhanViens.AsEnumerable();
            string keyword = txtSearchNhanVien.Text?.Trim().ToLower() ?? "";

            if (!string.IsNullOrEmpty(keyword))
                filteredList = filteredList.Where(n => n.HoTen.ToLower().Contains(keyword));

            string selectedRole = cmbRoleFilter.SelectedItem as string ?? "";
            if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "Tất cả")
                filteredList = filteredList.Where(n => n.TenVaiTro == selectedRole);

            dgNhanVien.ItemsSource = filteredList.ToList();
        }

        // ==========================================================
        // XỬ LÝ TÌM KIẾM & LỌC QUYỀN THEO YÊU CẦU
        // ==========================================================
        private void TxtSearchQuyen_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilterQuyen();
        private void CmbFilterQuyen_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilterQuyen();
        private void QuyenCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Nếu đang ở chế độ lọc "Quyền đã cấp/chưa cấp" thì khi tick phải ẩn/hiện ngay
            var cmb = this.FindName("cmbFilterQuyen") as ComboBox;
            if (cmb != null && cmb.SelectedIndex != 0) ApplyFilterQuyen();
        }

        private void ApplyFilterQuyen()
        {
            var txtSearch = this.FindName("txtSearchQuyen") as TextBox;
            var cmbFilter = this.FindName("cmbFilterQuyen") as ComboBox;
            if (txtSearch == null || cmbFilter == null) return;

            string keyword = txtSearch.Text?.Trim().ToLower() ?? "";
            int filterMode = cmbFilter.SelectedIndex;

            foreach (var nhom in _danhSachNhomQuyen)
            {
                bool hasVisibleQuyen = false;
                foreach (var q in nhom.Quyens)
                {
                    // 1. Lọc theo Scope của Vai trò (Theo yêu cầu của bạn)
                    bool matchRoleScope = true;
                    if (_currentRoleScope == "Nhân viên")
                        matchRoleScope = q.IdQuyen.StartsWith("NV_") || q.IdQuyen.StartsWith("FULL_") || q.IdQuyen.StartsWith("CM_");
                    else if (_currentRoleScope == "Quản lý")
                        matchRoleScope = q.IdQuyen.StartsWith("QL_") || q.IdQuyen.StartsWith("FULL_") || q.IdQuyen.StartsWith("CM_");

                    // 2. Lọc theo từ khóa tìm kiếm
                    bool matchKeyword = string.IsNullOrEmpty(keyword) || q.TenQuyen.ToLower().Contains(keyword);

                    // 3. Lọc theo trạng thái Checkbox
                    bool matchStatus = (filterMode == 0) || (filterMode == 1 && q.IsSelected) || (filterMode == 2 && !q.IsSelected);

                    if (matchRoleScope && matchKeyword && matchStatus)
                    {
                        q.Visibility = Visibility.Visible;
                        hasVisibleQuyen = true;
                    }
                    else q.Visibility = Visibility.Collapsed;
                }
                nhom.Visibility = hasVisibleQuyen ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // ==========================================================
        // CÁC THAO TÁC NHANH (CHỌN TẤT CẢ)
        // ==========================================================

        private void BtnChonTatCa_Click(object sender, RoutedEventArgs e)
        {
            // Chỉ chọn những quyền đang HIỂN THỊ (đã qua lọc vai trò và tìm kiếm)
            foreach (var nhom in _danhSachNhomQuyen.Where(n => n.Visibility == Visibility.Visible))
                foreach (var q in nhom.Quyens.Where(q => q.Visibility == Visibility.Visible))
                    q.IsSelected = true;
        }

        private void BtnBoChonTatCa_Click(object sender, RoutedEventArgs e)
        {
            foreach (var nhom in _danhSachNhomQuyen)
                foreach (var q in nhom.Quyens)
                    q.IsSelected = false;
            ApplyFilterQuyen();
        }

        // ==========================================================
        // LOAD QUYỀN KHI CHỌN NHÂN VIÊN VÀ LƯU
        // ==========================================================
        private async void DgNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dg = this.FindName("dgNhanVien") as DataGrid;
            if (dg?.SelectedItem is PhanQuyen_NhanVienDto selected)
            {
                _selectedNhanVien = selected;
                _currentRoleScope = selected.TenVaiTro; // Cập nhật scope để lọc quyền

                var txtInfo = this.FindName("txtTenNhanVienChon") as TextBlock;
                var btnLuu = this.FindName("btnLuu") as Button;
                if (txtInfo != null) txtInfo.Text = $"Đang cấu hình cho: {selected.HoTen} ({selected.TenVaiTro})";
                if (btnLuu != null) btnLuu.IsEnabled = true;

                // Tải quyền từ Server
                var loading = this.FindName("LoadingOverlay") as Border;
                if (loading != null) loading.Visibility = Visibility.Visible;
                try
                {
                    var activeIds = await httpClient.GetFromJsonAsync<List<string>>($"api/app/quanly-phanquyen/nhanvien/{selected.IdNhanVien}/quyen");
                    foreach (var nhom in _danhSachNhomQuyen)
                        foreach (var q in nhom.Quyens)
                            q.IsSelected = activeIds?.Contains(q.IdQuyen) ?? false;

                    ApplyFilterQuyen(); // Tự động lọc hiển thị theo vai trò vừa chọn
                }
                finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhanVien == null) return;

            var selectedIds = new List<string>();
            foreach (var nhom in _danhSachNhomQuyen)
            {
                foreach (var q in nhom.Quyens)
                {
                    if (q.IsSelected)
                    {
                        selectedIds.Add(q.IdQuyen);
                    }
                }
            }

            var loadingOverlay = this.FindName("LoadingOverlay") as Border;
            if (loadingOverlay != null) loadingOverlay.Visibility = Visibility.Visible;

            try
            {
                var req = new PhanQuyen_SaveRequestDto { SelectedQuyenIds = selectedIds };
                var response = await httpClient.PostAsJsonAsync($"api/app/quanly-phanquyen/nhanvien/{_selectedNhanVien.IdNhanVien}/quyen", req);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đã lưu cấu hình phân quyền thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Lỗi lưu quyền: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
            }
            finally
            {
                if (loadingOverlay != null) loadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}