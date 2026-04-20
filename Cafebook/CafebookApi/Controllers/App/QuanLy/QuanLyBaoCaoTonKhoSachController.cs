// File: CafebookApi/Controllers/App/QuanLy/QuanLyBaoCaoTonKhoSachController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly/baocaotonkhosach")]
    [ApiController]
    public class QuanLyBaoCaoTonKhoSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyBaoCaoTonKhoSachController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterData()
        {
            var dto = new QuanLyBaoCaoTonKhoSach_FiltersDto
            {
                TheLoais = await _context.TheLoais
                    .AsNoTracking()
                    .Select(t => new QuanLyFilterLookupDto { Id = t.IdTheLoai, Ten = t.TenTheLoai })
                    .OrderBy(t => t.Ten)
                    .ToListAsync(),

                TacGias = await _context.TacGias
                    .AsNoTracking()
                    .Select(t => new QuanLyFilterLookupDto { Id = t.IdTacGia, Ten = t.TenTacGia })
                    .OrderBy(t => t.Ten)
                    .ToListAsync()
            };

            return Ok(dto);
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetSachReport([FromBody] QuanLyBaoCaoSachRequestDto request)
        {
            string? pSearchText = string.IsNullOrEmpty(request.SearchText) ? null : $"%{request.SearchText}%";
            int? pTheLoaiId = request.TheLoaiId == 0 ? null : request.TheLoaiId;
            int? pTacGiaId = request.TacGiaId == 0 ? null : request.TacGiaId;

            // 1. TÍNH KPIs
            var kpi = (await _context.Database.SqlQuery<QuanLyBaoCaoSachKpiDto>($@"
                SELECT
                    ISNULL(COUNT(DISTINCT idSach), 0) AS TongDauSach,
                    ISNULL(SUM(soLuongTong), 0) AS TongSoLuong,
                    (SELECT ISNULL(COUNT(idSach), 0) FROM dbo.ChiTietPhieuThue WHERE ngayTraThucTe IS NULL) AS DangChoThue,
                    (ISNULL(SUM(soLuongTong), 0) - (SELECT ISNULL(COUNT(idSach), 0) FROM dbo.ChiTietPhieuThue WHERE ngayTraThucTe IS NULL)) AS SanSang
                FROM dbo.Sach;
            ").ToListAsync()).FirstOrDefault() ?? new QuanLyBaoCaoSachKpiDto();

            // 2. TỒN KHO CHI TIẾT
            var chiTietTonKho = await _context.Database.SqlQuery<QuanLyBaoCaoSachChiTietDto>($@"
                WITH SachDangThue AS (
                    SELECT idSach, COUNT(idSach) AS SoLuongDangMuon
                    FROM dbo.ChiTietPhieuThue
                    WHERE ngayTraThucTe IS NULL
                    GROUP BY idSach
                ),
                SachTacGiasAgg AS (
                    SELECT stg.idSach, STRING_AGG(tg.tenTacGia, ', ') AS tenTacGia
                    FROM dbo.Sach_TacGia stg JOIN dbo.TacGia tg ON stg.idTacGia = tg.idTacGia
                    GROUP BY stg.idSach
                ),
                SachTheLoaisAgg AS (
                    SELECT stl.idSach, STRING_AGG(tl.tenTheLoai, ', ') AS tenTheLoai
                    FROM dbo.Sach_TheLoai stl JOIN dbo.TheLoai tl ON stl.idTheLoai = tl.idTheLoai
                    GROUP BY stl.idSach
                )
                SELECT
                    s.tenSach AS TenSach,
                    ISNULL(stg_agg.tenTacGia, N'N/A') AS TenTacGia,
                    ISNULL(stl_agg.tenTheLoai, N'N/A') AS TenTheLoai,
                    s.soLuongTong AS SoLuongTong,
                    ISNULL(sdt.SoLuongDangMuon, 0) AS SoLuongDangMuon,
                    (s.soLuongTong - ISNULL(sdt.SoLuongDangMuon, 0)) AS SoLuongConLai
                FROM dbo.Sach s
                LEFT JOIN SachTheLoaisAgg stl_agg ON s.idSach = stl_agg.idSach
                LEFT JOIN SachTacGiasAgg stg_agg ON s.idSach = stg_agg.idSach
                LEFT JOIN SachDangThue sdt ON s.idSach = sdt.idSach
                WHERE
                    (s.tenSach LIKE {pSearchText} OR stg_agg.tenTacGia LIKE {pSearchText} OR {pSearchText} IS NULL)
                    AND (EXISTS(SELECT 1 FROM dbo.Sach_TheLoai stl WHERE stl.idSach = s.idSach AND stl.idTheLoai = {pTheLoaiId}) OR {pTheLoaiId} IS NULL)
                    AND (EXISTS(SELECT 1 FROM dbo.Sach_TacGia stg WHERE stg.idSach = s.idSach AND stg.idTacGia = {pTacGiaId}) OR {pTacGiaId} IS NULL)
                ORDER BY SoLuongConLai ASC, s.tenSach;
            ").ToListAsync();

            // 3. SÁCH TRỄ HẠN
            var sachTreHan = await _context.Database.SqlQuery<QuanLyBaoCaoSachTreHanDto>($@"
                SELECT
                    s.tenSach AS TenSach,
                    kh.hoTen AS HoTen,
                    kh.soDienThoai AS SoDienThoai,
                    pts.ngayThue AS NgayThue,
                    ctpt.ngayHenTra AS NgayHenTra,
                    CASE
                        WHEN ctpt.ngayHenTra < GETDATE() THEN N'Trễ ' + CAST(DATEDIFF(DAY, ctpt.ngayHenTra, GETDATE()) AS NVARCHAR) + N' ngày'
                        ELSE N'Đang thuê'
                    END AS TinhTrang
                FROM dbo.ChiTietPhieuThue ctpt
                JOIN dbo.PhieuThueSach pts ON ctpt.idPhieuThueSach = pts.idPhieuThueSach
                JOIN dbo.Sach s ON ctpt.idSach = s.idSach
                JOIN dbo.KhachHang kh ON pts.idKhachHang = kh.idKhachHang
                WHERE ctpt.ngayTraThucTe IS NULL
                ORDER BY ctpt.ngayHenTra ASC;
            ").ToListAsync();

            // 4. TOP SÁCH THUÊ
            var topSachThue = await _context.Database.SqlQuery<QuanLyTopSachDuocThueDto>($@"
                WITH SachTacGiasAgg AS (
                    SELECT stg.idSach, STRING_AGG(tg.tenTacGia, ', ') AS tenTacGia
                    FROM dbo.Sach_TacGia stg JOIN dbo.TacGia tg ON stg.idTacGia = tg.idTacGia
                    GROUP BY stg.idSach
                )
                SELECT TOP 10
                    s.tenSach AS TenSach,
                    ISNULL(stg_agg.tenTacGia, N'N/A') AS TenTacGia,
                    COUNT(ctpt.idSach) AS TongLuotThue
                FROM dbo.ChiTietPhieuThue ctpt
                JOIN dbo.Sach s ON ctpt.idSach = s.idSach
                LEFT JOIN SachTacGiasAgg stg_agg ON s.idSach = stg_agg.idSach
                GROUP BY s.tenSach, stg_agg.tenTacGia
                ORDER BY TongLuotThue DESC;
            ").ToListAsync();

            var dto = new QuanLyBaoCaoSachTongHopDto
            {
                Kpi = kpi,
                ChiTietTonKho = chiTietTonKho,
                SachTreHan = sachTreHan,
                TopSachThue = topSachThue
            };

            return Ok(dto);
        }
    }
}