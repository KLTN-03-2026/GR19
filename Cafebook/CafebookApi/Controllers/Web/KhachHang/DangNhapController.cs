using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EntityKhachHang = CafebookModel.Model.ModelEntities.KhachHang;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/dangnhap")]
    [ApiController]
    public class DangNhapController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public DangNhapController(CafebookDbContext context, IConfiguration config, IMemoryCache cache)
        {
            _context = context;
            _config = config;
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] DangNhapRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.TenDangNhap) || string.IsNullOrEmpty(request.MatKhau))
                return Ok(new DangNhapResponseDto { Success = false, Message = "Thông tin không hợp lệ." });

            var userInput = request.TenDangNhap.Trim();
            var passInput = request.MatKhau.Trim();

            // Lấy IP của người dùng
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

            // ====================================================================
            // 1. BỘ LỌC CHỐNG SPAM (2 LỚP: THEO IP VÀ THEO TÀI KHOẢN)
            // ====================================================================
            string lockIpKey = $"LOGIN_LOCK_IP_{ipAddress}";
            string lockUserKey = $"LOGIN_LOCK_USER_{userInput}";

            // Lớp 1: Kiểm tra xem IP của thiết bị này có đang bị khóa không?
            if (_cache.TryGetValue(lockIpKey, out DateTime ipLockExpiry))
            {
                return Ok(new DangNhapResponseDto
                {
                    Success = false,
                    Message = $"Thiết bị này đã nhập sai quá nhiều lần. Vui lòng thử lại sau {Math.Ceiling((ipLockExpiry - DateTime.Now).TotalMinutes)} phút."
                });
            }

            // Lớp 2: Kiểm tra xem Tài khoản này có đang bị khóa tạm thời không?
            if (_cache.TryGetValue(lockUserKey, out DateTime userLockExpiry))
            {
                return Ok(new DangNhapResponseDto
                {
                    Success = false,
                    Message = $"Tài khoản này đang bị khóa tạm thời do nhập sai nhiều lần. Thử lại sau {Math.Ceiling((userLockExpiry - DateTime.Now).TotalMinutes)} phút."
                });
            }

            // ====================================================================
            // 2. TÌM KHÁCH HÀNG & XỬ LÝ ĐẾM SỐ LẦN SAI
            // ====================================================================
            EntityKhachHang? khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k =>
                (k.TenDangNhap == userInput || k.SoDienThoai == userInput || k.Email == userInput) &&
                (k.MatKhau == passInput)
            );

            if (khachHang == null)
            {
                // Khách nhập sai -> Tăng bộ đếm cho CẢ IP VÀ TÀI KHOẢN
                string failIpKey = $"LOGIN_FAIL_IP_{ipAddress}";
                string failUserKey = $"LOGIN_FAIL_USER_{userInput}";

                int failIpCount = _cache.TryGetValue(failIpKey, out int curIp) ? curIp : 0;
                int failUserCount = _cache.TryGetValue(failUserKey, out int curUser) ? curUser : 0;

                failIpCount++;
                failUserCount++;

                // Phạt nặng IP nếu sai 7 lần (Chống hacker dùng 1 máy tính quét hàng loạt tài khoản)
                if (failIpCount >= 7)
                {
                    _cache.Set(lockIpKey, DateTime.Now.AddMinutes(20), TimeSpan.FromMinutes(20));
                    _cache.Remove(failIpKey);
                    return Ok(new DangNhapResponseDto { Success = false, Message = "Phát hiện đăng nhập bất thường. Thiết bị bị khóa 20 phút." });
                }

                // Phạt tài khoản nếu sai 5 lần (Chống người dùng cố tình dò mật khẩu của 1 người khác)
                if (failUserCount >= 5)
                {
                    _cache.Set(lockUserKey, DateTime.Now.AddMinutes(10), TimeSpan.FromMinutes(10));
                    _cache.Remove(failUserKey);
                    return Ok(new DangNhapResponseDto { Success = false, Message = "Tài khoản bị tạm khóa 10 phút vì lý do bảo mật (sai mật khẩu 5 lần liên tiếp)." });
                }

                // Lưu lại số lần sai vào Cache trong 30 phút
                _cache.Set(failIpKey, failIpCount, TimeSpan.FromMinutes(30));
                _cache.Set(failUserKey, failUserCount, TimeSpan.FromMinutes(30));

                return Ok(new DangNhapResponseDto { Success = false, Message = $"Sai thông tin đăng nhập hoặc mật khẩu. Tài khoản còn {5 - failUserCount} lần thử." });
            }

            // ====================================================================
            // 3. ĐĂNG NHẬP THÀNH CÔNG -> RESET TOÀN BỘ BỘ ĐẾM
            // ====================================================================
            _cache.Remove($"LOGIN_FAIL_IP_{ipAddress}");
            _cache.Remove($"LOGIN_FAIL_USER_{userInput}");

            // ====================================================================
            // 4. QUÉT TRẠNG THÁI TÀI KHOẢN (Khóa/Xóa/Tạm) 
            // ====================================================================
            if (khachHang.DaXoa)
                return Ok(new DangNhapResponseDto { Success = false, Message = "Tài khoản của bạn đã bị xóa khỏi hệ thống. Vui lòng liên hệ quản trị viên để được hỗ trợ." });

            if (khachHang.BiKhoa)
            {
                if (khachHang.ThoiGianMoKhoa.HasValue && khachHang.ThoiGianMoKhoa.Value <= DateTime.Now)
                {
                    // Auto-Unlock nếu đã hết hạn phạt từ DB
                    khachHang.BiKhoa = false;
                    khachHang.LyDoKhoa = null;
                    khachHang.ThoiGianMoKhoa = null;
                    _context.KhachHangs.Update(khachHang);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    string timeStr = khachHang.ThoiGianMoKhoa.HasValue ? $"đến {khachHang.ThoiGianMoKhoa.Value:dd/MM/yyyy HH:mm}" : "vĩnh viễn";
                    string reasonStr = !string.IsNullOrEmpty(khachHang.LyDoKhoa) ? $" Lý do: {khachHang.LyDoKhoa}." : "";
                    return Ok(new DangNhapResponseDto { Success = false, Message = $"Tài khoản của bạn đang bị khóa {timeStr}.{reasonStr} Vui lòng quay lại sau." });
                }
            }

            if (khachHang.TaiKhoanTam)
                return Ok(new DangNhapResponseDto { Success = false, Message = "Đây là tài khoản lưu trú (chưa kích hoạt). Vui lòng chuyển sang trang Đăng Ký, nhập lại thông tin để nâng cấp tài khoản." });

            // ====================================================================
            // 5. KHỞI TẠO DỮ LIỆU ĐĂNG NHẬP & TRẢ VỀ TOKEN
            // ====================================================================
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            string avatarUrl = string.IsNullOrEmpty(khachHang.AnhDaiDien)
                ? $"{baseUrl}{HinhAnhPaths.WebDefaultAvatar}"
                : (khachHang.AnhDaiDien.Replace("\\", "/").Contains("/")
                    ? (khachHang.AnhDaiDien.StartsWith("/") ? $"{baseUrl}{khachHang.AnhDaiDien}" : $"{baseUrl}/{khachHang.AnhDaiDien}")
                    : $"{baseUrl}{HinhAnhPaths.UrlAvatarKH}/{khachHang.AnhDaiDien}");

            var dto = new DangNhapKhachHangDto
            {
                IdKhachHang = khachHang.IdKhachHang,
                HoTen = khachHang.HoTen,
                Email = khachHang.Email,
                SoDienThoai = khachHang.SoDienThoai,
                TenDangNhap = khachHang.TenDangNhap,
                AnhDaiDienUrl = avatarUrl
            };

            string token = GenerateJwtToken(khachHang);
            return Ok(new DangNhapResponseDto { Success = true, KhachHangData = dto, Token = token });
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private string GenerateJwtToken(EntityKhachHang khachHang)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, khachHang.IdKhachHang.ToString()),
                new Claim(ClaimTypes.Name, khachHang.TenDangNhap ?? khachHang.Email ?? ""),
                new Claim(ClaimTypes.GivenName, khachHang.HoTen),
                new Claim(ClaimTypes.Email, khachHang.Email ?? ""),
                new Claim(ClaimTypes.MobilePhone, khachHang.SoDienThoai ?? ""),
                new Claim(ClaimTypes.Role, "KhachHang"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}