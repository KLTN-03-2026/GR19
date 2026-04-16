using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyChamCongGridDto
    {
        public int IdChamCong { get; set; }
        public int IdNhanVien { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public DateTime NgayLam { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public TimeSpan CaGioBatDau { get; set; }
        public TimeSpan CaGioKetThuc { get; set; }
        public TimeSpan? GioVao { get; set; }
        public TimeSpan? GioRa { get; set; }
        public double TongGioLam { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChuSua { get; set; }
    }

    public class QuanLyChamCongUpdateDto
    {
        public string? GioVao { get; set; } // "hh:mm"
        public string? GioRa { get; set; }  // "hh:mm"
        public string? GhiChuSua { get; set; }
    }

    public class ChamCongNhanVienLookupDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
    }
}