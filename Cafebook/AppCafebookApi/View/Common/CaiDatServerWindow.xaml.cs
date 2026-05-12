using System.Windows;
using AppCafebookApi.Services;

namespace AppCafebookApi.View.Common
{
    public partial class CaiDatServerWindow : Window
    {
        public CaiDatServerWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string url = txtServerUrl.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Vui lòng không để trống địa chỉ Server!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppConfigManager.SaveApiServerUrl(url);

            MessageBox.Show("Thiết lập thành công! Ứng dụng sẽ bắt đầu kết nối.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}