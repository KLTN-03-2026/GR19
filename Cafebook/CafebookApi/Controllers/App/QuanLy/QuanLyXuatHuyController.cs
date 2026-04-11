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
    [Route("api/app/quanly-xuathuy")]
    [ApiController]
    public class QuanLyXuatHuyController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyXuatHuyController(CafebookDbContext context) { _context = context; }

        [HttpGet("lookup-nl")]
        public async Task<IActionResult> GetNguyenLieu() => Ok(await _context.NguyenLieus.Select(n => new LookupXuatHuyDto { Id = n.IdNguyenLieu, Ten = n.TenNguyenLieu }).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PhieuXuatHuys.Include(p => p.NhanVienXuat).AsNoTracking()
                .Select(p => new QuanLyXuatHuyGridDto
                {
                    IdPhieuXuatHuy = p.IdPhieuXuatHuy,
                    ThoiGianTao = p.NgayXuatHuy, // Đã sửa thành NgayXuatHuy
                    TenNhanVien = p.NhanVienXuat != null ? p.NhanVienXuat.HoTen : "Hệ thống", // Đã sửa thành NhanVienXuat
                    LyDoHuy = p.LyDoXuatHuy // Đã sửa thành LyDoXuatHuy
                }).OrderByDescending(p => p.ThoiGianTao).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.PhieuXuatHuys.Include(x => x.ChiTietXuatHuys).ThenInclude(c => c.NguyenLieu).AsNoTracking().FirstOrDefaultAsync(x => x.IdPhieuXuatHuy == id);
            if (p == null) return NotFound();

            var dto = new QuanLyXuatHuyDetailDto
            {
                IdPhieuXuatHuy = p.IdPhieuXuatHuy,
                LyDoHuy = p.LyDoXuatHuy,
                ChiTiet = p.ChiTietXuatHuys.Select(c => new QuanLyChiTietXuatHuyDto
                {
                    IdNguyenLieu = c.IdNguyenLieu,
                    TenNguyenLieu = c.NguyenLieu!.TenNguyenLieu,
                    SoLuong = c.SoLuong, // Đã sửa thành SoLuong
                    LyDoChiTiet = "" // DB không có cột lý do chi tiết, trả về chuỗi rỗng
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyXuatHuySaveDto dto)
        {
            if (!dto.ChiTiet.Any()) return BadRequest("Phiếu xuất hủy phải có ít nhất 1 nguyên liệu.");

            var userIdClaim = User.FindFirst("IdNhanVien")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int idNhanVien = int.TryParse(userIdClaim, out int uid) ? uid : 1;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phieuHuy = new PhieuXuatHuy
                {
                    NgayXuatHuy = DateTime.Now,
                    IdNhanVienXuat = idNhanVien,
                    LyDoXuatHuy = dto.LyDoHuy,
                    TongGiaTriHuy = 0 // Tạm thời để 0 do DTO không truyền lên Đơn giá vốn
                };

                _context.PhieuXuatHuys.Add(phieuHuy);
                await _context.SaveChangesAsync(); // Để sinh IdPhieuXuatHuy

                foreach (var ct in dto.ChiTiet)
                {
                    _context.ChiTietXuatHuys.Add(new ChiTietXuatHuy
                    {
                        IdPhieuXuatHuy = phieuHuy.IdPhieuXuatHuy,
                        IdNguyenLieu = ct.IdNguyenLieu,
                        SoLuong = ct.SoLuong,
                        DonGiaVon = 0 // Entity yêu cầu có DonGiaVon, set mặc định là 0
                    });

                    // TRỪ TỒN KHO
                    var nl = await _context.NguyenLieus.FindAsync(ct.IdNguyenLieu);
                    if (nl != null)
                    {
                        if (nl.TonKho < ct.SoLuong)
                        {
                            throw new Exception($"Số lượng hủy vượt quá tồn kho thực tế của '{nl.TenNguyenLieu}'. Tồn hiện tại: {nl.TonKho}");
                        }
                        nl.TonKho -= ct.SoLuong;
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