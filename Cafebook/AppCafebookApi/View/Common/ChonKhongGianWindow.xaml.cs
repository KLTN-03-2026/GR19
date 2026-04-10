using System.Windows;

namespace AppCafebookApi.View.Common
{
    public partial class ChonKhongGianWindow : Window
    {
        // Biến lưu trữ lựa chọn của người dùng
        public string SelectedWorkspace { get; private set; } = "NhanVien";

        public ChonKhongGianWindow()
        {
            InitializeComponent();
        }

        private void BtnQuanLy_Click(object sender, RoutedEventArgs e)
        {
            SelectedWorkspace = "QuanLy";
            this.DialogResult = true; // Đóng popup và trả về kết quả Thành công
        }

        private void BtnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            SelectedWorkspace = "NhanVien";
            this.DialogResult = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Đóng popup, hủy quá trình đăng nhập
        }
    }
}