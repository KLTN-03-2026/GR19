using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class GoiMonViewWebDto
    {
        public HoaDonInfoWebDto HoaDonInfo { get; set; } = new();
        public List<ChiTietWebDto> ChiTietItems { get; set; } = new();
        public List<SanPhamWebDto> SanPhams { get; set; } = new();
        public List<DanhMucWebDto> DanhMucs { get; set; } = new();
        public List<KhuyenMaiWebDto> KhuyenMais { get; set; } = new();
    }

    public class HoaDonInfoWebDto
    {
        public int IdHoaDon { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public string LoaiHoaDon { get; set; } = string.Empty;
        public decimal TongTienGoc { get; set; }
        public decimal GiamGia { get; set; }
        public decimal ThanhTien { get; set; }
        public int? IdKhuyenMai { get; set; }
    }

    public class ChiTietWebDto
    {
        public int IdChiTietHoaDon { get; set; }
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string? GhiChu { get; set; }
    }

    public class SanPhamWebDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public string HinhAnh { get; set; } = string.Empty;
        public int IdDanhMuc { get; set; }
    }

    public class DanhMucWebDto
    {
        public int IdDanhMuc { get; set; }
        public string TenLoaiSP { get; set; } = string.Empty;
    }

    public class KhuyenMaiWebDto
    {
        public int IdKhuyenMai { get; set; }
        public string TenKhuyenMai { get; set; } = string.Empty;
    }

    public class KhuyenMaiHienThiGoiMonWebDto
    {
        public int IdKhuyenMai { get; set; }
        public string MaKhuyenMai { get; set; } = string.Empty;
        public string TenChuongTrinh { get; set; } = string.Empty;
        public string DieuKienApDung { get; set; } = string.Empty;
        public string LoaiGiamGia { get; set; } = string.Empty;
        public decimal GiaTriGiam { get; set; }
        public decimal? GiamToiDa { get; set; }
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public decimal CalculatedDiscount { get; set; }
    }

    public class AddItemWebRequest
    {
        public int IdHoaDon { get; set; }
        public int IdSanPham { get; set; }
        public int SoLuong { get; set; }
        public string? GhiChu { get; set; }
    }

    public class UpdateSoLuongWebRequest
    {
        public int IdChiTietHoaDon { get; set; }
        public int SoLuongMoi { get; set; }
    }

    public class ApplyPromotionWebRequest
    {
        public int IdHoaDon { get; set; }
        public int? IdKhuyenMai { get; set; }
    }
}