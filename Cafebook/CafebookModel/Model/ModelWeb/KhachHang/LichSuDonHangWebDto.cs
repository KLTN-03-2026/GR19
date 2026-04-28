using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class LichSuDonHangWebDto
    {
        public int IdHoaDon { get; set; }
        public string MaDonHang { get; set; } = string.Empty;
        public DateTime ThoiGianTao { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuongSanPhamKhac { get; set; }
        public string? HinhAnhUrl { get; set; }
        public decimal ThanhTien { get; set; }
        public string TrangThaiGiaoHang { get; set; } = string.Empty;
        public string TrangThaiThanhToan { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        public bool IsStoreOpen { get; set; } = true; 
    }

    public class DonHangChiTietWebDto
    {
        public int IdHoaDon { get; set; }
        public string MaDonHang { get; set; } = string.Empty;
        public string TrangThaiGiaoHang { get; set; } = string.Empty;
        public string TrangThaiThanhToan { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        public DateTime ThoiGianTao { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public List<TrackingEventDto> TrackingEvents { get; set; } = new();
        public List<DonHangItemWebDto> Items { get; set; } = new();
        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal PhiGiaoHang { get; set; }
        public decimal ThanhTien { get; set; }
        public string? AnhXacNhanGiaoHangUrl { get; set; }
        public bool IsStoreOpen { get; set; } = true; 
    }

    public class TrackingEventDto
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCurrent { get; set; } = false;
    }

    public class DonHangItemWebDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? HinhAnhUrl { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public bool DaDanhGia { get; set; }
    }

    public class HuyDonHangRequestDto
    {
        public string LyDoHuy { get; set; } = string.Empty;
    }

}