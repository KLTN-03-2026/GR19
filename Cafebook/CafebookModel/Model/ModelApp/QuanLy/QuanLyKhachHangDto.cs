using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyKhachHangGridDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public int DiemTichLuy { get; set; }
        public bool BiKhoa { get; set; }
        public string LoaiTaiKhoan { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public bool TaiKhoanTam { get; set; }
    }

    public class QuanLyKhachHangDetailDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public int DiemTichLuy { get; set; }
        public string? TenDangNhap { get; set; }
        public bool BiKhoa { get; set; }
        public string? LyDoKhoa { get; set; }
        public DateTime? ThoiGianMoKhoa { get; set; }
        public string? AnhDaiDien { get; set; }
        public bool TaiKhoanTam { get; set; }
        public DateTime NgayTao { get; set; }

        public List<KhachHangLichSuMuaDto> LichSuMuaHang { get; set; } = new();
        public List<KhachHangLichSuThueDto> LichSuThueSach { get; set; } = new();
    }

    public class KhoaKhachHangRequestDto
    {
        public string LyDoKhoa { get; set; } = string.Empty;
        public int? SoNgayKhoa { get; set; }
    }

    public class CapNhatDiemKhachHangDto
    {
        public int DiemThayDoi { get; set; }
        public string LyDo { get; set; } = string.Empty;
    }

    public class KhachHangLichSuMuaDto { public int IdHoaDon { get; set; } public DateTime ThoiGian { get; set; } public decimal TongTien { get; set; } public string SanPhamMua { get; set; } = string.Empty; }
    public class KhachHangLichSuThueDto { public int IdPhieuThue { get; set; } public string TieuDeSach { get; set; } = string.Empty; public DateTime NgayThue { get; set; } public string TrangThai { get; set; } = string.Empty; }
}