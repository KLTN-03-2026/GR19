// File: CafebookModel/Model/ModelApp/NhanVien/PhieuLuongDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class PhieuLuongViewDto
    {
        public List<PhieuLuongItemDto> DanhSachPhieuLuong { get; set; } = new List<PhieuLuongItemDto>();
    }

    public class PhieuLuongItemDto
    {
        public int IdPhieuLuong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal ThucLanh { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string TieuDe => $"Phiếu lương tháng {Thang}/{Nam}";
    }

    public class PhieuLuongChiTietDto
    {
        public int IdPhieuLuong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal TongGioLam { get; set; }
        public decimal TienLuongTheoGio { get; set; }
        public decimal TongTienThuong { get; set; }
        public decimal TongKhauTru { get; set; }
        public decimal ThucLanh { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public DateTime? NgayPhatLuong { get; set; }
        public string? TenNguoiPhat { get; set; }

        public List<PhieuThuongPhatItemDto> DanhSachThuong { get; set; } = new List<PhieuThuongPhatItemDto>();
        public List<PhieuThuongPhatItemDto> DanhSachPhat { get; set; } = new List<PhieuThuongPhatItemDto>();
    }

    public class PhieuThuongPhatItemDto
    {
        public DateTime NgayTao { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string TenNguoiTao { get; set; } = string.Empty;
    }
}