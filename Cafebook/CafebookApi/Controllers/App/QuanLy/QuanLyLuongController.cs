using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-luong")]
    [ApiController]
    public class QuanLyLuongController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyLuongController(CafebookDbContext context) { _context = context; }

        // SỬA LỖI CS0246 TẠI ĐÂY: Quét danh sách Thưởng Phạt thủ công đã tạo làm Mẫu
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
            var configs = await _context.Set<CaiDat>().Where(c => c.TenCaiDat.StartsWith("HR_")).AsNoTracking().ToListAsync();
            double heSoOT = 1.5; double heSoPhatTre = 1.0; int phutTreChoPhep = 5;
            if (double.TryParse(configs.FirstOrDefault(c => c.TenCaiDat == "HR_HeSoOT")?.GiaTri, out double ot)) heSoOT = ot;
            if (double.TryParse(configs.FirstOrDefault(c => c.TenCaiDat == "HR_PhatDiTre_HeSo")?.GiaTri, out double pt)) heSoPhatTre = pt;
            if (int.TryParse(configs.FirstOrDefault(c => c.TenCaiDat == "HR_PhatDiTre_Phut")?.GiaTri, out int pl)) phutTreChoPhep = pl;

            var rawData = await (from l in _context.Set<LichLamViec>()
                                 join b in _context.Set<BangChamCong>() on l.IdLichLamViec equals b.IdLichLamViec
                                 join nv in _context.Set<NhanVien>() on l.IdNhanVien equals nv.IdNhanVien
                                 join c in _context.Set<CaLamViec>() on l.IdCa equals c.IdCa
                                 where l.NgayLam >= tuNgay.Date && l.NgayLam <= denNgay.Date
                                 select new { l.IdNhanVien, nv.HoTen, nv.LuongCoBan, b.GioVao, b.GioRa, c.GioBatDau, c.GioKetThuc }).ToListAsync();

            var thuongPhatThuCong = await _context.Set<PhieuThuongPhat>()
                .Where(p => p.IdPhieuLuong == null && p.NgayTao >= tuNgay.Date && p.NgayTao <= denNgay.Date)
                .ToListAsync();

            var result = rawData.GroupBy(x => new { x.IdNhanVien, x.HoTen, x.LuongCoBan }).Select(g =>
            {
                double tongGioChuan = 0, tongGioOT = 0, tongGioTre = 0;
                foreach (var item in g)
                {
                    if (!item.GioVao.HasValue || !item.GioRa.HasValue || item.GioRa.Value <= item.GioVao.Value) continue;
                    double actualMins = (item.GioRa.Value - item.GioVao.Value).TotalMinutes;
                    double shiftMins = (item.GioKetThuc - item.GioBatDau).TotalMinutes;
                    if (shiftMins < 0) shiftMins += 24 * 60;

                    if (item.GioVao.Value.TimeOfDay > item.GioBatDau)
                    {
                        double lateMins = (item.GioVao.Value.TimeOfDay - item.GioBatDau).TotalMinutes;
                        if (lateMins > phutTreChoPhep) tongGioTre += (lateMins / 60.0);
                    }
                    if (actualMins > shiftMins) { tongGioChuan += (shiftMins / 60.0); tongGioOT += ((actualMins - shiftMins) / 60.0); }
                    else { tongGioChuan += (actualMins / 60.0); }
                }

                decimal luongCoBan = g.Key.LuongCoBan;
                decimal tienChuan = (decimal)tongGioChuan * luongCoBan;
                decimal tienOT = (decimal)tongGioOT * luongCoBan * (decimal)heSoOT;
                decimal tienPhatTre = (decimal)tongGioTre * luongCoBan * (decimal)heSoPhatTre;

                var chiTiet = new List<ChiTietThuongPhatDto>();
                if (tienOT > 0) chiTiet.Add(new ChiTietThuongPhatDto { Loai = "Thưởng", LyDo = "Tăng ca (Tự động)", SoTien = Math.Round(tienOT, 0), IsAuto = true });
                if (tienPhatTre > 0) chiTiet.Add(new ChiTietThuongPhatDto { Loai = "Phạt", LyDo = "Đi trễ (Tự động)", SoTien = Math.Round(tienPhatTre, 0), IsAuto = true });

                decimal thuongTC = 0, phatTC = 0;
                foreach (var tp in thuongPhatThuCong.Where(p => p.IdNhanVien == g.Key.IdNhanVien))
                {
                    if (tp.SoTien >= 0) { thuongTC += tp.SoTien; chiTiet.Add(new ChiTietThuongPhatDto { Id = tp.IdPhieuThuongPhat, Loai = "Thưởng", LyDo = tp.LyDo, SoTien = tp.SoTien, IsAuto = false }); }
                    else { phatTC += Math.Abs(tp.SoTien); chiTiet.Add(new ChiTietThuongPhatDto { Id = tp.IdPhieuThuongPhat, Loai = "Phạt", LyDo = tp.LyDo, SoTien = Math.Abs(tp.SoTien), IsAuto = false }); }
                }

                return new QuanLyLuongBangKeDto
                {
                    IdNhanVien = g.Key.IdNhanVien,
                    TenNhanVien = g.Key.HoTen,
                    LuongCoBan = luongCoBan,
                    TongGioLamChuan = Math.Round(tongGioChuan, 2),
                    TongGioOT = Math.Round(tongGioOT, 2),
                    TongGioTre = Math.Round(tongGioTre, 2),
                    TienLuongChuan = Math.Round(tienChuan, 0),
                    TienThuongOT = Math.Round(tienOT, 0),
                    TienPhatTre = Math.Round(tienPhatTre, 0),
                    ThuongThuCong = thuongTC,
                    PhatThuCong = phatTC,
                    DanhSachThuongPhat = chiTiet
                };
            }).Where(x => x.TongGioLamChuan > 0 || x.TongGioOT > 0).ToList();

            return Ok(result);
        }

        [HttpPost("thuong-phat")]
        public async Task<IActionResult> AddThuongPhat([FromBody] TaoThuongPhatDto dto)
        {
            var ptp = new PhieuThuongPhat
            {
                IdNhanVien = dto.IdNhanVien,
                IdNguoiTao = 1,
                NgayTao = DateTime.Now,
                LyDo = dto.LyDo,
                SoTien = dto.Loai == "Phạt" ? -Math.Abs(dto.SoTien) : Math.Abs(dto.SoTien)
            };
            _context.Set<PhieuThuongPhat>().Add(ptp);
            await _context.SaveChangesAsync(); return Ok();
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