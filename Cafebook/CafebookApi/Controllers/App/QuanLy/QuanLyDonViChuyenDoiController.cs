using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-donvichuyendoi")]
    [ApiController]
    public class QuanLyDonViChuyenDoiController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyDonViChuyenDoiController(CafebookDbContext context) { _context = context; }

        [HttpGet("lookup-nguyenlieu")]
        public async Task<IActionResult> GetNguyenLieuLookup() => Ok(await _context.NguyenLieus.Select(n => new LookupNguyenLieuDvtDto { Id = n.IdNguyenLieu, Ten = n.TenNguyenLieu }).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DonViChuyenDois.Include(d => d.NguyenLieu).AsNoTracking()
                .Select(d => new QuanLyDonViChuyenDoiGridDto
                {
                    IdChuyenDoi = d.IdChuyenDoi,
                    IdNguyenLieu = d.IdNguyenLieu,
                    TenNguyenLieu = d.NguyenLieu!.TenNguyenLieu,
                    TenDonVi = d.TenDonVi,
                    GiaTriQuyDoi = d.GiaTriQuyDoi,
                    LaDonViCoBan = d.LaDonViCoBan
                }).OrderBy(d => d.TenNguyenLieu).ToListAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyDonViChuyenDoiSaveDto dto)
        {
            if (dto.LaDonViCoBan)
            {
                if (await _context.DonViChuyenDois.AnyAsync(d => d.IdNguyenLieu == dto.IdNguyenLieu && d.LaDonViCoBan))
                    return Conflict("Nguyên liệu này đã có Đơn vị cơ bản. Không thể chọn thêm.");
                dto.GiaTriQuyDoi = 1; // Đơn vị cơ bản luôn có tỷ lệ 1
            }

            var entity = new DonViChuyenDoi { IdNguyenLieu = dto.IdNguyenLieu, TenDonVi = dto.TenDonVi, GiaTriQuyDoi = dto.GiaTriQuyDoi, LaDonViCoBan = dto.LaDonViCoBan };
            _context.DonViChuyenDois.Add(entity); await _context.SaveChangesAsync(); return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyDonViChuyenDoiSaveDto dto)
        {
            var entity = await _context.DonViChuyenDois.FindAsync(id);
            if (entity == null) return NotFound();

            if (dto.LaDonViCoBan)
            {
                if (await _context.DonViChuyenDois.AnyAsync(d => d.IdNguyenLieu == dto.IdNguyenLieu && d.LaDonViCoBan && d.IdChuyenDoi != id))
                    return Conflict("Nguyên liệu này đã có Đơn vị cơ bản. Không thể cập nhật.");
                dto.GiaTriQuyDoi = 1;
            }

            entity.IdNguyenLieu = dto.IdNguyenLieu; entity.TenDonVi = dto.TenDonVi; entity.GiaTriQuyDoi = dto.GiaTriQuyDoi; entity.LaDonViCoBan = dto.LaDonViCoBan;
            await _context.SaveChangesAsync(); return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _context.DinhLuongs.AnyAsync(d => d.IdDonViSuDung == id))
                return Conflict("Không thể xóa. Đơn vị này đang được sử dụng trong Định lượng sản phẩm.");

            var entity = await _context.DonViChuyenDois.FindAsync(id);
            if (entity == null) return NotFound();

            if (entity.LaDonViCoBan && await _context.DonViChuyenDois.AnyAsync(d => d.IdNguyenLieu == entity.IdNguyenLieu && d.IdChuyenDoi != id))
                return Conflict("Phải xóa các đơn vị quy đổi khác trước khi xóa Đơn vị cơ bản.");

            _context.DonViChuyenDois.Remove(entity); await _context.SaveChangesAsync(); return Ok();
        }
    }
}