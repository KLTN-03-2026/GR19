using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-khuvuc")]
    [ApiController]
    [Authorize]
    public class QuanLyKhuVucController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyKhuVucController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.KhuVucs.AsNoTracking()
                .Select(k => new QuanLyKhuVucDto
                {
                    IdKhuVuc = k.IdKhuVuc,
                    TenKhuVuc = k.TenKhuVuc,
                    MoTa = k.MoTa,
                    SoLuongBan = k.Bans.Count
                }).ToListAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyKhuVucSaveDto dto)
        {
            if (await _context.KhuVucs.AnyAsync(k => k.TenKhuVuc.ToLower() == dto.TenKhuVuc.ToLower()))
                return Conflict("Tên khu vực đã tồn tại.");
            var entity = new KhuVuc { TenKhuVuc = dto.TenKhuVuc, MoTa = dto.MoTa };
            _context.KhuVucs.Add(entity); await _context.SaveChangesAsync(); return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyKhuVucSaveDto dto)
        {
            if (await _context.KhuVucs.AnyAsync(k => k.TenKhuVuc.ToLower() == dto.TenKhuVuc.ToLower() && k.IdKhuVuc != id))
                return Conflict("Tên khu vực đã tồn tại.");
            var entity = await _context.KhuVucs.FindAsync(id);
            if (entity == null) return NotFound();
            entity.TenKhuVuc = dto.TenKhuVuc; entity.MoTa = dto.MoTa;
            await _context.SaveChangesAsync(); return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.Bans.AnyAsync(b => b.IdKhuVuc == id))
                return Conflict("Khu vực đang chứa bàn, không thể xóa.");
            var entity = await _context.KhuVucs.FindAsync(id);
            if (entity == null) return NotFound();
            _context.KhuVucs.Remove(entity); await _context.SaveChangesAsync(); return Ok();
        }
    }
}