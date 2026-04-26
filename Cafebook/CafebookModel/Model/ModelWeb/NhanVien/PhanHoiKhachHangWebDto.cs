using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class DanhGiaWebDto
    {
        public int IdDanhGia { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string? TenSanPham { get; set; }
        public int SoSao { get; set; }
        public string? BinhLuan { get; set; }
        public string? HinhAnhUrl { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = "Hiển thị";
        public List<PhanHoiReviewWebDto> DanhSachPhanHoi { get; set; } = new();
    }

    public class PhanHoiReviewWebDto
    {
        public string TenNhanVien { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
    }

    public class GopYWebDto
    {
        public int IdGopY { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = "Chờ xử lý";
    }

    public class PhanHoiInputWebDto
    {
        public string NoiDung { get; set; } = string.Empty;
    }

    public class GopYReplyRequestDto
    {
        public string NoiDungEmail { get; set; } = string.Empty;
    }
}