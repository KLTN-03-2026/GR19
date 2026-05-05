using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien;
using CafebookModel.Model.Shared;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IWebHostEnvironment _env;

        public TongQuanController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; // Inject IWebHostEnvironment để lưu file ảnh
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
                .Take(10)
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
                    DiaChi = nhanVien.DiaChi,
                    TrangThaiLamViec = nhanVien.TrangThaiLamViec
                },
                DanhSachThongBao = thongBaos,
                SoThongBaoChuaDoc = unreadCount
            };

            return Ok(result);
        }

        [HttpPut("update-info")]
        public async Task<IActionResult> UpdateInfo([FromBody] CapNhatThongTinWebDto req)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idNhanVien))
                return Unauthorized("Token không hợp lệ.");

            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");

            nhanVien.HoTen = req.HoTen;
            nhanVien.SoDienThoai = req.SoDienThoai;
            nhanVien.Email = req.Email;
            nhanVien.DiaChi = req.DiaChi;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin thành công!" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] DoiMatKhauWebDto req)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idNhanVien))
                return Unauthorized("Token không hợp lệ.");

            if (req.MatKhauCu == req.MatKhauMoi)
                return BadRequest("Mật khẩu mới không được trùng với mật khẩu cũ.");

            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");

            if (nhanVien.MatKhau != req.MatKhauCu)
                return BadRequest("Mật khẩu cũ không chính xác.");

            nhanVien.MatKhau = req.MatKhauMoi;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(Microsoft.AspNetCore.Http.IFormFile avatarFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idNhanVien))
                return Unauthorized("Token không hợp lệ.");

            if (avatarFile == null || avatarFile.Length == 0) return BadRequest("Chưa chọn file.");

            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");

            // Xóa ảnh cũ
            if (!string.IsNullOrEmpty(nhanVien.AnhDaiDien))
            {
                try
                {
                    string oldPhysicalPath = Path.Combine(_env.WebRootPath, nhanVien.AnhDaiDien.TrimStart('/'));
                    oldPhysicalPath = oldPhysicalPath.Replace('/', Path.DirectorySeparatorChar);
                    if (System.IO.File.Exists(oldPhysicalPath)) System.IO.File.Delete(oldPhysicalPath);
                }
                catch { /* Ignore error */ }
            }

            string folderUrl = HinhAnhPaths.UrlAvatarNV ?? "/images/avatars";
            string physicalFolder = Path.Combine(_env.WebRootPath, folderUrl.TrimStart('/'));

            if (!Directory.Exists(physicalFolder)) Directory.CreateDirectory(physicalFolder);

            string ext = Path.GetExtension(avatarFile.FileName);
            string slugName = nhanVien.HoTen.GenerateSlug() ?? "user";
            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{slugName}{ext}";

            var physicalPath = Path.Combine(physicalFolder, fileName);
            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            nhanVien.AnhDaiDien = $"{folderUrl}/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật ảnh đại diện thành công!" });
        }
    }
}