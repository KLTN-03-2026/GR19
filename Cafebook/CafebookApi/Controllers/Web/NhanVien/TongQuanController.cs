using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien;
using CafebookModel.Model.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class TongQuanController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public TongQuanController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idNhanVien))
            {
                return Unauthorized(new { message = "Token không hợp lệ." });
            }

            var nhanVien = await _context.NhanViens
                .Include(nv => nv.VaiTro)
                .FirstOrDefaultAsync(nv => nv.IdNhanVien == idNhanVien);

            if (nhanVien == null) return NotFound(new { message = "Không tìm thấy nhân viên." });

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var avatarUrl = string.IsNullOrEmpty(nhanVien.AnhDaiDien)
                ? null
                : (nhanVien.AnhDaiDien.StartsWith("http") ? nhanVien.AnhDaiDien : $"{baseUrl}{nhanVien.AnhDaiDien}");

            var userRoles = await _context.NhanVienQuyens
                .Where(nq => nq.IdNhanVien == idNhanVien)
                .Select(nq => nq.IdQuyen)
                .ToListAsync();

            var roleName = nhanVien.VaiTro.TenVaiTro;
            bool showAll = userRoles.Contains("FULL_QL") || userRoles.Contains("FULL_NV");

            var query = _context.Set<ThongBao>().Include(t => t.NhanVienTao).AsNoTracking().AsQueryable();

            if (!showAll)
            {
                var allowedTypes = new List<string> { "ThongBaoToanNhanVien" };

                if (roleName == "Nhân viên") allowedTypes.Add("ThongBaoNhanVien");
                if (roleName == "Quản lý") allowedTypes.Add("ThongBaoQuanLy");

                if (userRoles.Contains("QL_BAN") || userRoles.Contains("NV_DAT_BAN") || userRoles.Contains("QL_SU_CO_BAN"))
                    allowedTypes.AddRange(new[] { "DatBan", "SuCoBan" });

                if (userRoles.Contains("NV_CHE_BIEN") || userRoles.Contains("NV_GOI_MON"))
                    allowedTypes.Add("PhieuGoiMon");

                if (userRoles.Contains("QL_DON_HANG") || userRoles.Contains("NV_GIAO_HANG"))
                    allowedTypes.Add("DonHangMoi");

                if (userRoles.Contains("QL_TON_KHO"))
                    allowedTypes.AddRange(new[] { "HetHang", "CanhBaoKho", "Kho" });

                if (userRoles.Contains("QL_DON_XIN_NGHI"))
                    allowedTypes.Add("DonXinNghi");

                if (userRoles.Contains("QL_LICH_LAM_VIEC"))
                    allowedTypes.Add("DangKyLichMoi");

                query = query.Where(t => t.LoaiThongBao != null && allowedTypes.Contains(t.LoaiThongBao));
            }

            var thongBaos = await query
                .OrderByDescending(t => t.LoaiThongBao == "ThongBaoNhanVien" || t.LoaiThongBao == "ThongBaoToanNhanVien" || t.LoaiThongBao == "ThongBaoQuanLy")
                .ThenByDescending(t => t.ThoiGianTao)
                .Take(10) // Màn Dashboard chỉ lấy 10 cái
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

            // ==========================================
            // TRẢ VỀ DỮ LIỆU TỔNG HỢP
            // ==========================================
            var result = new TongQuanDto
            {
                ThongTin = new ThongTinNhanVienDto
                {
                    IdNhanVien = nhanVien.IdNhanVien,
                    HoTen = nhanVien.HoTen,
                    TenVaiTro = nhanVien.VaiTro.TenVaiTro,
                    AnhDaiDien = avatarUrl,
                    Email = nhanVien.Email,
                    SoDienThoai = nhanVien.SoDienThoai,
                    TrangThaiLamViec = nhanVien.TrangThaiLamViec
                },
                DanhSachThongBao = thongBaos,
                SoThongBaoChuaDoc = unreadCount
            };

            return Ok(result);
        }
    }
}