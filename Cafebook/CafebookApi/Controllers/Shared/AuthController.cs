using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CafebookApi.Controllers.Shared
{
    [Route("api/shared/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(CafebookDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login-nhan-vien")]
        public async Task<IActionResult> LoginNhanVien([FromBody] LoginRequest request)
        {
            // TÌM NHÂN VIÊN: Có thể nhập Tên đăng nhập, Số điện thoại hoặc Email
            var nv = await _context.NhanViens
                .Include(n => n.VaiTro)
                .FirstOrDefaultAsync(x =>
                    (x.TenDangNhap == request.Username ||
                     x.SoDienThoai == request.Username ||
                     x.Email == request.Username)
                    && x.MatKhau == request.Password);

            if (nv == null)
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác" });

            if (nv.TrangThaiLamViec == "Nghỉ việc" || nv.TrangThaiLamViec == "Đã nghỉ")
                return StatusCode(403, new { message = "Tài khoản của bạn đã bị khóa hoặc đã nghỉ việc." });

            // LẤY QUYỀN MỚI: Lấy trực tiếp từ bảng NhanVienQuyens thông qua IdNhanVien
            var quyens = await _context.NhanVienQuyens
                .Where(nq => nq.IdNhanVien == nv.IdNhanVien)
                .Select(nq => nq.IdQuyen)
                .ToListAsync();

            var token = GenerateJwtToken(nv);

            var log = new NhatKyHeThong
            {
                HanhDong = "Đăng nhập",
                BangBiAnhHuong = "Auth",
                ThoiGian = DateTime.Now,
                IdNhanVien = nv.IdNhanVien,
                DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.NhatKyHeThongs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                Token = token,
                IdNhanVien = nv.IdNhanVien,
                HoTen = nv.HoTen,
                TenVaiTro = nv.VaiTro.TenVaiTro,
                AnhDaiDien = nv.AnhDaiDien,
                Quyen = quyens
            });
        }

        private string GenerateJwtToken(CafebookModel.Model.ModelEntities.NhanVien nv)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nv.IdNhanVien.ToString()),
                new Claim(ClaimTypes.Name, nv.HoTen),
                new Claim(ClaimTypes.Role, nv.VaiTro.TenVaiTro),
                new Claim("Username", nv.TenDangNhap)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}