// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyBaoCaoTonKhoNguyenLieuDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyBaoCaoTonKhoNguyenLieuRequestDto
    {
        public string? SearchText { get; set; }
        public int? NhaCungCapId { get; set; }
        public bool ShowLowStockOnly { get; set; }
    }

    public class QuanLyBaoCaoTonKhoNguyenLieuTongHopDto
    {
        public QuanLyBaoCaoTonKhoKpiDto Kpi { get; set; } = new();
        public List<QuanLyBaoCaoTonKhoChiTietDto> ChiTietTonKho { get; set; } = new();
        public List<QuanLyBaoCaoKiemKeDto> LichSuKiemKe { get; set; } = new();
        public List<QuanLyBaoCaoHuyHangDto> LichSuHuyHang { get; set; } = new();
    }

    public class QuanLyBaoCaoTonKhoKpiDto
    {
        public decimal TongGiaTriTonKho { get; set; }
        public int SoLuongSPSapHet { get; set; }
        public decimal TongGiaTriDaHuy { get; set; }
    }

    public class QuanLyBaoCaoTonKhoChiTietDto
    {
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public decimal TonKho { get; set; }
        public decimal TonKhoToiThieu { get; set; }
        public string TinhTrang { get; set; } = string.Empty;
    }

    public class QuanLyBaoCaoKiemKeDto
    {
        public DateTime NgayKiem { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal TonKhoHeThong { get; set; }
        public decimal TonKhoThucTe { get; set; }
        public decimal ChenhLech { get; set; }
        public string? LyDoChenhLech { get; set; }
    }

    public class QuanLyBaoCaoHuyHangDto
    {
        public DateTime NgayHuy { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal SoLuongHuy { get; set; }
        public decimal GiaTriHuy { get; set; }
        public string LyDoHuy { get; set; } = string.Empty;
    }

    // THÊM CLASS NÀY VÀO FILE DTO
    public class QuanLyBaoCaoTonKho_FiltersDto
    {
        public List<QuanLyFilterLookupDto> NhaCungCaps { get; set; } = new();
    }
}