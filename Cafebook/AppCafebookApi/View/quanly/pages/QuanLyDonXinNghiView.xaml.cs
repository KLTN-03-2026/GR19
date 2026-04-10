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
    public partial class QuanLyDonXinNghiView : Page
    {
        private static readonly HttpClient httpClient;

        private List<QuanLyDonXinNghiGridDto> _allDonNghiList = new List<QuanLyDonXinNghiGridDto>();
        private QuanLyDonXinNghiGridDto? _selectedDon = null;

        static QuanLyDonXinNghiView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }

        public QuanLyDonXinNghiView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            if (!AuthService.CoQuyen("FULL_QL", "QL_DON_XIN_NGHI"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();
            cmbTrangThaiFilter.SelectedIndex = 1; // "Chờ duyệt"
            await LoadDataFromServerAsync();
            ResetForm();
        }

        private void ApplyPermissions()
        {
            if (FindName("btnGoToBaoCao") is Button btnBC)
                btnBC.Visibility = AuthService.CoQuyen("FULL_QL", "QL_BAO_CAO_NHAN_SU") ? Visibility.Visible : Visibility.Collapsed;
        }

        // ========================================================================
        // NGHIỆP VỤ TẢI VÀ LỌC DỮ LIỆU
        // ========================================================================
        private async Task LoadDataFromServerAsync()
        {
            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<QuanLyDonXinNghiGridDto>>("api/app/quanly-donxinnghi/search");
                if (response != null)
                {
                    _allDonNghiList = response;
                    ApplyClientFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách đơn: {ex.Message}", "Lỗi API");
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed;
            }
        }

        private void Filters_Changed(object sender, RoutedEventArgs e)
        {
            ApplyClientFilter();
        }

        private void ApplyClientFilter()
        {
            if (_allDonNghiList == null) return;

            var filtered = _allDonNghiList.AsEnumerable();

            // Lọc trạng thái
            string trangThai = (cmbTrangThaiFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
            if (trangThai != "Tất cả")
            {
                filtered = filtered.Where(d => d.TrangThai == trangThai);
            }

            // Lọc tìm kiếm (real-time)
            string search = txtSearchNhanVien.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(d => d.TenNhanVien.ToLower().Contains(search) || d.LyDo.ToLower().Contains(search));
            }

            dgDonXinNghi.ItemsSource = filtered.ToList();
            ResetForm();
        }

        // ========================================================================
        // TƯƠNG TÁC GIAO DIỆN
        // ========================================================================
        private void DgDonXinNghi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dg && dg.SelectedItem is QuanLyDonXinNghiGridDto selected)
            {
                _selectedDon = selected;

                txtDetailNhanVien.Text = selected.TenNhanVien;
                txtDetailLoaiDon.Text = selected.LoaiDon;
                txtDetailNgay.Text = $"{selected.NgayBatDau:dd/MM/yyyy} - {selected.NgayKetThuc:dd/MM/yyyy}";
                txtDetailLyDo.Text = selected.LyDo;

                if (selected.TrangThai == "Chờ duyệt")
                {
                    txtGhiChuPheDuyet.Text = "";
                    txtGhiChuPheDuyet.IsReadOnly = false;
                    formDuyetDon.Visibility = Visibility.Visible;
                }
                else
                {
                    txtGhiChuPheDuyet.Text = selected.GhiChuPheDuyet ?? "(Không có ghi chú)";
                    txtGhiChuPheDuyet.IsReadOnly = true;
                    formDuyetDon.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ResetForm()
        {
            _selectedDon = null;
            dgDonXinNghi.SelectedItem = null;

            txtDetailNhanVien.Text = "";
            txtDetailLoaiDon.Text = "";
            txtDetailNgay.Text = "";
            txtDetailLyDo.Text = "";
            txtGhiChuPheDuyet.Text = "";

            formDuyetDon.Visibility = Visibility.Collapsed;
        }

        // ========================================================================
        // DUYỆT / TỪ CHỐI / XÓA
        // ========================================================================
        private async Task HandleAction(string actionName, string urlSegment)
        {
            if (_selectedDon == null) return;

            var actionDto = new QuanLyDonXinNghiActionDto
            {
                IdNguoiDuyet = AuthService.CurrentUser?.IdNhanVien ?? 0,
                GhiChuPheDuyet = txtGhiChuPheDuyet.Text
            };

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                var response = await httpClient.PutAsJsonAsync($"api/app/quanly-donxinnghi/{urlSegment}/{_selectedDon.IdDonXinNghi}", actionDto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Đã {actionName} đơn thành công!", "Thông báo");
                    await LoadDataFromServerAsync();
                }
                else
                {
                    MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
                }
            }
            finally
            {
                if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnDuyet_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận DUYỆT đơn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await HandleAction("Duyệt", "approve");
            }
        }

        private async void BtnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận TỪ CHỐI đơn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await HandleAction("Từ chối", "reject");
            }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDon == null) return;

            if (MessageBox.Show("Xác nhận XÓA đơn này?", "Xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
                try
                {
                    var response = await httpClient.DeleteAsync($"api/app/quanly-donxinnghi/{_selectedDon.IdDonXinNghi}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Xóa đơn thành công!", "Thông báo");
                        await LoadDataFromServerAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
                    }
                }
                finally
                {
                    if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed;
                }
            }
        }

        // ========================================================================
        // ĐIỀU HƯỚNG
        // ========================================================================
        private void BtnGoToBaoCao_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang được phát triển.");
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