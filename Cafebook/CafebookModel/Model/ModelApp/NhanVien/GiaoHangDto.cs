using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class GiaoHangViewDto
    {
        public List<GiaoHangItemDto> DonGiaoHang { get; set; } = new();
        public List<NguoiGiaoHangDto> NguoiGiaoHangSanSang { get; set; } = new();
    }

    public class GiaoHangItemDto
    {
        public int IdHoaDon { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string? TenKhachHang { get; set; }
        public string? SoDienThoaiGiaoHang { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        public decimal ThanhTien { get; set; }
        public string TrangThaiThanhToan { get; set; } = string.Empty;
        public string? PhuongThucThanhToan { get; set; }

        public string? TrangThaiGiaoHang { get; set; }
        public int? IdNguoiGiaoHang { get; set; }
        public string? TenNguoiGiaoHang { get; set; }
        public string? GhiChu { get; set; }
        public int? IdNhanVien { get; set; }
    }

    public class NguoiGiaoHangDto
    {
        public int IdNguoiGiaoHang { get; set; }
        public string TenNguoiGiaoHang { get; set; } = string.Empty;
    }

    public class GiaoHangUpdateRequestDto
    {
        public string? TrangThaiGiaoHang { get; set; }
        public int? IdNguoiGiaoHang { get; set; }
    }
}