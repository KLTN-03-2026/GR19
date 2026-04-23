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
    [Route("api/app/quanly-nhatky")]
    [ApiController]
    [Authorize]
    public class QuanLyNhatKyController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyNhatKyController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetAffectedTables()
        {
            var tables = await _context.Set<NhatKyHeThong>()
                .Select(n => n.BangBiAnhHuong)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
            return Ok(tables);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] string? hanhDong, [FromQuery] string? bangBiAnhHuong, [FromQuery] string? keyword)
        {
            var query = _context.Set<NhatKyHeThong>()
                                .Include(n => n.NhanVien)
                                .Include(n => n.KhachHang)
                                .AsNoTracking()
                                .AsQueryable();

            if (tuNgay.HasValue) query = query.Where(n => n.ThoiGian >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(n => n.ThoiGian <= denNgay.Value.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrEmpty(hanhDong) && hanhDong != "Tất cả") query = query.Where(n => n.HanhDong.ToLower() == hanhDong.ToLower());
            if (!string.IsNullOrEmpty(bangBiAnhHuong) && bangBiAnhHuong != "Tất cả") query = query.Where(n => n.BangBiAnhHuong == bangBiAnhHuong);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(n =>
                    (n.NhanVien != null && n.NhanVien.HoTen.Contains(keyword)) ||
                    (n.KhachHang != null && n.KhachHang.HoTen.Contains(keyword)) || // ĐÃ FIX: HoTen
                    (n.DuLieuMoi != null && n.DuLieuMoi.Contains(keyword))
                );
            }

            var data = await query.OrderByDescending(n => n.ThoiGian).Take(500)
                .Select(n => new QuanLyNhatKyGridDto
                {
                    IdNhatKy = n.IdNhatKy,
                    NguoiThaoTac = n.NhanVien != null ? n.NhanVien.HoTen
                                 : (n.KhachHang != null ? n.KhachHang.HoTen : "Khách vãng lai / Hệ thống"), // ĐÃ FIX: HoTen
                    VaiTro = n.VaiTro ?? "Hệ thống",
                    HanhDong = n.HanhDong.ToUpper(),
                    BangBiAnhHuong = n.BangBiAnhHuong,
                    ThoiGian = n.ThoiGian,
                    DiaChiIP = n.DiaChiIP
                }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var n = await _context.Set<NhatKyHeThong>()
                                  .Include(x => x.NhanVien)
                                  .Include(x => x.KhachHang)
                                  .FirstOrDefaultAsync(x => x.IdNhatKy == id);

            if (n == null) return NotFound("Không tìm thấy nhật ký.");

            var detail = new QuanLyNhatKyDetailDto
            {
                IdNhatKy = n.IdNhatKy,
                NguoiThaoTac = n.NhanVien != null ? n.NhanVien.HoTen
                             : (n.KhachHang != null ? n.KhachHang.HoTen : "Khách vãng lai / Hệ thống"), // ĐÃ FIX: HoTen
                VaiTro = n.VaiTro ?? "Hệ thống",
                HanhDong = n.HanhDong.ToUpper(),
                BangBiAnhHuong = n.BangBiAnhHuong,
                ThoiGian = n.ThoiGian,
                DiaChiIP = n.DiaChiIP,
                KhoaChinh = n.KhoaChinh,
                DuLieuCu = n.DuLieuCu,
                DuLieuMoi = n.DuLieuMoi
            };

            return Ok(detail);
        }
    }
}