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
            InitializeAsync();
        }

        async void InitializeAsync()
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
                if (e.Uri.Contains("vnp_ResponseCode=00"))
                {
                    this.DialogResult = true; 
                }
                else
                {
                    MessageBox.Show("Khách hàng đã hủy hoặc giao dịch thất bại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.DialogResult = false;
                }

                this.Close();
            }
        }
    }
}