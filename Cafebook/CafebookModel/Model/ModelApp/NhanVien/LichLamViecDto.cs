// File: CafebookModel/Model/ModelApp/NhanVien/LichLamViecDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class LichLamViec_CaNhanDto
    {
        public int IdLichLamViec { get; set; }
        public DateTime NgayLam { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class LichLamViec_ConfigDto
    {
        public TimeSpan GioMoCua { get; set; }
        public TimeSpan GioDongCua { get; set; }
    }
}