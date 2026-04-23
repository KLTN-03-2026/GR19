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
    [Route("api/app/quanly-khuyenmai")]
    [ApiController]
    [Authorize]
    public class QuanLyKhuyenMaiController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyKhuyenMaiController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetKhuyenMaiFilters()
        {
            var sanPhams = await _context.Set<SanPham>()
                .Where(sp => sp.TrangThaiKinhDoanh == true)
                .OrderBy(sp => sp.TenSanPham)
                .Select(sp => new QuanLyKhuyenMaiLookupDto { Id = sp.IdSanPham, Ten = sp.TenSanPham })
                .ToListAsync();
            return Ok(sanPhams);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? maKhuyenMai, [FromQuery] string? trangThai)
        {
            var today = DateTime.Now.Date;

            // Auto-update hết hạn
            var hetHanList = await _context.Set<KhuyenMai>().Where(km => km.TrangThai != "Hết hạn" && km.NgayKetThuc < today).ToListAsync();
            if (hetHanList.Any())
            {
                foreach (var km in hetHanList) km.TrangThai = "Hết hạn";
                await _context.SaveChangesAsync();
            }

            var query = _context.Set<KhuyenMai>().AsNoTracking();
            if (!string.IsNullOrEmpty(maKhuyenMai)) query = query.Where(km => km.MaKhuyenMai.Contains(maKhuyenMai));
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả") query = query.Where(km => km.TrangThai == trangThai);

            // Format Giảm Giá trực tiếp trên Server để UI dễ hiển thị
            var result = await query.OrderByDescending(km => km.IdKhuyenMai)
                .Select(km => new QuanLyKhuyenMaiGridDto
                {
                    IdKhuyenMai = km.IdKhuyenMai,
                    MaKhuyenMai = km.MaKhuyenMai,
                    TenChuongTrinh = km.TenChuongTrinh,
                    LoaiGiamGia = km.LoaiGiamGia,
                    GiaTriGiam = km.LoaiGiamGia == "PhanTram" ? $"{km.GiaTriGiam:0.##}%" : $"{km.GiaTriGiam:N0}đ",
                    GiamToiDa = km.GiamToiDa,
                    NgayBatDau = km.NgayBatDau,
                    NgayKetThuc = km.NgayKetThuc,
                    SoLuongConLai = km.SoLuongConLai,
                    TrangThai = km.TrangThai
                }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var km = await _context.Set<KhuyenMai>().FindAsync(id);
            if (km == null) return NotFound();

            return Ok(new QuanLyKhuyenMaiSaveDto
            {
                IdKhuyenMai = km.IdKhuyenMai,
                MaKhuyenMai = km.MaKhuyenMai,
                TenChuongTrinh = km.TenChuongTrinh,
                MoTa = km.MoTa,
                LoaiGiamGia = km.LoaiGiamGia,
                GiaTriGiam = km.GiaTriGiam,
                GiamToiDa = km.GiamToiDa,
                HoaDonToiThieu = km.HoaDonToiThieu,
                IdSanPhamApDung = km.IdSanPhamApDung,
                NgayBatDau = km.NgayBatDau,
                NgayKetThuc = km.NgayKetThuc,
                NgayTrongTuan = km.NgayTrongTuan,
                GioBatDau = km.GioBatDau?.ToString(@"hh\:mm"),
                GioKetThuc = km.GioKetThuc?.ToString(@"hh\:mm"),
                SoLuongConLai = km.SoLuongConLai,
                DieuKienApDung = km.DieuKienApDung,
                TrangThai = km.TrangThai
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyKhuyenMaiSaveDto dto)
        {
            if (await _context.Set<KhuyenMai>().AnyAsync(km => km.MaKhuyenMai == dto.MaKhuyenMai))
                return BadRequest($"Mã khuyến mãi '{dto.MaKhuyenMai}' đã tồn tại.");

            var km = new KhuyenMai
            {
                MaKhuyenMai = dto.MaKhuyenMai,
                TenChuongTrinh = dto.TenChuongTrinh,
                MoTa = dto.MoTa,
                LoaiGiamGia = dto.LoaiGiamGia,
                GiaTriGiam = dto.GiaTriGiam,
                GiamToiDa = dto.GiamToiDa > 0 ? dto.GiamToiDa : null,
                HoaDonToiThieu = dto.HoaDonToiThieu > 0 ? dto.HoaDonToiThieu : null,
                IdSanPhamApDung = dto.IdSanPhamApDung > 0 ? dto.IdSanPhamApDung : null,
                NgayBatDau = dto.NgayBatDau.Date,
                NgayKetThuc = dto.NgayKetThuc.Date,
                NgayTrongTuan = string.IsNullOrWhiteSpace(dto.NgayTrongTuan) ? null : dto.NgayTrongTuan,
                GioBatDau = TimeSpan.TryParse(dto.GioBatDau, out var tsStart) ? tsStart : null,
                GioKetThuc = TimeSpan.TryParse(dto.GioKetThuc, out var tsEnd) ? tsEnd : null,
                SoLuongConLai = dto.SoLuongConLai > 0 ? dto.SoLuongConLai : null,
                DieuKienApDung = dto.DieuKienApDung,
                TrangThai = "Hoạt động"
            };

            _context.Set<KhuyenMai>().Add(km);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyKhuyenMaiSaveDto dto)
        {
            if (id != dto.IdKhuyenMai) return BadRequest("ID không khớp.");
            var km = await _context.Set<KhuyenMai>().FindAsync(id);
            if (km == null) return NotFound();

            if (km.MaKhuyenMai != dto.MaKhuyenMai && await _context.Set<KhuyenMai>().AnyAsync(k => k.MaKhuyenMai == dto.MaKhuyenMai))
                return BadRequest($"Mã khuyến mãi '{dto.MaKhuyenMai}' đã tồn tại.");

            km.MaKhuyenMai = dto.MaKhuyenMai; km.TenChuongTrinh = dto.TenChuongTrinh; km.MoTa = dto.MoTa;
            km.LoaiGiamGia = dto.LoaiGiamGia; km.GiaTriGiam = dto.GiaTriGiam;
            km.GiamToiDa = dto.GiamToiDa > 0 ? dto.GiamToiDa : null;
            km.HoaDonToiThieu = dto.HoaDonToiThieu > 0 ? dto.HoaDonToiThieu : null;
            km.IdSanPhamApDung = dto.IdSanPhamApDung > 0 ? dto.IdSanPhamApDung : null;
            km.NgayBatDau = dto.NgayBatDau.Date; km.NgayKetThuc = dto.NgayKetThuc.Date;
            km.NgayTrongTuan = string.IsNullOrWhiteSpace(dto.NgayTrongTuan) ? null : dto.NgayTrongTuan;
            km.GioBatDau = TimeSpan.TryParse(dto.GioBatDau, out var tsStart) ? tsStart : null;
            km.GioKetThuc = TimeSpan.TryParse(dto.GioKetThuc, out var tsEnd) ? tsEnd : null;
            km.SoLuongConLai = dto.SoLuongConLai > 0 ? dto.SoLuongConLai : null;
            km.DieuKienApDung = dto.DieuKienApDung;

            if (km.TrangThai != "Hết hạn") km.TrangThai = dto.TrangThai;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var km = await _context.Set<KhuyenMai>().FindAsync(id);
                if (km == null) return NotFound();

                _context.Set<KhuyenMai>().Remove(km);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException) { return Conflict("Không thể xóa. Khuyến mãi này đã được áp dụng trong Hóa Đơn Khách hàng."); }
        }

        [HttpPatch("togglestatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var km = await _context.Set<KhuyenMai>().FindAsync(id);
            if (km == null) return NotFound();

            if (km.TrangThai == "Hết hạn") return BadRequest("Không thể kích hoạt khuyến mãi đã hết hạn.");

            km.TrangThai = km.TrangThai == "Hoạt động" ? "Tạm dừng" : "Hoạt động";
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}