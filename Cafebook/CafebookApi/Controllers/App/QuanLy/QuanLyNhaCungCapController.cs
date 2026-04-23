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
    [Route("api/app/quanly-nhacungcap")]
    [ApiController]
    [Authorize]
    public class QuanLyNhaCungCapController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyNhaCungCapController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.NhaCungCaps.AsNoTracking()
                .Select(n => new QuanLyNhaCungCapGridDto
                {
                    IdNhaCungCap = n.IdNhaCungCap,
                    TenNhaCungCap = n.TenNhaCungCap,
                    SoDienThoai = n.SoDienThoai,
                    DiaChi = n.DiaChi,
                    Email = n.Email
                }).OrderBy(n => n.TenNhaCungCap).ToListAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyNhaCungCapSaveDto dto)
        {
            if (await _context.NhaCungCaps.AnyAsync(n => n.TenNhaCungCap.ToLower() == dto.TenNhaCungCap.ToLower()))
                return Conflict("Tên Nhà cung cấp đã tồn tại.");

            var entity = new NhaCungCap
            {
                TenNhaCungCap = dto.TenNhaCungCap,
                SoDienThoai = dto.SoDienThoai,
                DiaChi = dto.DiaChi,
                Email = dto.Email
            };

            _context.NhaCungCaps.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyNhaCungCapSaveDto dto)
        {
            if (await _context.NhaCungCaps.AnyAsync(n => n.TenNhaCungCap.ToLower() == dto.TenNhaCungCap.ToLower() && n.IdNhaCungCap != id))
                return Conflict("Tên Nhà cung cấp đã tồn tại.");

            var entity = await _context.NhaCungCaps.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenNhaCungCap = dto.TenNhaCungCap;
            entity.SoDienThoai = dto.SoDienThoai;
            entity.DiaChi = dto.DiaChi;
            entity.Email = dto.Email;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.PhieuNhapKhos.AnyAsync(p => p.IdNhaCungCap == id))
                return Conflict("Không thể xóa Nhà cung cấp đã có giao dịch Nhập kho.");

            var entity = await _context.NhaCungCaps.FindAsync(id);
            if (entity == null) return NotFound();

            _context.NhaCungCaps.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}