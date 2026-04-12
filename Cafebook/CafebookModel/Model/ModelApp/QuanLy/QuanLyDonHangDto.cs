using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDonHangGridDto
    {
        public int IdHoaDon { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TenBan { get; set; } = string.Empty;
        public string NhanVien { get; set; } = string.Empty;
        public string KhachHang { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public string LoaiHoaDon { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string TrangThaiGiaoHang { get; set; } = string.Empty;
    }

    public class QuanLyDonHangDetailDto
    {
        public int IdHoaDon { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TenBan { get; set; } = string.Empty;
        public string NhanVien { get; set; } = string.Empty;
        public string KhachHang { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public decimal GiamGia { get; set; }
        public decimal PhuThu { get; set; }
        public string LoaiHoaDon { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string TrangThaiGiaoHang { get; set; } = string.Empty;
        public string NguoiGiaoHang { get; set; } = string.Empty;
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public string SoDienThoaiGiaoHang { get; set; } = string.Empty;
        public List<QuanLyChiTietDonHangDto> ChiTiet { get; set; } = new();
    }

    public class QuanLyChiTietDonHangDto
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
        public string GhiChu { get; set; } = string.Empty;
    }

    public class QuanLyDonHangUpdateStatusDto
    {
        public string TrangThai { get; set; } = string.Empty;
    }
}