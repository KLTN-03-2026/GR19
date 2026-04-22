using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/[controller]")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class ThongTinCaNhanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ThongTinCaNhanController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim?.Value, out int id) ? id : 0;
        }

        private string GetFullImageUrl(string? relativePath)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            if (string.IsNullOrEmpty(relativePath))
            {
                return $"{baseUrl}{HinhAnhPaths.WebDefaultAvatar}";
            }
            return $"{baseUrl}{relativePath.Replace('\\', '/')}";
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            if (id != GetCurrentUserId()) return Forbid();

            var kh = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(x => x.IdKhachHang == id);
            if (kh == null) return NotFound(new { Message = "Không tìm thấy hồ sơ." });

            var dto = new ThongTinCaNhanDto
            {
                IdKhachHang = kh.IdKhachHang,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi,
                TenDangNhap = kh.TenDangNhap,
                AnhDaiDienUrl = GetFullImageUrl(kh.AnhDaiDien)
            };
            return Ok(dto);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromForm] ThongTinCaNhanUpdateDto model, IFormFile? avatarFile)
        {
            if (id != GetCurrentUserId()) return Forbid();

            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound();

            if (avatarFile != null)
            {
                var subFolder = HinhAnhPaths.UrlAvatarKH.TrimStart('/');
                var uploadPath = Path.Combine(_env.WebRootPath, subFolder);
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string slugName = model.HoTen.GenerateSlug();
                string extension = Path.GetExtension(avatarFile.FileName);
                // Định dạng: 1_lam-chu-bao-toan.jpg
                var uniqueName = $"{id}_{slugName}{extension}";
                var filePath = Path.Combine(uploadPath, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                kh.AnhDaiDien = $"{HinhAnhPaths.UrlAvatarKH}/{uniqueName}";
            }

            kh.HoTen = model.HoTen;
            kh.SoDienThoai = model.SoDienThoai;
            kh.Email = model.Email;
            kh.DiaChi = model.DiaChi;
            kh.TenDangNhap = model.TenDangNhap;

            await _context.SaveChangesAsync();

            string newUrlBypassCache = GetFullImageUrl(kh.AnhDaiDien) + "?v=" + DateTime.Now.Ticks;
            return Ok(new { newAvatarUrl = newUrlBypassCache, Message = "Cập nhật thành công!" });
        }
    }
}