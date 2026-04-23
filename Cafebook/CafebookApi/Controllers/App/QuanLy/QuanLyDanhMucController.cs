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
    [Route("api/app/quanly-danhmuc")]
    [ApiController]
    [Authorize]
    public class QuanLyDanhMucController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyDanhMucController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.DanhMucs.AsNoTracking().Select(d => new QuanLyDanhMucGridDto { IdDanhMuc = d.IdDanhMuc, TenDanhMuc = d.TenDanhMuc, SoLuongSanPham = d.SanPhams.Count }).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyDanhMucSaveDto dto)
        {
            if (await _context.DanhMucs.AnyAsync(d => d.TenDanhMuc.ToLower() == dto.TenDanhMuc.ToLower())) return Conflict("Tên danh mục đã tồn tại.");
            _context.DanhMucs.Add(new DanhMuc { TenDanhMuc = dto.TenDanhMuc }); await _context.SaveChangesAsync(); return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyDanhMucSaveDto dto)
        {
            if (await _context.DanhMucs.AnyAsync(d => d.TenDanhMuc.ToLower() == dto.TenDanhMuc.ToLower() && d.IdDanhMuc != id)) return Conflict("Tên danh mục đã tồn tại.");
            var e = await _context.DanhMucs.FindAsync(id); if (e == null) return NotFound();
            e.TenDanhMuc = dto.TenDanhMuc; await _context.SaveChangesAsync(); return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.SanPhams.AnyAsync(s => s.IdDanhMuc == id)) return Conflict("Danh mục có sản phẩm, không thể xóa.");
            var e = await _context.DanhMucs.FindAsync(id); if (e == null) return NotFound();
            _context.DanhMucs.Remove(e); await _context.SaveChangesAsync(); return Ok();
        }
    }
}