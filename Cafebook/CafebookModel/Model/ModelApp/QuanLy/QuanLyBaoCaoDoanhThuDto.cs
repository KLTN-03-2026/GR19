// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyBaoCaoDoanhThuDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyBaoCaoDoanhThuRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class QuanLyBaoCaoTongHopDto
    {
        public QuanLyBaoCaoKpiDto Kpi { get; set; } = new();
        public QuanLyBaoCaoChiTietDoanhThuDto ChiTietDoanhThu { get; set; } = new();
        public QuanLyBaoCaoChiPhiDto ChiTietChiPhi { get; set; } = new();
        public List<QuanLyTopSanPhamDto> TopSanPham { get; set; } = new();
        public List<QuanLyGoiYDoanhThuDto> GoiYDoanhThu { get; set; } = new(); // THÊM DÒNG NÀY
    }

    public class QuanLyBaoCaoKpiDto
    {
        public decimal DoanhThuRong { get; set; }
        public decimal TongGiaVon { get; set; }
        public decimal LoiNhuanGop { get; set; }
        public decimal ChiPhiOpex { get; set; }
        public decimal LoiNhuanRong { get; set; }
    }

    public class QuanLyBaoCaoChiTietDoanhThuDto
    {
        public decimal TongDoanhThuBanHang { get; set; }
        public decimal TongDoanhThuThueSach { get; set; } // Phí thuê sách
        public decimal TongGiamGia { get; set; }
        public decimal TongPhuThu { get; set; }
        public decimal DoanhThuRong { get; set; }
        public int SoLuongHoaDon { get; set; }
        public decimal GiaTriTrungBinhHD { get; set; }
    }

    public class QuanLyOpexDto
    {
        public decimal TongChiPhiLuong { get; set; }
        public decimal TongChiPhiHuyHang { get; set; }
    }

    public class QuanLyBaoCaoChiPhiDto
    {
        public decimal TongGiaVon_COGS { get; set; }
        public decimal TongChiPhiLuong { get; set; }
        public decimal TongChiPhiHuyHang { get; set; }
    }

    public class QuanLyTopSanPhamDto
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int TongSoLuongBan { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    public class QuanLyGoiYDoanhThuDto
    {
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaVon { get; set; }
        public decimal GiaBanHienTai { get; set; }
        public decimal GiaGoiY { get; set; }
        public decimal TiLeLoiNhuanCu { get; set; } // Tính theo %
    }
}