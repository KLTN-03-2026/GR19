using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-sucoban")]
    [ApiController]
    public class QuanLySuCoBanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLySuCoBanController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetSuCo([FromQuery] bool isHistory = false)
        {
            var data = await _context.ThongBaos.Include(t => t.NhanVienTao)
                .Where(t => t.LoaiThongBao == "SuCoBan" && t.DaXem == isHistory)
                .OrderByDescending(t => t.ThoiGianTao)
                .Select(t => new QuanLySuCoBanDto
                {
                    IdThongBao = t.IdThongBao,
                    IdBan = t.IdLienQuan,
                    NoiDung = t.NoiDung,
                    ThoiGianTao = t.ThoiGianTao,
                    DaXem = t.DaXem,
                    TenNhanVienTao = t.NhanVienTao != null ? t.NhanVienTao.HoTen : "Hệ thống"
                }).ToListAsync();
            return Ok(data);
        }

        [HttpPost("resolve/{id}")]
        public async Task<IActionResult> Resolve(int id, [FromBody] QuanLySuCoBanResolveDto dto)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();
            tb.DaXem = true;
            if (dto.IdBan > 0)
            {
                var ban = await _context.Bans.FindAsync(dto.IdBan);
                if (ban != null)
                {
                    ban.TrangThai = "Trống";
                    ban.GhiChu = string.Empty;
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}