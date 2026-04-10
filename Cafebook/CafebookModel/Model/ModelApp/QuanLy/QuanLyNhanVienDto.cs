using System;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyNhanVienGridDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public decimal LuongCoBan { get; set; }
        public string TrangThaiLamViec { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
    }

    public class QuanLyNhanVienDetailDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public int IdVaiTro { get; set; }
        public decimal LuongCoBan { get; set; }
        public string TrangThaiLamViec { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public string? AnhDaiDienUrl { get; set; }
    }

    public class QuanLyNhanVienSaveRequestDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public string? MatKhau { get; set; }

        [Required]
        public int IdVaiTro { get; set; }

        public decimal LuongCoBan { get; set; }
        public string TrangThaiLamViec { get; set; } = "Đang làm việc";
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public bool XoaAnhDaiDien { get; set; } = false;
    }

    public class RoleLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}