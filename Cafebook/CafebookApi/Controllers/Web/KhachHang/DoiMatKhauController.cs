using CafebookApi.Data;
using CafebookApi.Services;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/doi-mat-khau")]
    [ApiController]
    [Authorize(Roles = "KhachHang")] // [Quy tắc 3] Bảo mật lớp 2
    public class DoiMatKhauController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IPasswordService _passwordService;

        public DoiMatKhauController(CafebookDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] DoiMatKhauDto model)
        {
            // [Quy tắc 3] Chống IDOR: Xác minh ID người dùng đang gửi request
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int tokenUserId) || tokenUserId != id)
            {
                return Forbid(); // Trả về 403 nếu cố tình đổi pass của người khác
            }

            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound(new { Message = "Không tìm thấy tài khoản." });

            // SỬ DỤNG HÀM VERIFY KÈM KIỂM TRA NULL
            if (string.IsNullOrEmpty(kh.MatKhau) || !_passwordService.VerifyPassword(kh.MatKhau, model.MatKhauCu))
            {
                return BadRequest(new { Message = "Mật khẩu cũ không chính xác." });
            }

            // SỬ DỤNG HÀM HASH ĐỂ MÃ HÓA MẬT KHẨU MỚI
            kh.MatKhau = _passwordService.HashPassword(model.MatKhauMoi);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đổi mật khẩu thành công." });
        }
    }
}