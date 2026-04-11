using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-kiemkho")]
    [ApiController]
    public class QuanLyKiemKhoController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyKiemKhoController(CafebookDbContext context) { _context = context; }

        [HttpGet("lookup-nl")]
        public async Task<IActionResult> GetNguyenLieuToKiem()
        {
            var data = await _context.NguyenLieus.AsNoTracking().Select(n => new QuanLyKiemKhoNguyenLieuDto
            {
                IdNguyenLieu = n.IdNguyenLieu,
                TenNguyenLieu = n.TenNguyenLieu,
                DonViTinh = n.DonViTinh,
                TonKhoHeThong = n.TonKho,
                TonKhoThucTe = n.TonKho
            }).OrderBy(n => n.TenNguyenLieu).ToListAsync();
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // FIX: Sử dụng NgayKiem
            var data = await _context.PhieuKiemKhos.Include(p => p.NhanVienKiem).AsNoTracking()
                .Select(p => new QuanLyKiemKhoGridDto
                {
                    IdPhieuKiemKho = p.IdPhieuKiemKho,
                    NgayKiem = p.NgayKiem, // Đã sửa lại thành NgayKiem
                    TenNhanVien = p.NhanVienKiem != null ? p.NhanVienKiem.HoTen : "Hệ thống"
                }).OrderByDescending(p => p.NgayKiem).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.PhieuKiemKhos.Include(x => x.NhanVienKiem).Include(x => x.ChiTietKiemKhos).ThenInclude(c => c.NguyenLieu).AsNoTracking().FirstOrDefaultAsync(x => x.IdPhieuKiemKho == id);
            if (p == null) return NotFound();

            var dto = new QuanLyKiemKhoDetailDto
            {
                IdPhieuKiemKho = p.IdPhieuKiemKho,
                NgayKiem = p.NgayKiem, // Đã sửa lại thành NgayKiem
                TenNhanVien = p.NhanVienKiem != null ? p.NhanVienKiem.HoTen : "Hệ thống",
                ChiTiet = p.ChiTietKiemKhos.Select(c => new QuanLyChiTietKiemKhoDto
                {
                    IdNguyenLieu = c.IdNguyenLieu,
                    TenNguyenLieu = c.NguyenLieu!.TenNguyenLieu,
                    DonViTinh = c.NguyenLieu.DonViTinh,
                    TonKhoHeThong = c.TonKhoHeThong,
                    TonKhoThucTe = c.TonKhoThucTe,
                    LyDoChenhLech = c.LyDoChenhLech ?? ""
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyKiemKhoSaveDto dto)
        {
            if (!dto.ChiTiet.Any()) return BadRequest("Phiếu kiểm kho không có dữ liệu.");

            var userIdClaim = User.FindFirst("IdNhanVien")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int idNhanVien = int.TryParse(userIdClaim, out int uid) ? uid : 1;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phieuKiem = new PhieuKiemKho
                {
                    NgayKiem = DateTime.Now, // Đã sửa lại thành NgayKiem
                    IdNhanVienKiem = idNhanVien
                };

                _context.PhieuKiemKhos.Add(phieuKiem);
                await _context.SaveChangesAsync();

                foreach (var ct in dto.ChiTiet)
                {
                    _context.ChiTietKiemKhos.Add(new ChiTietKiemKho
                    {
                        IdPhieuKiemKho = phieuKiem.IdPhieuKiemKho,
                        IdNguyenLieu = ct.IdNguyenLieu,
                        TonKhoHeThong = ct.TonKhoHeThong,
                        TonKhoThucTe = ct.TonKhoThucTe,
                        LyDoChenhLech = ct.LyDoChenhLech
                    });

                    if (ct.TonKhoHeThong != ct.TonKhoThucTe)
                    {
                        var nl = await _context.NguyenLieus.FindAsync(ct.IdNguyenLieu);
                        if (nl != null) nl.TonKho = ct.TonKhoThucTe;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
    }
}