using System.Windows;

namespace AppCafebookApi.View.common // Sửa namespace trùng với vị trí bạn lưu file
{
    public partial class InputDialogWindow : Window
    {
        public string InputText { get; private set; } = string.Empty;

        // Constructor nhận 2 tham số để sửa lỗi CS1729
        public InputDialogWindow(string title, string message)
        {
            InitializeComponent();
            this.Title = title;
            txtMessage.Text = message;
            txtInput.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            InputText = txtInput.Text;
            this.DialogResult = true; // Lệnh này sửa lỗi ShowDialog() không có kết quả
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}