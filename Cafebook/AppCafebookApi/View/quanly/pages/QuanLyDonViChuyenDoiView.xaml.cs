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
    public partial class QuanLyDonViChuyenDoiView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyDonViChuyenDoiGridDto> _dataList = new();
        private List<LookupNguyenLieuDvtDto> _nlList = new();
        private QuanLyDonViChuyenDoiGridDto? _selectedItem;
        private bool _isAdding = false;

        static QuanLyDonViChuyenDoiView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLyDonViChuyenDoiView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // LỚP 2 BẢO MẬT
            if (!AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI")) { MessageBox.Show("Từ chối!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions(); await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            // LỚP 1 BẢO MẬT + FINDNAME
            bool canEdit = AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var nls = await httpClient.GetFromJsonAsync<List<LookupNguyenLieuDvtDto>>("api/app/quanly-donvichuyendoi/lookup-nguyenlieu");
                if (nls != null)
                {
                    _nlList = nls;
                    if (FindName("cmbNguyenLieu") is ComboBox cb1) cb1.ItemsSource = _nlList;
                    if (FindName("cmbFilterNL") is ComboBox cb2) { var flt = new List<LookupNguyenLieuDvtDto> { new LookupNguyenLieuDvtDto { Id = 0, Ten = "Tất cả" } }; flt.AddRange(nls); cb2.ItemsSource = flt; cb2.SelectedIndex = 0; }
                }
                var res = await httpClient.GetFromJsonAsync<List<QuanLyDonViChuyenDoiGridDto>>("api/app/quanly-donvichuyendoi");
                if (res != null) { _dataList = res; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgDonVi") is DataGrid dg)) return;
            var q = _dataList.AsEnumerable();
            if (FindName("cmbFilterNL") is ComboBox cb && cb.SelectedValue is int id && id > 0) q = q.Where(x => x.IdNguyenLieu == id);
            if (FindName("txtSearch") is TextBox t && !string.IsNullOrEmpty(t.Text)) q = q.Where(x => x.TenDonVi.ToLower().Contains(t.Text.ToLower()) || x.TenNguyenLieu.ToLower().Contains(t.Text.ToLower()));
            dg.ItemsSource = q.ToList();
        }

        private void DgDonVi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDonVi") is DataGrid dg && dg.SelectedItem is QuanLyDonViChuyenDoiGridDto item)
            {
                _selectedItem = item; _isAdding = false;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("cmbNguyenLieu") is ComboBox c1) c1.SelectedValue = item.IdNguyenLieu;
                if (FindName("txtTenDonVi") is TextBox t1) t1.Text = item.TenDonVi;
                if (FindName("txtGiaTriQuyDoi") is TextBox t2) t2.Text = item.GiaTriQuyDoi.ToString();
                if (FindName("chkLaDonViCoBan") is CheckBox chk) chk.IsChecked = item.LaDonViCoBan;
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI")) return;
            _selectedItem = new QuanLyDonViChuyenDoiGridDto(); _isAdding = true;
            if (FindName("dgDonVi") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("cmbNguyenLieu") is ComboBox c1) c1.SelectedItem = null;
            if (FindName("txtTenDonVi") is TextBox t1) t1.Text = "";
            if (FindName("txtGiaTriQuyDoi") is TextBox t2) t2.Text = "1";
            if (FindName("chkLaDonViCoBan") is CheckBox chk) chk.IsChecked = false;
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI") || _selectedItem == null) return;

            int idNl = (FindName("cmbNguyenLieu") as ComboBox)?.SelectedValue as int? ?? 0;
            string ten = (FindName("txtTenDonVi") as TextBox)?.Text.Trim() ?? "";
            if (idNl == 0 || string.IsNullOrEmpty(ten)) { MessageBox.Show("Nhập Nguyên liệu và Tên Đơn vị!"); return; }
            decimal.TryParse((FindName("txtGiaTriQuyDoi") as TextBox)?.Text, out decimal tyLe);
            bool isBase = (FindName("chkLaDonViCoBan") as CheckBox)?.IsChecked == true;

            var dto = new QuanLyDonViChuyenDoiSaveDto { IdNguyenLieu = idNl, TenDonVi = ten, GiaTriQuyDoi = tyLe, LaDonViCoBan = isBase };

            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var res = _isAdding ? await httpClient.PostAsJsonAsync("api/app/quanly-donvichuyendoi", dto) : await httpClient.PutAsJsonAsync($"api/app/quanly-donvichuyendoi/{_selectedItem.IdChuyenDoi}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã lưu!"); await LoadDataAsync(); } else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DON_VI_CHUYEN_DOI") || _selectedItem == null || _isAdding) return;
            if (MessageBox.Show("Xóa Đơn vị này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-donvichuyendoi/{_selectedItem.IdChuyenDoi}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa thành công"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}