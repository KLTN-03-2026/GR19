// File: CafebookApi/Controllers/App/QuanLy/QuanLyTongQuanController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-tongquan")]
    [ApiController]
    [Authorize]
    public class QuanLyTongQuanController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyTongQuanController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var dto = new QuanLyTongQuanDto();
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            DateTime startDate30Days = today.AddDays(-29);

            // 1. Tính KPI hôm nay
            var hoaDonsHomNay = _context.HoaDons
                .AsNoTracking()
                .Where(h => h.ThoiGianThanhToan >= today && h.ThoiGianThanhToan < tomorrow && h.TrangThai == "Đã thanh toán");

            dto.TongDoanhThuHomNay = await hoaDonsHomNay.SumAsync(h => h.ThanhTien);

            // 2. Sản phẩm bán chạy hôm nay
            var topSpHomNay = await _context.ChiTietHoaDons
                .AsNoTracking()
                .Where(ct => ct.HoaDon.ThoiGianThanhToan >= today && ct.HoaDon.ThoiGianThanhToan < tomorrow && ct.HoaDon.TrangThai == "Đã thanh toán")
                .GroupBy(ct => ct.SanPham.TenSanPham)
                .Select(g => new { Ten = g.Key, Sl = g.Sum(x => x.SoLuong) })
                .OrderByDescending(x => x.Sl)
                .FirstOrDefaultAsync();

            if (topSpHomNay != null)
            {
                dto.SanPhamBanChayHomNay = topSpHomNay.Ten;
                dto.SoLuongBanChayHomNay = topSpHomNay.Sl;
            }
            else
            {
                dto.SanPhamBanChayHomNay = "Chưa có dữ liệu";
                dto.SoLuongBanChayHomNay = 0;
            }

            // 3. Biểu đồ 30 ngày (Đường)
            var chartData = await _context.HoaDons
                .AsNoTracking()
                .Where(h => h.ThoiGianThanhToan >= startDate30Days && h.TrangThai == "Đã thanh toán")
                .GroupBy(h => h.ThoiGianThanhToan!.Value.Date)
                .Select(g => new QuanLyTongQuanChartPoint { Ngay = g.Key, TongTien = g.Sum(h => h.ThanhTien) })
                .OrderBy(x => x.Ngay).ToListAsync();

            dto.DoanhThu30Ngay = Enumerable.Range(0, 30)
                .Select(i => startDate30Days.AddDays(i))
                .Select(d => new QuanLyTongQuanChartPoint
                {
                    Ngay = d,
                    TongTien = chartData.FirstOrDefault(c => c.Ngay == d)?.TongTien ?? 0
                }).ToList();

            // 4. [BIỂU ĐỒ MỚI] Top 5 Sản phẩm 30 ngày (Cột)
            var top5Db = await _context.ChiTietHoaDons
                .AsNoTracking()
                .Where(ct => ct.HoaDon.ThoiGianThanhToan >= startDate30Days && ct.HoaDon.TrangThai == "Đã thanh toán")
                .GroupBy(ct => ct.SanPham.TenSanPham)
                .Select(g => new QuanLyTongQuanBarChartPoint
                {
                    TenSanPham = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();
            dto.Top5SanPham = top5Db;

            // 5. [BIỂU ĐỒ MỚI] Cơ cấu danh mục 30 ngày (Tròn)
            var coCauDb = await _context.ChiTietHoaDons
                .AsNoTracking()
                .Where(ct => ct.HoaDon.ThoiGianThanhToan >= startDate30Days && ct.HoaDon.TrangThai == "Đã thanh toán")
                .GroupBy(ct => ct.SanPham.DanhMuc.TenDanhMuc)
                .Select(g => new QuanLyTongQuanPieChartPoint
                {
                    TenDanhMuc = g.Key ?? "Khác",
                    GiaTri = (decimal)g.Sum(x => x.ThanhTien)
                })
                .OrderByDescending(x => x.GiaTri)
                .ToListAsync();
            dto.CoCauDoanhThu = coCauDb;

            return Ok(dto);
        }
    }
}