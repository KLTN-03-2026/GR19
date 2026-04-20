// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyBaoCaoHieuSuatDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyBaoCaoHieuSuatRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? VaiTroId { get; set; }
        public string? SearchText { get; set; }
    }

    public class QuanLyBaoCaoHieuSuatTongHopDto
    {
        public QuanLyBaoCaoHieuSuatKpiDto Kpi { get; set; } = new();
        public List<QuanLyBaoCaoSalesDto> SalesPerformance { get; set; } = new();
        public List<QuanLyBaoCaoOperationsDto> OperationalPerformance { get; set; } = new();
        public List<QuanLyBaoCaoAttendanceDto> Attendance { get; set; } = new();
    }

    public class QuanLyBaoCaoHieuSuatKpiDto
    {
        public decimal TongDoanhThu { get; set; }
        public decimal TongGioLam { get; set; }
        public int TongSoCaLam { get; set; }
        public int TongLanHuyMon { get; set; }
    }

    public class QuanLyBaoCaoSalesDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public decimal TongDoanhThu { get; set; }
        public int SoHoaDon { get; set; }
        public decimal DoanhThuTrungBinh { get; set; }
        public int SoLanHuyMon { get; set; }
    }

    public class QuanLyBaoCaoOperationsDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public int PhieuNhap { get; set; }
        public int PhieuKiem { get; set; }
        public int PhieuHuy { get; set; }
        public int DonDuyet { get; set; }
    }

    public class QuanLyBaoCaoAttendanceDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public int SoCaLam { get; set; }
        public decimal TongGioLam { get; set; }
        public int SoDonXinNghi { get; set; }
        public int SoDonDaDuyet { get; set; }
        public int SoDonChoDuyet { get; set; }
    }

    public class QuanLyFilterLookupDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }
}