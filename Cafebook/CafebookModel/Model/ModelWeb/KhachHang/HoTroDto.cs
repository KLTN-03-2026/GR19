using System;
using System.ComponentModel.DataAnnotations;
using CafebookModel.Model.ModelWeb.KhachHang;

namespace CafebookModel.Model.ModelWeb.KhachVangLai
{
    public class HoTroViewDto
    {
        public string GuestSessionId { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = "Khách vãng lai";
    }

    public class SendChatGuestRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string NoiDung { get; set; } = string.Empty;
        public string GuestSessionId { get; set; } = string.Empty;
    }

    public class SendChatGuestResponseDto
    {
        public ChatMessageDto TinNhanCuaKhach { get; set; } = null!;
        public ChatMessageDto? TinNhanPhanHoi { get; set; }
    }
}