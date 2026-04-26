using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly CafebookDbContext _context;

        public ChatHub(CafebookDbContext context)
        {
            _context = context;
        }

        public async Task JoinGroup(string groupName)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
        }

        public async Task SendMessageFromClient(string groupName, string noiDung, int? idKhachHang, string? guestSessionId, int? idThongBaoHoTro)
        {
            try
            {
                var msgKhach = await SaveChatHistoryAsync(idKhachHang, guestSessionId, null, noiDung, "KhachHang", idThongBaoHoTro);
                await Clients.Group(groupName).SendAsync("ReceiveMessage", new
                {
                    idChat = msgKhach.IdChat,
                    noiDung = msgKhach.NoiDungTraLoi,
                    thoiGian = msgKhach.ThoiGian,
                    loaiTinNhan = msgKhach.LoaiTinNhan ?? "KhachHang",
                    idThongBaoHoTro = msgKhach.IdThongBaoHoTro
                });
            }
            catch (Exception ex)
            {
                throw new HubException($"Lỗi khi gửi tin nhắn: {ex.Message}");
            }
        }

        [Authorize]
        public async Task SendMessageFromStaff(string groupName, string noiDung, int idThongBao, int? idKhachHang, string? guestSessionId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? Context.User?.FindFirst("IdNhanVien")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idNhanVien))
            {
                throw new HubException("Không thể xác định danh tính nhân viên. Vui lòng đăng nhập lại.");
            }

            try
            {
                var msgNV = await SaveChatHistoryAsync(idKhachHang, guestSessionId, idNhanVien, noiDung, "NhanVien", idThongBao);
                var ticket = await _context.ThongBaoHoTros.FindAsync(idThongBao);
                if (ticket != null && ticket.TrangThai != "Đã xử lý")
                {
                    ticket.TrangThai = "Đã trả lời";
                    ticket.IdNhanVien = idNhanVien; 
                    ticket.ThoiGianPhanHoi = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                await Clients.Group(groupName).SendAsync("ReceiveMessage", new
                {
                    idChat = msgNV.IdChat,
                    noiDung = msgNV.NoiDungTraLoi,
                    thoiGian = msgNV.ThoiGian,
                    loaiTinNhan = msgNV.LoaiTinNhan ?? "NhanVien",
                    idThongBaoHoTro = msgNV.IdThongBaoHoTro
                });

                await Clients.All.SendAsync("ReloadTicketList");
            }
            catch (Exception ex)
            {
                throw new HubException($"Lỗi hệ thống khi phản hồi: {ex.Message}");
            }
        }

        private async Task<ChatLichSu> SaveChatHistoryAsync(int? idKhachHang, string? guestSessionId, int? idNhanVien, string traLoi, string loaiTinNhan, int? idThongBao)
        {
            var lichSu = new ChatLichSu
            {
                IdKhachHang = (idKhachHang > 0) ? idKhachHang : null,
                GuestSessionId = guestSessionId,
                IdNhanVien = idNhanVien,
                NoiDungHoi = "Chat Realtime",
                NoiDungTraLoi = traLoi,
                ThoiGian = DateTime.Now,
                LoaiChat = "Web_SignalR",
                LoaiTinNhan = loaiTinNhan,
                IdThongBaoHoTro = idThongBao
            };

            _context.ChatLichSus.Add(lichSu);
            await _context.SaveChangesAsync();
            return lichSu;
        }
    }
}