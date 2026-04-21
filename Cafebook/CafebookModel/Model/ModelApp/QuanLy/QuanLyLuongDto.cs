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

        // Thống kê vi phạm tự động
        public int SoLanTre { get; set; }
        public int SoLanSom { get; set; }

        public decimal TienLuongChuan { get; set; }
        public decimal TienThuongOT { get; set; }
        public decimal ThuongChuyenCan { get; set; }
        public decimal TienPhatTreSom { get; set; }

        // Khoản thủ công (Tuyệt đối không xóa)
        public decimal ThuongThuCong { get; set; }
        public decimal PhatThuCong { get; set; }

        // Thuộc tính tính toán động
        public decimal TongThuong => TienThuongOT + ThuongChuyenCan + ThuongThuCong;
        public decimal TongPhat => TienPhatTreSom + PhatThuCong;
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
        public bool IsAuto { get; set; } // true: OT/Trễ/CC (Hệ thống tính), false: Thủ công
        public string Nguon => IsAuto ? "Tự động" : "Thủ công";
    }

    public class TaoThuongPhatDto
    {
        public int IdNhanVien { get; set; }
        public string Loai { get; set; } = "Thưởng";
        public string LyDo { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        // Thêm IdNguoiTao để tránh lỗi khóa ngoại nếu truyền từ UI
        public int IdNguoiTao { get; set; } = 1;
    }

    public class QuanLyLuongChotRequestDto
    {
        public DateTime TuNgay { get; set; }
        public List<QuanLyLuongBangKeDto> DanhSachChot { get; set; } = new();
    }

    public class ThuongPhatMauLookupDto
    {
        public int IdMau { get; set; }
        public string TenMau { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }
}