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
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1);
            var roleId = request.VaiTroId ?? -1;
            var nvId = request.NhanVienId ?? -1;

            var report = new QuanLyBaoCaoNhanSuTongHopDto();

            // Lấy tháng và năm từ khoảng thời gian được chọn
            var startMonth = startDate.Month;
            var startYear = startDate.Year;
            var endMonth = request.EndDate.Date.Month;
            var endYear = request.EndDate.Date.Year;

            // =========================================================================
            // 1. Phân tích Bảng lương (Lọc theo THÁNG và NĂM trực tiếp trên phiếu lương)
            // =========================================================================
            report.BangLuongChiTiet = await _context.Database.SqlQuery<QuanLyBangLuongChiTietDto>($@"
                SELECT 
                    nv.idNhanVien AS IdNhanVien,
                    nv.hoTen AS HoTenNhanVien,
                    vt.tenVaiTro AS TenVaiTro,
                    CAST(ISNULL(SUM(pl.luongCoBan), 0) AS DECIMAL(18,2)) AS LuongCoBan,
                    CAST(ISNULL(SUM(pl.tongGioLam), 0) AS DECIMAL(18,2)) AS TongGioLam,
                    CAST(ISNULL(SUM(pl.tienThuong), 0) AS DECIMAL(18,2)) AS TienThuong,
                    CAST(ISNULL(SUM(pl.khauTru), 0) AS DECIMAL(18,2)) AS KhauTru,
                    CAST(ISNULL(SUM(pl.thucLanh), 0) AS DECIMAL(18,2)) AS ThucLanh
                FROM dbo.PhieuLuong pl
                JOIN dbo.NhanVien nv ON pl.idNhanVien = nv.idNhanVien
                JOIN dbo.VaiTro vt ON nv.idVaiTro = vt.idVaiTro
                WHERE ISNULL(pl.trangThai, '') NOT LIKE N'%Hủy%'
                  -- Lọc theo cột 'nam' và 'thang' trên database để không bị sót data
                  AND ((pl.nam > {startYear}) OR (pl.nam = {startYear} AND pl.thang >= {startMonth}))
                  AND ((pl.nam < {endYear}) OR (pl.nam = {endYear} AND pl.thang <= {endMonth}))
                  AND ({roleId} = -1 OR nv.idVaiTro = {roleId})
                  AND ({nvId} = -1 OR nv.idNhanVien = {nvId})
                GROUP BY nv.idNhanVien, nv.hoTen, vt.tenVaiTro
                ORDER BY ThucLanh DESC
            ").AsNoTracking().ToListAsync();

            report.Kpi.TongLuongDaTra = report.BangLuongChiTiet.Sum(x => x.ThucLanh);
            report.Kpi.TongGioLam = report.BangLuongChiTiet.Sum(x => x.TongGioLam);

            // =========================================================================
            // 2. Phân tích Nghỉ phép 
            // =========================================================================
            report.ThongKeNghiPhep = await _context.Database.SqlQuery<QuanLyThongKeNghiPhepDto>($@"
                SELECT 
                    nv.idNhanVien AS IdNhanVien,
                    nv.hoTen AS HoTenNhanVien,
                    vt.tenVaiTro AS TenVaiTro,
                    ISNULL(COUNT(dn.idDonXinNghi), 0) AS SoDonDaDuyet,
                    ISNULL(SUM(DATEDIFF(day, dn.NgayBatDau, dn.NgayKetThuc) + 1), 0) AS TongSoNgayNghi
                FROM dbo.DonXinNghi dn
                JOIN dbo.NhanVien nv ON dn.idNhanVien = nv.idNhanVien
                JOIN dbo.VaiTro vt ON nv.idVaiTro = vt.idVaiTro
                WHERE dn.TrangThai = N'Đã duyệt'
                  AND dn.NgayBatDau < {endDate} 
                  AND dn.NgayKetThuc >= {startDate}
                  AND ({roleId} = -1 OR nv.idVaiTro = {roleId})
                  AND ({nvId} = -1 OR nv.idNhanVien = {nvId})
                GROUP BY nv.idNhanVien, nv.hoTen, vt.tenVaiTro
                ORDER BY TongSoNgayNghi DESC
            ").AsNoTracking().ToListAsync();

            report.Kpi.TongSoNgayNghi = report.ThongKeNghiPhep.Sum(x => x.TongSoNgayNghi);

            // =========================================================================
            // 3. Dữ liệu Biểu đồ (Gộp theo 'thang' và 'nam' của phiếu lương)
            // =========================================================================
            report.LuongChartData = await _context.Database.SqlQuery<QuanLyChartDataPointDto>($@"
                SELECT 
                    CAST(DATEFROMPARTS(pl.nam, pl.thang, 1) AS DATETIME) AS Ngay,
                    CAST(ISNULL(SUM(pl.thucLanh), 0) AS DECIMAL(18,2)) AS TongTien
                FROM dbo.PhieuLuong pl
                WHERE ISNULL(pl.trangThai, '') NOT LIKE N'%Hủy%'
                  AND ((pl.nam > {startYear}) OR (pl.nam = {startYear} AND pl.thang >= {startMonth}))
                  AND ((pl.nam < {endYear}) OR (pl.nam = {endYear} AND pl.thang <= {endMonth}))
                  AND ({roleId} = -1 OR pl.idNhanVien IN (SELECT idNhanVien FROM dbo.NhanVien WHERE idVaiTro = {roleId}))
                  AND ({nvId} = -1 OR pl.idNhanVien = {nvId})
                GROUP BY pl.nam, pl.thang
                ORDER BY Ngay ASC
            ").AsNoTracking().ToListAsync();

            return Ok(report);
        }
    }
}