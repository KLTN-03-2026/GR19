namespace CafebookModel.Model.ModelWeb.KhachHang
{
    // DTO gửi lên từ View
    public class DangNhapRequestDto
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    // DTO trả về từ API
    public class DangNhapResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DangNhapKhachHangDto? KhachHangData { get; set; }
        public string? Token { get; set; }
    }

    // DTO chứa dữ liệu User an toàn
    public class DangNhapKhachHangDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? TenDangNhap { get; set; }
        public string? AnhDaiDienUrl { get; set; }
    }
}