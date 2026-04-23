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
        public async Task<IActionResult> Search([FromQuery] int idNhanVien, [FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay)
        {
            var query = _context.Set<PhieuThuongPhat>().Include(p => p.NguoiTao).AsNoTracking().AsQueryable();

            if (idNhanVien > 0) query = query.Where(p => p.IdNhanVien == idNhanVien);
            if (tuNgay.HasValue) query = query.Where(p => p.NgayTao.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(p => p.NgayTao.Date <= denNgay.Value.Date);

            var data = await query.OrderByDescending(p => p.NgayTao)
                .Select(p => new QuanLyThuongPhatGridDto
                {
                    IdPhieuThuongPhat = p.IdPhieuThuongPhat,
                    NgayTao = p.NgayTao,
                    SoTien = Math.Abs(p.SoTien), // Lấy giá trị tuyệt đối để hiển thị UI
                    Loai = p.SoTien >= 0 ? "Thưởng" : "Phạt",
                    LyDo = p.LyDo,
                    TenNguoiTao = p.NguoiTao != null ? p.NguoiTao.HoTen : "Hệ thống",
                    DaChot = p.IdPhieuLuong != null
                }).ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyThuongPhatSaveDto dto)
        {
            decimal tienThucTe = dto.Loai == "Phạt" ? -Math.Abs(dto.SoTien) : Math.Abs(dto.SoTien);

            var phieu = new PhieuThuongPhat
            {
                IdNhanVien = dto.IdNhanVien,
                IdNguoiTao = dto.IdNguoiTao,
                NgayTao = dto.NgayTao,
                SoTien = tienThucTe,
                LyDo = dto.LyDo
            };

            _context.Set<PhieuThuongPhat>().Add(phieu);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyThuongPhatSaveDto dto)
        {
            var phieu = await _context.Set<PhieuThuongPhat>().FindAsync(id);
            if (phieu == null) return NotFound();
            if (phieu.IdPhieuLuong != null) return BadRequest("Không thể sửa khoản Thưởng/Phạt đã được chốt vào Phiếu Lương.");

            decimal tienThucTe = dto.Loai == "Phạt" ? -Math.Abs(dto.SoTien) : Math.Abs(dto.SoTien);

            phieu.IdNhanVien = dto.IdNhanVien;
            // Không đổi người tạo ban đầu
            phieu.NgayTao = dto.NgayTao;
            phieu.SoTien = tienThucTe;
            phieu.LyDo = dto.LyDo;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var phieu = await _context.Set<PhieuThuongPhat>().FindAsync(id);
            if (phieu == null) return NotFound();
            if (phieu.IdPhieuLuong != null) return BadRequest("Không thể xóa khoản Thưởng/Phạt đã được chốt vào Phiếu Lương.");

            _context.Set<PhieuThuongPhat>().Remove(phieu);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}