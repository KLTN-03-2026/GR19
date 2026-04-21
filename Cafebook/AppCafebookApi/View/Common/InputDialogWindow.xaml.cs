using System.Windows;
using System.Windows.Input;

namespace AppCafebookApi.View.common
{
    public partial class InputDialogWindow : Window
    {
        public string InputText { get; private set; } = string.Empty;

        // Constructor nhận 2 tham số để truyền Title và Message linh hoạt
        public InputDialogWindow(string title, string message)
        {
            InitializeComponent();

            // Cập nhật text cho UI
            txtTitle.Text = title;
            txtMessage.Text = message;

            // Auto focus vào textbox khi mở cửa sổ
            txtInput.Focus();
        }

        // Hỗ trợ kéo thả cửa sổ khi click chuột trái vào vùng trống
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Có thể thêm validate ở đây nếu cần (VD: không cho nhập rỗng)
            /*
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            */

            InputText = txtInput.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}