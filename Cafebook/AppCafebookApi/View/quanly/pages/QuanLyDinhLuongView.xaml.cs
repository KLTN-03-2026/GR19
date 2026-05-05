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
        private List<QuanLyDinhLuongSPDto> _spList = new();
        private QuanLyDinhLuongSPDto? _selectedSP;
        private QuanLyDinhLuongNLDto? _selectedNL;

        private bool _isDataLoaded = false;

        public QuanLyDinhLuongView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken)) 
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL") && !AuthService.CoQuyen("QL_DINH_LUONG")) 
            { 
                MessageBox.Show("Bạn không có quyền truy cập module Định lượng!", "Từ chối"); 
                this.NavigationService?.GoBack(); 
                return; 
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                ApplyPermissions(); 
                
                await LoadMasterDataAsync();
                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tại module Định lượng: {ex.Message}");
            }
        }


        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_DINH_LUONG");
            if (FindName("btnLuu") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadMasterDataAsync()
        {
            var loading = FindName("LoadingOverlay") as Border;
            if (loading != null) loading.Visibility = Visibility.Visible;
            try
            {
                if (GlobalDataCache.QL_DinhLuongSPCache != null && GlobalDataCache.QL_DinhLuongSPCache.Count > 0)
                {
                    _spList = GlobalDataCache.QL_DinhLuongSPCache;
                    FilterSP();

                    if (loading != null) loading.Visibility = Visibility.Collapsed;

                    _ = BackgroundRefreshAsync();
                    return;
                }
                await FetchApiAndSetupUI();
            }
            catch { }
            finally
            {
                if (loading != null) loading.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (ĐỒNG BỘ NGẦM)
        // ==========================================

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                // Bắn 3 API CÙNG LÚC để đạt tốc độ tối đa
                var tSp = ApiClient.Instance.GetFromJsonAsync<List<QuanLyDinhLuongSPDto>>("api/app/quanly-dinhluong/lookup-sp");
                var tNl = ApiClient.Instance.GetFromJsonAsync<List<LookupDinhLuongDto>>("api/app/quanly-dinhluong/lookup-nl");
                var tDv = ApiClient.Instance.GetFromJsonAsync<List<LookupDinhLuongDto>>("api/app/quanly-dinhluong/lookup-dv");

                await Task.WhenAll(tSp, tNl, tDv);

                var sp = await tSp;
                var nl = await tNl;
                var dv = await tDv;

                // Nạp dữ liệu vào ComboBox (Chạy ngầm nên UI không bị đơ)
                if (nl != null && FindName("cmbNguyenLieu") is ComboBox cbNL) cbNL.ItemsSource = nl;
                if (dv != null && FindName("cmbDonVi") is ComboBox cbDV) cbDV.ItemsSource = dv;

                // Cập nhật lại RAM và Danh sách Sản Phẩm (Nếu có món mới thêm)
                if (sp != null)
                {
                    GlobalDataCache.QL_DinhLuongSPCache = sp;
                    _spList = sp;

                    // Ghi nhớ sản phẩm đang click để không làm gián đoạn
                    int? currentSelectedId = _selectedSP?.IdSanPham;
                    FilterSP();

                    if (currentSelectedId.HasValue && FindName("dgSanPham") is DataGrid dg)
                    {
                        var itemToSelect = _spList.FirstOrDefault(x => x.IdSanPham == currentSelectedId);
                        if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                    }
                }
            }
            catch { /* Lỗi mạng thì bỏ qua, người dùng vẫn dùng dữ liệu cũ trên RAM */ }
        }

        private async Task FetchApiAndSetupUI()
        {
            var tSp = ApiClient.Instance.GetFromJsonAsync<List<QuanLyDinhLuongSPDto>>("api/app/quanly-dinhluong/lookup-sp");
            var tNl = ApiClient.Instance.GetFromJsonAsync<List<LookupDinhLuongDto>>("api/app/quanly-dinhluong/lookup-nl");
            var tDv = ApiClient.Instance.GetFromJsonAsync<List<LookupDinhLuongDto>>("api/app/quanly-dinhluong/lookup-dv");

            await Task.WhenAll(tSp, tNl, tDv);

            var sp = await tSp;
            var nl = await tNl;
            var dv = await tDv;

            if (nl != null && FindName("cmbNguyenLieu") is ComboBox cbNL) cbNL.ItemsSource = nl;
            if (dv != null && FindName("cmbDonVi") is ComboBox cbDV) cbDV.ItemsSource = dv;

            if (sp != null)
            {
                GlobalDataCache.QL_DinhLuongSPCache = sp;
                _spList = sp;
                FilterSP();
            }
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
            try { var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyDinhLuongNLDto>>($"api/app/quanly-dinhluong/{idSp}"); if (FindName("dgDinhLuong") is DataGrid dg) dg.ItemsSource = res; ResetNLForm(); } catch { }
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

            var loading = FindName("LoadingOverlay") as Border;
            if (loading != null) loading.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync($"api/app/quanly-dinhluong/{_selectedSP.IdSanPham}", dto);
                if (res.IsSuccessStatusCode) await LoadDinhLuongAsync(_selectedSP.IdSanPham);
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_DINH_LUONG") || _selectedSP == null || _selectedNL == null) return;
            if (MessageBox.Show("Xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var loading = FindName("LoadingOverlay") as Border;
                if (loading != null) loading.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-dinhluong/{_selectedSP.IdSanPham}/{_selectedNL.IdNguyenLieu}");
                    if (res.IsSuccessStatusCode) await LoadDinhLuongAsync(_selectedSP.IdSanPham);
                }
                finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}