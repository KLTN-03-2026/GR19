using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.Shared
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }

        // Danh sách quyền lấy từ bảng VaiTro_Quyen để phân quyền UI
        public List<string> Quyen { get; set; } = new List<string>();
    }
}