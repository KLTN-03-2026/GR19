using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _isDataLoaded = false;

        public QuanLySuCoBanView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken)) 
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_SU_CO_BAN")) 
            { 
                MessageBox.Show("Bạn không có quyền truy cập module Quản lý sự cố!", "Bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning); 
                this.NavigationService?.GoBack(); 
                return; 
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                await LoadDataAsync();

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tại module Sự cố bán: {ex.Message}");
            }
        }
        private async Task LoadDataAsync()
        {
            bool isHistory = (FindName("chkHistory") as CheckBox)?.IsChecked == true;
            if (FindName("lblTitle") is TextBlock title) title.Text = isHistory ? "Lịch sử sự cố đã xử lý" : "Sự cố Bàn cần xử lý";
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLySuCoBanDto>>($"api/app/quanly-sucoban?isHistory={isHistory}");
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
                    var payload = new QuanLySuCoBanResolveDto
                    {
                        IdBan = tb.IdBan ?? 0
                    };

                    var response = await ApiClient.Instance.PostAsJsonAsync($"api/app/quanly-sucoban/resolve/{tb.IdThongBao}", payload);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Lỗi kết nối hoặc API trả về lỗi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}