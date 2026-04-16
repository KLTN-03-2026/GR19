using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyLuongBangKeDto
    {
        public int IdNhanVien { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public decimal LuongCoBan { get; set; }

        public double TongGioLamChuan { get; set; }
        public double TongGioOT { get; set; }
        public double TongGioTre { get; set; }

        public decimal TienLuongChuan { get; set; }
        public decimal TienThuongOT { get; set; }
        public decimal TienPhatTre { get; set; }

        public decimal ThuongThuCong { get; set; }
        public decimal PhatThuCong { get; set; }

        public decimal TongThuong => TienThuongOT + ThuongThuCong;
        public decimal TongPhat => TienPhatTre + PhatThuCong;
        public decimal ThucLanh => TienLuongChuan + TongThuong - TongPhat;

        // Danh sách bóc tách để hiển thị bên phải UI
        public List<ChiTietThuongPhatDto> DanhSachThuongPhat { get; set; } = new();
    }

    public class ChiTietThuongPhatDto
    {
        public int Id { get; set; }
        public string Loai { get; set; } = string.Empty; // "Thưởng" hoặc "Phạt"
        public string LyDo { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public bool IsAuto { get; set; } // true: OT/Trễ (Hệ thống tính), false: Thủ công
        public string Nguon => IsAuto ? "Tự động" : "Thủ công";
    }

    public class TaoThuongPhatDto
    {
        public int IdNhanVien { get; set; }
        public string Loai { get; set; } = "Thưởng";
        public string LyDo { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }

    public class QuanLyLuongChotRequestDto
    {
        public DateTime TuNgay { get; set; }
        public List<QuanLyLuongBangKeDto> DanhSachChot { get; set; } = new();
    }

    // CLASS BỊ THIẾU ĐÃ ĐƯỢC THÊM VÀO ĐÂY
    public class ThuongPhatMauLookupDto
    {
        public int IdMau { get; set; }
        public string TenMau { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }
}