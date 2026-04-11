using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLySuCoBanView : Page
    {
        private static readonly HttpClient httpClient;
        static QuanLySuCoBanView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLySuCoBanView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            if (!AuthService.CoQuyen("QL_SU_CO_BAN")) { MessageBox.Show("Từ chối!"); this.NavigationService?.GoBack(); return; }
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            bool isHistory = (FindName("chkHistory") as CheckBox)?.IsChecked == true;
            if (FindName("lblTitle") is TextBlock title) title.Text = isHistory ? "Lịch sử sự cố đã xử lý" : "Sự cố Bàn cần xử lý";
            try
            {
                var res = await httpClient.GetFromJsonAsync<List<QuanLySuCoBanDto>>($"api/app/quanly-sucoban?isHistory={isHistory}");
                if (FindName("dgSuCo") is DataGrid dg) dg.ItemsSource = res;
            }
            catch { }
        }

        private void ChkHistory_Changed(object sender, RoutedEventArgs e) => _ = LoadDataAsync();

        private async void BtnResolve_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_SU_CO_BAN")) return;
            if (sender is Button btn && btn.DataContext is QuanLySuCoBanDto tb)
            {
                if (MessageBox.Show("Đánh dấu đã xử lý?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await httpClient.PostAsync($"api/app/quanly-sucoban/resolve/{tb.IdThongBao}", null!);
                    await LoadDataAsync();
                }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}