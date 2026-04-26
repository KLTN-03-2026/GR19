// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyDeXuatDto.cs
using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDeXuatGridDto
    {
        public string LoaiDoiTuong { get; set; } = string.Empty; // "SACH" hoặc "SANPHAM"
        public int IdGoc { get; set; }
        public string TenGoc { get; set; } = string.Empty;
        public int IdDeXuat { get; set; }
        public string TenDeXuat { get; set; } = string.Empty;
        public double DoLienQuan { get; set; }
        public string LoaiDeXuat { get; set; } = string.Empty;
    }

    public class QuanLyDeXuatSaveDto
    {
        public string LoaiDoiTuong { get; set; } = string.Empty;
        public int IdGoc { get; set; }
        public int IdDeXuat { get; set; }
        public double DoLienQuan { get; set; }
        public string LoaiDeXuat { get; set; } = string.Empty;
    }

    public class DeXuatLookupDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }
}