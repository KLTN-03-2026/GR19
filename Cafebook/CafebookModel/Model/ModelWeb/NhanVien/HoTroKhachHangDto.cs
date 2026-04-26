using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class ChatMessageNVDto
    {
        public long IdChat { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime ThoiGian { get; set; }
        public string LoaiTinNhan { get; set; } = "KhachHang";
        public int? IdThongBaoHoTro { get; set; }
    }

    public class HoTroKhachHangListDto
    {
        public int IdThongBao { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string? NoiDungYeuCau { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChuTuAI { get; set; }
    }

    public class HoTroKhachHangDetailDto
    {
        public int IdThongBao { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public int? IdKhachHang { get; set; }
        public string? GuestSessionId { get; set; }
        public string? NoiDungYeuCau { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChuTuAI { get; set; }
        public List<ChatMessageNVDto> LichSuChat { get; set; } = new List<ChatMessageNVDto>();
    }
}