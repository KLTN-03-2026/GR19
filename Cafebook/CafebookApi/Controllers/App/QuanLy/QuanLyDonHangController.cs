using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-donhang")]
    [ApiController]
    public class QuanLyDonHangController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyDonHangController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] string? trangThai, [FromQuery] string? search)
        {
            var query = _context.HoaDons
                .Include(h => h.Ban)
                .Include(h => h.NhanVienTao)
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVienGiaoHang)
                .AsNoTracking();

            if (tuNgay.HasValue)
            {
                var from = tuNgay.Value.Date;
                query = query.Where(h => h.ThoiGianTao >= from);
            }
            if (denNgay.HasValue)
            {
                var to = denNgay.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(h => h.ThoiGianTao <= to);
            }
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            if (!string.IsNullOrEmpty(search))
            {
                bool isNumeric = int.TryParse(search, out int idSearch);
                query = query.Where(h => (isNumeric && h.IdHoaDon == idSearch) || (h.SoDienThoaiGiaoHang != null && h.SoDienThoaiGiaoHang.Contains(search)));
            }

            var data = await query.OrderByDescending(h => h.ThoiGianTao).Select(h => new QuanLyDonHangGridDto
            {
                IdHoaDon = h.IdHoaDon,
                ThoiGianTao = h.ThoiGianTao, // Đã bỏ ?? DateTime.Now
                TenBan = h.Ban != null ? h.Ban.SoBan : "Mang đi",
                NhanVien = h.NhanVienTao != null ? h.NhanVienTao.HoTen : "Hệ thống",
                KhachHang = h.KhachHang != null ? h.KhachHang.HoTen : "Khách lẻ",
                TongTien = h.ThanhTien, // Đã bỏ ?? 0
                LoaiHoaDon = h.LoaiHoaDon ?? "Tại quán",
                TrangThai = h.TrangThai, // Đã bỏ ?? ""
                TrangThaiGiaoHang = h.TrangThaiGiaoHang ?? ""
            }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var h = await _context.HoaDons
                .Include(x => x.Ban)
                .Include(x => x.NhanVienTao)
                .Include(x => x.KhachHang)
                .Include(x => x.NhanVienGiaoHang)
                .Include(x => x.ChiTietHoaDons).ThenInclude(c => c.SanPham)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdHoaDon == id);

            if (h == null) return NotFound();

            var dto = new QuanLyDonHangDetailDto
            {
                IdHoaDon = h.IdHoaDon,
                ThoiGianTao = h.ThoiGianTao, // Đã bỏ ?? DateTime.Now
                TenBan = h.Ban != null ? h.Ban.SoBan : "Mang đi",
                NhanVien = h.NhanVienTao != null ? h.NhanVienTao.HoTen : "Hệ thống",
                KhachHang = h.KhachHang != null ? h.KhachHang.HoTen : "Khách lẻ",
                TongTien = h.ThanhTien, // Đã bỏ ?? 0
                GiamGia = h.GiamGia, // Đã bỏ ?? 0
                PhuThu = h.TongPhuThu, // Đã bỏ ?? 0
                LoaiHoaDon = h.LoaiHoaDon ?? "Tại quán",
                TrangThai = h.TrangThai, // Đã bỏ ?? ""
                GhiChu = h.GhiChu ?? "",
                TrangThaiGiaoHang = h.TrangThaiGiaoHang ?? "",
                NguoiGiaoHang = h.NhanVienGiaoHang != null ? h.NhanVienGiaoHang.HoTen : "",
                DiaChiGiaoHang = h.DiaChiGiaoHang ?? "",
                SoDienThoaiGiaoHang = h.SoDienThoaiGiaoHang ?? "",
                ChiTiet = h.ChiTietHoaDons.Select(c => new QuanLyChiTietDonHangDto
                {
                    TenSanPham = c.SanPham != null ? c.SanPham.TenSanPham : "SP Đã Xóa",
                    SoLuong = c.SoLuong,
                    DonGia = c.DonGia,
                    GhiChu = c.GhiChu ?? ""
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] QuanLyDonHangUpdateStatusDto dto)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == id);
            if (hoaDon == null) return NotFound();

            if (hoaDon.TrangThai == "Đã thanh toán") return Conflict("Không thể cập nhật trạng thái cho hóa đơn đã thanh toán.");

            if (dto.TrangThai == "Hủy")
            {
                hoaDon.TrangThai = "Đã hủy";
                if (hoaDon.Ban != null) hoaDon.Ban.TrangThai = "Trống";
            }
            else return BadRequest("Trạng thái cập nhật không hợp lệ.");

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}