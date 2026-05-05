using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-thuongphat")]
    [ApiController]
    [Authorize]
    public class QuanLyThuongPhatController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyThuongPhatController(CafebookDbContext context) { _context = context; }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            var query = _context.Set<ThuongPhatMau>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.TenMau.Contains(keyword));

            var data = await query.OrderBy(p => p.Loai).ThenBy(p => p.TenMau)
                .Select(p => new QuanLyThuongPhatGridDto
                {
                    IdMau = p.IdMau,
                    Loai = p.Loai,
                    TenMau = p.TenMau,
                    SoTien = p.SoTien
                }).ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyThuongPhatSaveDto dto)
        {
            if (await _context.Set<ThuongPhatMau>().AnyAsync(x => x.TenMau == dto.TenMau && x.Loai == dto.Loai))
                return BadRequest("Mẫu này đã tồn tại trong hệ thống!");

            var mau = new ThuongPhatMau
            {
                Loai = dto.Loai,
                TenMau = dto.TenMau,
                SoTien = Math.Abs(dto.SoTien)
            };

            _context.Set<ThuongPhatMau>().Add(mau);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyThuongPhatSaveDto dto)
        {
            var mau = await _context.Set<ThuongPhatMau>().FindAsync(id);
            if (mau == null) return NotFound("Không tìm thấy mẫu.");

            mau.Loai = dto.Loai;
            mau.TenMau = dto.TenMau;
            mau.SoTien = Math.Abs(dto.SoTien);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mau = await _context.Set<ThuongPhatMau>().FindAsync(id);
            if (mau == null) return NotFound();

            _context.Set<ThuongPhatMau>().Remove(mau);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}