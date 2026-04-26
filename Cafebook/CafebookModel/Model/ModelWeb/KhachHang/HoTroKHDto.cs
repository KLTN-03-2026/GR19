using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class ChatMessageDto
    {
        public long IdChat { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime ThoiGian { get; set; }
        public string LoaiTinNhan { get; set; } = "KhachHang";
        public int? IdThongBaoHoTro { get; set; }

        [JsonIgnore] public bool IsUser => LoaiTinNhan == "KhachHang";
        [JsonIgnore] public bool IsBot => LoaiTinNhan == "AI" || LoaiTinNhan == "NhanVien";
        [JsonIgnore] public string AvatarCssClass => LoaiTinNhan == "KhachHang" ? "avatar-user" : (LoaiTinNhan == "NhanVien" ? "avatar-staff" : "avatar-bot");
    }

    public class HoTroKHViewDto
    {
        public int IdKhachHang { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
    }

    public class ChatSessionKHDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = "Cuộc trò chuyện mới";
        public DateTime LastActive { get; set; }
        public int? IdThongBao { get; set; }
    }

    public class SendChatKHRequestDto
    {
        [Required] public string NoiDung { get; set; } = string.Empty;
        [Required] public string SessionId { get; set; } = string.Empty;
    }

    public class SendChatKHResponseDto
    {
        public ChatMessageDto TinNhanCuaKhach { get; set; } = null!;
        public ChatMessageDto? TinNhanPhanHoi { get; set; }
        public bool DaChuyenNhanVien { get; set; } = false;
        public int? IdThongBaoHoTro { get; set; }
    }
}