// File: CafebookApi/Controllers/App/QuanLy/QuanLyBaoCaoHieuSuatController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly/baocaohieusuat")]
    [ApiController]
    [Authorize]
    public class QuanLyBaoCaoHieuSuatController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyBaoCaoHieuSuatController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterData()
        {
            var vaiTros = await _context.VaiTros
                .AsNoTracking()
                .Select(t => new QuanLyFilterLookupDto { Id = t.IdVaiTro, Ten = t.TenVaiTro })
                .OrderBy(t => t.Ten)
                .ToListAsync();

            return Ok(new QuanLyBaoCaoHieuSuat_FiltersDto { VaiTros = vaiTros });
        }

        [HttpGet("search-nhan-vien")]
        public async Task<IActionResult> SearchNhanVien([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return Ok(new List<QuanLyFilterLookupDto>());

            var nvs = await _context.NhanViens
                .AsNoTracking()
                .Where(x => x.HoTen.Contains(keyword) || x.SoDienThoai.Contains(keyword))
                .Select(x => new QuanLyFilterLookupDto { Id = x.IdNhanVien, Ten = x.HoTen + " (" + x.SoDienThoai + ")" })
                .Take(10)
                .ToListAsync();

            return Ok(nvs);
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetHieuSuatReport([FromBody] QuanLyBaoCaoHieuSuatRequestDto request)
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1);
            var nvId = request.NhanVienId ?? -1;
            var roleId = request.VaiTroId ?? -1;
            // Xử lý parameter an toàn cho EF Core SqlQuery
            var searchStr = string.IsNullOrWhiteSpace(request.SearchText) ? "" : request.SearchText.Trim();

            // 1. DỮ LIỆU BÁN HÀNG (SALES)
            var salesData = await _context.Database.SqlQuery<QuanLyBaoCaoSalesDto>($@"
                SELECT 
                    nv.hoTen AS HoTen,
                    vt.tenVaiTro AS TenVaiTro,
                    CAST(ISNULL(SUM(hd.thanhTien), 0) AS DECIMAL(18,2)) AS TongDoanhThu,
                    ISNULL(COUNT(DISTINCT hd.idHoaDon), 0) AS SoHoaDon,
                    -- Dùng NULLIF để chống lỗi chia cho 0 (Divide by Zero)
                    CAST(ISNULL(SUM(hd.thanhTien) / NULLIF(COUNT(DISTINCT hd.idHoaDon), 0), 0) AS DECIMAL(18,2)) AS DoanhThuTrungBinh,
                    (SELECT COUNT(idNhatKy) FROM dbo.NhatKyHuyMon WHERE idNhanVienHuy = nv.idNhanVien AND ThoiGianHuy >= {startDate} AND ThoiGianHuy < {endDate}) AS SoLanHuyMon
                FROM dbo.NhanVien nv
                JOIN dbo.VaiTro vt ON nv.idVaiTro = vt.idVaiTro
                LEFT JOIN dbo.HoaDon hd ON nv.idNhanVien = hd.idNhanVien 
                    AND hd.thoiGianThanhToan >= {startDate} AND hd.thoiGianThanhToan < {endDate} 
                    AND hd.trangThai = N'Đã thanh toán'
                WHERE ({roleId} = -1 OR nv.idVaiTro = {roleId})
                  AND ({searchStr} = '' OR nv.hoTen LIKE N'%' + {searchStr} + N'%')
                GROUP BY nv.idNhanVien, nv.hoTen, vt.tenVaiTro
            ").AsNoTracking().ToListAsync();

            // 2. DỮ LIỆU VẬN HÀNH (OPERATIONS)
            var opsData = await _context.Database.SqlQuery<QuanLyBaoCaoOperationsDto>($@"
                SELECT 
                    nv.hoTen AS HoTen,
                    vt.tenVaiTro AS TenVaiTro,
                    (SELECT COUNT(idPhieuNhapKho) FROM dbo.PhieuNhapKho WHERE idNhanVien = nv.idNhanVien AND ngayNhap >= {startDate} AND ngayNhap < {endDate}) AS PhieuNhap,
                    (SELECT COUNT(idPhieuKiemKho) FROM dbo.PhieuKiemKho WHERE idNhanVienKiem = nv.idNhanVien AND NgayKiem >= {startDate} AND NgayKiem < {endDate}) AS PhieuKiem,
                    (SELECT COUNT(idPhieuXuatHuy) FROM dbo.PhieuXuatHuy WHERE idNhanVienXuat = nv.idNhanVien AND NgayXuatHuy >= {startDate} AND NgayXuatHuy < {endDate}) AS PhieuHuy,
                    (SELECT COUNT(idDonXinNghi) FROM dbo.DonXinNghi WHERE idNguoiDuyet = nv.idNhanVien AND NgayDuyet >= {startDate} AND NgayDuyet < {endDate}) AS DonDuyet
                FROM dbo.NhanVien nv
                JOIN dbo.VaiTro vt ON nv.idVaiTro = vt.idVaiTro
                WHERE ({roleId} = -1 OR nv.idVaiTro = {roleId})
                  AND ({searchStr} = '' OR nv.hoTen LIKE N'%' + {searchStr} + N'%')
            ").AsNoTracking().ToListAsync();

            // 3. DỮ LIỆU CHẤM CÔNG & NGHỈ PHÉP (ATTENDANCE)
            var attData = await _context.Database.SqlQuery<QuanLyBaoCaoAttendanceDto>($@"
                SELECT 
                    nv.hoTen AS HoTen,
                    vt.tenVaiTro AS TenVaiTro,
                    ISNULL(COUNT(DISTINCT llv.idLichLamViec), 0) AS SoCaLam,
                    CAST(ISNULL(SUM(bc.soGioLam), 0) AS DECIMAL(18,2)) AS TongGioLam,
                    -- FIX overlap date: NgayBatDau < endDate AND NgayKetThuc >= startDate
                    (SELECT COUNT(idDonXinNghi) FROM dbo.DonXinNghi WHERE idNhanVien = nv.idNhanVien AND NgayBatDau < {endDate} AND NgayKetThuc >= {startDate}) AS SoDonXinNghi,
                    (SELECT COUNT(idDonXinNghi) FROM dbo.DonXinNghi WHERE idNhanVien = nv.idNhanVien AND TrangThai = N'Đã duyệt' AND NgayBatDau < {endDate} AND NgayKetThuc >= {startDate}) AS SoDonDaDuyet,
                    (SELECT COUNT(idDonXinNghi) FROM dbo.DonXinNghi WHERE idNhanVien = nv.idNhanVien AND TrangThai = N'Chờ duyệt' AND NgayBatDau < {endDate} AND NgayKetThuc >= {startDate}) AS SoDonChoDuyet
                FROM dbo.NhanVien nv
                JOIN dbo.VaiTro vt ON nv.idVaiTro = vt.idVaiTro
                LEFT JOIN dbo.LichLamViec llv ON nv.idNhanVien = llv.idNhanVien
                    AND llv.ngayLam >= {startDate} AND llv.ngayLam < {endDate}
                    AND llv.trangThai = N'Đã chấm công'
                LEFT JOIN dbo.BangChamCong bc ON llv.idLichLamViec = bc.idLichLamViec
                WHERE ({roleId} = -1 OR nv.idVaiTro = {roleId})
                  AND ({searchStr} = '' OR nv.hoTen LIKE N'%' + {searchStr} + N'%')
                GROUP BY nv.idNhanVien, nv.hoTen, vt.tenVaiTro
            ").AsNoTracking().ToListAsync();

            // 4. TRẢ KẾT QUẢ VÀ TÍNH TỔNG KPI
            var dto = new QuanLyBaoCaoHieuSuatTongHopDto
            {
                SalesPerformance = salesData,
                OperationalPerformance = opsData,
                Attendance = attData,
                Kpi = new QuanLyBaoCaoHieuSuatKpiDto
                {
                    TongDoanhThu = salesData.Sum(x => x.TongDoanhThu),
                    TongGioLam = attData.Sum(x => x.TongGioLam),
                    TongSoCaLam = attData.Sum(x => x.SoCaLam),
                    TongLanHuyMon = salesData.Sum(x => x.SoLanHuyMon)
                }
            };

            return Ok(dto);
        }
    }
}