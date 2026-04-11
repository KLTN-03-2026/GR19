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
    public partial class QuanLyDinhLuongView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyDinhLuongSPDto> _spList = new();
        private QuanLyDinhLuongSPDto? _selectedSP;
        private QuanLyDinhLuongNLDto? _selectedNL;

        static QuanLyDinhLuongView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLyDinhLuongView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            if (!AuthService.CoQuyen("QL_DINH_LUONG")) { MessageBox.Show("Từ chối!"); this.NavigationService?.GoBack(); return; }
            ApplyPermissions(); await LoadMasterDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_DINH_LUONG");
            if (FindName("btnLuu") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadMasterDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var sp = await httpClient.GetFromJsonAsync<List<QuanLyDinhLuongSPDto>>("api/app/quanly-dinhluong/lookup-sp"); if (sp != null) { _spList = sp; FilterSP(); }
                var nl = await httpClient.GetFromJsonAsync<List<LookupDinhLuongDto>>("api/app/quanly-dinhluong/lookup-nl"); if (FindName("cmbNguyenLieu") is ComboBox cbNL) cbNL.ItemsSource = nl;
                var dv = await httpClient.GetFromJsonAsync<List<LookupDinhLuongDto>>("api/app/quanly-dinhluong/lookup-dv"); if (FindName("cmbDonVi") is ComboBox cbDV) cbDV.ItemsSource = dv;
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearchSP_TextChanged(object sender, TextChangedEventArgs e) => FilterSP();
        private void FilterSP() { if (FindName("dgSanPham") is DataGrid dg) { string k = (FindName("txtSearchSP") as TextBox)?.Text.ToLower() ?? ""; dg.ItemsSource = string.IsNullOrEmpty(k) ? _spList : _spList.Where(x => x.TenSanPham.ToLower().Contains(k)).ToList(); } }

        private async void DgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgSanPham") is DataGrid dg && dg.SelectedItem is QuanLyDinhLuongSPDto item)
            {
                _selectedSP = item; if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = $"Định lượng: {item.TenSanPham}";
                await LoadDinhLuongAsync(item.IdSanPham);
            }
        }

        private async Task LoadDinhLuongAsync(int idSp)
        {
            try { var res = await httpClient.GetFromJsonAsync<List<QuanLyDinhLuongNLDto>>($"api/app/quanly-dinhluong/{idSp}"); if (FindName("dgDinhLuong") is DataGrid dg) dg.ItemsSource = res; ResetNLForm(); } catch { }
        }

        private void DgDinhLuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgDinhLuong") is DataGrid dg && dg.SelectedItem is QuanLyDinhLuongNLDto item) { _selectedNL = item; if (FindName("cmbNguyenLieu") is ComboBox c1) c1.SelectedValue = item.IdNguyenLieu; if (FindName("txtSoLuong") is TextBox t1) t1.Text = item.SoLuongSuDung.ToString(); if (FindName("cmbDonVi") is ComboBox c2) c2.SelectedValue = item.IdDonViSuDung; } else ResetNLForm();
        }

        private void ResetNLForm() { _selectedNL = null; if (FindName("cmbNguyenLieu") is ComboBox c1) c1.SelectedItem = null; if (FindName("txtSoLuong") is TextBox t1) t1.Text = ""; if (FindName("cmbDonVi") is ComboBox c2) c2.SelectedItem = null; }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DINH_LUONG") || _selectedSP == null) return;
            int idNl = (FindName("cmbNguyenLieu") as ComboBox)?.SelectedValue as int? ?? 0;
            int idDv = (FindName("cmbDonVi") as ComboBox)?.SelectedValue as int? ?? 0;
            decimal.TryParse((FindName("txtSoLuong") as TextBox)?.Text, out decimal sl);

            if (idNl == 0 || idDv == 0 || sl <= 0) { MessageBox.Show("Nhập đủ thông tin hợp lệ!"); return; }
            var dto = new QuanLyDinhLuongSaveDto { IdNguyenLieu = idNl, SoLuongSuDung = sl, IdDonViSuDung = idDv };
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try { var res = await httpClient.PostAsJsonAsync($"api/app/quanly-dinhluong/{_selectedSP.IdSanPham}", dto); if (res.IsSuccessStatusCode) await LoadDinhLuongAsync(_selectedSP.IdSanPham); else MessageBox.Show(await res.Content.ReadAsStringAsync()); } finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e) { if (!AuthService.CoQuyen("QL_DINH_LUONG") || _selectedSP == null || _selectedNL == null) return; if (MessageBox.Show("Xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { var res = await httpClient.DeleteAsync($"api/app/quanly-dinhluong/{_selectedSP.IdSanPham}/{_selectedNL.IdNguyenLieu}"); if (res.IsSuccessStatusCode) await LoadDinhLuongAsync(_selectedSP.IdSanPham); } }
        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}