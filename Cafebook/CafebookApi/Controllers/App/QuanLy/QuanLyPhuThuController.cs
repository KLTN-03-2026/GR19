using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-phuthu")]
    [ApiController]
    public class QuanLyPhuThuController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyPhuThuController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PhuThus.AsNoTracking()
                .Select(p => new QuanLyPhuThuGridDto
                {
                    IdPhuThu = p.IdPhuThu,
                    TenPhuThu = p.TenPhuThu,
                    GiaTri = p.GiaTri,
                    LoaiGiaTri = p.LoaiGiaTri // Trả về trực tiếp chuỗi từ DB
                }).OrderBy(p => p.TenPhuThu).ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyPhuThuSaveDto dto)
        {
            if (await _context.PhuThus.AnyAsync(p => p.TenPhuThu.ToLower() == dto.TenPhuThu.ToLower()))
                return Conflict("Tên phụ thu đã tồn tại.");

            var entity = new PhuThu
            {
                TenPhuThu = dto.TenPhuThu,
                GiaTri = dto.GiaTri,
                LoaiGiaTri = dto.LoaiGiaTri
            };

            _context.PhuThus.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyPhuThuSaveDto dto)
        {
            var entity = await _context.PhuThus.FindAsync(id);
            if (entity == null) return NotFound();

            if (await _context.PhuThus.AnyAsync(p => p.TenPhuThu.ToLower() == dto.TenPhuThu.ToLower() && p.IdPhuThu != id))
                return Conflict("Tên phụ thu đã tồn tại.");

            entity.TenPhuThu = dto.TenPhuThu;
            entity.GiaTri = dto.GiaTri;
            entity.LoaiGiaTri = dto.LoaiGiaTri;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.ChiTietPhuThuHoaDons.AnyAsync(ct => ct.IdPhuThu == id))
                return Conflict("Không thể xóa phụ thu này vì đã được sử dụng trong các Hóa đơn.");

            var entity = await _context.PhuThus.FindAsync(id);
            if (entity == null) return NotFound();

            _context.PhuThus.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}