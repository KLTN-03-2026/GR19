using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLySachGridDto
    {
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string TenTacGia { get; set; } = string.Empty;
        public string TenTheLoai { get; set; } = string.Empty;
        public string? ViTri { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongDangMuon { get; set; }
        public int SoLuongHienCo { get; set; }
    }

    public class QuanLySachDetailDto
    {
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string DanhSachTacGia { get; set; } = string.Empty;
        public string DanhSachTheLoai { get; set; } = string.Empty;
        public string DanhSachNhaXuatBan { get; set; } = string.Empty;
        public string? ViTri { get; set; }
        public int? NamXuatBan { get; set; }
        public decimal? GiaBia { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongHienCo { get; set; }
        public string? MoTa { get; set; }
        public string? AnhBia { get; set; }
    }

    public class QuanLySachFilterLookupDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }
}