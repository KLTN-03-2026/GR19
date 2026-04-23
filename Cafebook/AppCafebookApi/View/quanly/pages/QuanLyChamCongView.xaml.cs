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
    public partial class QuanLyChamCongView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyChamCongGridDto> _allDataList = new();
        private List<ChamCongNhanVienLookupDto> _lookupNhanVien = new();
        private QuanLyChamCongGridDto? _selectedItem = null;

     //   static QuanLyChamCongView() { ApiClient.Instance = new ApiClient.Instance { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyChamCongView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_CHAM_CONG"))
            {
                MessageBox.Show("Từ chối truy cập module Chấm công!");
                this.NavigationService?.GoBack(); return;
            }

            ApplyPermissions();

            // Setup Default Dates (Đầu tuần đến cuối tuần)
            DateTime today = DateTime.Today;
            int offset = today.DayOfWeek - DayOfWeek.Monday;
            if (offset < 0) offset += 7;
            DateTime startOfWeek = today.AddDays(-offset);

            if (FindName("dpFilterTuNgay") is DatePicker tu) tu.SelectedDate = startOfWeek;
            if (FindName("dpFilterDenNgay") is DatePicker den) den.SelectedDate = startOfWeek.AddDays(6);

            await LoadLookupsAsync();
            await LoadDataAsync();
        }

        private void ApplyPermissions()
        {
            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_CHAM_CONG");
            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
            if (FindName("btnLuu") is Button btn2) btn2.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                var nvs = await ApiClient.Instance.GetFromJsonAsync<List<ChamCongNhanVienLookupDto>>("api/app/quanly-chamcong/nhanvien-lookup");
                if (nvs != null) _lookupNhanVien = nvs;
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var queryParams = new List<string>();
                if (FindName("dpFilterTuNgay") is DatePicker tu && tu.SelectedDate.HasValue) queryParams.Add($"tuNgay={tu.SelectedDate.Value:yyyy-MM-dd}");
                if (FindName("dpFilterDenNgay") is DatePicker den && den.SelectedDate.HasValue) queryParams.Add($"denNgay={den.SelectedDate.Value:yyyy-MM-dd}");

                string url = "api/app/quanly-chamcong/search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyChamCongGridDto>>(url);

                if (res != null) { _allDataList = res; FilterData(); }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void FilterData()
        {
            if (!(FindName("dgChamCong") is DataGrid dg)) return;
            var query = _allDataList.AsEnumerable();

            string k = (FindName("txtFilterNhanVien") as TextBox)?.Text.ToLower() ?? "";
            if (!string.IsNullOrEmpty(k)) query = query.Where(x => x.TenNhanVien.ToLower().Contains(k));

            dg.ItemsSource = query.ToList();
        }

        #region AUTO-SUGGEST NHÂN VIÊN
        private void TxtFilterNhanVien_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FindName("txtFilterNhanVien") is not TextBox txt || FindName("popNhanVien") is not System.Windows.Controls.Primitives.Popup pop || FindName("lstNhanVien") is not ListBox lst) return;

            string keyword = txt.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword)) { pop.IsOpen = false; FilterData(); return; }

            var matches = _lookupNhanVien.Where(x => x.HoTen.ToLower().Contains(keyword)).ToList();
            if (matches.Any())
            {
                lst.SelectionChanged -= LstNhanVien_SelectionChanged;
                lst.ItemsSource = matches;
                lst.SelectionChanged += LstNhanVien_SelectionChanged;
                pop.IsOpen = true;
            }
            else pop.IsOpen = false;

            FilterData();
        }

        private void LstNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("lstNhanVien") is ListBox lst && lst.SelectedItem is ChamCongNhanVienLookupDto selected)
            {
                if (FindName("txtFilterNhanVien") is TextBox txt)
                {
                    txt.TextChanged -= TxtFilterNhanVien_TextChanged;
                    txt.Text = selected.HoTen;
                    txt.TextChanged += TxtFilterNhanVien_TextChanged;
                    txt.CaretIndex = txt.Text.Length;
                }
                if (FindName("popNhanVien") is System.Windows.Controls.Primitives.Popup pop) pop.IsOpen = false;
                FilterData();
            }
        }
        #endregion

        private async void BtnLoc_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private async void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("txtFilterNhanVien") is TextBox txt) txt.Text = "";
            await LoadDataAsync();
        }

        private void DgChamCong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgChamCong") is DataGrid dg && dg.SelectedItem is QuanLyChamCongGridDto item)
            {
                _selectedItem = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;

                if (FindName("lblTenNhanVien") is TextBlock l1) l1.Text = item.TenNhanVien;
                if (FindName("lblNgayLam") is TextBlock l2) l2.Text = item.NgayLam.ToString("dd/MM/yyyy");
                if (FindName("lblCaLam") is TextBlock l3) l3.Text = $"{item.TenCa} ({item.CaGioBatDau:hh\\:mm} - {item.CaGioKetThuc:hh\\:mm})";

                if (FindName("txtGioVao") is TextBox t1) t1.Text = item.GioVao?.ToString(@"hh\:mm");
                if (FindName("txtGioRa") is TextBox t2) t2.Text = item.GioRa?.ToString(@"hh\:mm");
                if (FindName("txtGhiChu") is TextBox t3) t3.Text = item.GhiChuSua;
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;

            var dto = new QuanLyChamCongUpdateDto
            {
                GioVao = (FindName("txtGioVao") as TextBox)?.Text.Trim(),
                GioRa = (FindName("txtGioRa") as TextBox)?.Text.Trim(),
                GhiChuSua = (FindName("txtGhiChu") as TextBox)?.Text.Trim()
            };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = await ApiClient.Instance.PutAsJsonAsync($"api/app/quanly-chamcong/{_selectedItem.IdChamCong}", dto);
                if (res.IsSuccessStatusCode) { MessageBox.Show("Cập nhật thành công!"); await LoadDataAsync(); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}