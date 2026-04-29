using CafebookApi.Data;
using CafebookApi.Hubs;
using CafebookApi.Services;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/hotro")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class HoTroKHController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly AiService _aiService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public HoTroKHController(CafebookDbContext context, AiService aiService, IHubContext<ChatHub> chatHubContext)
        {
            _context = context; _aiService = aiService; _chatHubContext = chatHubContext;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            int.TryParse(idClaim?.Value, out int id); return id;
        }

        // HÀM BỊ THIẾU TRONG LẦN TRƯỚC - ĐÃ THÊM LẠI
        private async Task<(bool IsOpen, string Message)> CheckBusinessHoursAsync()
        {
            var settings = await _context.CaiDats.Where(c => c.TenCaiDat.StartsWith("ThongTin_")).ToListAsync();
            var thuMoCuaStr = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_ThuMoCua")?.GiaTri ?? "2,3,4,5,6,7,8";
            var gioMoCuaStr = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioMoCua")?.GiaTri ?? "07:00";
            var gioDongCuaStr = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioDongCua")?.GiaTri ?? "22:00";

            var now = DateTime.Now;
            int currentDayVn = now.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)now.DayOfWeek + 1;
            bool isDayOpen = thuMoCuaStr.Split(',').Contains(currentDayVn.ToString());
            TimeSpan.TryParse(gioMoCuaStr, out TimeSpan gioMo);
            TimeSpan.TryParse(gioDongCuaStr, out TimeSpan gioDong);

            if (isDayOpen && now.TimeOfDay >= gioMo && now.TimeOfDay <= gioDong) return (true, "");
            return (false, $"Hiện tại quán đang ngoài giờ làm việc (Giờ hoạt động: {gioMoCuaStr} - {gioDongCuaStr}). Yêu cầu của bạn đã được ghi nhận. Nhân viên sẽ xem và phản hồi bạn vào khung giờ làm việc tiếp theo.");
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions([FromQuery] string? guestSessionId)
        {
            var idKhachHang = GetCurrentUserId();

            // ========================================================
            // ĐỒNG BỘ LIỀN MẠCH: CHUYỂN CHỦ SỞ HỮU ĐOẠN CHAT VÃNG LAI
            // ========================================================
            if (!string.IsNullOrEmpty(guestSessionId))
            {
                var unlinkedChats = await _context.ChatLichSus
                    .Where(c => c.GuestSessionId == guestSessionId && c.IdKhachHang == null).ToListAsync();
                var unlinkedTickets = await _context.ThongBaoHoTros
                    .Where(t => t.GuestSessionId == guestSessionId && t.IdKhachHang == null).ToListAsync();

                bool hasChanges = false;
                if (unlinkedChats.Any())
                {
                    foreach (var c in unlinkedChats) c.IdKhachHang = idKhachHang;
                    hasChanges = true;
                }
                if (unlinkedTickets.Any())
                {
                    foreach (var t in unlinkedTickets) t.IdKhachHang = idKhachHang;
                    hasChanges = true;
                }
                if (hasChanges) await _context.SaveChangesAsync();
            }

            // Lấy danh sách các session đang mở
            var validSessionIds = await _context.ThongBaoHoTros
                .AsNoTracking()
                .Where(t => t.IdKhachHang == idKhachHang && t.TrangThai != "Đã xử lý")
                .Select(t => t.GuestSessionId)
                .Distinct()
                .ToListAsync();

            if (!string.IsNullOrEmpty(guestSessionId) && !validSessionIds.Contains(guestSessionId))
            {
                bool hasHistory = await _context.ChatLichSus.AnyAsync(c => c.GuestSessionId == guestSessionId && c.IdKhachHang == idKhachHang);
                if (hasHistory) validSessionIds.Add(guestSessionId);
            }

            if (!validSessionIds.Any()) return Ok(new List<ChatSessionKHDto>());

            var sessionData = await _context.ChatLichSus
                .AsNoTracking()
                .Where(c => validSessionIds.Contains(c.GuestSessionId))
                .GroupBy(c => c.GuestSessionId)
                .Select(g => new {
                    SessionId = g.Key!,
                    LastActive = g.Max(x => x.ThoiGian),
                    IdThongBao = g.Max(x => x.IdThongBaoHoTro)
                })
                .OrderByDescending(x => x.LastActive)
                .ToListAsync();

            var sessions = new List<ChatSessionKHDto>();
            foreach (var s in sessionData)
            {
                var firstMsg = await _context.ChatLichSus
                    .AsNoTracking()
                    .Where(c => c.GuestSessionId == s.SessionId && c.LoaiTinNhan == "KhachHang" && c.LoaiChat == "Web_HTTP")
                    .OrderBy(c => c.ThoiGian).Select(c => c.NoiDungHoi).FirstOrDefaultAsync();

                string title = "Trò chuyện với AI";
                if (s.IdThongBao > 0)
                {
                    title = "Yêu cầu hỗ trợ";
                }
                else if (!string.IsNullOrEmpty(firstMsg))
                {
                    title = firstMsg.Length > 25 ? firstMsg.Substring(0, 25) + "..." : firstMsg;
                }

                sessions.Add(new ChatSessionKHDto
                {
                    SessionId = s.SessionId,
                    Title = title,
                    LastActive = s.LastActive,
                    IdThongBao = s.IdThongBao
                });
            }
            return Ok(sessions);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string sessionId)
        {
            var idKhachHang = GetCurrentUserId();

            bool isOwner = await _context.ChatLichSus.AnyAsync(c => c.IdKhachHang == idKhachHang && c.GuestSessionId == sessionId);
            if (!isOwner) return Ok(new List<ChatMessageDto>());

            var rawChats = await _context.ChatLichSus
                .AsNoTracking()
                .Where(c => c.GuestSessionId == sessionId)
                .OrderByDescending(c => c.ThoiGian)
                .Take(50)
                .ToListAsync();

            rawChats.Reverse();
            var chatHistory = new List<ChatMessageDto>();

            foreach (var c in rawChats)
            {
                if (c.LoaiChat == "Web_HTTP")
                {
                    if (!string.IsNullOrEmpty(c.NoiDungHoi) && c.LoaiChat != "Web_SignalR")
                        chatHistory.Add(new ChatMessageDto { IdChat = c.IdChat, NoiDung = c.NoiDungHoi, ThoiGian = c.ThoiGian.AddMilliseconds(-10), LoaiTinNhan = "KhachHang" });

                    if (!string.IsNullOrEmpty(c.NoiDungTraLoi))
                        chatHistory.Add(new ChatMessageDto { IdChat = c.IdChat, NoiDung = c.NoiDungTraLoi, ThoiGian = c.ThoiGian, LoaiTinNhan = "AI", IdThongBaoHoTro = c.IdThongBaoHoTro });
                }
                else
                {
                    chatHistory.Add(new ChatMessageDto { IdChat = c.IdChat, NoiDung = c.NoiDungTraLoi, ThoiGian = c.ThoiGian, LoaiTinNhan = c.LoaiTinNhan ?? "KhachHang", IdThongBaoHoTro = c.IdThongBaoHoTro });
                }
            }
            return Ok(chatHistory);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatKHRequestDto request)
        {
            var idKhachHang = GetCurrentUserId();
            var responseDto = new SendChatKHResponseDto();

            var existingTicket = await _context.ThongBaoHoTros
                .FirstOrDefaultAsync(t => t.IdKhachHang == idKhachHang
                                     && t.GuestSessionId == request.SessionId
                                     && t.TrangThai != "Đã xử lý");

            if (existingTicket != null)
            {
                var msgRealtime = new ChatLichSu
                {
                    IdKhachHang = idKhachHang,
                    GuestSessionId = request.SessionId,
                    NoiDungHoi = "Chat Realtime", 
                    NoiDungTraLoi = request.NoiDung,
                    ThoiGian = DateTime.Now,
                    LoaiChat = "Web_SignalR",
                    LoaiTinNhan = "KhachHang",
                    IdThongBaoHoTro = existingTicket.IdThongBao
                };
                _context.ChatLichSus.Add(msgRealtime);
                await _context.SaveChangesAsync();

                await _chatHubContext.Clients.All.SendAsync("ReloadTicketList");

                responseDto.TinNhanCuaKhach = new ChatMessageDto
                {
                    IdChat = msgRealtime.IdChat,
                    NoiDung = request.NoiDung,
                    ThoiGian = msgRealtime.ThoiGian,
                    LoaiTinNhan = "KhachHang"
                };
                responseDto.DaChuyenNhanVien = true;
                responseDto.IdThongBaoHoTro = existingTicket.IdThongBao;

                return Ok(responseDto);
            }

            var history = await _context.ChatLichSus
                .AsNoTracking()
                .Where(c => c.IdKhachHang == idKhachHang && c.GuestSessionId == request.SessionId)
                .OrderByDescending(c => c.ThoiGian).Take(15).OrderBy(c => c.ThoiGian).ToListAsync();

            var chatHistory = new List<object>();
            foreach (var msg in history)
            {
                string role = (msg.LoaiTinNhan == "KhachHang") ? "user" : "model";
                string content = (msg.LoaiChat == "Web_HTTP" && role == "user") ? msg.NoiDungHoi : msg.NoiDungTraLoi;
                if (!string.IsNullOrEmpty(content))
                    chatHistory.Add(new { role = role, parts = new[] { new { text = content } } });
            }

            string? aiResponse = await _aiService.GetAnswerAsync(request.NoiDung, idKhachHang, chatHistory);
            string phanHoiCuoiCung = string.Empty;
            ThongBaoHoTro? thongBaoHoTro = null;

            if (aiResponse == null || aiResponse.Contains("[NEEDS_SUPPORT]"))
            {
                var (isOpen, offHoursMsg) = await CheckBusinessHoursAsync();
                if (!isOpen)
                {
                    phanHoiCuoiCung = offHoursMsg;
                    responseDto.DaChuyenNhanVien = false;
                }
                else
                {
                    phanHoiCuoiCung = aiResponse == null ? "AI đang bận. Đang kết nối nhân viên..." : "Đang kết nối nhân viên hỗ trợ...";
                    responseDto.DaChuyenNhanVien = true;
                }

                thongBaoHoTro = new ThongBaoHoTro
                {
                    IdKhachHang = idKhachHang,
                    GuestSessionId = request.SessionId,
                    NoiDungYeuCau = request.NoiDung,
                    ThoiGianTao = DateTime.Now,
                    TrangThai = "Chờ xử lý"
                };
                _context.ThongBaoHoTros.Add(thongBaoHoTro);
                await _context.SaveChangesAsync();
                responseDto.IdThongBaoHoTro = thongBaoHoTro.IdThongBao;
                await _chatHubContext.Clients.All.SendAsync("ReloadTicketList");
            }
            else
            {
                phanHoiCuoiCung = aiResponse;
            }

            var lichSuGop = new ChatLichSu
            {
                IdKhachHang = idKhachHang,
                GuestSessionId = request.SessionId,
                NoiDungHoi = request.NoiDung,
                NoiDungTraLoi = phanHoiCuoiCung,
                ThoiGian = DateTime.Now,
                LoaiChat = "Web_HTTP",
                LoaiTinNhan = "AI",
                IdThongBaoHoTro = thongBaoHoTro?.IdThongBao
            };
            _context.ChatLichSus.Add(lichSuGop);
            await _context.SaveChangesAsync();

            responseDto.TinNhanCuaKhach = new ChatMessageDto { IdChat = lichSuGop.IdChat, NoiDung = request.NoiDung, ThoiGian = lichSuGop.ThoiGian, LoaiTinNhan = "KhachHang" };
            responseDto.TinNhanPhanHoi = new ChatMessageDto { IdChat = lichSuGop.IdChat, NoiDung = phanHoiCuoiCung, ThoiGian = lichSuGop.ThoiGian, LoaiTinNhan = "AI", IdThongBaoHoTro = lichSuGop.IdThongBaoHoTro };

            return Ok(responseDto);
        }
    }
}