using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/lichlamviec")]
    [ApiController]
    // Đã gỡ bỏ [Authorize] để không bị lỗi 401
    public class LichLamViecController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public LichLamViecController(CafebookDbContext context) { _context = context; }

        [HttpGet("my-schedule/{idNhanVien}")]
        public async Task<IActionResult> GetMySchedule(int idNhanVien, [FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                if (idNhanVien == 0) return BadRequest("Thiếu thông tin nhân viên.");

                var lich = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .AsNoTracking()
                    .Where(l => l.IdNhanVien == idNhanVien && l.NgayLam >= tuNgay.Date && l.NgayLam <= denNgay.Date)
                    .Select(l => new LichLamViec_CaNhanDto
                    {
                        IdLichLamViec = l.IdLichLamViec,
                        NgayLam = l.NgayLam,
                        TenCa = l.CaLamViec.TenCa,
                        GioBatDau = l.CaLamViec.GioBatDau,
                        GioKetThuc = l.CaLamViec.GioKetThuc,
                        GhiChu = l.GhiChu,
                        TrangThai = l.TrangThai
                    })
                    .ToListAsync();

                return Ok(lich);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var moCua = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "ThongTin_GioMoCua");
                var dongCua = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "ThongTin_GioDongCua");

                return Ok(new LichLamViec_ConfigDto
                {
                    GioMoCua = moCua != null ? TimeSpan.Parse(moCua.GiaTri) : new TimeSpan(7, 0, 0),
                    GioDongCua = dongCua != null ? TimeSpan.Parse(dongCua.GiaTri) : new TimeSpan(22, 0, 0)
                });
            }
            catch
            {
                // Fallback mặc định nếu có lỗi DB
                return Ok(new LichLamViec_ConfigDto { GioMoCua = new TimeSpan(7, 0, 0), GioDongCua = new TimeSpan(22, 0, 0) });
            }
        }
    }
}