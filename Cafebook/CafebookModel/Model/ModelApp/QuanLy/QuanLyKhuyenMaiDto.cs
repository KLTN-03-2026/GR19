using System;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyKhuyenMaiGridDto
    {
        public int IdKhuyenMai { get; set; }
        public string MaKhuyenMai { get; set; } = string.Empty;
        public string TenChuongTrinh { get; set; } = string.Empty;
        public string LoaiGiamGia { get; set; } = string.Empty;
        public string GiaTriGiam { get; set; } = string.Empty; // Trả về string đã format (vd: 10,00% hoặc 20.000đ)
        public decimal? GiamToiDa { get; set; }
        public int? SoLuongConLai { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class QuanLyKhuyenMaiSaveDto
    {
        public int IdKhuyenMai { get; set; }
        [Required] public string MaKhuyenMai { get; set; } = string.Empty;
        [Required] public string TenChuongTrinh { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        [Required] public string LoaiGiamGia { get; set; } = string.Empty;
        public decimal GiaTriGiam { get; set; }
        public decimal? GiamToiDa { get; set; }
        public decimal? HoaDonToiThieu { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }
        public string? NgayTrongTuan { get; set; }
        public int? IdSanPhamApDung { get; set; }
        public int? SoLuongConLai { get; set; }
        public string? DieuKienApDung { get; set; }
        public string TrangThai { get; set; } = "Hoạt động";
    }

    public class QuanLyKhuyenMaiLookupDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }
}