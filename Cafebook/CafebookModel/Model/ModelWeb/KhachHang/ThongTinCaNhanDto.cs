using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class ThongTinCaNhanDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? TenDangNhap { get; set; }
        public string AnhDaiDienUrl { get; set; } = string.Empty; // Trả về Full URL
    }

    public class ThongTinCaNhanUpdateDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(100, ErrorMessage = "Tên đăng nhập từ 6-100 ký tự.", MinimumLength = 6)]
        public string TenDangNhap { get; set; } = string.Empty;
    }
}