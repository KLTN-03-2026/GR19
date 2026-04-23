using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-lichsuthuesach")]
    [ApiController]
    [Authorize]
    public class QuanLyLichSuThueSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyLichSuThueSachController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetRentalData(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var toDateEnd = toDate?.AddDays(1);
            var sqlParams = new List<SqlParameter>();

            // 1. SQL cho Sách Quá Hạn (Đã thêm % Độ Mới lúc thuê)
            string sqlQuaHan = @"
                SELECT 
                    ctpt.idPhieuThueSach AS IdPhieuThue,
                    ctpt.idSach AS IdSach,
                    s.tenSach AS TenSach, 
                    kh.hoTen AS TenKhachHang, 
                    kh.soDienThoai AS SoDienThoai,
                    pts.ngayThue AS NgayThue, 
                    ctpt.ngayHenTra AS NgayHenTra,
                    ISNULL(ctpt.DoMoiKhiThue, 100) AS DoMoiKhiThue,
                    ctpt.GhiChuKhiThue AS GhiChuKhiThue,
                    N'Trễ ' + CAST(DATEDIFF(DAY, ctpt.ngayHenTra, GETDATE()) AS NVARCHAR) + N' ngày' AS TinhTrang
                FROM dbo.ChiTietPhieuThue ctpt
                JOIN dbo.PhieuThueSach pts ON ctpt.idPhieuThueSach = pts.idPhieuThueSach
                JOIN dbo.Sach s ON ctpt.idSach = s.idSach
                JOIN dbo.KhachHang kh ON pts.idKhachHang = kh.idKhachHang
                WHERE ctpt.ngayTraThucTe IS NULL AND ctpt.ngayHenTra < GETDATE()
                ORDER BY ctpt.ngayHenTra ASC;";

            // 2. SQL cho Lịch sử thuê (Có lọc ngày + Join với bảng Trả để lấy Phạt Hư Hỏng)
            var whereClauses = new List<string>();
            if (fromDate.HasValue)
            {
                whereClauses.Add("pts.ngayThue >= @fromDate");
                sqlParams.Add(new SqlParameter("@fromDate", fromDate.Value));
            }
            if (toDateEnd.HasValue)
            {
                whereClauses.Add("pts.ngayThue < @toDateEnd");
                sqlParams.Add(new SqlParameter("@toDateEnd", toDateEnd.Value));
            }

            string whereSqlLichSu = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            string sqlLichSu = $@"
                SELECT TOP 100
                    pts.idPhieuThueSach AS IdPhieuThue,
                    s.tenSach AS TenSach, 
                    kh.hoTen AS TenKhachHang, 
                    pts.ngayThue AS NgayThue,
                    ctpt.ngayHenTra AS NgayHenTra, 
                    ctpt.ngayTraThucTe AS NgayTraThucTe,
                    ISNULL(ctpt.TienPhatTraTre, 0) AS TienPhat,
                    ISNULL(cptra.TienPhatHuHong, 0) AS TienPhatHuHong,
                    ISNULL(ctpt.DoMoiKhiThue, 100) AS DoMoiKhiThue,
                    ctpt.GhiChuKhiThue AS GhiChuKhiThue,
                    cptra.DoMoiKhiTra AS DoMoiKhiTra,
                    cptra.GhiChuKhiTra AS GhiChuKhiTra,
                    pts.trangThai AS TrangThai
                FROM dbo.ChiTietPhieuThue ctpt
                JOIN dbo.PhieuThueSach pts ON ctpt.idPhieuThueSach = pts.idPhieuThueSach
                JOIN dbo.Sach s ON ctpt.idSach = s.idSach
                JOIN dbo.KhachHang kh ON pts.idKhachHang = kh.idKhachHang
                LEFT JOIN (
                    SELECT pt.IdPhieuThueSach, cpt.IdSach, cpt.TienPhatHuHong, cpt.DoMoiKhiTra, cpt.GhiChuKhiTra
                    FROM dbo.ChiTietPhieuTra cpt
                    JOIN dbo.PhieuTraSach pt ON cpt.IdPhieuTra = pt.IdPhieuTra
                ) cptra ON cptra.IdPhieuThueSach = pts.idPhieuThueSach AND cptra.IdSach = ctpt.idSach
                {whereSqlLichSu}
                ORDER BY pts.ngayThue DESC;";

            var dto = new BaoCaoLichSuThueDto
            {
                SachQuaHan = await _context.Database.SqlQueryRaw<SachQuaHanGridDto>(sqlQuaHan).ToListAsync(),
                LichSuThue = await _context.Database.SqlQueryRaw<LichSuThueSachGridDto>(sqlLichSu, sqlParams.ToArray()).ToListAsync()
            };

            return Ok(dto);
        }
    }
}