// File: CafebookApi/Controllers/App/QuanLy/QuanLyBaoCaoTonKhoNguyenLieuController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly/baocaotonkhonguyenlieu")]
    [ApiController]
    [Authorize]
    public class QuanLyBaoCaoTonKhoNguyenLieuController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyBaoCaoTonKhoNguyenLieuController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterData()
        {
            var dto = new QuanLyBaoCaoTonKho_FiltersDto
            {
                NhaCungCaps = await _context.NhaCungCaps
                    .AsNoTracking()
                    .Select(t => new QuanLyFilterLookupDto { Id = t.IdNhaCungCap, Ten = t.TenNhaCungCap })
                    .OrderBy(t => t.Ten)
                    .ToListAsync()
            };

            return Ok(dto); // Trả về DTO thay vì Object vô danh
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetKhoReport([FromBody] QuanLyBaoCaoTonKhoNguyenLieuRequestDto request)
        {
            var searchTxt = string.IsNullOrEmpty(request.SearchText) ? null : $"%{request.SearchText}%";

            // --- 1. TÍNH KPIs ---

            // [FIX LỖI SQL]: Tách Subquery ra khỏi hàm SUM bằng CTE (WITH...)
            var kpiGiaTriKho = await _context.Database.SqlQuery<decimal>($@"
                WITH GiaNhapTB AS (
                    SELECT idNguyenLieu, AVG(donGiaNhap) AS GiaVonTrungBinh
                    FROM dbo.ChiTietNhapKho 
                    GROUP BY idNguyenLieu
                )
                SELECT CAST(ISNULL(SUM(nl.tonKho * ISNULL(gn.GiaVonTrungBinh, 0)), 0) AS DECIMAL(18,2))
                FROM dbo.NguyenLieu nl
                LEFT JOIN GiaNhapTB gn ON nl.idNguyenLieu = gn.idNguyenLieu;
            ").ToListAsync();

            var kpiSpSapHet = await _context.Database.SqlQuery<int>($@"
                SELECT ISNULL(COUNT(*), 0)
                FROM dbo.NguyenLieu
                WHERE tonKho <= TonKhoToiThieu;
            ").ToListAsync();

            var kpiGiaTriHuy = await _context.Database.SqlQuery<decimal>($@"
                SELECT CAST(ISNULL(SUM(TongGiaTriHuy), 0) AS DECIMAL(18,2))
                FROM dbo.PhieuXuatHuy;
            ").ToListAsync();

            var kpi = new QuanLyBaoCaoTonKhoKpiDto
            {
                TongGiaTriTonKho = kpiGiaTriKho.FirstOrDefault(),
                SoLuongSPSapHet = kpiSpSapHet.FirstOrDefault(),
                TongGiaTriDaHuy = kpiGiaTriHuy.FirstOrDefault()
            };

            // --- 2. TAB 1: CHI TIẾT TỒN KHO ---
            var chiTietTonKho = await _context.Database.SqlQuery<QuanLyBaoCaoTonKhoChiTietDto>($@"
                SELECT 
                    tenNguyenLieu AS TenNguyenLieu,
                    donViTinh AS DonViTinh,
                    CAST(tonKho AS DECIMAL(18,2)) AS TonKho,
                    CAST(TonKhoToiThieu AS DECIMAL(18,2)) AS TonKhoToiThieu,
                    CASE WHEN tonKho <= TonKhoToiThieu THEN N'Sắp hết' ELSE N'Bình thường' END AS TinhTrang
                FROM dbo.NguyenLieu
                WHERE ({searchTxt} IS NULL OR tenNguyenLieu LIKE {searchTxt})
                  AND ({request.ShowLowStockOnly} = 0 OR tonKho <= TonKhoToiThieu)
                ORDER BY tonKho ASC;
            ").ToListAsync();

            // --- 3. TAB 2: LỊCH SỬ KIỂM KÊ ---
            var lichSuKiemKe = await _context.Database.SqlQuery<QuanLyBaoCaoKiemKeDto>($@"
                SELECT 
                    ISNULL(pkk.NgayKiem, GETDATE()) AS NgayKiem,
                    nl.tenNguyenLieu AS TenNguyenLieu,
                    CAST(ctkk.TonKhoHeThong AS DECIMAL(18,2)) AS TonKhoHeThong,
                    CAST(ctkk.TonKhoThucTe AS DECIMAL(18,2)) AS TonKhoThucTe,
                    CAST(ctkk.ChenhLech AS DECIMAL(18,2)) AS ChenhLech,
                    ctkk.LyDoChenhLech AS LyDoChenhLech
                FROM dbo.ChiTietKiemKho ctkk
                JOIN dbo.PhieuKiemKho pkk ON ctkk.idPhieuKiemKho = pkk.idPhieuKiemKho
                JOIN dbo.NguyenLieu nl ON ctkk.idNguyenLieu = nl.idNguyenLieu
                WHERE ctkk.ChenhLech != 0
                  AND ({searchTxt} IS NULL OR nl.tenNguyenLieu LIKE {searchTxt})
                ORDER BY pkk.NgayKiem DESC;
            ").ToListAsync();

            // --- 4. TAB 3: LỊCH SỬ HỦY HÀNG ---
            var lichSuHuyHang = await _context.Database.SqlQuery<QuanLyBaoCaoHuyHangDto>($@"
                SELECT 
                    ISNULL(pxh.NgayXuatHuy, GETDATE()) AS NgayHuy,
                    nl.tenNguyenLieu AS TenNguyenLieu,
                    CAST(ctxh.SoLuong AS DECIMAL(18,2)) AS SoLuongHuy,
                    CAST(ctxh.ThanhTien AS DECIMAL(18,2)) AS GiaTriHuy,
                    pxh.LyDoXuatHuy AS LyDoHuy
                FROM dbo.ChiTietXuatHuy ctxh
                JOIN dbo.PhieuXuatHuy pxh ON ctxh.idPhieuXuatHuy = pxh.idPhieuXuatHuy
                JOIN dbo.NguyenLieu nl ON ctxh.idNguyenLieu = nl.idNguyenLieu
                WHERE ({searchTxt} IS NULL OR nl.tenNguyenLieu LIKE {searchTxt})
                ORDER BY pxh.NgayXuatHuy DESC;
            ").ToListAsync();

            var result = new QuanLyBaoCaoTonKhoNguyenLieuTongHopDto
            {
                Kpi = kpi,
                ChiTietTonKho = chiTietTonKho,
                LichSuKiemKe = lichSuKiemKe,
                LichSuHuyHang = lichSuHuyHang
            };

            return Ok(result);
        }
    }
}