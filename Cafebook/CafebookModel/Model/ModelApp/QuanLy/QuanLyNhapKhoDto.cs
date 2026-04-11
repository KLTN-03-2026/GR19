using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyNhapKhoGridDto
    {
        public int IdPhieuNhap { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TenNhaCungCap { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class QuanLyNhapKhoDetailDto
    {
        public int IdPhieuNhap { get; set; }
        public int IdNhaCungCap { get; set; }
        public string? GhiChu { get; set; }
        public decimal TienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongTien { get; set; }
        public List<QuanLyChiTietNhapKhoDto> ChiTiet { get; set; } = new();
        public string? HoaDonDinhKem { get; set; }
    }

    public class QuanLyChiTietNhapKhoDto
    {
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; }
        public decimal ThanhTien => SoLuong * DonGiaNhap;
    }

    public class QuanLyNhapKhoSaveDto
    {
        public int IdNhaCungCap { get; set; }
        public decimal GiamGia { get; set; }
        public string? GhiChu { get; set; }
        public List<QuanLyChiTietNhapKhoSaveDto> ChiTiet { get; set; } = new();
        public string? FileDinhKemBase64 { get; set; }
        public string? TenFileDinhKem { get; set; }
    }

    public class QuanLyChiTietNhapKhoSaveDto
    {
        public int IdNguyenLieu { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; }
    }

    public class LookupNhapKhoDto { public int Id { get; set; } public string Ten { get; set; } = string.Empty; }
}