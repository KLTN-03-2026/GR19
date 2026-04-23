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
using AppCafebookApi.View.Common; // Gọi thư mục chứa Window Preview

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyPhatLuongView : Page
    {
        /*private static readonly HttpClient httpClient;

        static QuanLyPhatLuongView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        */
        public QuanLyPhatLuongView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_LUONG"))
            {
                MessageBox.Show("Từ chối truy cập module Phát lương!");
                this.NavigationService?.GoBack(); return;
            }

            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_LUONG");
            if (FindName("GridDuLieu") is Border g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;

            if (hasQuyen)
            {
                if (FindName("cmbNam") is ComboBox cNam && FindName("cmbThang") is ComboBox cThang)
                {
                    int currentYear = DateTime.Now.Year;
                    for (int i = currentYear - 2; i <= currentYear + 1; i++) cNam.Items.Add(i);
                    for (int i = 1; i <= 12; i++) cThang.Items.Add(i);

                    cNam.SelectedItem = currentYear;
                    cThang.SelectedItem = DateTime.Now.Month;
                }
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                int nam = (FindName("cmbNam") as ComboBox)?.SelectedItem as int? ?? DateTime.Now.Year;
                int thang = (FindName("cmbThang") as ComboBox)?.SelectedItem as int? ?? DateTime.Now.Month;

                var res = await ApiClient.Instance.GetFromJsonAsync<List<PhatLuongGridDto>>($"api/app/phatluong/danhsach?nam={nam}&thang={thang}");
                if (res != null && FindName("dgPhieuLuong") is DataGrid dg) dg.ItemsSource = res;
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private async void BtnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PhatLuongGridDto item)
            {
                var popup = new PhieuLuongPreviewWindow(item.IdPhieuLuong);
                if (popup.ShowDialog() == true)
                {
                    await LoadDataAsync(); // Tải lại nếu phát lương thành công
                }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}