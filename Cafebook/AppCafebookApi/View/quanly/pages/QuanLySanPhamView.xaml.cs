using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;
using System.IO;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLySanPhamView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLySanPhamGridDto> _dataList = new();
        private List<LookupDanhMucDto> _danhMucList = new();
        private QuanLySanPhamDetailDto? _selectedItem;
        private string? _currentImgPath = null;
        private bool _deleteImgRequest = false;

        static QuanLySanPhamView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLySanPhamView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            if (!AuthService.CoQuyen("QL_SAN_PHAM")) { MessageBox.Show("Từ chối!"); this.NavigationService?.GoBack(); return; }
            ApplyPermissions(); await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_SAN_PHAM");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavDanhMuc") is Button b4) b4.Visibility = AuthService.CoQuyen("QL_DANH_MUC") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavDinhLuong") is Button b5) b5.Visibility = AuthService.CoQuyen("QL_DINH_LUONG") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var dms = await httpClient.GetFromJsonAsync<List<LookupDanhMucDto>>("api/app/quanly-sanpham/lookup-danhmuc");
                if (dms != null)
                {
                    _danhMucList = dms;
                    if (FindName("cmbDanhMuc") is ComboBox cb1) cb1.ItemsSource = _danhMucList;
                    if (FindName("cmbFilterDanhMuc") is ComboBox cb2) { var flt = new List<LookupDanhMucDto> { new LookupDanhMucDto { Id = 0, Ten = "Tất cả" } }; flt.AddRange(dms); cb2.ItemsSource = flt; cb2.SelectedIndex = 0; }
                }
                var sps = await httpClient.GetFromJsonAsync<List<QuanLySanPhamGridDto>>("api/app/quanly-sanpham");
                if (sps != null) { _dataList = sps; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => FilterData();
        private void FilterData()
        {
            if (!(FindName("dgSanPham") is DataGrid dg)) return;
            var q = _dataList.AsEnumerable();
            if (FindName("cmbFilterDanhMuc") is ComboBox cb && cb.SelectedValue is int id && id > 0) q = q.Where(x => x.TenDanhMuc == _danhMucList.FirstOrDefault(d => d.Id == id)?.Ten);
            if (FindName("txtSearchSanPham") is TextBox t && !string.IsNullOrEmpty(t.Text)) q = q.Where(x => x.TenSanPham.ToLower().Contains(t.Text.ToLower()));
            dg.ItemsSource = q.ToList();
        }

        private async void DgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgSanPham") is DataGrid dg && dg.SelectedItem is QuanLySanPhamGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                _selectedItem = await httpClient.GetFromJsonAsync<QuanLySanPhamDetailDto>($"api/app/quanly-sanpham/{item.IdSanPham}");
                if (_selectedItem != null)
                {
                    if (FindName("txtTenSanPham") is TextBox t1) t1.Text = _selectedItem.TenSanPham;
                    if (FindName("txtDonGia") is TextBox t2) t2.Text = _selectedItem.GiaBan.ToString();
                    if (FindName("cmbDanhMuc") is ComboBox c1) c1.SelectedValue = _selectedItem.IdDanhMuc;
                    if (FindName("cmbNhomIn") is ComboBox c2) c2.Text = _selectedItem.NhomIn;
                    if (FindName("cmbTrangThai") is ComboBox c3) c3.SelectedIndex = _selectedItem.TrangThaiKinhDoanh ? 0 : 1;
                    if (FindName("txtMoTa") is TextBox t3) t3.Text = _selectedItem.MoTa;

                    _currentImgPath = null; _deleteImgRequest = false;
                    string url = string.IsNullOrEmpty(_selectedItem.HinhAnh) ? "" : (AppConfigManager.GetApiServerUrl() + _selectedItem.HinhAnh);
                    if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage(url, HinhAnhPaths.DefaultFoodIcon);
                }
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM")) return;
            _selectedItem = new QuanLySanPhamDetailDto();
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("dgSanPham") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("txtTenSanPham") is TextBox t1) t1.Text = "";
            if (FindName("txtDonGia") is TextBox t2) t2.Text = "0";
            if (FindName("txtMoTa") is TextBox t3) t3.Text = "";
            if (FindName("cmbTrangThai") is ComboBox c3) c3.SelectedIndex = 0;
            _currentImgPath = null; _deleteImgRequest = false;
            if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage("", HinhAnhPaths.DefaultFoodIcon);
        }

        private void BtnChonAnh_Click(object sender, RoutedEventArgs e) { var op = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp" }; if (op.ShowDialog() == true) { _currentImgPath = op.FileName; _deleteImgRequest = false; if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage(_currentImgPath, HinhAnhPaths.DefaultFoodIcon); } }
        private void BtnXoaAnh_Click(object sender, RoutedEventArgs e) { _currentImgPath = null; _deleteImgRequest = true; if (FindName("AnhPreview") is Image img) img.Source = HinhAnhHelper.LoadImage("", HinhAnhPaths.DefaultFoodIcon); }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM") || _selectedItem == null) return;
            string ten = (FindName("txtTenSanPham") as TextBox)?.Text.Trim() ?? "";
            int idDm = (FindName("cmbDanhMuc") as ComboBox)?.SelectedValue as int? ?? 0;
            if (string.IsNullOrEmpty(ten) || idDm == 0) { MessageBox.Show("Nhập Tên SP và Danh mục!"); return; }
            decimal.TryParse((FindName("txtDonGia") as TextBox)?.Text, out decimal gia);
            bool isSelling = (FindName("cmbTrangThai") as ComboBox)?.SelectedIndex == 0;

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(ten), "TenSanPham"); content.Add(new StringContent(gia.ToString()), "GiaBan");
            content.Add(new StringContent(idDm.ToString()), "IdDanhMuc"); content.Add(new StringContent((FindName("cmbNhomIn") as ComboBox)?.Text ?? "Khác"), "NhomIn");
            content.Add(new StringContent(isSelling.ToString()), "TrangThaiKinhDoanh"); content.Add(new StringContent((FindName("txtMoTa") as TextBox)?.Text ?? ""), "MoTa");
            content.Add(new StringContent(_deleteImgRequest.ToString()), "DeleteImage");
            if (!string.IsNullOrEmpty(_currentImgPath)) { var fileContent = new ByteArrayContent(File.ReadAllBytes(_currentImgPath)); fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); content.Add(fileContent, "AnhBia", Path.GetFileName(_currentImgPath)); }

            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var res = _selectedItem.IdSanPham == 0 ? await httpClient.PostAsync("api/app/quanly-sanpham", content) : await httpClient.PutAsync($"api/app/quanly-sanpham/{_selectedItem.IdSanPham}", content);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã lưu!"); await LoadDataAsync(); } else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SAN_PHAM") || _selectedItem == null || _selectedItem.IdSanPham == 0) return;
            if (MessageBox.Show("Xóa SP này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await httpClient.DeleteAsync($"api/app/quanly-sanpham/{_selectedItem.IdSanPham}");
                if (res.IsSuccessStatusCode) { MessageBox.Show("Đã xóa"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadDataAsync(); }
            }
        }

        private void BtnNavDanhMuc_Click(object sender, RoutedEventArgs e) { if (AuthService.CoQuyen("QL_DANH_MUC")) this.NavigationService?.Navigate(new QuanLyDanhMucView()); }
        private void BtnNavDinhLuong_Click(object sender, RoutedEventArgs e) { if (AuthService.CoQuyen("QL_DINH_LUONG")) this.NavigationService?.Navigate(new QuanLyDinhLuongView()); }
    }
}