using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OtpNet;

namespace CafebookApi.Controllers.Shared
{
    [Route("api/shared/[controller]")]
    [ApiController]
    [AllowAnonymous]
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
            if (request.Username == "ADMIN")
            {
                string? secretKey = _configuration.GetValue<string>("AdminSettings:SecretKey");

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    return StatusCode(500, new { message = "Hệ thống chưa thiết lập khóa bảo mật cho ADMIN. Vui lòng kiểm tra file cấu hình Server." });
                }

                try
                {
                    var base32Bytes = Base32Encoding.ToBytes(secretKey);
                    var totp = new Totp(base32Bytes);

                    bool isValid = totp.VerifyTotp(request.Password, out long timeStepMatched);

                    if (isValid)
                    {
                        var adminNv = new CafebookModel.Model.ModelEntities.NhanVien
                        {
                            IdNhanVien = 0,
                            TenDangNhap = "ADMIN",
                            HoTen = "System Administrator",
                            VaiTro = new CafebookModel.Model.ModelEntities.VaiTro { TenVaiTro = "Quản trị hệ thống" }
                        };

                        var adminToken = GenerateJwtToken(adminNv);

                        return Ok(new LoginResponse
                        {
                            Token = adminToken,
                            IdNhanVien = 0,
                            HoTen = "System Administrator",
                            TenVaiTro = "Quản trị hệ thống",
                            AnhDaiDien = null,
                            Quyen = new List<string> { "FULL_ADMIN" }
                        });
                    }
                    else
                    {
                        return Unauthorized(new { message = "Mã Authenticator không hợp lệ cho tài khoản ADMIN." });
                    }
                }
                catch (Exception)
                {
                    return StatusCode(500, new { message = "Cấu hình SecretKey của ADMIN bị sai định dạng Base32." });
                }
            }

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
                return StatusCode(403, new { message = "Tài khoản của bạn đã bị Quản lý đánh dấu là đã nghỉ việc, Vui lòng liên hệ Quản lý để được mở khóa." });

            var quyens = await _context.NhanVienQuyens
                .Where(nq => nq.IdNhanVien == nv.IdNhanVien)
                .Select(nq => nq.IdQuyen)
                .ToListAsync();

            if (quyens == null || quyens.Count == 0)
            {
                return StatusCode(403, new { message = "Tài khoản của bạn không được cấp một quyền nào không thể đăng nhập vui lòng liên hệ quản lý để được hỗ trợ" });
            }

            var token = GenerateJwtToken(nv);

            var log = new CafebookModel.Model.ModelEntities.NhatKyHeThong
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
            string roleCode = "NhanVien"; 
            if (nv.VaiTro.TenVaiTro.ToLower().Contains("quản lý") || nv.VaiTro.TenVaiTro.ToLower().Contains("quản trị"))
            {
                roleCode = "QuanLy";
            }

            var claims = new List<Claim>
            {
                new Claim("IdNhanVien", nv.IdNhanVien.ToString()),
                new Claim(ClaimTypes.NameIdentifier, nv.IdNhanVien.ToString()),
                new Claim(ClaimTypes.Name, nv.HoTen),
                
                new Claim(ClaimTypes.Role, roleCode),

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