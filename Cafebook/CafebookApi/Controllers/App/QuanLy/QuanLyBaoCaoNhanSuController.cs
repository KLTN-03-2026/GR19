// File: CafebookApi/Controllers/App/QuanLy/QuanLyBaoCaoNhanSuController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly/baocaonhansu")]
    [ApiController]
    [Authorize]
    public class QuanLyBaoCaoNhanSuController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyBaoCaoNhanSuController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var dto = new QuanLyBaoCaoNhanSu_FiltersDto
            {
                NhanViens = await _context.NhanViens
                    .AsNoTracking()
                    .OrderBy(nv => nv.HoTen)
                    .Select(nv => new QuanLyFilterLookupDto { Id = nv.IdNhanVien, Ten = nv.HoTen })
                    .ToListAsync(),

                VaiTros = await _context.VaiTros
                    .AsNoTracking()
                    .OrderBy(v => v.TenVaiTro)
                    .Select(v => new QuanLyFilterLookupDto { Id = v.IdVaiTro, Ten = v.TenVaiTro })
                    .ToListAsync()
            };

            return Ok(dto);
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] QuanLyBaoCaoNhanSuRequestDto request)
        {
            var endDate = request.EndDate.Date.AddDays(1);
            var report = new QuanLyBaoCaoNhanSuTongHopDto();

            // 1. Phân tích Bảng lương
            var phieuLuongQuery = _context.PhieuLuongs
                .AsNoTracking()
                .Include(p => p.NhanVien).ThenInclude(nv => nv.VaiTro)
                .Where(p => p.TrangThai == "Đã thanh toán"
                         && p.NgayTao >= request.StartDate.Date
                         && p.NgayTao < endDate);

            if (request.VaiTroId.HasValue && request.VaiTroId > 0)
                phieuLuongQuery = phieuLuongQuery.Where(p => p.NhanVien.IdVaiTro == request.VaiTroId.Value);

            if (request.NhanVienId.HasValue && request.NhanVienId > 0)
                phieuLuongQuery = phieuLuongQuery.Where(p => p.NhanVien.IdNhanVien == request.NhanVienId.Value);

            var luongList = await phieuLuongQuery.ToListAsync();

            report.BangLuongChiTiet = luongList
                .GroupBy(p => p.IdNhanVien)
                .Select(g => new QuanLyBangLuongChiTietDto
                {
                    IdNhanVien = g.Key,
                    HoTenNhanVien = g.First().NhanVien.HoTen,
                    TenVaiTro = g.First().NhanVien.VaiTro.TenVaiTro,
                    LuongCoBan = g.Sum(p => p.LuongCoBan),
                    TongGioLam = g.Sum(p => p.TongGioLam),
                    TienThuong = g.Sum(p => p.TienThuong ?? 0),
                    KhauTru = g.Sum(p => p.KhauTru ?? 0),
                    ThucLanh = g.Sum(p => p.ThucLanh)
                })
                .OrderByDescending(x => x.ThucLanh)
                .ToList();

            report.Kpi.TongLuongDaTra = report.BangLuongChiTiet.Sum(x => x.ThucLanh);
            report.Kpi.TongGioLam = report.BangLuongChiTiet.Sum(x => x.TongGioLam);

            // 2. Phân tích Nghỉ phép
            var donNghiQuery = _context.DonXinNghis
                .AsNoTracking()
                .Include(d => d.NhanVien).ThenInclude(nv => nv.VaiTro)
                .Where(d => d.TrangThai == "Đã duyệt"
                         && d.NgayBatDau >= request.StartDate.Date
                         && d.NgayBatDau < endDate);

            if (request.VaiTroId.HasValue && request.VaiTroId > 0)
                donNghiQuery = donNghiQuery.Where(d => d.NhanVien.IdVaiTro == request.VaiTroId.Value);

            if (request.NhanVienId.HasValue && request.NhanVienId > 0)
                donNghiQuery = donNghiQuery.Where(d => d.NhanVien.IdNhanVien == request.NhanVienId.Value);

            var donNghiList = await donNghiQuery.ToListAsync();

            report.ThongKeNghiPhep = donNghiList
                .GroupBy(d => d.IdNhanVien)
                .Select(g => new QuanLyThongKeNghiPhepDto
                {
                    IdNhanVien = g.Key,
                    HoTenNhanVien = g.First().NhanVien.HoTen,
                    TenVaiTro = g.First().NhanVien.VaiTro.TenVaiTro,
                    SoDonDaDuyet = g.Count(),
                    TongSoNgayNghi = g.Sum(d => (int)Math.Ceiling((d.NgayKetThuc - d.NgayBatDau).TotalDays))
                })
                .OrderByDescending(x => x.TongSoNgayNghi)
                .ToList();

            report.Kpi.TongSoNgayNghi = report.ThongKeNghiPhep.Sum(x => x.TongSoNgayNghi);

            // 3. Dữ liệu Biểu đồ (Tổng lương trả theo ngày)
            report.LuongChartData = luongList
                .GroupBy(p => p.NgayTao.Date) // XÓA CHỮ .Value Ở ĐÂY
                .Select(g => new QuanLyChartDataPointDto
                {
                    Ngay = g.Key,
                    TongTien = g.Sum(p => p.ThucLanh)
                })
                .OrderBy(c => c.Ngay)
                .ToList();

            return Ok(report);
        }
    }
}