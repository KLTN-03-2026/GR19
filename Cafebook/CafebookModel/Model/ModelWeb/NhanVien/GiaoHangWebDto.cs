using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class GiaoHangSummaryWebDto
    {
        public int TongDonCho { get; set; }
        public int TongDonDangGiao { get; set; }
        public decimal TongTienMatCanThu { get; set; }
    }

    public class DonGiaoHangWebDto
    {
        public int IdHoaDon { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public decimal ThanhTien { get; set; }

        public string TrangThaiThanhToan { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; } = string.Empty; 
        public string TrangThaiGiaoHang { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class ChiTietDonGiaoWebDto
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string? GhiChuMon { get; set; }
    }

    public class ShipperHistorySummaryWebDto
    {
        public decimal TongTienMatCam { get; set; }
        public int TongDonHoanThanh { get; set; }
        public int TongDonHuy { get; set; }
        public List<DonGiaoHangWebDto> LichSuDonHang { get; set; } = new();
    }

    public class CapNhatGiaoHangWebRequest
    {
        public string TacVu { get; set; } = string.Empty;
        public string? LyDoHuy { get; set; }
    }
}