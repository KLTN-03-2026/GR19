// File: CafebookApi/Controllers/App/QuanLy/QuanLyBaoCaoDoanhThuController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly/baocaodoanhthu")]
    [ApiController]
    public class QuanLyBaoCaoDoanhThuController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyBaoCaoDoanhThuController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpPost("xem-bao-cao")]
        public async Task<IActionResult> GetDoanhThuReport([FromBody] QuanLyBaoCaoDoanhThuRequestDto request)
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1);

            // 1. TÍNH DOANH THU BÁN HÀNG VÀ TỔNG HÓA ĐƠN
            var chiTietDoanhThu = (await _context.Database.SqlQuery<QuanLyBaoCaoChiTietDoanhThuDto>($@"
                    SELECT
                        CAST(ISNULL(SUM(tongTienGoc), 0) AS DECIMAL(18,2)) AS TongDoanhThuBanHang,
                        CAST(0 AS DECIMAL(18,2)) AS TongDoanhThuThueSach, 
                        CAST(ISNULL(SUM(giamGia), 0) AS DECIMAL(18,2)) AS TongGiamGia,
                        CAST(ISNULL(SUM(TongPhuThu), 0) AS DECIMAL(18,2)) AS TongPhuThu,
                        CAST(ISNULL(SUM(thanhTien), 0) AS DECIMAL(18,2)) AS DoanhThuRong,
                        ISNULL(COUNT(idHoaDon), 0) AS SoLuongHoaDon,
                        CAST(ISNULL(AVG(thanhTien), 0) AS DECIMAL(18,2)) AS GiaTriTrungBinhHD
                    FROM dbo.HoaDon
                    WHERE trangThai = N'Đã thanh toán'
                    AND thoiGianThanhToan >= {startDate} AND thoiGianThanhToan < {endDate};
                ").AsNoTracking().ToListAsync()).FirstOrDefault() ?? new QuanLyBaoCaoChiTietDoanhThuDto();

            // 1.1 TÍNH DOANH THU THUÊ SÁCH
            var thueSachResult = await _context.Database.SqlQuery<decimal>($@"
                SELECT CAST(ISNULL(SUM(TongPhiThue + TongTienPhat), 0) AS DECIMAL(18,2))
                FROM dbo.PhieuTraSach
                WHERE NgayTra >= {startDate} AND NgayTra < {endDate};
            ").ToListAsync();

            decimal doanhThuThueSach = thueSachResult.FirstOrDefault();
            chiTietDoanhThu.TongDoanhThuThueSach = doanhThuThueSach;
            chiTietDoanhThu.DoanhThuRong += doanhThuThueSach;

            // 2. TÍNH GIÁ VỐN (COGS) - FIX: Tính toán chuẩn xác theo Đơn vị quy đổi
            var cogsResult = await _context.Database.SqlQuery<decimal>($@"
                WITH GiaVonNguyenLieu AS (
                    SELECT idNguyenLieu, AVG(donGiaNhap) AS GiaVonTrungBinh
                    FROM dbo.ChiTietNhapKho GROUP BY idNguyenLieu
                ),
                SanPhamDaBan AS (
                    SELECT cthd.idSanPham, SUM(cthd.soLuong) AS TongSoLuongBan
                    FROM dbo.ChiTietHoaDon cthd
                    JOIN dbo.HoaDon hd ON cthd.idHoaDon = hd.idHoaDon
                    WHERE hd.trangThai = N'Đã thanh toán'
                    AND hd.thoiGianThanhToan >= {startDate} AND hd.thoiGianThanhToan < {endDate}
                    GROUP BY cthd.idSanPham
                )
                SELECT CAST(ISNULL(SUM(
                    spb.TongSoLuongBan * (dl.SoLuongSuDung / dvcd.GiaTriQuyDoi) * gv.GiaVonTrungBinh
                ), 0) AS DECIMAL(18,2))
                FROM SanPhamDaBan spb
                JOIN dbo.DinhLuong dl ON spb.idSanPham = dl.idSanPham
                JOIN GiaVonNguyenLieu gv ON dl.idNguyenLieu = gv.idNguyenLieu
                JOIN dbo.DonViChuyenDoi dvcd ON dl.idDonViSuDung = dvcd.idChuyenDoi;
            ").ToListAsync();

            decimal tongGiaVon_COGS = cogsResult.FirstOrDefault();

            // 3. TÍNH CHI PHÍ VẬN HÀNH (OPEX)
            var opexResult = (await _context.Database.SqlQuery<QuanLyOpexDto>($@"
                    SELECT 
                    CAST(ISNULL((SELECT SUM(thucLanh) 
                        FROM dbo.PhieuLuong
                        WHERE trangThai = N'Đã thanh toán'
                        AND ngayTao >= {startDate} AND ngayTao < {endDate}
                    ), 0) AS DECIMAL(18,2)) AS TongChiPhiLuong,
                    
                    CAST(ISNULL((SELECT SUM(TongGiaTriHuy) 
                        FROM dbo.PhieuXuatHuy
                        WHERE NgayXuatHuy >= {startDate} AND NgayXuatHuy < {endDate}
                    ), 0) AS DECIMAL(18,2)) AS TongChiPhiHuyHang;
                ").AsNoTracking().ToListAsync()).FirstOrDefault() ?? new QuanLyOpexDto();

            // 4. TOP SẢN PHẨM BÁN CHẠY
            var topSanPham = await _context.Database.SqlQuery<QuanLyTopSanPhamDto>($@"
                    SELECT TOP 10
                        sp.tenSanPham AS TenSanPham,
                        ISNULL(SUM(cthd.soLuong), 0) AS TongSoLuongBan,
                        CAST(ISNULL(SUM(cthd.thanhTien), 0) AS DECIMAL(18,2)) AS TongDoanhThu
                    FROM dbo.ChiTietHoaDon cthd
                    JOIN dbo.SanPham sp ON cthd.idSanPham = sp.idSanPham
                    JOIN dbo.HoaDon hd ON cthd.idHoaDon = hd.idHoaDon
                    WHERE hd.trangThai = N'Đã thanh toán'
                    AND hd.thoiGianThanhToan >= {startDate} AND hd.thoiGianThanhToan < {endDate}
                    GROUP BY sp.tenSanPham
                    ORDER BY TongSoLuongBan DESC;
                ").AsNoTracking().ToListAsync();

            var goiYDoanhThu = await _context.Database.SqlQuery<QuanLyGoiYDoanhThuDto>($@"
                WITH GiaVonNguyenLieu AS (
                    SELECT idNguyenLieu, AVG(donGiaNhap) AS GiaVonTrungBinh
                    FROM dbo.ChiTietNhapKho GROUP BY idNguyenLieu
                ),
                GiaVonSanPham AS (
                    SELECT 
                        dl.idSanPham, 
                        SUM((dl.SoLuongSuDung / dvcd.GiaTriQuyDoi) * gv.GiaVonTrungBinh) AS TongGiaVon
                    FROM dbo.DinhLuong dl
                    JOIN GiaVonNguyenLieu gv ON dl.idNguyenLieu = gv.idNguyenLieu
                    JOIN dbo.DonViChuyenDoi dvcd ON dl.idDonViSuDung = dvcd.idChuyenDoi
                    GROUP BY dl.idSanPham
                )
                SELECT 
                    sp.tenSanPham AS TenSanPham,
                    CAST(ISNULL(gvs.TongGiaVon, 0) AS DECIMAL(18,2)) AS GiaVon,
                    CAST(sp.giaBan AS DECIMAL(18,2)) AS GiaBanHienTai,
                    CAST(
                        CASE 
                            -- Nếu có giá vốn thì Giá Gợi ý = Giá vốn / 30% (Làm tròn nghìn đồng)
                            WHEN ISNULL(gvs.TongGiaVon, 0) > 0 THEN ROUND((gvs.TongGiaVon / 0.3) / 1000, 0) * 1000 
                            ELSE sp.giaBan
                        END 
                    AS DECIMAL(18,2)) AS GiaGoiY,
                    CAST(
                        CASE 
                            -- Tính biên lợi nhuận hiện tại (%)
                            WHEN sp.giaBan > 0 THEN ((sp.giaBan - ISNULL(gvs.TongGiaVon, 0)) / sp.giaBan) * 100
                            ELSE 0
                        END
                    AS DECIMAL(18,2)) AS TiLeLoiNhuanCu
                FROM dbo.SanPham sp
                LEFT JOIN GiaVonSanPham gvs ON sp.idSanPham = gvs.idSanPham
                WHERE sp.trangThaiKinhDoanh = 1;
            ").ToListAsync();

            // 5. TỔNG HỢP KẾT QUẢ VÀ TÍNH KPI
            var dto = new QuanLyBaoCaoTongHopDto
            {
                ChiTietDoanhThu = chiTietDoanhThu,
                ChiTietChiPhi = new QuanLyBaoCaoChiPhiDto
                {
                    TongGiaVon_COGS = tongGiaVon_COGS,
                    TongChiPhiLuong = opexResult.TongChiPhiLuong,
                    TongChiPhiHuyHang = opexResult.TongChiPhiHuyHang
                },
                TopSanPham = topSanPham,
                GoiYDoanhThu = goiYDoanhThu, // THÊM DÒNG NÀY ĐỂ GÁN DỮ LIỆU
                Kpi = new QuanLyBaoCaoKpiDto()
            };

            dto.Kpi.DoanhThuRong = dto.ChiTietDoanhThu.DoanhThuRong;
            dto.Kpi.TongGiaVon = dto.ChiTietChiPhi.TongGiaVon_COGS;
            dto.Kpi.LoiNhuanGop = dto.Kpi.DoanhThuRong - dto.Kpi.TongGiaVon;
            dto.Kpi.ChiPhiOpex = dto.ChiTietChiPhi.TongChiPhiLuong + dto.ChiTietChiPhi.TongChiPhiHuyHang;
            dto.Kpi.LoiNhuanRong = dto.Kpi.LoiNhuanGop - dto.Kpi.ChiPhiOpex;

            return Ok(dto);
        }
    }
}