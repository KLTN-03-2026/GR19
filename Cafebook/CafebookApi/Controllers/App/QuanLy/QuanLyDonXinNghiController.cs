using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-donxinnghi")]
    [ApiController]
    public class QuanLyDonXinNghiController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyDonXinNghiController(CafebookDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách Đơn Xin Nghỉ
        [HttpGet("search")]
        public async Task<IActionResult> Search()
        {
            var result = await _context.DonXinNghis
                .Include(d => d.NhanVien)
                .Include(d => d.NguoiDuyet)
                .OrderByDescending(d => d.NgayBatDau)
                .Select(d => new QuanLyDonXinNghiGridDto
                {
                    IdDonXinNghi = d.IdDonXinNghi,
                    TenNhanVien = d.NhanVien!.HoTen,
                    LoaiDon = d.LoaiDon,
                    NgayBatDau = d.NgayBatDau,
                    NgayKetThuc = d.NgayKetThuc,
                    LyDo = d.LyDo,
                    TrangThai = d.TrangThai,
                    TenNguoiDuyet = d.NguoiDuyet != null ? d.NguoiDuyet.HoTen : null,
                    NgayDuyet = d.NgayDuyet,
                    GhiChuPheDuyet = d.GhiChuPheDuyet
                }).ToListAsync();

            return Ok(result);
        }

        // Duyệt đơn (Kèm tự động xóa lịch làm việc bị trùng)
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

            // Xử lý xóa lịch làm việc trùng với ngày nghỉ
            var conflictingSchedules = await _context.LichLamViecs
                .Where(l => l.IdNhanVien == don.IdNhanVien &&
                            l.NgayLam >= don.NgayBatDau.Date &&
                            l.NgayLam <= don.NgayKetThuc.Date)
                .ToListAsync();

            foreach (var lich in conflictingSchedules)
            {
                if (await _context.BangChamCongs.AnyAsync(c => c.IdLichLamViec == lich.IdLichLamViec))
                {
                    return Conflict($"Không thể duyệt. Nhân viên đã chấm công ngày {lich.NgayLam:dd/MM/yyyy}.");
                }
            }

            if (conflictingSchedules.Any())
            {
                _context.LichLamViecs.RemoveRange(conflictingSchedules);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đơn thành công và đã tự động cập nhật lịch làm việc." });
        }

        // Từ chối đơn
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

        // Xóa đơn (Hủy yêu cầu)
        [HttpDelete("{idDon}")]
        public async Task<IActionResult> DeleteDon(int idDon)
        {
            var don = await _context.DonXinNghis.FindAsync(idDon);
            if (don == null) return NotFound();

            if (don.TrangThai != "Chờ duyệt")
                return Conflict("Chỉ có thể xóa đơn đang ở trạng thái Chờ duyệt.");

            _context.DonXinNghis.Remove(don);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa đơn thành công." });
        }
    }
}