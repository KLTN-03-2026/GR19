// Tệp: CafebookApi/Controllers/App/NhanVien/CheBienController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/chebien")]
    [ApiController]
    public class CheBienController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public CheBienController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("load")]
        public async Task<IActionResult> LoadCheBienItems()
        {
            // NÂNG CẤP: Thêm AsNoTracking() để tối ưu tốc độ tải dữ liệu
            var items = await _context.TrangThaiCheBiens
                .AsNoTracking()
                .Where(cb => cb.TrangThai == "Chờ làm" || cb.TrangThai == "Đang làm")
                .OrderBy(cb => cb.ThoiGianGoi)
                .Select(cb => new CheBienItemDto
                {
                    IdTrangThaiCheBien = cb.IdTrangThaiCheBien,
                    IdSanPham = cb.IdSanPham,
                    TenMon = cb.TenMon,
                    SoLuong = cb.SoLuong,
                    SoBan = cb.SoBan,
                    GhiChu = cb.GhiChu,
                    TrangThai = cb.TrangThai,
                    ThoiGianGoi = cb.ThoiGianGoi,
                    NhomIn = cb.NhomIn ?? "Bếp"
                })
                 .ToListAsync();

            return Ok(items);
        }

        // NÂNG CẤP: Đổi thành HttpPut để đồng bộ với PutAsJsonAsync trên WPF
        [HttpPut("start/{idTrangThaiCheBien}")]
        public async Task<IActionResult> StartItem(int idTrangThaiCheBien)
        {
            var item = await _context.TrangThaiCheBiens.FindAsync(idTrangThaiCheBien);
            if (item == null) return NotFound("Không tìm thấy món.");

            if (item.TrangThai == "Chờ làm")
            {
                item.TrangThai = "Đang làm";
                item.ThoiGianBatDau = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã bắt đầu làm món." });
            }
            return Conflict("Món đã được làm hoặc đã hoàn thành.");
        }

        // NÂNG CẤP: Đổi thành HttpPut để đồng bộ với PutAsJsonAsync trên WPF
        [HttpPut("complete/{idTrangThaiCheBien}")]
        public async Task<IActionResult> CompleteItem(int idTrangThaiCheBien)
        {
            var item = await _context.TrangThaiCheBiens.FindAsync(idTrangThaiCheBien);
            if (item == null) return NotFound("Không tìm thấy món.");

            if (item.TrangThai == "Đang làm")
            {
                item.TrangThai = "Hoàn thành";
                item.ThoiGianHoanThanh = DateTime.Now;
                await _context.SaveChangesAsync();

                // CẬP NHẬT TRẠNG THÁI GIAO HÀNG
                await UpdateGiaoHangStatusIfCompleted(item.IdHoaDon);

                return Ok(new { message = "Đã hoàn thành món." });
            }
            return Conflict("Món này chưa được bắt đầu làm.");
        }

        private async Task UpdateGiaoHangStatusIfCompleted(int idHoaDon)
        {
            try
            {
                bool allDone = !await _context.TrangThaiCheBiens
                    .AnyAsync(cb => cb.IdHoaDon == idHoaDon && cb.TrangThai != "Hoàn thành");

                if (allDone)
                {
                    var hoaDon = await _context.HoaDons.FindAsync(idHoaDon);

                    if (hoaDon != null &&
                        hoaDon.LoaiHoaDon == "Giao hàng" &&
                        hoaDon.TrangThaiGiaoHang == "Đang chuẩn bị")
                    {
                        hoaDon.TrangThaiGiaoHang = "Chờ lấy hàng";
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateGiaoHangStatusIfCompleted Error]: {ex.Message}");
            }
        }

        [HttpGet("congthuc/{idSanPham}")]
        public async Task<IActionResult> GetCongThuc(int idSanPham)
        {
            // NÂNG CẤP: Thêm AsNoTracking() 
            var items = await _context.DinhLuongs
                .AsNoTracking()
                .Where(d => d.IdSanPham == idSanPham)
                .Include(d => d.NguyenLieu)
                .Include(d => d.DonViSuDung)
                .Select(d => new CongThucItemDto
                {
                    TenNguyenLieu = d.NguyenLieu.TenNguyenLieu,
                    SoLuongSuDung = d.SoLuongSuDung,
                    TenDonVi = d.DonViSuDung.TenDonVi
                })
                .ToListAsync();

            return Ok(items);
        }

        // =========================================================================
        // API ĐỘC LẬP: Lấy lịch sử chế biến trong ngày
        // =========================================================================
        [HttpGet("history")]
        public async Task<IActionResult> GetHistoryToday()
        {
            try
            {
                var today = DateTime.Today;
                var items = await _context.TrangThaiCheBiens
                    .AsNoTracking()
                    .Where(cb => cb.TrangThai == "Hoàn thành" &&
                                 cb.ThoiGianHoanThanh.HasValue &&
                                 cb.ThoiGianHoanThanh.Value.Date == today)
                    .OrderByDescending(cb => cb.ThoiGianHoanThanh)
                    .Select(cb => new CheBienItemDto
                    {
                        IdTrangThaiCheBien = cb.IdTrangThaiCheBien,
                        IdSanPham = cb.IdSanPham,
                        TenMon = cb.TenMon,
                        SoLuong = cb.SoLuong,
                        SoBan = cb.SoBan,
                        GhiChu = cb.GhiChu,
                        TrangThai = cb.TrangThai,
                        ThoiGianGoi = cb.ThoiGianGoi,
                        NhomIn = cb.NhomIn ?? "Bếp"
                        // Nếu DTO của bạn có ThoiGianHoanThanh thì map thêm vào đây
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}