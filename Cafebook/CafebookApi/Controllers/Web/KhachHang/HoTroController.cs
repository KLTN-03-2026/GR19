using CafebookApi.Data;
using CafebookApi.Services;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachVangLai;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachVangLai
{
    [Route("api/web/guest/hotro")]
    [ApiController]
    [AllowAnonymous]
    public class HoTroController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly AiService _aiService;

        public HoTroController(CafebookDbContext context, AiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string guestSessionId)
        {
            if (string.IsNullOrEmpty(guestSessionId)) return Ok(new List<ChatMessageDto>());

            var rawChats = await _context.ChatLichSus
                .Where(c => c.IdKhachHang == null && c.GuestSessionId == guestSessionId)
                .OrderByDescending(c => c.ThoiGian).Take(50).ToListAsync();

            rawChats.Reverse();
            var chatHistory = new List<ChatMessageDto>();
            foreach (var c in rawChats)
            {
                if (c.LoaiChat == "Web_HTTP")
                {
                    if (!string.IsNullOrEmpty(c.NoiDungHoi) && c.NoiDungHoi != "Chat Realtime")
                        chatHistory.Add(new ChatMessageDto { IdChat = c.IdChat, NoiDung = c.NoiDungHoi, ThoiGian = c.ThoiGian.AddMilliseconds(-10), LoaiTinNhan = "KhachHang" });

                    if (!string.IsNullOrEmpty(c.NoiDungTraLoi))
                        chatHistory.Add(new ChatMessageDto { IdChat = c.IdChat, NoiDung = c.NoiDungTraLoi, ThoiGian = c.ThoiGian, LoaiTinNhan = "AI" });
                }
                else
                {
                    chatHistory.Add(new ChatMessageDto { IdChat = c.IdChat, NoiDung = c.NoiDungTraLoi, ThoiGian = c.ThoiGian, LoaiTinNhan = c.LoaiTinNhan ?? "KhachHang" });
                }
            }
            return Ok(chatHistory);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatGuestRequestDto request)
        {
            var responseDto = new SendChatGuestResponseDto();
            var history = await _context.ChatLichSus
                .Where(c => c.IdKhachHang == null && c.GuestSessionId == request.GuestSessionId)
                .OrderByDescending(c => c.ThoiGian).Take(15).OrderBy(c => c.ThoiGian).ToListAsync();

            var chatHistory = new List<object>();
            foreach (var msg in history)
            {
                if (msg.LoaiChat == "Web_HTTP")
                {
                    if (!string.IsNullOrEmpty(msg.NoiDungHoi)) chatHistory.Add(new { role = "user", parts = new[] { new { text = msg.NoiDungHoi } } });
                    if (!string.IsNullOrEmpty(msg.NoiDungTraLoi)) chatHistory.Add(new { role = "model", parts = new[] { new { text = msg.NoiDungTraLoi } } });
                }
                else
                {
                    chatHistory.Add(new { role = msg.LoaiTinNhan == "KhachHang" ? "user" : "model", parts = new[] { new { text = msg.NoiDungTraLoi } } });
                }
            }

            string? aiResponse = await _aiService.GetAnswerAsync(request.NoiDung, null, chatHistory);

            // Xử lý khi AI báo lỗi hoặc có tag fallback nhân viên
            string phanHoiCuoiCung = aiResponse ?? "";
            if (string.IsNullOrEmpty(aiResponse) || aiResponse.Contains("[NEEDS_SUPPORT]"))
            {
                phanHoiCuoiCung = "Hiện tại trợ lý AI đang gặp chút sự cố hoặc không thể xử lý yêu cầu này. Xin vui lòng thử lại sau!";
            }

            var lichSuGop = new ChatLichSu
            {
                GuestSessionId = request.GuestSessionId,
                NoiDungHoi = request.NoiDung,
                NoiDungTraLoi = phanHoiCuoiCung,
                ThoiGian = DateTime.Now,
                LoaiChat = "Web_HTTP",
                LoaiTinNhan = "AI"
            };

            _context.ChatLichSus.Add(lichSuGop);
            await _context.SaveChangesAsync();

            responseDto.TinNhanCuaKhach = new ChatMessageDto { IdChat = lichSuGop.IdChat, NoiDung = request.NoiDung, ThoiGian = lichSuGop.ThoiGian, LoaiTinNhan = "KhachHang" };
            responseDto.TinNhanPhanHoi = new ChatMessageDto { IdChat = lichSuGop.IdChat, NoiDung = phanHoiCuoiCung, ThoiGian = lichSuGop.ThoiGian, LoaiTinNhan = "AI" };

            return Ok(responseDto);
        }
    }
}