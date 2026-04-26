using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class DangKyLichConfigWebDto
    {
        public TimeSpan GioMoCua { get; set; }
        public TimeSpan GioDongCua { get; set; }
        public List<int> ThuMoCua { get; set; } = new();
    }

    public class NhuCauLichWebDto
    {
        public int IdNhuCau { get; set; }
        public DateTime NgayLam { get; set; }
        public int IdCa { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string TenVaiTroYeuCau { get; set; } = string.Empty;
        public int SoLuongCan { get; set; }
        public int SoLuongDaDangKy { get; set; }
        public string? GhiChu { get; set; }

        public string TrangThaiCuaToi { get; set; } = "Chưa đăng ký";
        public int? IdLichCuaToi { get; set; }
        public string? GhiChuCuaToi { get; set; }
        public bool IsQuaHan { get; set; }
    }

    public class DangKyCaRequestDto
    {
        public int IdNhuCau { get; set; }
        public string? GhiChuNhanVien { get; set; }
    }
}