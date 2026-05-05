using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyNhatKyView : Page
    {
        private List<QuanLyNhatKyGridDto> _allData = new();

        private bool _isDataLoaded = false;

        public QuanLyNhatKyView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken)) 
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "CM_NHAT_KY_HE_THONG"))
            {
                if (FindName("GridDuLieu") is Grid g) g.Visibility = Visibility.Collapsed;
                if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = Visibility.Visible;
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                if (FindName("dpTuNgay") is DatePicker tu) tu.SelectedDate = DateTime.Today;
                if (FindName("dpDenNgay") is DatePicker den) den.SelectedDate = DateTime.Today;

                await LoadLookupsAsync();
                await LoadDataAsync();

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tại module Nhật ký hệ thống: {ex.Message}");
            }
        }
        private async Task LoadLookupsAsync()
        {
            try
            {
                var tables = await ApiClient.Instance.GetFromJsonAsync<List<string>>("api/app/quanly-nhatky/tables");
                if (tables != null && FindName("cmbBangAnhHuong") is ComboBox cmb)
                {
                    var list = new List<string> { "Tất cả" };
                    list.AddRange(tables);
                    cmb.ItemsSource = list;
                    cmb.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var queryParams = new List<string>();
                if (FindName("dpTuNgay") is DatePicker tu && tu.SelectedDate.HasValue) queryParams.Add($"tuNgay={tu.SelectedDate.Value:yyyy-MM-dd}");
                if (FindName("dpDenNgay") is DatePicker den && den.SelectedDate.HasValue) queryParams.Add($"denNgay={den.SelectedDate.Value:yyyy-MM-dd}");
                if (FindName("cmbHanhDong") is ComboBox cbHd && cbHd.Text != "Tất cả") queryParams.Add($"hanhDong={cbHd.Text}");
                if (FindName("cmbBangAnhHuong") is ComboBox cbTb && cbTb.Text != "Tất cả" && cbTb.SelectedIndex > 0) queryParams.Add($"bangBiAnhHuong={cbTb.Text}");
                if (FindName("txtKeyword") is TextBox txt && !string.IsNullOrWhiteSpace(txt.Text)) queryParams.Add($"keyword={txt.Text.Trim()}");

                string url = "api/app/quanly-nhatky/search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyNhatKyGridDto>>(url);

                if (res != null && FindName("dgNhatKy") is DataGrid dg)
                {
                    _allData = res;
                    dg.ItemsSource = _allData;
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}"); }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private async void DgNhatKy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgNhatKy") is DataGrid dg && dg.SelectedItem is QuanLyNhatKyGridDto item)
            {
                if (FindName("panelChiTiet") is Grid panel) panel.IsEnabled = true;

                try
                {
                    var detail = await ApiClient.Instance.GetFromJsonAsync<QuanLyNhatKyDetailDto>($"api/app/quanly-nhatky/{item.IdNhatKy}");
                    if (detail != null)
                    {
                        if (FindName("lblThongTinChung") is TextBlock lbl)
                            lbl.Text = $"Mã Log: #{detail.IdNhatKy} | Khóa chính: {detail.KhoaChinh ?? "N/A"} | Bảng: {detail.BangBiAnhHuong}";

                        if (FindName("txtDuLieuCu") is TextBlock txtCu)
                            txtCu.Text = FormatJson(detail.DuLieuCu);

                        if (FindName("txtDuLieuMoi") is TextBlock txtMoi)
                            txtMoi.Text = FormatJson(detail.DuLieuMoi);
                    }
                }
                catch { }
            }
        }

        // Hàm tiện ích: Biến chuỗi JSON thuần thành JSON có thụt đầu dòng (Beautify)
        private string FormatJson(string? jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString)) return "Không có dữ liệu.";

            // Xử lý trường hợp chuỗi đơn giản (ví dụ Log Đăng nhập không phải JSON)
            if (!jsonString.Trim().StartsWith("{") && !jsonString.Trim().StartsWith("["))
                return jsonString;

            try
            {
                var doc = JsonDocument.Parse(jsonString);
                var options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(doc, options);
            }
            catch
            {
                // Trả về nguyên gốc nếu JSON parse lỗi
                return jsonString;
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}