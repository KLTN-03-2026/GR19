using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class ThongTinCaNhanViewDto
    {
        public NhanVienInfoDto NhanVien { get; set; } = null!;
        public LichLamViecDto? LichLamViecHomNay { get; set; }
        public int SoLanXinNghiThangNay { get; set; }
        public List<LichLamViecChiTietDto> LichLamViecThangNay { get; set; } = new List<LichLamViecChiTietDto>();
    }

    public class LichLamViecChiTietDto
    {
        public int IdLichLamViec { get; set; }
        public DateTime NgayLam { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
    }

    public class NhanVienInfoDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public decimal LuongCoBan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
    }

    public class LichLamViecDto
    {
        public string TenCa { get; set; } = string.Empty;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
    }

    public class DonXinNghiRequestDto
    {
        public string LoaiDon { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
    }

    public class DonXinNghiDto
    {
        public int IdDonXinNghi { get; set; }
        public string LoaiDon { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChuPheDuyet { get; set; }
    }

    public class CapNhatThongTinDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
    }

    public class DoiMatKhauRequestDto
    {
        public string MatKhauCu { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
    }
}