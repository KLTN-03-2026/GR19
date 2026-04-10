using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-tongquan")]
    [ApiController]
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
                .Where(h => h.ThoiGianThanhToan >= today && h.ThoiGianThanhToan < tomorrow && h.TrangThai == "Đã thanh toán");

            dto.TongDoanhThuHomNay = await hoaDonsHomNay.SumAsync(h => h.ThanhTien);
            dto.TongDonHangHomNay = await hoaDonsHomNay.CountAsync();

            // 2. Sản phẩm bán chạy
            var topSp = await _context.ChiTietHoaDons
                .Where(ct => ct.HoaDon.ThoiGianThanhToan >= today && ct.HoaDon.ThoiGianThanhToan < tomorrow && ct.HoaDon.TrangThai == "Đã thanh toán")
                .GroupBy(ct => ct.SanPham.TenSanPham)
                .Select(g => new { Ten = g.Key, Sl = g.Sum(x => x.SoLuong) })
                .OrderByDescending(x => x.Sl)
                .FirstOrDefaultAsync();

            dto.SanPhamBanChayHomNay = topSp?.Ten ?? "Chưa có dữ liệu";

            // 3. Biểu đồ 30 ngày
            var chartData = await _context.HoaDons
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

            return Ok(dto);
        }
    }
}