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

        public QuanLyBanController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("khuvuc-tree")]
        public async Task<IActionResult> GetKhuVucTree()
        {
            var data = await _context.KhuVucs
                .AsNoTracking()
                .Select(kv => new QuanLyKhuVucDto
                {
                    IdKhuVuc = kv.IdKhuVuc,
                    TenKhuVuc = kv.TenKhuVuc,
                    MoTa = kv.MoTa,
                    Bans = kv.Bans.Select(b => new QuanLyBanGridDto
                    {
                        IdBan = b.IdBan,
                        SoBan = b.SoBan,
                        SoGhe = b.SoGhe,
                        TrangThai = b.TrangThai,
                        GhiChu = b.GhiChu,
                        IdKhuVuc = b.IdKhuVuc ?? 0 // FIX LỖI CS0266 (Xử lý null)
                    }).ToList()
                }).ToListAsync();
            return Ok(data);
        }

        [HttpPost("khuvuc")]
        public async Task<IActionResult> CreateKhuVuc([FromBody] QuanLyKhuVucSaveDto dto)
        {
            if (await _context.KhuVucs.AnyAsync(k => k.TenKhuVuc.ToLower() == dto.TenKhuVuc.ToLower()))
                return Conflict($"Tên khu vực '{dto.TenKhuVuc}' đã tồn tại.");

            var entity = new KhuVuc { TenKhuVuc = dto.TenKhuVuc, MoTa = dto.MoTa };
            _context.KhuVucs.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("khuvuc/{id}")]
        public async Task<IActionResult> UpdateKhuVuc(int id, [FromBody] QuanLyKhuVucSaveDto dto)
        {
            if (await _context.KhuVucs.AnyAsync(k => k.TenKhuVuc.ToLower() == dto.TenKhuVuc.ToLower() && k.IdKhuVuc != id))
                return Conflict($"Tên khu vực '{dto.TenKhuVuc}' đã tồn tại.");

            var entity = await _context.KhuVucs.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenKhuVuc = dto.TenKhuVuc;
            entity.MoTa = dto.MoTa;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("khuvuc/{id}")]
        public async Task<IActionResult> DeleteKhuVuc(int id)
        {
            if (await _context.Bans.AnyAsync(b => b.IdKhuVuc == id))
                return Conflict("Không thể xóa. Khu vực này đang chứa bàn.");

            var entity = await _context.KhuVucs.FindAsync(id);
            if (entity == null) return NotFound();

            _context.KhuVucs.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ban")]
        public async Task<IActionResult> CreateBan([FromBody] QuanLyBanSaveDto dto)
        {
            if (await _context.Bans.AnyAsync(b => b.SoBan.ToLower() == dto.SoBan.ToLower() && b.IdKhuVuc == dto.IdKhuVuc))
                return Conflict($"Số bàn '{dto.SoBan}' đã tồn tại trong khu vực này.");

            var entity = new Ban { SoBan = dto.SoBan, SoGhe = dto.SoGhe, TrangThai = dto.TrangThai, GhiChu = dto.GhiChu, IdKhuVuc = dto.IdKhuVuc };
            _context.Bans.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("ban/{id}")]
        public async Task<IActionResult> UpdateBan(int id, [FromBody] QuanLyBanSaveDto dto)
        {
            if (await _context.Bans.AnyAsync(b => b.SoBan.ToLower() == dto.SoBan.ToLower() && b.IdKhuVuc == dto.IdKhuVuc && b.IdBan != id))
                return Conflict($"Số bàn '{dto.SoBan}' đã tồn tại trong khu vực này.");

            var entity = await _context.Bans.FindAsync(id);
            if (entity == null) return NotFound();

            entity.SoBan = dto.SoBan; entity.SoGhe = dto.SoGhe; entity.TrangThai = dto.TrangThai; entity.GhiChu = dto.GhiChu; entity.IdKhuVuc = dto.IdKhuVuc;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("ban/{id}")]
        public async Task<IActionResult> DeleteBan(int id)
        {
            if (await _context.HoaDons.AnyAsync(h => h.IdBan == id && h.TrangThai != "Đã thanh toán"))
                return Conflict("Không thể xóa. Bàn đang có hóa đơn CHƯA thanh toán.");

            if (await _context.PhieuDatBans.AnyAsync(p => p.IdBan == id && p.ThoiGianDat > DateTime.Now))
                return Conflict("Không thể xóa. Bàn đang có phiếu đặt trước CHƯA diễn ra.");

            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();

            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("ban/{id}/history")]
        public async Task<IActionResult> GetBanHistory(int id)
        {
            var history = new QuanLyBanHistoryDto
            {
                SoLuotPhucVu = await _context.HoaDons.CountAsync(h => h.IdBan == id && h.TrangThai == "Đã thanh toán"),
                TongDoanhThu = await _context.HoaDons.Where(h => h.IdBan == id && h.TrangThai == "Đã thanh toán").SumAsync(h => h.ThanhTien),
                SoLuotDatTruoc = await _context.PhieuDatBans.CountAsync(p => p.IdBan == id)
            };
            return Ok(history);
        }
    }
}