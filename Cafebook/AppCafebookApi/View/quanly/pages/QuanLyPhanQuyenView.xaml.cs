using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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

        // Class Wrapper dùng để Binding Checkbox lên giao diện
        public class QuyenWrapper
        {
            public string IdQuyen { get; set; } = string.Empty;
            public string TenQuyen { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
        }

        public class NhomQuyenWrapper
        {
            public string NhomName { get; set; } = string.Empty;
            public List<QuyenWrapper> Quyens { get; set; } = new List<QuyenWrapper>();
        }

        // Danh sách gốc chứa toàn bộ nhân viên tải từ Server
        private List<PhanQuyen_NhanVienDto> _allNhanViens = new List<PhanQuyen_NhanVienDto>();

        private List<NhomQuyenWrapper> _danhSachNhomQuyen = new List<NhomQuyenWrapper>();
        private PhanQuyen_NhanVienDto? _selectedNhanVien = null;

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
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                // 1. Tải danh sách Nhân viên
                var nhanViens = await httpClient.GetFromJsonAsync<List<PhanQuyen_NhanVienDto>>("api/app/quanly-phanquyen/nhanvien");
                if (nhanViens != null)
                {
                    _allNhanViens = nhanViens;

                    // Tự động trích xuất các Vai Trò có trong danh sách để nạp vào ComboBox
                    var roles = new List<string> { "Tất cả" };
                    roles.AddRange(_allNhanViens.Select(n => n.TenVaiTro).Distinct());

                    cmbRoleFilter.ItemsSource = roles;
                    cmbRoleFilter.SelectedIndex = 0; // Trigger hàm SelectionChanged -> Gọi ApplyFilter()
                }

                // 2. Tải danh sách tất cả Quyền và nhóm lại
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

                    icNhomQuyen.ItemsSource = _danhSachNhomQuyen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================================
        // CÁC HÀM XỬ LÝ TÌM KIẾM VÀ LỌC
        // ==========================================================
        private void TxtSearchNhanVien_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allNhanViens == null || _allNhanViens.Count == 0) return;

            var filteredList = _allNhanViens.AsEnumerable();

            // 1. Lọc theo chữ nhập vào (Tìm theo Họ Tên)
            string keyword = txtSearchNhanVien.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(keyword))
            {
                filteredList = filteredList.Where(n => n.HoTen.ToLower().Contains(keyword));
            }

            // 2. Lọc theo Combo Vai Trò
            string selectedRole = cmbRoleFilter.SelectedItem as string ?? "";
            if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "Tất cả")
            {
                filteredList = filteredList.Where(n => n.TenVaiTro == selectedRole);
            }

            // 3. Đổ dữ liệu đã lọc lên DataGrid
            dgNhanVien.ItemsSource = filteredList.ToList();
        }

        // ==========================================================
        // CÁC HÀM XỬ LÝ QUYỀN VÀ LƯU DỮ LIỆU
        // ==========================================================
        private async void DgNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgNhanVien.SelectedItem is PhanQuyen_NhanVienDto selected)
            {
                _selectedNhanVien = selected;
                txtTenNhanVienChon.Text = $"Đang cấu hình quyền cho: {selected.HoTen} ({selected.TenVaiTro})";
                btnLuu.IsEnabled = true;

                // Reset toàn bộ checkbox về false
                foreach (var nhom in _danhSachNhomQuyen)
                    foreach (var q in nhom.Quyens)
                        q.IsSelected = false;

                // Call API lấy danh sách quyền của người này
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    var userQuyenIds = await httpClient.GetFromJsonAsync<List<string>>($"api/app/quanly-phanquyen/nhanvien/{selected.IdNhanVien}/quyen");
                    if (userQuyenIds != null)
                    {
                        // Tick các quyền tương ứng
                        foreach (var nhom in _danhSachNhomQuyen)
                        {
                            foreach (var q in nhom.Quyens)
                            {
                                if (userQuyenIds.Contains(q.IdQuyen))
                                {
                                    q.IsSelected = true;
                                }
                            }
                        }
                    }

                    // Refresh UI binding
                    icNhomQuyen.ItemsSource = null;
                    icNhomQuyen.ItemsSource = _danhSachNhomQuyen;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải quyền: {ex.Message}");
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhanVien == null) return;

            // Lấy ra tất cả các ID Quyền đã được tick
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

            LoadingOverlay.Visibility = Visibility.Visible;
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
                LoadingOverlay.Visibility = Visibility.Collapsed;
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