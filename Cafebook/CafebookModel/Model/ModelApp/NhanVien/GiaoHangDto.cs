using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class GiaoHangViewDto
    {
        public List<GiaoHangItemDto> DonGiaoHang { get; set; } = new();
        public List<NguoiGiaoHangDto> NguoiGiaoHangSanSang { get; set; } = new(); // Sẽ chứa danh sách Nhân viên Ship nội bộ
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
        public string? TrangThaiGiaoHang { get; set; }

        public int? IdNguoiGiaoHang { get; set; } // Lưu IdNhanVien của Shipper nội bộ
        public string? TenNguoiGiaoHang { get; set; }
    }

    public class NguoiGiaoHangDto
    {
        public int IdNguoiGiaoHang { get; set; } // Đây là IdNhanVien
        public string TenNguoiGiaoHang { get; set; } = string.Empty;
    }

    public class GiaoHangUpdateRequestDto
    {
        public string? TrangThaiGiaoHang { get; set; }
        public int? IdNguoiGiaoHang { get; set; }
    }
}