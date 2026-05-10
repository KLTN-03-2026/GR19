using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

using NhanVienEntity = CafebookModel.Model.ModelEntities.NhanVien;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-luong")]
    [ApiController]
    [Authorize]
    public class QuanLyLuongController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyLuongController(CafebookDbContext context) { _context = context; }

        [HttpGet("thuong-phat-mau")]
        public async Task<IActionResult> GetThuongPhatMau()
        {
            try
            {
                var list = await _context.Set<ThuongPhatMau>().AsNoTracking().ToListAsync();
                var dtos = list.Select(m => new ThuongPhatMauLookupDto
                {
                    IdMau = m.IdMau,
                    TenMau = m.TenMau,
                    Loai = m.Loai,
                    SoTien = m.SoTien
                }).ToList();
                return Ok(dtos);
            }
            catch { return Ok(new List<ThuongPhatMauLookupDto>()); }
        }

        [HttpGet("preview")]
        public async Task<IActionResult> PreviewLuong([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            var configs = await _context.Set<CaiDat>().Where(c => c.TenCaiDat.StartsWith("HR_")).AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            int phutTreChoPhep = int.TryParse(configs.GetValueOrDefault("HR_PhatDiTre_Phut"), out var p1) ? p1 : 10;
            decimal phatTreMoiLan = decimal.TryParse(configs.GetValueOrDefault("HR_PhatDiTreMoiLan")?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p2) ? p2 : 5000m;
            int phutSomChoPhep = int.TryParse(configs.GetValueOrDefault("HR_PhatRaSom_Phut"), out var p3) ? p3 : 10;
            decimal phatSomMoiLan = decimal.TryParse(configs.GetValueOrDefault("HR_PhatVeSomMoiLan")?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p4) ? p4 : 6000m;

            double heSoOT = double.TryParse(configs.GetValueOrDefault("HR_HeSoOT")?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p5) ? p5 : 1.5;

            int phutTinhTangCa = int.TryParse(configs.GetValueOrDefault("HR_TinhTangCa_Phut"), out var p6) ? p6 : 60;
            double gioChuyenCan = double.TryParse(configs.GetValueOrDefault("HR_ChuyenCan_SoGio")?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p7) ? p7 : 120.0;
            decimal tienThuongChuyenCan = decimal.TryParse(configs.GetValueOrDefault("HR_ChuyenCan_TienThuong")?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p8) ? p8 : 500000m;

            var rawData = await (from l in _context.Set<LichLamViec>()
                                 join b in _context.Set<BangChamCong>() on l.IdLichLamViec equals b.IdLichLamViec
                                 join nv in _context.Set<NhanVienEntity>() on l.IdNhanVien equals nv.IdNhanVien
                                 join c in _context.Set<CaLamViec>() on l.IdCa equals c.IdCa
                                 where l.NgayLam >= tuNgay.Date && l.NgayLam <= denNgay.Date
                                 select new
                                 {
                                     l.IdNhanVien,
                                     nv.HoTen,
                                     nv.LuongCoBan,
                                     nv.TrangThaiLamViec, 
                                     b.GioVao,
                                     b.GioRa,
                                     c.GioBatDau,
                                     c.GioKetThuc,
                                     l.NgayLam
                                 }).ToListAsync();

            var thuongPhatThuCong = await _context.Set<PhieuThuongPhat>()
                .Where(p => p.IdPhieuLuong == null
                         && p.NgayTao.Month == tuNgay.Month
                         && p.NgayTao.Year == tuNgay.Year)
                .ToListAsync();

            var result = rawData.GroupBy(x => new { x.IdNhanVien, x.HoTen, x.LuongCoBan, x.TrangThaiLamViec }).Select(g =>
            {
                double tongGioChuan = 0, tongGioOT = 0;
                int soLanTre = 0, soLanSom = 0;

                foreach (var item in g)
                {
                    if (!item.GioVao.HasValue) continue;

                    DateTime gioRaThucTe;
                    if (!item.GioRa.HasValue)
                    {
                        gioRaThucTe = item.NgayLam.Add(item.GioKetThuc);
                        if (item.GioKetThuc < item.GioBatDau) gioRaThucTe = gioRaThucTe.AddDays(1);
                    }
                    else
                    {
                        gioRaThucTe = item.GioRa.Value;
                    }

                    if (gioRaThucTe <= item.GioVao.Value) continue;

                    double actualMins = (gioRaThucTe - item.GioVao.Value).TotalMinutes;
                    double shiftMins = (item.GioKetThuc - item.GioBatDau).TotalMinutes;
                    if (shiftMins < 0) shiftMins += 24 * 60;

                    if (item.GioVao.Value.TimeOfDay > item.GioBatDau.Add(TimeSpan.FromMinutes(phutTreChoPhep))) soLanTre++;
                    if (gioRaThucTe.TimeOfDay < item.GioKetThuc.Subtract(TimeSpan.FromMinutes(phutSomChoPhep))) soLanSom++;

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

                var dto = new QuanLyLuongBangKeDto
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
                    DanhSachThuongPhat = chiTiet.OrderBy(c => c.IsAuto ? 0 : 1).ToList()
                };

                return new { Dto = dto, TrangThai = g.Key.TrangThaiLamViec };
            })
            .Where(x =>
                (x.Dto.TongGioLamChuan > 0 || x.Dto.TongGioOT > 0) ||
                (x.TrangThai != "Nghỉ việc")
            )
            .Select(x => x.Dto)
            .ToList();

            return Ok(result);
        }

        [HttpPost("thuong-phat")]
        public async Task<IActionResult> AddThuongPhat([FromBody] TaoThuongPhatDto dto)
        {
            var ptp = new PhieuThuongPhat
            {
                IdNhanVien = dto.IdNhanVien,
                IdNguoiTao = dto.IdNguoiTao > 0 ? dto.IdNguoiTao : 1,
                NgayTao = dto.NgayTao ?? DateTime.Now,
                LyDo = dto.LyDo,
                SoTien = dto.Loai == "Phạt" ? -Math.Abs(dto.SoTien) : Math.Abs(dto.SoTien)
            };

            try
            {
                bool existsMau = await _context.Set<ThuongPhatMau>()
                    .AnyAsync(m => m.TenMau.ToLower() == dto.LyDo.ToLower() && m.Loai == dto.Loai);

                if (!existsMau)
                {
                    _context.Set<ThuongPhatMau>().Add(new ThuongPhatMau
                    {
                        Loai = dto.Loai,
                        TenMau = dto.LyDo,
                        SoTien = Math.Abs(dto.SoTien)
                    });
                }

                _context.Set<PhieuThuongPhat>().Add(ptp);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tạo phiếu: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPost("thuong-phat-hang-loat")]
        public async Task<IActionResult> AddThuongPhatHangLoat([FromBody] TaoThuongPhatHangLoatDto dto)
        {
            if (dto.IdNhanViens == null || !dto.IdNhanViens.Any())
                return BadRequest("Chưa chọn nhân viên nào để áp dụng.");

            try
            {
                bool existsMau = await _context.Set<ThuongPhatMau>()
                    .AnyAsync(m => m.TenMau.ToLower() == dto.LyDo.ToLower() && m.Loai == dto.Loai);

                if (!existsMau)
                {
                    _context.Set<ThuongPhatMau>().Add(new ThuongPhatMau
                    {
                        Loai = dto.Loai,
                        TenMau = dto.LyDo,
                        SoTien = Math.Abs(dto.SoTien)
                    });
                }

                foreach (var idNv in dto.IdNhanViens)
                {
                    var ptp = new PhieuThuongPhat
                    {
                        IdNhanVien = idNv,
                        IdNguoiTao = dto.IdNguoiTao > 0 ? dto.IdNguoiTao : 1,
                        NgayTao = dto.NgayTao ?? DateTime.Now,
                        LyDo = dto.LyDo,
                        SoTien = dto.Loai == "Phạt" ? -Math.Abs(dto.SoTien) : Math.Abs(dto.SoTien)
                    };
                    _context.Set<PhieuThuongPhat>().Add(ptp);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
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
                int countSuccess = 0;
                var listSkipped = new List<string>();

                foreach (var item in dto.DanhSachChot)
                {
                    // FIX: Bỏ qua những nhân viên đã chốt trong tháng này
                    bool isDaChot = await _context.Set<PhieuLuong>()
                        .AnyAsync(p => p.IdNhanVien == item.IdNhanVien && p.Thang == dto.TuNgay.Month && p.Nam == dto.TuNgay.Year);

                    if (isDaChot)
                    {
                        listSkipped.Add(item.TenNhanVien);
                        continue; // Lướt qua, không quăng lỗi
                    }

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

                    if (item.DanhSachThuongPhat != null && item.DanhSachThuongPhat.Any())
                    {
                        foreach (var tp in item.DanhSachThuongPhat)
                        {
                            if (tp.IsAuto)
                            {
                                var autoPhieu = new PhieuThuongPhat
                                {
                                    IdNhanVien = item.IdNhanVien,
                                    IdPhieuLuong = phieu.IdPhieuLuong,
                                    NgayTao = DateTime.Now,
                                    LyDo = tp.LyDo,
                                    SoTien = tp.Loai == "Phạt" ? -Math.Abs(tp.SoTien) : Math.Abs(tp.SoTien),
                                    IdNguoiTao = 1
                                };
                                _context.Set<PhieuThuongPhat>().Add(autoPhieu);
                            }
                            else
                            {
                                var existingManual = await _context.Set<PhieuThuongPhat>().FindAsync(tp.Id);
                                if (existingManual != null)
                                {
                                    existingManual.IdPhieuLuong = phieu.IdPhieuLuong;
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    countSuccess++;
                }

                await transaction.CommitAsync();

                string msg = $"Chốt thành công: {countSuccess} nhân viên.";
                if (listSkipped.Any()) msg += $"\nBỏ qua (đã chốt trước đó): {string.Join(", ", listSkipped)}";

                return Ok(new { message = msg });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}