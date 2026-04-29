using CafebookApi.Data;
using CafebookApi.Hubs;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien; // BẮT BUỘC TRỎ ĐÚNG THƯ MỤC NÀY
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/hotro")]
    [ApiController]
    [Authorize]
    public class HoTroKhachHangController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public HoTroKhachHangController(CafebookDbContext context, IHubContext<ChatHub> chatHubContext)
        {
            _context = context;
            _chatHubContext = chatHubContext;
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets([FromQuery] int skip = 0, [FromQuery] int take = 5, [FromQuery] bool onlyPending = true, [FromQuery] string? search = null)
        {
            var query = _context.ThongBaoHoTros.Include(t => t.KhachHang).AsQueryable();
            if (onlyPending)
            {
                query = query.Where(t => t.TrangThai == "Chờ xử lý" || t.TrangThai == "Đã trả lời");
            }
            else
            {
                query = query.Where(t => t.TrangThai == "Đã xử lý");
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    (t.NoiDungYeuCau != null && t.NoiDungYeuCau.Contains(search)) ||
                    (t.KhachHang != null && t.KhachHang.HoTen != null && t.KhachHang.HoTen.Contains(search)));
            }

            var rawTickets = await query
                .OrderByDescending(t => t.ThoiGianTao) 
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var tickets = rawTickets.Select(t => new HoTroKhachHangListDto
            {
                IdThongBao = t.IdThongBao,
                TenKhachHang = (t.KhachHang != null && !string.IsNullOrEmpty(t.KhachHang.HoTen))
                               ? t.KhachHang.HoTen
                               : $"Khách vãng lai ({t.GuestSessionId})",
                NoiDungYeuCau = t.NoiDungYeuCau,
                ThoiGianTao = t.ThoiGianTao,
                TrangThai = t.TrangThai,
                GhiChuTuAI = t.GhiChu
            }).ToList();

            return Ok(tickets);
        }

        [HttpGet("ticket/{id}")]
        public async Task<IActionResult> GetTicketDetail(int id)
        {
            var ticket = await _context.ThongBaoHoTros
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.IdThongBao == id);

            if (ticket == null) return NotFound("Không tìm thấy phiếu hỗ trợ.");

            var rawChats = await _context.ChatLichSus
                .Where(c => c.IdThongBaoHoTro == id)
                .OrderBy(c => c.ThoiGian)
                .ToListAsync();

            var chatHistory = new List<ChatMessageNVDto>();

            foreach (var c in rawChats)
            {
                if (c.LoaiChat == "Web_HTTP")
                {
                    if (!string.IsNullOrEmpty(c.NoiDungHoi) && c.NoiDungHoi != "Chat Realtime")
                    {
                        chatHistory.Add(new ChatMessageNVDto { IdChat = c.IdChat, NoiDung = c.NoiDungHoi, ThoiGian = c.ThoiGian.AddMilliseconds(-10), LoaiTinNhan = "KhachHang" });
                    }

                    if (!string.IsNullOrEmpty(c.NoiDungTraLoi))
                    {
                        chatHistory.Add(new ChatMessageNVDto { IdChat = c.IdChat, NoiDung = c.NoiDungTraLoi, ThoiGian = c.ThoiGian, LoaiTinNhan = "AI" });
                    }
                }
                else
                {
                    chatHistory.Add(new ChatMessageNVDto { IdChat = c.IdChat, NoiDung = c.NoiDungTraLoi, ThoiGian = c.ThoiGian, LoaiTinNhan = c.LoaiTinNhan ?? "KhachHang" });
                }
            }

            var dto = new HoTroKhachHangDetailDto
            {
                IdThongBao = ticket.IdThongBao,
                IdKhachHang = ticket.IdKhachHang,
                GuestSessionId = ticket.GuestSessionId,
                TenKhachHang = ticket.KhachHang != null ? ticket.KhachHang.HoTen : $"Khách vãng lai ({ticket.GuestSessionId})",
                NoiDungYeuCau = ticket.NoiDungYeuCau,
                ThoiGianTao = ticket.ThoiGianTao,
                TrangThai = ticket.TrangThai,
                GhiChuTuAI = ticket.GhiChu,
                LichSuChat = chatHistory.OrderBy(c => c.ThoiGian).ToList()
            };

            return Ok(dto);
        }

        [HttpPost("resolve/{id}")]
        public async Task<IActionResult> ResolveTicket(int id)
        {
            var ticket = await _context.ThongBaoHoTros.FindAsync(id);
            if (ticket == null) return NotFound("Không tìm thấy phiếu hỗ trợ.");

            ticket.TrangThai = "Đã xử lý";
            await _context.SaveChangesAsync();
            await _chatHubContext.Clients.All.SendAsync("ReloadTicketList");

            return Ok();
        }
    }
}