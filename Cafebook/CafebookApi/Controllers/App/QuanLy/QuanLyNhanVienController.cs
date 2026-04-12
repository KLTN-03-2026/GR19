using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils; // Kéo thư viện Utils
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NhanVienEntity = CafebookModel.Model.ModelEntities.NhanVien;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-nhanvien")]
    [ApiController]
    public class QuanLyNhanVienController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuanLyNhanVienController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            if (string.IsNullOrEmpty(_env.WebRootPath))
            {
                _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.NhanViens
                .Include(n => n.VaiTro)
                .Select(n => new QuanLyNhanVienGridDto
                {
                    IdNhanVien = n.IdNhanVien,
                    HoTen = n.HoTen,
                    TenDangNhap = n.TenDangNhap,
                    TenVaiTro = n.VaiTro.TenVaiTro,
                    LuongCoBan = n.LuongCoBan,
                    TrangThaiLamViec = n.TrangThaiLamViec,
                    SoDienThoai = n.SoDienThoai
                })
                .OrderByDescending(n => n.IdNhanVien)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return NotFound();

            var dto = new QuanLyNhanVienDetailDto
            {
                IdNhanVien = nv.IdNhanVien,
                HoTen = nv.HoTen,
                TenDangNhap = nv.TenDangNhap,
                IdVaiTro = nv.IdVaiTro, // ĐÃ XÓA '?? 0' Ở ĐÂY
                LuongCoBan = nv.LuongCoBan,
                TrangThaiLamViec = nv.TrangThaiLamViec,
                SoDienThoai = nv.SoDienThoai,
                Email = nv.Email,
                DiaChi = nv.DiaChi,
                NgayVaoLam = nv.NgayVaoLam,
                AnhDaiDienUrl = nv.AnhDaiDien
            };
            return Ok(dto);
        }

        [HttpGet("roles-lookup")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.VaiTros
                .Where(v => v.TenVaiTro == "Quản lý" || v.TenVaiTro == "Nhân viên")
                .Select(v => new RoleLookupDto
                {
                    Id = v.IdVaiTro,
                    Name = v.TenVaiTro
                }).ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] QuanLyNhanVienSaveRequestDto request, IFormFile? AnhDaiDienUpload)
        {
            if (string.IsNullOrWhiteSpace(request.HoTen)) return BadRequest("Họ tên không được để trống!");
            if (string.IsNullOrWhiteSpace(request.TenDangNhap)) return BadRequest("Tên đăng nhập không được để trống!");
            if (string.IsNullOrWhiteSpace(request.MatKhau)) return BadRequest("Mật khẩu không được để trống khi thêm mới!");
            if (string.IsNullOrWhiteSpace(request.SoDienThoai)) return BadRequest("Số điện thoại không được để trống!");
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest("Email không được để trống!");
            if (string.IsNullOrWhiteSpace(request.DiaChi)) return BadRequest("Địa chỉ không được để trống!");

            string tenDangNhap = request.TenDangNhap.Trim();
            string soDienThoai = request.SoDienThoai.Trim();
            string email = request.Email.Trim();

            if (await _context.NhanViens.AnyAsync(n => n.TenDangNhap == tenDangNhap)) return BadRequest("Tên đăng nhập này đã tồn tại trên hệ thống!");
            if (await _context.NhanViens.AnyAsync(n => n.SoDienThoai == soDienThoai)) return BadRequest("Số điện thoại này đã được cấp cho nhân viên khác!");
            if (await _context.NhanViens.AnyAsync(n => n.Email == email)) return BadRequest("Email này đã được cấp cho nhân viên khác!");

            var entity = new NhanVienEntity
            {
                HoTen = request.HoTen.Trim(),
                TenDangNhap = tenDangNhap,
                MatKhau = request.MatKhau,
                IdVaiTro = request.IdVaiTro,
                LuongCoBan = request.LuongCoBan,
                TrangThaiLamViec = request.TrangThaiLamViec,
                SoDienThoai = soDienThoai,
                Email = email,
                DiaChi = request.DiaChi.Trim(),
                NgayVaoLam = request.NgayVaoLam
            };

            // LƯU ẢNH CHUẨN SLUG
            if (AnhDaiDienUpload != null)
            {
                entity.AnhDaiDien = await SaveImageAsync(AnhDaiDienUpload, entity.HoTen);
            }

            _context.NhanViens.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Thêm nhân viên thành công" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] QuanLyNhanVienSaveRequestDto request, IFormFile? AnhDaiDienUpload)
        {
            var entity = await _context.NhanViens.FindAsync(id);
            if (entity == null) return NotFound("Không tìm thấy nhân viên");

            if (string.IsNullOrWhiteSpace(request.HoTen)) return BadRequest("Họ tên không được để trống!");
            if (string.IsNullOrWhiteSpace(request.TenDangNhap)) return BadRequest("Tên đăng nhập không được để trống!");
            if (string.IsNullOrWhiteSpace(request.SoDienThoai)) return BadRequest("Số điện thoại không được để trống!");
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest("Email không được để trống!");
            if (string.IsNullOrWhiteSpace(request.DiaChi)) return BadRequest("Địa chỉ không được để trống!");

            string tenDangNhap = request.TenDangNhap.Trim();
            string soDienThoai = request.SoDienThoai.Trim();
            string email = request.Email.Trim();

            if (await _context.NhanViens.AnyAsync(n => n.TenDangNhap == tenDangNhap && n.IdNhanVien != id)) return BadRequest("Tên đăng nhập này đã tồn tại trên hệ thống!");
            if (await _context.NhanViens.AnyAsync(n => n.SoDienThoai == soDienThoai && n.IdNhanVien != id)) return BadRequest("Số điện thoại này đã được cấp cho nhân viên khác!");
            if (await _context.NhanViens.AnyAsync(n => n.Email == email && n.IdNhanVien != id)) return BadRequest("Email này đã được cấp cho nhân viên khác!");

            entity.HoTen = request.HoTen.Trim();
            entity.TenDangNhap = tenDangNhap;
            entity.IdVaiTro = request.IdVaiTro;
            entity.LuongCoBan = request.LuongCoBan;
            entity.TrangThaiLamViec = request.TrangThaiLamViec;
            entity.SoDienThoai = soDienThoai;
            entity.Email = email;
            entity.DiaChi = request.DiaChi.Trim();
            entity.NgayVaoLam = request.NgayVaoLam;

            if (!string.IsNullOrWhiteSpace(request.MatKhau))
            {
                entity.MatKhau = request.MatKhau;
            }

            // XỬ LÝ ẢNH
            if (request.XoaAnhDaiDien && !string.IsNullOrEmpty(entity.AnhDaiDien))
            {
                DeleteOldImage(entity.AnhDaiDien);
                entity.AnhDaiDien = null;
            }
            else if (AnhDaiDienUpload != null)
            {
                if (!string.IsNullOrEmpty(entity.AnhDaiDien)) DeleteOldImage(entity.AnhDaiDien);
                entity.AnhDaiDien = await SaveImageAsync(AnhDaiDienUpload, entity.HoTen);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cập nhật thành công" });
        }

        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var entity = await _context.NhanViens.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TrangThaiLamViec = newStatus;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.NhanViens.FindAsync(id);
            if (entity == null) return NotFound();

            if (await _context.LichLamViecs.AnyAsync(l => l.IdNhanVien == id) ||
                await _context.HoaDons.AnyAsync(h => h.IdNhanVien == id))
            {
                return Conflict("Không thể xóa! Nhân viên này đã có dữ liệu Hóa đơn hoặc Lịch làm việc. Vui lòng chuyển trạng thái 'Nghỉ việc'.");
            }

            if (!string.IsNullOrEmpty(entity.AnhDaiDien)) DeleteOldImage(entity.AnhDaiDien);

            _context.NhanViens.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đã xóa nhân viên" });
        }

        // ĐÃ NÂNG CẤP LƯU THEO TÊN
        private async Task<string> SaveImageAsync(IFormFile file, string hoTen)
        {
            var folderPath = Path.Combine(_env.WebRootPath, "images", "avatars", "avatarNV");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string extension = Path.GetExtension(file.FileName);
            string safeName = hoTen.GenerateSlug();
            string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{safeName}{extension}";

            var filePath = Path.Combine(folderPath, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"{HinhAnhPaths.UrlAvatarNV}/{uniqueFileName}";
        }

        private void DeleteOldImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;
            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }
    }
}