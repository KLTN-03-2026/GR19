// File: CafebookApi/Controllers/App/QuanLy/QuanLyBaoCaoDoanhThuController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly/baocaodoanhthu")]
    [ApiController]
    [Authorize]
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

            // Trích xuất kỳ báo cáo để đồng bộ với thuật toán của Báo Cáo Nhân Sự
            var startMonth = startDate.Month;
            var startYear = startDate.Year;
            var endMonth = request.EndDate.Date.Month;
            var endYear = request.EndDate.Date.Year;

            // ---------------------------------------------------------
            // 1. TÍNH DOANH THU BÁN HÀNG VÀ TỔNG HÓA ĐƠN
            // ---------------------------------------------------------
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

            // ---------------------------------------------------------
            // 1.1. TÍNH DOANH THU TỪ PHÍ THUÊ SÁCH & TIỀN PHẠT
            // ---------------------------------------------------------
            var thueSachResult = await _context.Database.SqlQuery<decimal>($@"
                SELECT CAST(ISNULL(SUM(TongPhiThue + TongTienPhat), 0) AS DECIMAL(18,2))
                FROM dbo.PhieuTraSach
                WHERE NgayTra >= {startDate} AND NgayTra < {endDate};
            ").ToListAsync();

            decimal doanhThuThueSach = thueSachResult.FirstOrDefault();
            chiTietDoanhThu.TongDoanhThuThueSach = doanhThuThueSach;
            chiTietDoanhThu.DoanhThuRong += doanhThuThueSach;

            // ---------------------------------------------------------
            // 2. TÍNH TỔNG GIÁ VỐN HÀNG BÁN (COGS)
            // ---------------------------------------------------------
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
                    spb.TongSoLuongBan * (dl.SoLuongSuDung / ISNULL(dvcd.GiaTriQuyDoi, 1)) * ISNULL(gv.GiaVonTrungBinh, 0)
                ), 0) AS DECIMAL(18,2))
                FROM SanPhamDaBan spb
                JOIN dbo.DinhLuong dl ON spb.idSanPham = dl.idSanPham
                LEFT JOIN GiaVonNguyenLieu gv ON dl.idNguyenLieu = gv.idNguyenLieu
                LEFT JOIN dbo.DonViChuyenDoi dvcd ON dl.idDonViSuDung = dvcd.idChuyenDoi;
            ").ToListAsync();

            decimal tongGiaVon_COGS = cogsResult.FirstOrDefault();

            // ---------------------------------------------------------
            // 3. TÍNH CHI PHÍ VẬN HÀNH (OPEX)
            // ---------------------------------------------------------
            var opexResult = (await _context.Database.SqlQuery<QuanLyOpexDto>($@"
                    SELECT 
                    CAST(ISNULL((SELECT SUM(thucLanh) 
                        FROM dbo.PhieuLuong
                        WHERE ISNULL(trangThai, '') NOT LIKE N'%Hủy%'
                          AND ((nam > {startYear}) OR (nam = {startYear} AND thang >= {startMonth}))
                          AND ((nam < {endYear}) OR (nam = {endYear} AND thang <= {endMonth}))
                    ), 0) AS DECIMAL(18,2)) AS TongChiPhiLuong,
                    
                    CAST(ISNULL((SELECT SUM(TongGiaTriHuy) 
                        FROM dbo.PhieuXuatHuy
                        WHERE NgayXuatHuy >= {startDate} AND NgayXuatHuy < {endDate}
                    ), 0) AS DECIMAL(18,2)) AS TongChiPhiHuyHang;
                ").AsNoTracking().ToListAsync()).FirstOrDefault() ?? new QuanLyOpexDto();

            decimal totalOpex = opexResult.TongChiPhiLuong + opexResult.TongChiPhiHuyHang;

            // ---------------------------------------------------------
            // 4. TOP 10 SẢN PHẨM BÁN CHẠY NHẤT & TÍNH TỔNG SẢN PHẨM ĐÃ BÁN
            // ---------------------------------------------------------
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

            // Tính tổng số lượng sản phẩm đã bán toàn quán để phân bổ OPEX
            var totalItemsSoldResult = await _context.Database.SqlQuery<int>($@"
                SELECT ISNULL(SUM(cthd.soLuong), 0)
                FROM dbo.ChiTietHoaDon cthd
                JOIN dbo.HoaDon hd ON cthd.idHoaDon = hd.idHoaDon
                WHERE hd.trangThai = N'Đã thanh toán'
                AND hd.thoiGianThanhToan >= {startDate} AND hd.thoiGianThanhToan < {endDate}
            ").ToListAsync();

            int totalItemsSold = totalItemsSoldResult.FirstOrDefault();

            // Chi phí OPEX gánh trên mỗi một ly nước/sản phẩm bán ra
            decimal opexPerItem = totalItemsSold > 0 ? (totalOpex / totalItemsSold) : 0;

            // ---------------------------------------------------------
            // 5. GỢI Ý DOANH THU & PHÂN TÍCH GIÁ BÁN (NÂNG CẤP BAO GỒM OPEX)
            // ---------------------------------------------------------
            var goiYDoanhThu = await _context.Database.SqlQuery<QuanLyGoiYDoanhThuDto>($@"
                WITH GiaVonNguyenLieu AS (
                    SELECT idNguyenLieu, AVG(donGiaNhap) AS GiaVonTrungBinh
                    FROM dbo.ChiTietNhapKho GROUP BY idNguyenLieu
                ),
                GiaVonSanPham AS (
                    SELECT 
                        dl.idSanPham, 
                        SUM((dl.SoLuongSuDung / ISNULL(dvcd.GiaTriQuyDoi, 1)) * ISNULL(gv.GiaVonTrungBinh, 0)) AS TongGiaVon
                    FROM dbo.DinhLuong dl
                    LEFT JOIN GiaVonNguyenLieu gv ON dl.idNguyenLieu = gv.idNguyenLieu
                    LEFT JOIN dbo.DonViChuyenDoi dvcd ON dl.idDonViSuDung = dvcd.idChuyenDoi
                    GROUP BY dl.idSanPham
                )
                SELECT 
                    sp.tenSanPham AS TenSanPham,
                    CAST(ISNULL(gvs.TongGiaVon, 0) AS DECIMAL(18,2)) AS GiaVon,
                    CAST(sp.giaBan AS DECIMAL(18,2)) AS GiaBanHienTai,
                    CAST(
                        CASE 
                            -- Giá vốn thực tế = Giá vốn NVL + OPEX phân bổ trên mỗi món
                            -- Gợi ý giá bán mới đảm bảo tỷ suất Lợi Nhuận Ròng (Net Margin) 70%
                            WHEN (ISNULL(gvs.TongGiaVon, 0) + {opexPerItem}) > 0 
                            THEN ROUND(((ISNULL(gvs.TongGiaVon, 0) + {opexPerItem}) / 0.3) / 1000, 0) * 1000 
                            ELSE sp.giaBan
                        END 
                    AS DECIMAL(18,2)) AS GiaGoiY,
                    CAST(
                        CASE 
                            -- Tính Net Margin hiện tại: (Giá Bán - (Giá Vốn NVL + OPEX phân bổ)) / Giá Bán
                            WHEN sp.giaBan > 0 THEN ((sp.giaBan - (ISNULL(gvs.TongGiaVon, 0) + {opexPerItem})) / sp.giaBan) * 100
                            ELSE 0
                        END
                    AS DECIMAL(18,2)) AS TiLeLoiNhuanCu
                FROM dbo.SanPham sp
                LEFT JOIN GiaVonSanPham gvs ON sp.idSanPham = gvs.idSanPham
                WHERE sp.trangThaiKinhDoanh = 1;
            ").ToListAsync();

            // ---------------------------------------------------------
            // 6. TỔNG HỢP VÀ ĐÓNG GÓI DỮ LIỆU ĐỂ TRẢ VỀ FRONT-END
            // ---------------------------------------------------------
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
                GoiYDoanhThu = goiYDoanhThu,
                Kpi = new QuanLyBaoCaoKpiDto()
            };

            // Tính toán cây phân cấp Lợi nhuận
            dto.Kpi.DoanhThuRong = dto.ChiTietDoanhThu.DoanhThuRong;
            dto.Kpi.TongGiaVon = dto.ChiTietChiPhi.TongGiaVon_COGS;
            dto.Kpi.LoiNhuanGop = dto.Kpi.DoanhThuRong - dto.Kpi.TongGiaVon;
            dto.Kpi.ChiPhiOpex = dto.ChiTietChiPhi.TongChiPhiLuong + dto.ChiTietChiPhi.TongChiPhiHuyHang;
            dto.Kpi.LoiNhuanRong = dto.Kpi.LoiNhuanGop - dto.Kpi.ChiPhiOpex;

            return Ok(dto);
        }
    }
}