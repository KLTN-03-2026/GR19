using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyLichLamViec_CaiDatDto
    {
        public TimeSpan GioMoCua { get; set; }
        public TimeSpan GioDongCua { get; set; }
        public List<int> ThuMoCua { get; set; } = new List<int>();
    }

    public class QuanLyLichLamViec_ItemDto
    {
        public int IdNhuCau { get; set; }
        public DateTime NgayLam { get; set; }
        public int IdCa { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }

        public string TenVaiTroYeuCau { get; set; } = string.Empty;
        public int SoLuongCan { get; set; }
        public string LoaiYeuCau { get; set; } = string.Empty;
        public string? GhiChu { get; set; } // THÊM THUỘC TÍNH NÀY

        public List<QuanLyLichLamViec_NhanVienDangKyDto> NhanViens { get; set; } = new();
    }

    public class QuanLyLichLamViec_NhanVienDangKyDto
    {
        public int IdLichLamViec { get; set; }
        public int IdNhanVien { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Chờ duyệt";
        public string? GhiChu { get; set; } // THÊM DÒNG NÀY
    }

    public class QuanLyLichLamViec_CaDto
    {
        public int IdCa { get; set; }
        [Required] public string TenCa { get; set; } = string.Empty;
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
    }

    public class QuanLyLichLamViec_NhuCauSaveDto
    {
        public DateTime NgayLam { get; set; }
        public int IdCa { get; set; }
        public int IdVaiTro { get; set; }
        public int SoLuongCan { get; set; }
        public string LoaiYeuCau { get; set; } = "Tất cả";
        public string? GhiChu { get; set; }
    }

    public class QuanLyLichLamViec_AssignDto
    {
        public int IdNhanVien { get; set; }
        public int IdCa { get; set; }
        public DateTime NgayLam { get; set; }
        public string TrangThai { get; set; } = "Đã duyệt";
        public string? GhiChu { get; set; } // THÊM DÒNG NÀY
    }

    public class QuanLyNhanVienLookupDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
    }

    public class QuanLyVaiTroLookupDto
    {
        public int IdVaiTro { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
    }
}