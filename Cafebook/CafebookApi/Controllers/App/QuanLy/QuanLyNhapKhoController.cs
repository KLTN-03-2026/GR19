using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Utils; // <-- Thêm dòng này để gọi HinhAnhPaths và SlugifyUtil
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-nhapkho")]
    [ApiController]
    [Authorize]
    public class QuanLyNhapKhoController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuanLyNhapKhoController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("lookup-ncc")]
        public async Task<IActionResult> GetNhaCungCap() => Ok(await _context.NhaCungCaps.Select(n => new LookupNhapKhoDto { Id = n.IdNhaCungCap, Ten = n.TenNhaCungCap }).ToListAsync());

        [HttpGet("lookup-nl")]
        public async Task<IActionResult> GetNguyenLieu() => Ok(await _context.NguyenLieus.Select(n => new LookupNhapKhoDto { Id = n.IdNguyenLieu, Ten = n.TenNguyenLieu }).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.PhieuNhapKhos.Include(p => p.NhaCungCap).Include(p => p.NhanVien).AsNoTracking()
                .Select(p => new QuanLyNhapKhoGridDto
                {
                    IdPhieuNhap = p.IdPhieuNhapKho,
                    ThoiGianTao = p.NgayNhap,
                    TenNhaCungCap = p.NhaCungCap != null ? p.NhaCungCap.TenNhaCungCap : "Khách lẻ",
                    TenNhanVien = p.NhanVien != null ? p.NhanVien.HoTen : "Hệ thống",
                    TongTien = p.TongTien,
                    TrangThai = p.TrangThai
                }).OrderByDescending(p => p.ThoiGianTao).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.PhieuNhapKhos.Include(x => x.ChiTietNhapKhos).ThenInclude(c => c.NguyenLieu).AsNoTracking().FirstOrDefaultAsync(x => x.IdPhieuNhapKho == id);
            if (p == null) return NotFound();

            decimal tienHang = p.ChiTietNhapKhos.Sum(c => c.SoLuongNhap * c.DonGiaNhap);
            decimal giamGia = tienHang - p.TongTien;

            var dto = new QuanLyNhapKhoDetailDto
            {
                IdPhieuNhap = p.IdPhieuNhapKho,
                IdNhaCungCap = p.IdNhaCungCap ?? 0,
                GhiChu = p.GhiChu,
                HoaDonDinhKem = p.HoaDonDinhKem, // Lấy đường dẫn file từ DB
                TienHang = tienHang,
                GiamGia = giamGia,
                TongTien = p.TongTien,
                ChiTiet = p.ChiTietNhapKhos.Select(c => new QuanLyChiTietNhapKhoDto
                {
                    IdNguyenLieu = c.IdNguyenLieu,
                    TenNguyenLieu = c.NguyenLieu!.TenNguyenLieu,
                    SoLuong = c.SoLuongNhap,
                    DonGiaNhap = c.DonGiaNhap
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyNhapKhoSaveDto dto)
        {
            if (!dto.ChiTiet.Any()) return BadRequest("Phiếu nhập phải có ít nhất 1 nguyên liệu.");

            var userIdClaim = User.FindFirst("IdNhanVien")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int idNhanVien = int.TryParse(userIdClaim, out int uid) ? uid : 1;

            // ========================================================
            // XỬ LÝ LƯU FILE ĐÍNH KÈM VÀO THƯ MỤC CHUẨN CỦA HỆ THỐNG
            // ========================================================
            string? hoaDonDinhKemPath = null;
            if (!string.IsNullOrEmpty(dto.FileDinhKemBase64) && !string.IsNullOrEmpty(dto.TenFileDinhKem))
            {
                try
                {
                    // 1. Tạo thư mục vật lý: .../wwwroot/images/BuildNhapKho
                    string folderPath = Path.Combine(_env.WebRootPath, "images", "BuildNhapKho");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // 2. Dùng SlugifyUtil để làm sạch tên file (xóa dấu, dấu cách, ký tự lạ)
                    string extension = Path.GetExtension(dto.TenFileDinhKem);
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(dto.TenFileDinhKem);
                    string safeFileName = fileNameWithoutExt.GenerateSlug();

                    // 3. Đặt tên file chống trùng lặp
                    string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 4)}_{safeFileName}{extension}";
                    string filePath = Path.Combine(folderPath, uniqueFileName);

                    // 4. Lưu file vật lý
                    byte[] fileBytes = Convert.FromBase64String(dto.FileDinhKemBase64);
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    // 5. Lưu đường dẫn chuẩn vào DB sử dụng HinhAnhPaths
                    hoaDonDinhKemPath = $"{HinhAnhPaths.UrlBuildnhapkho}/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return BadRequest("Lỗi khi lưu file hóa đơn đính kèm: " + ex.Message);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal tienHang = dto.ChiTiet.Sum(c => c.SoLuong * c.DonGiaNhap);
                var phieuNhap = new PhieuNhapKho
                {
                    NgayNhap = DateTime.Now,
                    IdNhaCungCap = dto.IdNhaCungCap > 0 ? dto.IdNhaCungCap : null,
                    IdNhanVien = idNhanVien,
                    GhiChu = dto.GhiChu,
                    HoaDonDinhKem = hoaDonDinhKemPath, // LƯU ĐƯỜNG DẪN VÀO DB
                    TrangThai = "Hoàn thành",
                    TongTien = tienHang - dto.GiamGia
                };

                _context.PhieuNhapKhos.Add(phieuNhap);
                await _context.SaveChangesAsync();

                foreach (var ct in dto.ChiTiet)
                {
                    _context.ChiTietNhapKhos.Add(new ChiTietNhapKho
                    {
                        IdPhieuNhapKho = phieuNhap.IdPhieuNhapKho,
                        IdNguyenLieu = ct.IdNguyenLieu,
                        SoLuongNhap = ct.SoLuong,
                        DonGiaNhap = ct.DonGiaNhap
                    });

                    // CỘNG TỒN KHO
                    var nl = await _context.NguyenLieus.FindAsync(ct.IdNguyenLieu);
                    if (nl != null) nl.TonKho += ct.SoLuong;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); return StatusCode(500, ex.Message);
            }
        }
    }
}