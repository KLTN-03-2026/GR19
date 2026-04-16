using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class PhatLuongGridDto
    {
        public int IdPhieuLuong { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string KyLuong { get; set; } = string.Empty; // "Tháng X/Y (Chốt: dd/MM)"
        public decimal TongGioLam { get; set; }
        public decimal ThucLanh { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class PhatLuongDetailDto
    {
        public int IdPhieuLuong { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string KyLuong { get; set; } = string.Empty;
        public DateTime NgayChot { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal TongGioLam { get; set; }

        public decimal LuongGoc => ThucLanh - TienThuong + KhauTru;

        public decimal TienThuong { get; set; }
        public decimal KhauTru { get; set; }
        public decimal ThucLanh { get; set; }
        public string TrangThai { get; set; } = string.Empty;

        // BỔ SUNG: Thông tin cấu hình quán để in hóa đơn
        public string TenQuan { get; set; } = "CAFEBOOK";
        public string DiaChiQuan { get; set; } = string.Empty;
        public string SoDienThoaiQuan { get; set; } = string.Empty;

        public List<ChiTietThuongPhatPhatLuongDto> DanhSachThuongPhat { get; set; } = new();
    }

    public class ChiTietThuongPhatPhatLuongDto
    {
        public string Loai { get; set; } = string.Empty; // "Thưởng" / "Phạt"
        public string LyDo { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }
}