using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-dinhluong")]
    [ApiController]
    public class QuanLyDinhLuongController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyDinhLuongController(CafebookDbContext context) { _context = context; }

        [HttpGet("lookup-sp")]
        public async Task<IActionResult> GetSanPham() => Ok(await _context.SanPhams.Include(s => s.DanhMuc).Select(s => new QuanLyDinhLuongSPDto { IdSanPham = s.IdSanPham, TenSanPham = s.TenSanPham, TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : "Khác" }).ToListAsync());

        [HttpGet("lookup-nl")]
        public async Task<IActionResult> GetNguyenLieu() => Ok(await _context.NguyenLieus.Select(n => new LookupDinhLuongDto { Id = n.IdNguyenLieu, Ten = n.TenNguyenLieu }).ToListAsync());

        [HttpGet("lookup-dv")]
        public async Task<IActionResult> GetDonVi() => Ok(await _context.DonViChuyenDois.Select(d => new LookupDinhLuongDto { Id = d.IdChuyenDoi, Ten = d.TenDonVi }).ToListAsync()); // FIX: Sử dụng DonViChuyenDois và IdChuyenDoi

        [HttpGet("{idSp}")]
        public async Task<IActionResult> GetDinhLuong(int idSp) => Ok(await _context.DinhLuongs.Include(d => d.NguyenLieu).Include(d => d.DonViSuDung).Where(d => d.IdSanPham == idSp).Select(d => new QuanLyDinhLuongNLDto { IdNguyenLieu = d.IdNguyenLieu, TenNguyenLieu = d.NguyenLieu!.TenNguyenLieu, SoLuongSuDung = d.SoLuongSuDung, IdDonViSuDung = d.IdDonViSuDung, TenDonVi = d.DonViSuDung!.TenDonVi }).ToListAsync()); // FIX: Dùng DonViSuDung thay vì DonViDo

        [HttpPost("{idSp}")]
        public async Task<IActionResult> Save(int idSp, [FromBody] QuanLyDinhLuongSaveDto dto)
        {
            var entity = await _context.DinhLuongs.FindAsync(idSp, dto.IdNguyenLieu);
            if (entity != null)
            {
                entity.SoLuongSuDung = dto.SoLuongSuDung; entity.IdDonViSuDung = dto.IdDonViSuDung;
            }
            else
            {
                _context.DinhLuongs.Add(new DinhLuong { IdSanPham = idSp, IdNguyenLieu = dto.IdNguyenLieu, SoLuongSuDung = dto.SoLuongSuDung, IdDonViSuDung = dto.IdDonViSuDung });
            }
            await _context.SaveChangesAsync(); return Ok();
        }

        [HttpDelete("{idSp}/{idNl}")]
        public async Task<IActionResult> Delete(int idSp, int idNl)
        {
            var entity = await _context.DinhLuongs.FindAsync(idSp, idNl);
            if (entity == null) return NotFound();
            _context.DinhLuongs.Remove(entity); await _context.SaveChangesAsync(); return Ok();
        }
    }
}