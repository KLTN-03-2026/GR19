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
    [Route("api/app/quanly-donxinnghi")]
    [ApiController]
    [Authorize]
    public class QuanLyDonXinNghiController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyDonXinNghiController(CafebookDbContext context) { _context = context; }

        [HttpGet("search")]
        public async Task<IActionResult> Search()
        {
            var result = await _context.DonXinNghis
                .Include(d => d.NhanVien)
                .Include(d => d.NguoiDuyet)
                .OrderByDescending(d => d.IdDonXinNghi) // Đơn mới nhất lên đầu
                .Select(d => new QuanLyDonXinNghiGridDto
                {
                    IdDonXinNghi = d.IdDonXinNghi,
                    TenNhanVien = d.NhanVien!.HoTen,
                    LoaiDon = d.LoaiDon,
                    NgayBatDau = d.NgayBatDau,
                    NgayKetThuc = d.NgayKetThuc,
                    LyDo = d.LyDo,
                    TrangThai = d.TrangThai,
                    TenNguoiDuyet = d.NguoiDuyet != null ? d.NguoiDuyet.HoTen : "",
                    NgayDuyet = d.NgayDuyet,
                    GhiChuPheDuyet = d.GhiChuPheDuyet
                }).ToListAsync();
            return Ok(result);
        }

        // THÊM MỚI: API Xem trước các ca làm việc bị ảnh hưởng
        [HttpGet("affected-shifts/{idDon}")]
        public async Task<IActionResult> GetAffectedShifts(int idDon)
        {
            var don = await _context.DonXinNghis.FindAsync(idDon);
            if (don == null) return NotFound("Không tìm thấy đơn.");

            var affectedShifts = await _context.LichLamViecs
                .Include(l => l.CaLamViec)
                .Where(l => l.IdNhanVien == don.IdNhanVien
                         && l.NgayLam.Date >= don.NgayBatDau.Date
                         && l.NgayLam.Date <= don.NgayKetThuc.Date)
                .OrderBy(l => l.NgayLam)
                .Select(l => new AffectedShiftDto
                {
                    IdLichLamViec = l.IdLichLamViec,
                    NgayLam = l.NgayLam,
                    TenCa = l.CaLamViec != null ? l.CaLamViec.TenCa : "Ca không xác định"
                }).ToListAsync();

            return Ok(affectedShifts);
        }

        [HttpPut("approve/{idDon}")]
        public async Task<IActionResult> ApproveDon(int idDon, [FromBody] QuanLyDonXinNghiActionDto dto)
        {
            var don = await _context.DonXinNghis.FindAsync(idDon);
            if (don == null) return NotFound("Không tìm thấy đơn.");
            if (don.TrangThai != "Chờ duyệt") return Conflict("Đơn này đã được xử lý trước đó.");

            don.TrangThai = "Đã duyệt";
            don.IdNguoiDuyet = dto.IdNguoiDuyet;
            don.GhiChuPheDuyet = dto.GhiChuPheDuyet;
            don.NgayDuyet = DateTime.Now;

            // XÓA CA LÀM VIỆC TỰ ĐỘNG
            var caLamHuy = await _context.LichLamViecs
                .Where(l => l.IdNhanVien == don.IdNhanVien
                         && l.NgayLam.Date >= don.NgayBatDau.Date
                         && l.NgayLam.Date <= don.NgayKetThuc.Date)
                .ToListAsync();

            if (caLamHuy.Any()) _context.LichLamViecs.RemoveRange(caLamHuy);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đơn thành công." });
        }

        [HttpPut("reject/{idDon}")]
        public async Task<IActionResult> RejectDon(int idDon, [FromBody] QuanLyDonXinNghiActionDto dto)
        {
            var don = await _context.DonXinNghis.FindAsync(idDon);
            if (don == null) return NotFound("Không tìm thấy đơn.");
            if (don.TrangThai != "Chờ duyệt") return Conflict("Đơn này đã được xử lý trước đó.");

            don.TrangThai = "Đã từ chối";
            don.IdNguoiDuyet = dto.IdNguoiDuyet;
            don.GhiChuPheDuyet = dto.GhiChuPheDuyet;
            don.NgayDuyet = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Từ chối đơn thành công." });
        }
    }
}