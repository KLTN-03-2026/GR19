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
    [Route("api/app/quanly-thongbao")]
    [ApiController]
    [Authorize]
    public class QuanLyThongBaoController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyThongBaoController(CafebookDbContext context) { _context = context; }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] string? loaiThongBao, [FromQuery] string? keyword)
        {
            var query = _context.Set<ThongBao>().Include(t => t.NhanVienTao).AsNoTracking().AsQueryable();

            if (tuNgay.HasValue) query = query.Where(t => t.ThoiGianTao.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(t => t.ThoiGianTao.Date <= denNgay.Value.Date);
            if (!string.IsNullOrEmpty(loaiThongBao) && loaiThongBao != "Tất cả") query = query.Where(t => t.LoaiThongBao == loaiThongBao);
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(t => t.NoiDung.Contains(keyword));

            var data = await query.OrderByDescending(t => t.ThoiGianTao).Take(200)
                .Select(t => new QuanLyThongBaoGridDto
                {
                    IdThongBao = t.IdThongBao,
                    NoiDung = t.NoiDung,
                    ThoiGianTao = t.ThoiGianTao,
                    LoaiThongBao = t.LoaiThongBao ?? "Khác",
                    DaXem = t.DaXem,
                    TenNhanVienTao = t.NhanVienTao != null ? t.NhanVienTao.HoTen : "Hệ thống"
                }).ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyThongBaoSaveDto dto)
        {
            // Chỉ cho phép thêm mới các loại thông báo thủ công
            string[] allowedTypes = { "ThongBaoNhanVien", "ThongBaoQuanLy", "ThongBaoToanNhanVien" };
            if (!allowedTypes.Contains(dto.LoaiThongBao))
                return BadRequest("Chỉ được phép tạo thủ công các loại: Thông báo nhân viên, quản lý, toàn hệ thống.");

            var tb = new ThongBao
            {
                NoiDung = dto.NoiDung,
                LoaiThongBao = dto.LoaiThongBao,
                IdNhanVienTao = dto.IdNhanVienTao,
                ThoiGianTao = DateTime.Now,
                DaXem = false
            };

            _context.Set<ThongBao>().Add(tb);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyThongBaoSaveDto dto)
        {
            var tb = await _context.Set<ThongBao>().FindAsync(id);
            if (tb == null) return NotFound();

            tb.NoiDung = dto.NoiDung;
            tb.DaXem = dto.DaXem;
            // Cố tình không cho phép đổi LoaiThongBao ở hàm Update để bảo vệ dữ liệu hệ thống

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.Set<ThongBao>().FindAsync(id);
            if (tb == null) return NotFound();

            _context.Set<ThongBao>().Remove(tb);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}