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
            var roles = string.IsNullOrEmpty(userRoles) ? new HashSet<string>() : userRoles.Split(',').ToHashSet();
            var query = _context.Set<ThongBao>().Include(t => t.NhanVienTao).AsNoTracking().AsQueryable();

            bool isFullAdmin = roles.Contains("FULL_ADMIN");

            if (!isFullAdmin)
            {
                var allowedTypes = new HashSet<string> { "ThongBaoToanNhanVien" };

                // =======================================================
                // 1. NHÓM THÔNG BÁO CHUNG (Theo Role chức danh)
                // =======================================================
                if (roleName == "Nhân viên" || roles.Contains("FULL_NV"))
                    allowedTypes.Add("ThongBaoNhanVien");

                if (roleName == "Quản lý" || roles.Contains("FULL_QL"))
                    allowedTypes.Add("ThongBaoQuanLy");

                // =======================================================
                // 2. NHÓM VẬN HÀNH POS (App Nhân Viên)
                // =======================================================
                if (roles.Contains("FULL_NV") || roles.Contains("NV_DAT_BAN"))
                    allowedTypes.UnionWith(new[] { "DatBan", "HuyDatBan" });

                // Bếp/Pha chế mới cần nhận thông báo Phiếu gọi món để làm
                if (roles.Contains("FULL_NV") || roles.Contains("NV_CHE_BIEN"))
                    allowedTypes.Add("PhieuGoiMon");

                if (roles.Contains("FULL_NV") || roles.Contains("NV_GIAO_HANG"))
                    allowedTypes.Add("DonHangMoi");

                // =======================================================
                // 3. NHÓM CHỨC NĂNG WEB (Workspace Web Nhân Viên)
                // =======================================================
                if (roles.Contains("FULL_NV") || roles.Contains("NV_HO_TRO_KH"))
                    allowedTypes.Add("HoTroKhachHang");

                if (roles.Contains("FULL_NV") || roles.Contains("NV_PHAN_HOI"))
                    allowedTypes.Add("GopY");

                if (roles.Contains("FULL_NV") || roles.Contains("NV_SHIP_HANG"))
                    allowedTypes.Add("DonHangMoi"); // Shipper trên web cũng nhận đơn hàng mới

                // =======================================================
                // 4. NHÓM NGHIỆP VỤ QUẢN LÝ (App Quản Lý)
                // =======================================================
                // Quản lý Cơ sở vật chất - Kho
                if (roles.Contains("FULL_QL") || roles.Contains("QL_SU_CO_BAN") || roles.Contains("QL_BAN"))
                    allowedTypes.Add("SuCoBan");

                if (roles.Contains("FULL_QL") || roles.Contains("QL_TON_KHO") || roles.Contains("QL_NGUYEN_LIEU"))
                    allowedTypes.UnionWith(new[] { "HetHang", "CanhBaoKho", "Kho" });

                // Quản lý Nhân sự
                if (roles.Contains("FULL_QL") || roles.Contains("QL_DON_XIN_NGHI") || roles.Contains("QL_NHAN_VIEN"))
                    allowedTypes.Add("DonXinNghi");

                if (roles.Contains("FULL_QL") || roles.Contains("QL_LICH_LAM_VIEC") || roles.Contains("QL_NHAN_VIEN"))
                    allowedTypes.Add("DangKyLichMoi");

                // Quản lý Giao dịch & Khách hàng
                if (roles.Contains("FULL_QL") || roles.Contains("QL_DON_HANG"))
                    allowedTypes.Add("DonHangMoi");

                if (roles.Contains("FULL_QL") || roles.Contains("QL_KHACH_HANG"))
                    allowedTypes.Add("GopY");

                // Áp dụng bộ lọc List LoaiThongBao vào DB
                query = query.Where(t => t.LoaiThongBao != null && allowedTypes.Contains(t.LoaiThongBao));
            }

            // =======================================================
            // 5. TRUY VẤN VÀ SORT TỐI ƯU
            // =======================================================
            var notifications = await query
                .OrderByDescending(t => (t.LoaiThongBao == "ThongBaoNhanVien" || t.LoaiThongBao == "ThongBaoToanNhanVien" || t.LoaiThongBao == "ThongBaoQuanLy") ? 1 : 0)
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