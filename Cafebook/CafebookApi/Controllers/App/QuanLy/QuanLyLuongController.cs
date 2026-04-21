using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using NhanVienEntity = CafebookModel.Model.ModelEntities.NhanVien;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-luong")]
    [ApiController]
    public class QuanLyLuongController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyLuongController(CafebookDbContext context) { _context = context; }

        [HttpGet("thuong-phat-mau")]
        public async Task<IActionResult> GetThuongPhatMau()
        {
            try
            {
                var list = await _context.Set<PhieuThuongPhat>().AsNoTracking()
                    .Select(p => new { p.LyDo, p.SoTien })
                    .Distinct()
                    .ToListAsync();

                var dtos = list.Select((m, index) => new ThuongPhatMauLookupDto
                {
                    IdMau = index + 1,
                    TenMau = m.LyDo,
                    Loai = m.SoTien >= 0 ? "Thưởng" : "Phạt",
                    SoTien = Math.Abs(m.SoTien)
                }).ToList();

                return Ok(dtos);
            }
            catch { return Ok(new List<ThuongPhatMauLookupDto>()); }
        }

        [HttpGet("preview")]
        public async Task<IActionResult> PreviewLuong([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            // 1. Lấy toàn bộ tham số cài đặt và ép kiểu an toàn
            var configs = await _context.Set<CaiDat>().Where(c => c.TenCaiDat.StartsWith("HR_")).AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            int phutTreChoPhep = int.TryParse(configs.GetValueOrDefault("HR_PhatDiTre_Phut"), out var p1) ? p1 : 10;
            decimal phatTreMoiLan = decimal.TryParse(configs.GetValueOrDefault("HR_PhatDiTreMoiLan"), out var p2) ? p2 : 5000m;
            int phutSomChoPhep = int.TryParse(configs.GetValueOrDefault("HR_PhatRaSom_Phut"), out var p3) ? p3 : 10;
            decimal phatSomMoiLan = decimal.TryParse(configs.GetValueOrDefault("HR_PhatVeSomMoiLan"), out var p4) ? p4 : 6000m;

            double heSoOT = double.TryParse(configs.GetValueOrDefault("HR_HeSoOT"), out var p5) ? p5 : 1.5;
            int phutTinhTangCa = int.TryParse(configs.GetValueOrDefault("HR_TinhTangCa_Phut"), out var p6) ? p6 : 60;

            double gioChuyenCan = double.TryParse(configs.GetValueOrDefault("HR_ChuyenCan_SoGio"), out var p7) ? p7 : 120.0;
            decimal tienThuongChuyenCan = decimal.TryParse(configs.GetValueOrDefault("HR_ChuyenCan_TienThuong"), out var p8) ? p8 : 500000m;

            var rawData = await (from l in _context.Set<LichLamViec>()
                                 join b in _context.Set<BangChamCong>() on l.IdLichLamViec equals b.IdLichLamViec
                                 join nv in _context.Set<NhanVienEntity>() on l.IdNhanVien equals nv.IdNhanVien
                                 join c in _context.Set<CaLamViec>() on l.IdCa equals c.IdCa
                                 where l.NgayLam >= tuNgay.Date && l.NgayLam <= denNgay.Date
                                 select new { l.IdNhanVien, nv.HoTen, nv.LuongCoBan, b.GioVao, b.GioRa, c.GioBatDau, c.GioKetThuc, l.NgayLam }).ToListAsync();

            var thuongPhatThuCong = await _context.Set<PhieuThuongPhat>()
                .Where(p => p.IdPhieuLuong == null && p.NgayTao >= tuNgay.Date && p.NgayTao <= denNgay.Date)
                .ToListAsync();

            var result = rawData.GroupBy(x => new { x.IdNhanVien, x.HoTen, x.LuongCoBan }).Select(g =>
            {
                double tongGioChuan = 0, tongGioOT = 0;
                int soLanTre = 0, soLanSom = 0;

                foreach (var item in g)
                {
                    if (!item.GioVao.HasValue) continue;

                    // Tự động chốt ca qua đêm nếu quên bấm giờ ra
                    DateTime gioRaThucTe;
                    if (!item.GioRa.HasValue)
                    {
                        gioRaThucTe = item.NgayLam.Add(item.GioKetThuc);
                        if (item.GioKetThuc < item.GioBatDau) gioRaThucTe = gioRaThucTe.AddDays(1); // Ca xuyên đêm
                    }
                    else
                    {
                        gioRaThucTe = item.GioRa.Value;
                    }

                    if (gioRaThucTe <= item.GioVao.Value) continue;

                    double actualMins = (gioRaThucTe - item.GioVao.Value).TotalMinutes;
                    double shiftMins = (item.GioKetThuc - item.GioBatDau).TotalMinutes;
                    if (shiftMins < 0) shiftMins += 24 * 60;

                    // Tính phạt trễ
                    if (item.GioVao.Value.TimeOfDay > item.GioBatDau.Add(TimeSpan.FromMinutes(phutTreChoPhep))) soLanTre++;
                    // Tính phạt sớm
                    if (gioRaThucTe.TimeOfDay < item.GioKetThuc.Subtract(TimeSpan.FromMinutes(phutSomChoPhep))) soLanSom++;

                    // Tính OT (chỉ tính nếu làm lố lớn hơn cấu hình HR_TinhTangCa_Phut)
                    if (actualMins - shiftMins >= phutTinhTangCa)
                    {
                        tongGioChuan += (shiftMins / 60.0);
                        tongGioOT += ((actualMins - shiftMins) / 60.0);
                    }
                    else
                    {
                        tongGioChuan += (Math.Min(actualMins, shiftMins) / 60.0);
                    }
                }

                decimal luongCoBan = g.Key.LuongCoBan;
                decimal tienChuan = (decimal)tongGioChuan * luongCoBan;
                decimal tienOT = (decimal)tongGioOT * luongCoBan * (decimal)heSoOT;
                decimal tienPhatTS = (soLanTre * phatTreMoiLan) + (soLanSom * phatSomMoiLan);
                decimal thuongCC = (tongGioChuan >= gioChuyenCan) ? tienThuongChuyenCan : 0;

                var chiTiet = new List<ChiTietThuongPhatDto>();
                if (tienOT > 0) chiTiet.Add(new ChiTietThuongPhatDto { Loai = "Thưởng", LyDo = $"Tăng ca {Math.Round(tongGioOT, 1)}h (Hệ thống)", SoTien = Math.Round(tienOT, 0), IsAuto = true });
                if (thuongCC > 0) chiTiet.Add(new ChiTietThuongPhatDto { Loai = "Thưởng", LyDo = "Chuyên cần tháng (Hệ thống)", SoTien = thuongCC, IsAuto = true });
                if (soLanTre > 0) chiTiet.Add(new ChiTietThuongPhatDto { Loai = "Phạt", LyDo = $"Đi trễ {soLanTre} lần (Hệ thống)", SoTien = soLanTre * phatTreMoiLan, IsAuto = true });
                if (soLanSom > 0) chiTiet.Add(new ChiTietThuongPhatDto { Loai = "Phạt", LyDo = $"Về sớm {soLanSom} lần (Hệ thống)", SoTien = soLanSom * phatSomMoiLan, IsAuto = true });

                // Tính toán và nạp các khoản Thủ Công
                decimal thuongTC = 0, phatTC = 0;
                foreach (var tp in thuongPhatThuCong.Where(p => p.IdNhanVien == g.Key.IdNhanVien))
                {
                    if (tp.SoTien >= 0)
                    {
                        thuongTC += tp.SoTien;
                        chiTiet.Add(new ChiTietThuongPhatDto { Id = tp.IdPhieuThuongPhat, Loai = "Thưởng", LyDo = tp.LyDo, SoTien = tp.SoTien, IsAuto = false });
                    }
                    else
                    {
                        phatTC += Math.Abs(tp.SoTien);
                        chiTiet.Add(new ChiTietThuongPhatDto { Id = tp.IdPhieuThuongPhat, Loai = "Phạt", LyDo = tp.LyDo, SoTien = Math.Abs(tp.SoTien), IsAuto = false });
                    }
                }

                return new QuanLyLuongBangKeDto
                {
                    IdNhanVien = g.Key.IdNhanVien,
                    TenNhanVien = g.Key.HoTen,
                    LuongCoBan = luongCoBan,
                    TongGioLamChuan = Math.Round(tongGioChuan, 2),
                    TongGioOT = Math.Round(tongGioOT, 2),
                    SoLanTre = soLanTre,
                    SoLanSom = soLanSom,
                    TienLuongChuan = Math.Round(tienChuan, 0),
                    TienThuongOT = Math.Round(tienOT, 0),
                    ThuongChuyenCan = Math.Round(thuongCC, 0),
                    TienPhatTreSom = Math.Round(tienPhatTS, 0),
                    ThuongThuCong = thuongTC,
                    PhatThuCong = phatTC,
                    DanhSachThuongPhat = chiTiet.OrderBy(c => c.IsAuto ? 0 : 1).ToList() // Xếp tự động lên trên, thủ công xuống dưới
                };
            }).Where(x => x.TongGioLamChuan > 0 || x.TongGioOT > 0).ToList();

            return Ok(result);
        }

        // LƯU Ý LỖI: Lỗi thêm khoản mới bị hỏng thường do IdNguoiTao không hợp lệ với Foreign Key của DB.
        [HttpPost("thuong-phat")]
        public async Task<IActionResult> AddThuongPhat([FromBody] TaoThuongPhatDto dto)
        {
            var ptp = new PhieuThuongPhat
            {
                IdNhanVien = dto.IdNhanVien,
                IdNguoiTao = dto.IdNguoiTao > 0 ? dto.IdNguoiTao : 1, // Đảm bảo luôn có giá trị hợp lệ
                NgayTao = DateTime.Now,
                LyDo = dto.LyDo,
                SoTien = dto.Loai == "Phạt" ? -Math.Abs(dto.SoTien) : Math.Abs(dto.SoTien)
            };

            try
            {
                _context.Set<PhieuThuongPhat>().Add(ptp);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                // Nếu dính lỗi FK từ IdNguoiTao, nó sẽ in ra đây để bạn debug
                return BadRequest($"Lỗi tạo phiếu: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("thuong-phat/{id}")]
        public async Task<IActionResult> DeleteThuongPhat(int id)
        {
            var ptp = await _context.Set<PhieuThuongPhat>().FindAsync(id);
            if (ptp == null || ptp.IdPhieuLuong != null) return BadRequest("Không thể xóa khoản đã chốt.");
            _context.Set<PhieuThuongPhat>().Remove(ptp);
            await _context.SaveChangesAsync(); return Ok();
        }

        [HttpPost("chot-luong")]
        public async Task<IActionResult> ChotLuong([FromBody] QuanLyLuongChotRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in dto.DanhSachChot)
                {
                    var phieu = new PhieuLuong
                    {
                        IdNhanVien = item.IdNhanVien,
                        Thang = dto.TuNgay.Month,
                        Nam = dto.TuNgay.Year,
                        LuongCoBan = item.LuongCoBan,
                        TongGioLam = (decimal)(item.TongGioLamChuan + item.TongGioOT),
                        TienThuong = item.TongThuong,
                        KhauTru = item.TongPhat,
                        ThucLanh = item.ThucLanh,
                        NgayTao = DateTime.Now,
                        TrangThai = "Chưa phát"
                    };
                    _context.Set<PhieuLuong>().Add(phieu);
                    await _context.SaveChangesAsync();

                    var manual = await _context.Set<PhieuThuongPhat>().Where(p => p.IdNhanVien == item.IdNhanVien && p.IdPhieuLuong == null).ToListAsync();
                    foreach (var m in manual) m.IdPhieuLuong = phieu.IdPhieuLuong;
                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();
                return Ok(new { message = "Chốt lương thành công." });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return StatusCode(500, ex.Message); }
        }
    }
}