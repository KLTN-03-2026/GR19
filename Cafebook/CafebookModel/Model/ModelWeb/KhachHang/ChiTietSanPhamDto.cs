using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class ChiTietSanPhamDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? TenLoaiSP { get; set; }
        public decimal DonGia { get; set; }
        public string? HinhAnhUrl { get; set; }
        public string? MoTa { get; set; }
        public List<SanPhamGoiYDto> GoiY { get; set; } = new();
    }

    public class SanPhamGoiYDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public string? AnhSanPhamUrl { get; set; }
    }

    public class DanhGiaChiTietDto
    {
        public string TenKhachHang { get; set; } = string.Empty;
        public string? AvatarKhachHang { get; set; }
        public int SoSao { get; set; }
        public DateTime NgayTao { get; set; }
        public string? BinhLuan { get; set; }
        public string? HinhAnhDanhGiaUrl { get; set; }
        public PhanHoiChiTietDto? PhanHoi { get; set; }
    }

    public class PhanHoiChiTietDto
    {
        public string TenNhanVien { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
    }
}