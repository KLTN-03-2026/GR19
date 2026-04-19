using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/thongtincanhan")]
    [ApiController]
    public class ThongTinCaNhanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ThongTinCaNhanController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("me/{idNhanVien}")]
        public async Task<IActionResult> GetMyInfo(int idNhanVien)
        {
            var nhanVien = await _context.NhanViens.AsNoTracking()
                .Include(nv => nv.VaiTro)
                .FirstOrDefaultAsync(nv => nv.IdNhanVien == idNhanVien);

            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");

            var today = DateTime.Today;

            var lichHomNay = await _context.LichLamViecs.AsNoTracking()
                .Include(l => l.CaLamViec)
                .FirstOrDefaultAsync(l => l.IdNhanVien == idNhanVien && l.NgayLam.Date == today && l.TrangThai == "Đã duyệt");

            var lichThangNay = await _context.LichLamViecs.AsNoTracking()
                .Include(l => l.CaLamViec)
                .Where(l => l.IdNhanVien == idNhanVien && l.NgayLam.Month == today.Month && l.NgayLam.Year == today.Year && l.TrangThai == "Đã duyệt")
                .OrderBy(l => l.NgayLam).ThenBy(l => l.CaLamViec.GioBatDau)
                .Select(l => new LichLamViecChiTietDto
                {
                    IdLichLamViec = l.IdLichLamViec,
                    NgayLam = l.NgayLam,
                    TenCa = l.CaLamViec.TenCa,
                    GioBatDau = l.CaLamViec.GioBatDau,
                    GioKetThuc = l.CaLamViec.GioKetThuc
                }).ToListAsync();

            var soLanNghi = await _context.DonXinNghis.AsNoTracking()
                .CountAsync(d => d.IdNhanVien == idNhanVien && d.NgayBatDau.Month == today.Month && d.NgayBatDau.Year == today.Year && d.TrangThai == "Đã duyệt");

            var result = new ThongTinCaNhanViewDto
            {
                NhanVien = new NhanVienInfoDto
                {
                    IdNhanVien = nhanVien.IdNhanVien,
                    HoTen = nhanVien.HoTen,
                    SoDienThoai = nhanVien.SoDienThoai,
                    Email = nhanVien.Email,
                    DiaChi = nhanVien.DiaChi,
                    NgayVaoLam = nhanVien.NgayVaoLam,
                    LuongCoBan = nhanVien.LuongCoBan,
                    TenDangNhap = nhanVien.TenDangNhap,
                    AnhDaiDien = nhanVien.AnhDaiDien,
                    TenVaiTro = nhanVien.VaiTro.TenVaiTro
                },
                SoLanXinNghiThangNay = soLanNghi,
                LichLamViecThangNay = lichThangNay,
                LichLamViecHomNay = lichHomNay != null ? new LichLamViecDto
                {
                    TenCa = lichHomNay.CaLamViec.TenCa,
                    GioBatDau = lichHomNay.CaLamViec.GioBatDau,
                    GioKetThuc = lichHomNay.CaLamViec.GioKetThuc
                } : null
            };

            return Ok(result);
        }

        [HttpGet("leave-history/{idNhanVien}")]
        public async Task<IActionResult> GetLeaveHistory(int idNhanVien)
        {
            var history = await _context.DonXinNghis.AsNoTracking()
                .Where(d => d.IdNhanVien == idNhanVien)
                .OrderByDescending(d => d.NgayBatDau)
                .Select(d => new DonXinNghiDto
                {
                    IdDonXinNghi = d.IdDonXinNghi,
                    LoaiDon = d.LoaiDon,
                    LyDo = d.LyDo,
                    NgayBatDau = d.NgayBatDau,
                    NgayKetThuc = d.NgayKetThuc,
                    TrangThai = d.TrangThai,
                    GhiChuPheDuyet = d.GhiChuPheDuyet
                })
                .ToListAsync();
            return Ok(history);
        }

        [HttpPost("submit-leave/{idNhanVien}")]
        public async Task<IActionResult> SubmitLeave(int idNhanVien, [FromBody] DonXinNghiRequestDto req)
        {
            if (req.NgayBatDau.Date < DateTime.Today) return BadRequest("Ngày bắt đầu không được trong quá khứ.");
            if (req.NgayKetThuc.Date < req.NgayBatDau.Date) return BadRequest("Ngày kết thúc không hợp lệ.");

            var overlappingLeave = await _context.DonXinNghis.AsNoTracking()
                .Where(d => d.IdNhanVien == idNhanVien &&
                            d.TrangThai != "Đã hủy" && d.TrangThai != "Từ chối" &&
                            d.NgayBatDau.Date <= req.NgayKetThuc.Date &&
                            d.NgayKetThuc.Date >= req.NgayBatDau.Date)
                .FirstOrDefaultAsync();

            if (overlappingLeave != null)
            {
                return Conflict($"Bạn đã có đơn xin nghỉ từ ngày {overlappingLeave.NgayBatDau:dd/MM/yyyy} đến {overlappingLeave.NgayKetThuc:dd/MM/yyyy}. Vui lòng kiểm tra lại lịch sử.");
            }

            var don = new DonXinNghi
            {
                IdNhanVien = idNhanVien,
                LoaiDon = req.LoaiDon,
                LyDo = req.LyDo,
                NgayBatDau = req.NgayBatDau,
                NgayKetThuc = req.NgayKetThuc,
                TrangThai = "Chờ duyệt"
            };
            _context.DonXinNghis.Add(don);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Gửi đơn xin nghỉ thành công." });
        }

        [HttpPut("update-info/{idNhanVien}")]
        public async Task<IActionResult> UpdateInfo(int idNhanVien, [FromBody] CapNhatThongTinDto req)
        {
            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound();

            nhanVien.HoTen = req.HoTen;
            nhanVien.SoDienThoai = req.SoDienThoai;
            nhanVien.Email = req.Email;
            nhanVien.DiaChi = req.DiaChi;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin thành công!" });
        }

        [HttpPost("upload-avatar/{idNhanVien}")]
        public async Task<IActionResult> UploadAvatar(int idNhanVien, Microsoft.AspNetCore.Http.IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0) return BadRequest("Chưa chọn file.");

            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound();

            if (!string.IsNullOrEmpty(nhanVien.AnhDaiDien))
            {
                try
                {
                    string oldPhysicalPath = Path.Combine(_env.WebRootPath, nhanVien.AnhDaiDien.TrimStart('/'));
                    oldPhysicalPath = oldPhysicalPath.Replace('/', Path.DirectorySeparatorChar); 

                    if (System.IO.File.Exists(oldPhysicalPath))
                    {
                        System.IO.File.Delete(oldPhysicalPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xóa ảnh cũ: {ex.Message}");
                }
            }

            string folderUrl = HinhAnhPaths.UrlAvatarNV; 
            string physicalFolder = Path.Combine(_env.WebRootPath, folderUrl.TrimStart('/'));

            if (!Directory.Exists(physicalFolder)) Directory.CreateDirectory(physicalFolder);

            string ext = Path.GetExtension(avatarFile.FileName);
            string slugName = nhanVien.HoTen.GenerateSlug();
            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{slugName}{ext}";

            var physicalPath = Path.Combine(physicalFolder, fileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            nhanVien.AnhDaiDien = $"{folderUrl}/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tải ảnh lên thành công!" });
        }

        [HttpPost("change-password/{idNhanVien}")]
        public async Task<IActionResult> ChangePassword(int idNhanVien, [FromBody] DoiMatKhauRequestDto req)
        {
            if (string.IsNullOrEmpty(req.MatKhauCu) || string.IsNullOrEmpty(req.MatKhauMoi))
                return BadRequest("Mật khẩu không được để trống.");

            // Validation Backend
            if (req.MatKhauMoi.Length < 6)
                return BadRequest("Mật khẩu mới phải có ít nhất 6 ký tự.");

            if (req.MatKhauCu == req.MatKhauMoi)
                return BadRequest("Mật khẩu mới không được trùng với mật khẩu cũ.");

            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound();

            if (nhanVien.MatKhau != req.MatKhauCu) return BadRequest("Mật khẩu cũ không chính xác.");

            nhanVien.MatKhau = req.MatKhauMoi;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
    }
}