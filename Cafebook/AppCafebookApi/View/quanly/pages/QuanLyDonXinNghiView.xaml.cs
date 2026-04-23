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
        //private static readonly HttpClient httpClient;
        private List<QuanLyDonXinNghiGridDto> _allDonNghiList = new List<QuanLyDonXinNghiGridDto>();
        private QuanLyDonXinNghiGridDto? _selectedDon = null;
        /*
        static QuanLyDonXinNghiView()
        {
            string apiUrl = AppConfigManager.GetApiServerUrl() ?? "http://localhost:5166";
            httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }
        */
        public QuanLyDonXinNghiView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_DON_XIN_NGHI"))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await LoadDataFromServerAsync();
        }

        private async Task LoadDataFromServerAsync()
        {
            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                var response = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDonXinNghiGridDto>>("api/app/quanly-donxinnghi/search");
                if (response != null)
                {
                    _allDonNghiList = response;
                    ApplyClientFilter();
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed; }
        }

        private void Filters_Changed(object sender, RoutedEventArgs e) => ApplyClientFilter();
        private void Filters_Changed(object sender, TextChangedEventArgs e) => ApplyClientFilter();
        private void Filters_Changed(object sender, SelectionChangedEventArgs e) => ApplyClientFilter();

        private void ApplyClientFilter()
        {
            if (_allDonNghiList == null) return;
            var filtered = _allDonNghiList.AsEnumerable();

            if (FindName("cmbTrangThaiFilter") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item)
            {
                string trangThai = item.Content.ToString() ?? "Tất cả";
                if (trangThai != "Tất cả") filtered = filtered.Where(d => d.TrangThai == trangThai);
            }

            string search = (FindName("txtSearchNhanVien") as TextBox)?.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(d => d.TenNhanVien.ToLower().Contains(search) || d.LyDo.ToLower().Contains(search));
            }

            if (FindName("dgDonXinNghi") is DataGrid dg) dg.ItemsSource = filtered.ToList();
            ResetForm();
        }

        private void DgDonXinNghi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDonXinNghi") is DataGrid dg && dg.SelectedItem is QuanLyDonXinNghiGridDto selected)
            {
                _selectedDon = selected;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;

                if (FindName("txtDetailNhanVien") is TextBox t1) t1.Text = selected.TenNhanVien;
                if (FindName("txtDetailLoaiDon") is TextBox t2) t2.Text = selected.LoaiDon;
                if (FindName("txtDetailNgay") is TextBox t3) t3.Text = $"{selected.NgayBatDau:dd/MM/yyyy} - {selected.NgayKetThuc:dd/MM/yyyy}";
                if (FindName("txtDetailLyDo") is TextBox t4) t4.Text = selected.LyDo;

                if (selected.TrangThai == "Chờ duyệt")
                {
                    if (FindName("txtGhiChuPheDuyet") is TextBox t5) { t5.Text = ""; t5.IsReadOnly = false; }
                    if (FindName("formDuyetDon") is StackPanel p1) p1.Visibility = Visibility.Visible;
                }
                else
                {
                    if (FindName("txtGhiChuPheDuyet") is TextBox t6) { t6.Text = selected.GhiChuPheDuyet ?? "(Không có ghi chú)"; t6.IsReadOnly = true; }
                    if (FindName("formDuyetDon") is StackPanel p2) p2.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ResetForm()
        {
            _selectedDon = null;
            if (FindName("dgDonXinNghi") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = false;

            if (FindName("txtDetailNhanVien") is TextBox t1) t1.Text = "";
            if (FindName("txtDetailLoaiDon") is TextBox t2) t2.Text = "";
            if (FindName("txtDetailNgay") is TextBox t3) t3.Text = "";
            if (FindName("txtDetailLyDo") is TextBox t4) t4.Text = "";
            if (FindName("txtGhiChuPheDuyet") is TextBox t5) t5.Text = "";
            if (FindName("formDuyetDon") is StackPanel p) p.Visibility = Visibility.Collapsed;
        }

        // HIỂN THỊ POPUP DUYỆT ĐƠN & DANH SÁCH CA BỊ HỦY
        private async void BtnDuyet_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDon == null) return;

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                var affectedShifts = await ApiClient.Instance.GetFromJsonAsync<List<AffectedShiftDto>>($"api/app/quanly-donxinnghi/affected-shifts/{_selectedDon.IdDonXinNghi}");

                if (affectedShifts != null)
                {
                    if (FindName("dgAffectedShifts") is DataGrid dgShifts) dgShifts.ItemsSource = affectedShifts;

                    if (FindName("dgAffectedShifts") is DataGrid dg) dg.Visibility = affectedShifts.Any() ? Visibility.Visible : Visibility.Collapsed;
                    if (FindName("txtNoShifts") is TextBlock txtNo) txtNo.Visibility = affectedShifts.Any() ? Visibility.Collapsed : Visibility.Visible;

                    if (FindName("PopupConfirmShifts") is Border popup) popup.Visibility = Visibility.Visible;
                }
            }
            catch { MessageBox.Show("Không thể tải danh sách ca làm việc bị trùng. Vui lòng thử lại."); }
            finally { if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed; }
        }

        // TẮT POPUP
        private void BtnCancelPopup_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("PopupConfirmShifts") is Border popup) popup.Visibility = Visibility.Collapsed;
        }

        // CHÍNH THỨC XÁC NHẬN DUYỆT & XÓA CA TỪ POPUP
        private async void BtnConfirmApprove_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("PopupConfirmShifts") is Border popup) popup.Visibility = Visibility.Collapsed;
            await HandleAction("Duyệt", "approve");
        }

        // TỪ CHỐI ĐƠN
        private async void BtnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xác nhận TỪ CHỐI đơn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                await HandleAction("Từ chối", "reject");
        }

        // HÀM GỌI API CHUNG
        private async Task HandleAction(string actionName, string urlSegment)
        {
            if (_selectedDon == null) return;

            var actionDto = new QuanLyDonXinNghiActionDto
            {
                IdNguoiDuyet = AuthService.CurrentUser?.IdNhanVien ?? 0,
                GhiChuPheDuyet = (FindName("txtGhiChuPheDuyet") as TextBox)?.Text
            };

            if (FindName("LoadingOverlay") is Border loading) loading.Visibility = Visibility.Visible;
            try
            {
                var response = await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-donxinnghi/{urlSegment}/{_selectedDon.IdDonXinNghi}", actionDto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Đã {actionName} đơn thành công!", "Thông báo");
                    await LoadDataFromServerAsync();
                }
                else MessageBox.Show($"Lỗi: {await response.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border loadingEnd) loadingEnd.Visibility = Visibility.Collapsed; }
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