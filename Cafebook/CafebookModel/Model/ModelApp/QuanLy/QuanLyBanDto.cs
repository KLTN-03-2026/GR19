using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyKhuVucDto
    {
        public int IdKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public List<QuanLyBanGridDto> Bans { get; set; } = new List<QuanLyBanGridDto>();
    }

    public class QuanLyBanGridDto
    {
        public int IdBan { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public int SoGhe { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public int IdKhuVuc { get; set; }
        public string? TenKhuVuc { get; set; }
    }

    public class QuanLyBanSaveDto
    {
        public string SoBan { get; set; } = string.Empty;
        public int SoGhe { get; set; }
        public string TrangThai { get; set; } = "Trống";
        public string? GhiChu { get; set; }
        public int IdKhuVuc { get; set; }
    }

    public class QuanLyKhuVucSaveDto
    {
        public string TenKhuVuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }

    public class QuanLyBanHistoryDto
    {
        public int SoLuotPhucVu { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int SoLuotDatTruoc { get; set; }
    }

    // DTO chuyên biệt để đọc Thông báo sự cố bàn
    public class QuanLyBanThongBaoDto
    {
        public int IdThongBao { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime ThoiGianTao { get; set; }
        public string TenNhanVienTao { get; set; } = string.Empty;
        public bool DaXem { get; set; }
        public string LoaiThongBao { get; set; } = string.Empty;
    }
}