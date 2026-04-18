// Tập tin: CafebookModel/Model/ModelApp/NhanVien/GoiMonDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class GoiMonViewDto
    {
        public HoaDonInfoDto HoaDonInfo { get; set; } = default!;
        public List<ChiTietDto> ChiTietItems { get; set; } = default!;
        public List<SanPhamDto> SanPhams { get; set; } = default!;
        public List<DanhMucDto> DanhMucs { get; set; } = default!;
        public List<KhuyenMaiDto> KhuyenMais { get; set; } = default!;
    }

    public class HoaDonInfoDto
    {
        public int IdHoaDon { get; set; }
        public string SoBan { get; set; } = default!;
        public string LoaiHoaDon { get; set; } = default!;
        public decimal TongTienGoc { get; set; }
        public decimal GiamGia { get; set; }
        public decimal ThanhTien { get; set; }
        public int? IdKhuyenMai { get; set; }
        public bool ChoPhepThanhToan { get; set; }
    }

    public class ChiTietDto
    {
        public int IdChiTietHoaDon { get; set; }
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = default!;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string? GhiChu { get; set; }
    }

    public class SanPhamDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = default!;
        public decimal DonGia { get; set; }
        public string HinhAnh { get; set; } = default!;
        public int IdDanhMuc { get; set; }
    }

    public class DanhMucDto
    {
        public int IdDanhMuc { get; set; }
        public string TenLoaiSP { get; set; } = default!;
    }

    public class KhuyenMaiDto
    {
        public int IdKhuyenMai { get; set; }
        public string TenKhuyenMai { get; set; } = default!;
        public string LoaiGiamGia { get; set; } = default!;
        public decimal GiaTriGiam { get; set; }
    }

    public class AddItemRequest
    {
        public int IdHoaDon { get; set; }
        public int IdSanPham { get; set; }
        public int SoLuong { get; set; }
        public string? GhiChu { get; set; }
    }

    public class UpdateSoLuongRequest
    {
        public int IdChiTietHoaDon { get; set; }
        public int SoLuongMoi { get; set; }
    }

    public class ThanhToanRequest
    {
        public int IdHoaDon { get; set; }
        public int? IdKhuyenMai { get; set; }
        public string PhuongThucThanhToan { get; set; } = default!;
    }

    public class ApplyPromotionRequest
    {
        public int IdHoaDon { get; set; }
        public int? IdKhuyenMai { get; set; }
    }

    public class PhieuGoiMonPrintDto
    {
        public string IdPhieu { get; set; } = string.Empty;
        public string TenQuan { get; set; } = string.Empty;
        public string DiaChiQuan { get; set; } = string.Empty;
        public string SdtQuan { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string SoBan { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public List<ChiTietDto> ChiTiet { get; set; } = new List<ChiTietDto>();
        public decimal TongTienGoc { get; set; }
        public decimal GiamGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class KhuyenMaiHienThiGoiMonDto
    {
        public int IdKhuyenMai { get; set; }
        public string MaKhuyenMai { get; set; } = string.Empty;
        public string TenChuongTrinh { get; set; } = string.Empty;
        public string? DieuKienApDung { get; set; }
        public string LoaiGiamGia { get; set; } = string.Empty;
        public decimal GiaTriGiam { get; set; }
        public decimal? GiamToiDa { get; set; }
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public decimal CalculatedDiscount { get; set; }
    }
}