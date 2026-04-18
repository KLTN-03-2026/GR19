using CafebookApi.Data;
using CafebookModel.Model.Shared;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CafebookApi.Controllers.Shared
{
    [Route("api/shared/thongbao")]
    [ApiController]
    public class SharedThongBaoController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public SharedThongBaoController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int userId, [FromQuery] string userRoles)
        {
            // userRoles là chuỗi phân cách dấu phẩy, ví dụ: "FULL_QL,QL_THONG_BAO" hoặc "NV_PHUC_VU"
            var roles = string.IsNullOrEmpty(userRoles) ? new List<string>() : userRoles.Split(',').ToList();
            bool isManager = roles.Contains("FULL_QL") || roles.Contains("QL_THONG_BAO");

            // Phân loại thông báo được phép xem
            var allowedTypes = new List<string> { "HeThong", "ThongBaoToanNhanVien" };

            if (isManager)
            {
                allowedTypes.AddRange(new[] { "SuCoBan", "HetHang", "DonXinNghi", "Kho", "DatBan", "CanhBaoKho", "PhanHoiKhachHang", "ThongBaoQuanLy", "ThongBaoNhanVien", "DonHangMoi" });
            }
            else
            {
                // Giả lập quyền nhân viên thường (Phục vụ/Pha chế)
                allowedTypes.AddRange(new[] { "PhieuGoiMon", "DatBan", "DonHangMoi", "ThongBaoNhanVien" });
            }

            var query = _context.Set<ThongBao>().Include(t => t.NhanVienTao).AsNoTracking().AsQueryable();

            // Chỉ lấy thông báo thuộc loại được phép xem
            query = query.Where(t => t.LoaiThongBao != null && allowedTypes.Contains(t.LoaiThongBao));

            var notifications = await query
                .OrderByDescending(t => t.ThoiGianTao)
                .Take(20) // Lấy 20 cái mới nhất cho Popup
                .Select(t => new SharedThongBaoItemDto
                {
                    IdThongBao = t.IdThongBao,
                    NoiDung = t.NoiDung,
                    ThoiGianTao = t.ThoiGianTao,
                    LoaiThongBao = t.LoaiThongBao ?? "Khác",
                    IdLienQuan = t.IdLienQuan,
                    DaXem = t.DaXem,
                    TenNhanVienTao = t.NhanVienTao != null ? t.NhanVienTao.HoTen : "Hệ thống"
                }).ToListAsync();

            int unreadCount = await query.CountAsync(t => !t.DaXem);

            return Ok(new SharedThongBaoResponseDto
            {
                UnreadCount = unreadCount,
                Notifications = notifications
            });
        }

        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var tb = await _context.Set<ThongBao>().FindAsync(id);
            if (tb != null)
            {
                tb.DaXem = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}