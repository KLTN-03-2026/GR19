using System;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace AppCafebookApi.View.Common
{
    public partial class VNPayWindow : Window
    {
        private readonly string _paymentUrl;

        public VNPayWindow(string paymentUrl)
        {
            InitializeComponent();
            _paymentUrl = paymentUrl;

            // Dời việc khởi tạo WebView vào sự kiện Loaded để đảm bảo Window đã được tạo xong hoàn toàn
            this.Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView.Source = new Uri(_paymentUrl);
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://localhost/vnpay-app-return", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                bool isSuccess = e.Uri.Contains("vnp_ResponseCode=00");

                // Dùng Dispatcher để trì hoãn việc set DialogResult cho đến khi luồng UI rảnh rỗi
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (this.IsLoaded)
                    {
                        this.DialogResult = isSuccess;
                    }
                    else
                    {
                        this.Close();
                    }
                }));
            }
        }
    }
}