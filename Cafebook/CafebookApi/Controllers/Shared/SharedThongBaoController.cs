using CafebookApi.Data;
using CafebookModel.Model.Shared;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.Shared
{
    [Route("api/shared/thongbao")]
    [ApiController]
    [Authorize]
    public class SharedThongBaoController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public SharedThongBaoController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int userId, [FromQuery] string? userRoles, [FromQuery] string? roleName)
        {
            var roles = string.IsNullOrEmpty(userRoles) ? new List<string>() : userRoles.Split(',').ToList();

            // 1. KIỂM TRA QUYỀN TOÀN QUYỀN
            bool showAll = roles.Contains("FULL_QL") || roles.Contains("FULL_NV");

            var query = _context.Set<ThongBao>().Include(t => t.NhanVienTao).AsNoTracking().AsQueryable();

            if (!showAll)
            {
                var allowedTypes = new List<string> { "ThongBaoToanNhanVien" };

                if (roleName == "Nhân viên") allowedTypes.Add("ThongBaoNhanVien");
                if (roleName == "Quản lý") allowedTypes.Add("ThongBaoQuanLy");

                if (roles.Contains("QL_BAN") || roles.Contains("NV_DAT_BAN") || roles.Contains("QL_SU_CO_BAN"))
                    allowedTypes.AddRange(new[] { "DatBan", "SuCoBan" });

                if (roles.Contains("NV_CHE_BIEN") || roles.Contains("NV_GOI_MON"))
                    allowedTypes.Add("PhieuGoiMon");

                if (roles.Contains("QL_DON_HANG") || roles.Contains("NV_GIAO_HANG"))
                    allowedTypes.Add("DonHangMoi");

                if (roles.Contains("QL_TON_KHO"))
                    allowedTypes.AddRange(new[] { "HetHang", "CanhBaoKho", "Kho" });

                if (roles.Contains("QL_DON_XIN_NGHI"))
                    allowedTypes.Add("DonXinNghi");

                if (roles.Contains("QL_LICH_LAM_VIEC"))
                    allowedTypes.Add("DangKyLichMoi");

                query = query.Where(t => t.LoaiThongBao != null && allowedTypes.Contains(t.LoaiThongBao));
            }

            var notifications = await query
                // Ghim các thông báo hệ thống lên đầu
                .OrderByDescending(t => t.LoaiThongBao == "ThongBaoNhanVien" || t.LoaiThongBao == "ThongBaoToanNhanVien" || t.LoaiThongBao == "ThongBaoQuanLy")
                // Sau đó mới sắp xếp theo thời gian mới nhất
                .ThenByDescending(t => t.ThoiGianTao)
                .Take(20)
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