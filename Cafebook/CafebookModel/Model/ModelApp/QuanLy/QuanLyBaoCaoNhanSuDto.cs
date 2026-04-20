// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyBaoCaoNhanSuDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyBaoCaoNhanSuRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? VaiTroId { get; set; }
        public int? NhanVienId { get; set; }
    }

    public class QuanLyBaoCaoNhanSu_FiltersDto
    {
        public List<QuanLyFilterLookupDto> NhanViens { get; set; } = new();
        public List<QuanLyFilterLookupDto> VaiTros { get; set; } = new();
    }

    public class QuanLyBaoCaoNhanSuTongHopDto
    {
        public QuanLyBaoCaoNhanSuKpiDto Kpi { get; set; } = new();
        public List<QuanLyBangLuongChiTietDto> BangLuongChiTiet { get; set; } = new();
        public List<QuanLyThongKeNghiPhepDto> ThongKeNghiPhep { get; set; } = new();
        public List<QuanLyChartDataPointDto> LuongChartData { get; set; } = new();
    }

    public class QuanLyBaoCaoNhanSuKpiDto
    {
        public decimal TongLuongDaTra { get; set; }
        public decimal TongGioLam { get; set; }
        public int TongSoNgayNghi { get; set; }
    }

    public class QuanLyBangLuongChiTietDto
    {
        public int IdNhanVien { get; set; }
        public string HoTenNhanVien { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public decimal LuongCoBan { get; set; }
        public decimal TongGioLam { get; set; }
        public decimal TienThuong { get; set; }
        public decimal KhauTru { get; set; }
        public decimal ThucLanh { get; set; }
    }

    public class QuanLyThongKeNghiPhepDto
    {
        public int IdNhanVien { get; set; }
        public string HoTenNhanVien { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public int SoDonDaDuyet { get; set; }
        public int TongSoNgayNghi { get; set; }
    }

    public class QuanLyChartDataPointDto
    {
        public DateTime Ngay { get; set; }
        public decimal TongTien { get; set; }
    }
}