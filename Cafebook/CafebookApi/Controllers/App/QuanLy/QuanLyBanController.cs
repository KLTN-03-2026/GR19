using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-ban")]
    [ApiController]
    public class QuanLyBanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyBanController(CafebookDbContext context) { _context = context; }

        [HttpGet("lookup-khuvuc")]
        public async Task<IActionResult> GetLookup() => Ok(await _context.KhuVucs.Select(k => new LookupKhuVucDto { IdKhuVuc = k.IdKhuVuc, TenKhuVuc = k.TenKhuVuc }).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Bans.Include(b => b.KhuVuc).AsNoTracking().Select(b => new QuanLyBanGridDto
            {
                IdBan = b.IdBan,
                SoBan = b.SoBan,
                SoGhe = b.SoGhe,
                TrangThai = b.TrangThai,
                GhiChu = b.GhiChu,
                IdKhuVuc = b.IdKhuVuc ?? 0,
                TenKhuVuc = b.KhuVuc!.TenKhuVuc
            }).ToListAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyBanSaveDto dto)
        {
            if (await _context.Bans.AnyAsync(b => b.SoBan.ToLower() == dto.SoBan.ToLower() && b.IdKhuVuc == dto.IdKhuVuc)) return Conflict("Số bàn đã tồn tại trong khu vực.");
            var entity = new Ban { SoBan = dto.SoBan, SoGhe = dto.SoGhe, TrangThai = dto.TrangThai, GhiChu = dto.GhiChu, IdKhuVuc = dto.IdKhuVuc };
            _context.Bans.Add(entity); await _context.SaveChangesAsync(); return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyBanSaveDto dto)
        {
            if (await _context.Bans.AnyAsync(b => b.SoBan.ToLower() == dto.SoBan.ToLower() && b.IdKhuVuc == dto.IdKhuVuc && b.IdBan != id)) return Conflict("Số bàn đã tồn tại.");
            var entity = await _context.Bans.FindAsync(id);
            if (entity == null) return NotFound();
            entity.SoBan = dto.SoBan; entity.SoGhe = dto.SoGhe; entity.TrangThai = dto.TrangThai; entity.GhiChu = dto.GhiChu; entity.IdKhuVuc = dto.IdKhuVuc;
            await _context.SaveChangesAsync(); return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.HoaDons.AnyAsync(h => h.IdBan == id && h.TrangThai != "Đã thanh toán")) return Conflict("Bàn có hóa đơn chưa thanh toán.");
            var entity = await _context.Bans.FindAsync(id);
            if (entity == null) return NotFound();
            _context.Bans.Remove(entity); await _context.SaveChangesAsync(); return Ok();
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> History(int id)
        {
            return Ok(new QuanLyBanHistoryDto
            {
                SoLuotPhucVu = await _context.HoaDons.CountAsync(h => h.IdBan == id && h.TrangThai == "Đã thanh toán"),
                TongDoanhThu = await _context.HoaDons.Where(h => h.IdBan == id && h.TrangThai == "Đã thanh toán").SumAsync(h => h.ThanhTien)
            });
        }
    }
}