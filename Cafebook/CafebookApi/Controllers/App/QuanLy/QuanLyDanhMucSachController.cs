using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-danhmucsach")]
    [ApiController]
    [Authorize]
    public class QuanLyDanhMucSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyDanhMucSachController(CafebookDbContext context)
        {
            _context = context;
        }

        // ======================= 1. TÁC GIẢ =======================
        [HttpGet("tacgia")]
        public async Task<IActionResult> GetTacGias() => Ok(await _context.Set<TacGia>().AsNoTracking()
            .OrderBy(x => x.TenTacGia)
            .Select(x => new QuanLyDanhMucSachItemDto { Id = x.IdTacGia, Ten = x.TenTacGia, MoTa = x.GioiThieu }).ToListAsync());

        [HttpPost("tacgia")]
        public async Task<IActionResult> CreateTacGia([FromBody] QuanLyDanhMucSachSaveDto dto)
        {
            if (await _context.Set<TacGia>().AnyAsync(x => x.TenTacGia.ToLower() == dto.Ten.ToLower()))
                return Conflict("Tên tác giả đã tồn tại.");

            var entity = new TacGia { TenTacGia = dto.Ten.Trim(), GioiThieu = dto.MoTa };
            _context.Set<TacGia>().Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("tacgia/{id}")]
        public async Task<IActionResult> UpdateTacGia(int id, [FromBody] QuanLyDanhMucSachSaveDto dto)
        {
            if (await _context.Set<TacGia>().AnyAsync(x => x.TenTacGia.ToLower() == dto.Ten.ToLower() && x.IdTacGia != id))
                return Conflict("Tên tác giả đã tồn tại.");

            var entity = await _context.Set<TacGia>().FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenTacGia = dto.Ten.Trim();
            entity.GioiThieu = dto.MoTa;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("tacgia/{id}")]
        public async Task<IActionResult> DeleteTacGia(int id)
        {
            if (await _context.Set<SachTacGia>().AnyAsync(x => x.IdTacGia == id))
                return Conflict("Không thể xóa tác giả này vì đang có sách liên kết trong thư viện.");

            var entity = await _context.Set<TacGia>().FindAsync(id);
            if (entity == null) return NotFound();

            _context.Set<TacGia>().Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ======================= 2. THỂ LOẠI =======================
        [HttpGet("theloai")]
        public async Task<IActionResult> GetTheLoais() => Ok(await _context.Set<TheLoai>().AsNoTracking()
            .OrderBy(x => x.TenTheLoai)
            .Select(x => new QuanLyDanhMucSachItemDto { Id = x.IdTheLoai, Ten = x.TenTheLoai, MoTa = x.MoTa }).ToListAsync());

        [HttpPost("theloai")]
        public async Task<IActionResult> CreateTheLoai([FromBody] QuanLyDanhMucSachSaveDto dto)
        {
            if (await _context.Set<TheLoai>().AnyAsync(x => x.TenTheLoai.ToLower() == dto.Ten.ToLower()))
                return Conflict("Tên thể loại đã tồn tại.");

            var entity = new TheLoai { TenTheLoai = dto.Ten.Trim(), MoTa = dto.MoTa };
            _context.Set<TheLoai>().Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("theloai/{id}")]
        public async Task<IActionResult> UpdateTheLoai(int id, [FromBody] QuanLyDanhMucSachSaveDto dto)
        {
            if (await _context.Set<TheLoai>().AnyAsync(x => x.TenTheLoai.ToLower() == dto.Ten.ToLower() && x.IdTheLoai != id))
                return Conflict("Tên thể loại đã tồn tại.");

            var entity = await _context.Set<TheLoai>().FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenTheLoai = dto.Ten.Trim();
            entity.MoTa = dto.MoTa;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("theloai/{id}")]
        public async Task<IActionResult> DeleteTheLoai(int id)
        {
            if (await _context.Set<SachTheLoai>().AnyAsync(x => x.IdTheLoai == id))
                return Conflict("Không thể xóa thể loại này vì đang có sách liên kết.");

            var entity = await _context.Set<TheLoai>().FindAsync(id);
            if (entity == null) return NotFound();

            _context.Set<TheLoai>().Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ======================= 3. NHÀ XUẤT BẢN =======================
        [HttpGet("nhaxuatban")]
        public async Task<IActionResult> GetNXBs() => Ok(await _context.Set<NhaXuatBan>().AsNoTracking()
            .OrderBy(x => x.TenNhaXuatBan)
            .Select(x => new QuanLyDanhMucSachItemDto { Id = x.IdNhaXuatBan, Ten = x.TenNhaXuatBan, MoTa = x.MoTa }).ToListAsync());

        [HttpPost("nhaxuatban")]
        public async Task<IActionResult> CreateNXB([FromBody] QuanLyDanhMucSachSaveDto dto)
        {
            if (await _context.Set<NhaXuatBan>().AnyAsync(x => x.TenNhaXuatBan.ToLower() == dto.Ten.ToLower()))
                return Conflict("Tên NXB đã tồn tại.");

            var entity = new NhaXuatBan { TenNhaXuatBan = dto.Ten.Trim(), MoTa = dto.MoTa };
            _context.Set<NhaXuatBan>().Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("nhaxuatban/{id}")]
        public async Task<IActionResult> UpdateNXB(int id, [FromBody] QuanLyDanhMucSachSaveDto dto)
        {
            if (await _context.Set<NhaXuatBan>().AnyAsync(x => x.TenNhaXuatBan.ToLower() == dto.Ten.ToLower() && x.IdNhaXuatBan != id))
                return Conflict("Tên NXB đã tồn tại.");

            var entity = await _context.Set<NhaXuatBan>().FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenNhaXuatBan = dto.Ten.Trim();
            entity.MoTa = dto.MoTa;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("nhaxuatban/{id}")]
        public async Task<IActionResult> DeleteNXB(int id)
        {
            if (await _context.Set<SachNhaXuatBan>().AnyAsync(x => x.IdNhaXuatBan == id))
                return Conflict("Không thể xóa NXB này vì đang có sách liên kết.");

            var entity = await _context.Set<NhaXuatBan>().FindAsync(id);
            if (entity == null) return NotFound();

            _context.Set<NhaXuatBan>().Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}