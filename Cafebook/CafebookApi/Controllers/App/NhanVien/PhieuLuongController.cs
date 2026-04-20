using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/phieuluong")]
    [ApiController]
    public class PhieuLuongController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public PhieuLuongController(CafebookDbContext context) { _context = context; }

        [HttpGet("list/{idNhanVien}")]
        public async Task<IActionResult> GetDanhSachPhieuLuong(int idNhanVien)
        {
            try
            {
                if (idNhanVien == 0) return BadRequest("Thiếu ID Nhân Viên");

                var list = await _context.PhieuLuongs.AsNoTracking()
                    .Where(pl => pl.IdNhanVien == idNhanVien && (pl.TrangThai == "Đã phát" || pl.TrangThai == "Đã chốt"))
                    .OrderByDescending(pl => pl.Nam).ThenByDescending(pl => pl.Thang)
                    .Select(pl => new PhieuLuongItemDto
                    {
                        IdPhieuLuong = pl.IdPhieuLuong,
                        Thang = pl.Thang,
                        Nam = pl.Nam,
                        ThucLanh = pl.ThucLanh,
                        TrangThai = pl.TrangThai
                    }).ToListAsync();

                return Ok(new PhieuLuongViewDto { DanhSachPhieuLuong = list });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpGet("detail/{idNhanVien}/{idPhieuLuong}")]
        public async Task<IActionResult> GetChiTietPhieuLuong(int idNhanVien, int idPhieuLuong)
        {
            try
            {
                // XÓA BỎ Include NguoiPhat để tránh lỗi biên dịch CSDL
                var phieuLuong = await _context.PhieuLuongs.AsNoTracking()
                    .FirstOrDefaultAsync(pl => pl.IdPhieuLuong == idPhieuLuong && pl.IdNhanVien == idNhanVien);

                if (phieuLuong == null) return NotFound("Không tìm thấy phiếu lương.");

                var chiTietThuongPhat = await _context.PhieuThuongPhats.AsNoTracking()
                    .Where(ptp => ptp.IdPhieuLuong == idPhieuLuong)
                    .Select(ptp => new PhieuThuongPhatItemDto
                    {
                        NgayTao = ptp.NgayTao,
                        SoTien = ptp.SoTien,
                        LyDo = ptp.LyDo,
                        TenNguoiTao = "Quản lý" // Fix cứng để tránh lỗi biên dịch Navigation
                    }).ToListAsync();

                var chiTietDto = new PhieuLuongChiTietDto
                {
                    IdPhieuLuong = phieuLuong.IdPhieuLuong,
                    Thang = phieuLuong.Thang,
                    Nam = phieuLuong.Nam,
                    LuongCoBan = phieuLuong.LuongCoBan,
                    TongGioLam = phieuLuong.TongGioLam,
                    TienLuongTheoGio = phieuLuong.LuongCoBan * phieuLuong.TongGioLam,
                    TongTienThuong = phieuLuong.TienThuong ?? 0,
                    TongKhauTru = phieuLuong.KhauTru ?? 0,
                    ThucLanh = phieuLuong.ThucLanh,
                    TrangThai = phieuLuong.TrangThai,
                    NgayPhatLuong = phieuLuong.NgayPhatLuong,
                    TenNguoiPhat = "Quản lý", // Fix lỗi biên dịch
                    DanhSachThuong = chiTietThuongPhat.Where(ptp => ptp.SoTien > 0).ToList(),
                    DanhSachPhat = chiTietThuongPhat.Where(ptp => ptp.SoTien < 0).ToList()
                };

                return Ok(chiTietDto);
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }
    }
}