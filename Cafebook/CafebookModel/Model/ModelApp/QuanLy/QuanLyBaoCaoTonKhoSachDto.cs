// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyBaoCaoTonKhoSachDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyBaoCaoSachRequestDto
    {
        public string? SearchText { get; set; }
        public int? TheLoaiId { get; set; }
        public int? TacGiaId { get; set; }
    }

    public class QuanLyBaoCaoTonKhoSach_FiltersDto
    {
        public List<QuanLyFilterLookupDto> TheLoais { get; set; } = new();
        public List<QuanLyFilterLookupDto> TacGias { get; set; } = new();
    }

    public class QuanLyBaoCaoSachTongHopDto
    {
        public QuanLyBaoCaoSachKpiDto Kpi { get; set; } = new();
        public List<QuanLyBaoCaoSachChiTietDto> ChiTietTonKho { get; set; } = new();
        public List<QuanLyBaoCaoSachTreHanDto> SachTreHan { get; set; } = new();
        public List<QuanLyTopSachDuocThueDto> TopSachThue { get; set; } = new();
    }

    public class QuanLyBaoCaoSachKpiDto
    {
        public int TongDauSach { get; set; }
        public int TongSoLuong { get; set; }
        public int DangChoThue { get; set; }
        public int SanSang { get; set; }
    }

    public class QuanLyBaoCaoSachChiTietDto
    {
        public string TenSach { get; set; } = string.Empty;
        public string? TenTacGia { get; set; }
        public string? TenTheLoai { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongDangMuon { get; set; }
        public int SoLuongConLai { get; set; }
    }

    public class QuanLyBaoCaoSachTreHanDto
    {
        public string TenSach { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public DateTime NgayThue { get; set; }
        public DateTime NgayHenTra { get; set; }
        public string TinhTrang { get; set; } = string.Empty;
    }

    public class QuanLyTopSachDuocThueDto
    {
        public string TenSach { get; set; } = string.Empty;
        public string? TenTacGia { get; set; }
        public int TongLuotThue { get; set; }
    }
}