using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Media;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyCaiDatView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyCaiDatDto> _allSettings = new();
        private ObservableCollection<QuanLyCaiDatDto> _viewSettings = new();
        private DispatcherTimer _notificationTimer;
        /*
        static QuanLyCaiDatView()
        {
            httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") };
        }
        */
        public QuanLyCaiDatView()
        {
            InitializeComponent();
            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _notificationTimer.Tick += (s, e) => { NotificationBorder.Visibility = Visibility.Collapsed; _notificationTimer.Stop(); };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken))
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyCaiDatDto>>("api/app/quanly-caidat/all");
                if (res != null)
                {
                    _allSettings = res;
                    ApplyFilter();
                }
            }
            catch (Exception ex) { ShowNotification("Lỗi tải dữ liệu: " + ex.Message, true); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.ToLower().Trim();
            var filtered = string.IsNullOrEmpty(keyword)
                ? _allSettings
                : _allSettings.Where(x => x.TenCaiDat.ToLower().Contains(keyword) || (x.MoTa?.ToLower().Contains(keyword) ?? false)).ToList();

            _viewSettings = new ObservableCollection<QuanLyCaiDatDto>(filtered);
            lvCaiDat.ItemsSource = _viewSettings;

            // Thiết lập Grouping
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvCaiDat.ItemsSource);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("Nhom"));
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is QuanLyCaiDatDto item)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    var response = await ApiClient.Instance.PutAsJsonAsync("api/app/quanly-caidat/update-single", item);
                    if (response.IsSuccessStatusCode) ShowNotification($"Đã lưu: {item.TenCaiDat}");
                    else ShowNotification("Lỗi khi lưu: " + await response.Content.ReadAsStringAsync(), true);
                }
                catch (Exception ex) { ShowNotification("Lỗi kết nối: " + ex.Message, true); }
                finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
            }
        }

        private void ShowNotification(string message, bool isError = false)
        {
            NotificationText.Text = message;
            NotificationBorder.Background = (SolidColorBrush)FindResource(isError ? "ErrorBrush" : "SuccessBrush");
            NotificationBorder.Visibility = Visibility.Visible;
            _notificationTimer.Stop();
            _notificationTimer.Start();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}