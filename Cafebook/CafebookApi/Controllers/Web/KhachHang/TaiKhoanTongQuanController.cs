using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims; 

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/taikhoantongquan")]
    [ApiController]
    [Authorize]
    [Authorize(Roles = "KhachHang")]
    public class TaikhoanTongquanController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public TaikhoanTongquanController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOverview(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int tokenId) || tokenId != id)
            {
                return Forbid("Bạn không có quyền xem thông tin tài khoản của người khác.");
            }


            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound();

            var hoaDons = await _context.HoaDons
                .Where(hd => hd.IdKhachHang == id && hd.TrangThai == "Đã thanh toán")
                .ToListAsync();

            var settingDoiVND = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "DiemTichLuy_DoiVND");
            decimal tyGiaDoi = 1000M;

            if (settingDoiVND != null && decimal.TryParse(settingDoiVND.GiaTri, out decimal parsed))
            {
                tyGiaDoi = parsed;
            }

            var dto = new TaiKhoanTongQuanDto
            {
                DiemTichLuy = kh.DiemTichLuy,
                GiaTriQuyDoiVND = kh.DiemTichLuy * tyGiaDoi,
                NgayTao = kh.NgayTao,
                TongHoaDon = hoaDons.Count,
                TongChiTieu = hoaDons.Sum(hd => hd.ThanhTien)
            };

            return Ok(dto);
        }
    }
}