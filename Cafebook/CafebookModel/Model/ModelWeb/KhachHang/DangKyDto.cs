namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class DangKyRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyOtpRequestDto
    {
        public int TempId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }

    public class DangKyResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public bool RequireOtp { get; set; }
        public bool IsOfficialAccount { get; set; } 
        public int? TempId { get; set; }
        public string? TempEmail { get; set; }
        public string? TempPhone { get; set; }

        public DangKyKhachHangDto? KhachHangData { get; set; }
        public string? Token { get; set; }
    }

    public class DangKyKhachHangDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? TenDangNhap { get; set; }
        public string? AnhDaiDienUrl { get; set; }
    }
}