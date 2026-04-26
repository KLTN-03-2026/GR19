// Tệp: CafebookApi/Controllers/App/NhanVien/CheBienController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/chebien")]
    [ApiController]
    [Authorize]
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

        [HttpPut("complete/{idTrangThaiCheBien}")]
        public async Task<IActionResult> CompleteItem(int idTrangThaiCheBien)
        {
            var item = await _context.TrangThaiCheBiens.FindAsync(idTrangThaiCheBien);
            if (item == null) return NotFound("Không tìm thấy món.");

            if (item.TrangThai == "Đang làm")
            {
                item.TrangThai = "Hoàn thành";
                item.ThoiGianHoanThanh = DateTime.Now;
                var hoaDon = await _context.HoaDons.FindAsync(item.IdHoaDon);
                int idNhanVien = hoaDon?.IdNhanVien ?? 1; 

                await TruKhoChoMonAn(item.IdSanPham, item.SoLuong, idNhanVien);

                await _context.SaveChangesAsync();

                await UpdateGiaoHangStatusIfCompleted(item.IdHoaDon);

                return Ok(new { message = "Đã hoàn thành món." });
            }
            return Conflict("Món này chưa được bắt đầu làm.");
        }

        private async Task TruKhoChoMonAn(int idSanPham, int soLuong, int idNhanVien)
        {
            var dinhLuongList = await _context.DinhLuongs
                .Include(d => d.NguyenLieu)
                .Include(d => d.DonViSuDung)
                .Where(d => d.IdSanPham == idSanPham)
                .ToListAsync();

            foreach (var dl in dinhLuongList)
            {
                if (dl.NguyenLieu != null && dl.DonViSuDung != null)
                {
                    var nguyenLieu = dl.NguyenLieu;
                    decimal luongTru1SP = 0;

                    if (dl.DonViSuDung.LaDonViCoBan)
                    {
                        luongTru1SP = dl.SoLuongSuDung;
                    }
                    else
                    {
                        decimal heSoQuyDoi = dl.DonViSuDung.GiaTriQuyDoi > 0 ? dl.DonViSuDung.GiaTriQuyDoi : 1m;
                        luongTru1SP = dl.SoLuongSuDung / heSoQuyDoi;
                    }

                    decimal luongCanTruTong = luongTru1SP * soLuong;
                    nguyenLieu.TonKho -= luongCanTruTong;

                    if (nguyenLieu.TonKho <= nguyenLieu.TonKhoToiThieu)
                    {
                        _context.ThongBaos.Add(new ThongBao
                        {
                            IdNhanVienTao = idNhanVien,
                            NoiDung = $"Cảnh báo: Tồn kho '{nguyenLieu.TenNguyenLieu}' sắp hết. Hiện chỉ còn {nguyenLieu.TonKho:N2} {nguyenLieu.DonViTinh}.",
                            ThoiGianTao = DateTime.Now,
                            LoaiThongBao = "CanhBaoKho",
                            IdLienQuan = nguyenLieu.IdNguyenLieu
                        });
                    }
                }
            }
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