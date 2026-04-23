// File: CafebookApi/Controllers/App/NhanVien/ChamCongController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/chamcong")]
    [ApiController]
    [Authorize]
    public class ChamCongController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ChamCongController(CafebookDbContext context) { _context = context; }

        [HttpGet("status/{idNhanVien}")]
        public async Task<IActionResult> GetChamCongStatus(int idNhanVien)
        {
            try
            {
                if (idNhanVien == 0) return BadRequest("Thiếu IdNhanVien.");

                // [1] QUÉT VÀ CHỐT CÁC CA QUÊN OUT TỪ HÔM QUA TRỞ VỀ TRƯỚC
                await AutoClosePastShifts(idNhanVien);

                return Ok(await GetDashboardDto(idNhanVien));
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpPost("clock-in/{idNhanVien}")]
        public async Task<IActionResult> ClockIn(int idNhanVien)
        {
            try
            {
                await AutoClosePastShifts(idNhanVien);

                var today = DateTime.Today;
                var now = DateTime.Now;

                var lichCaSapToi = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Where(l => l.IdNhanVien == idNhanVien && l.NgayLam == today && l.TrangThai == "Đã duyệt")
                    .OrderBy(l => l.CaLamViec.GioBatDau)
                    .FirstOrDefaultAsync(l => !_context.BangChamCongs.Any(c => c.IdLichLamViec == l.IdLichLamViec && c.GioVao.HasValue));

                if (lichCaSapToi == null)
                    return BadRequest("Bạn không có ca nào chờ vào làm lúc này hoặc đã hoàn thành tất cả các ca!");

                var settingVaoSom = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "HR_VaoCaSom_Phut");
                int phutVaoSom = (settingVaoSom != null && int.TryParse(settingVaoSom.GiaTri, out int vs)) ? vs : 30;

                var thoiDiemBatDauCa = lichCaSapToi.NgayLam.Add(lichCaSapToi.CaLamViec.GioBatDau);
                var thoiDiemChoPhepVaoCa = thoiDiemBatDauCa.AddMinutes(-phutVaoSom);

                if (now < thoiDiemChoPhepVaoCa)
                    return BadRequest($"Chưa đến giờ. Bạn chỉ được vào ca trước {phutVaoSom} phút (từ {thoiDiemChoPhepVaoCa:HH:mm}).");

                var chamCongMoi = new BangChamCong
                {
                    IdLichLamViec = lichCaSapToi.IdLichLamViec,
                    GioVao = now
                };

                _context.BangChamCongs.Add(chamCongMoi);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Vào ca thành công!" });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpPost("clock-out/{idNhanVien}")]
        public async Task<IActionResult> ClockOut(int idNhanVien)
        {
            try
            {
                var today = DateTime.Today;

                var chamCongDangMo = await _context.BangChamCongs
                    .Include(c => c.LichLamViec)
                    .FirstOrDefaultAsync(c => c.LichLamViec.IdNhanVien == idNhanVien
                                           && c.LichLamViec.NgayLam == today
                                           && c.GioVao.HasValue
                                           && !c.GioRa.HasValue);

                if (chamCongDangMo == null)
                    return BadRequest("Hệ thống không tìm thấy ca làm việc nào đang mở để ra ca!");

                var actualGioVao = chamCongDangMo.GioVao ?? DateTime.Now;
                var actualGioRa = DateTime.Now;

                var cacCaTrongNgay = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Where(l => l.IdNhanVien == idNhanVien && l.NgayLam == today && l.TrangThai == "Đã duyệt")
                    .OrderBy(l => l.CaLamViec.GioBatDau)
                    .ToListAsync();

                var chuoiCa = GetChuoiCaLienTiep(cacCaTrongNgay, chamCongDangMo.LichLamViec);

                foreach (var ca in chuoiCa)
                {
                    var shiftStart = ca.NgayLam.Add(ca.CaLamViec.GioBatDau);
                    var shiftEnd = ca.NgayLam.Add(ca.CaLamViec.GioKetThuc);
                    var isLastShiftInChain = (ca == chuoiCa.Last());

                    var bcc = await _context.BangChamCongs.FirstOrDefaultAsync(c => c.IdLichLamViec == ca.IdLichLamViec);
                    if (bcc == null)
                    {
                        if (actualGioRa <= shiftStart) break;

                        bcc = new BangChamCong
                        {
                            IdLichLamViec = ca.IdLichLamViec,
                            GioVao = shiftStart,
                            GhiChuSua = "Tự động chốt liên ca"
                        };
                        _context.BangChamCongs.Add(bcc);
                    }

                    // Quyết định giờ ra
                    if (actualGioRa >= shiftEnd)
                    {
                        bcc.GioRa = isLastShiftInChain ? actualGioRa : shiftEnd;

                        // [2] KIỂM TRA LÀM LỐ GIỜ (TĂNG CA) CHO CA CUỐI CÙNG
                        if (isLastShiftInChain)
                        {
                            var settingOT = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_TinhTangCa_Phut");
                            int phutOTQuyDinh = (settingOT != null && int.TryParse(settingOT.GiaTri, out int ot)) ? ot : 60;

                            var phutLamLho = (int)(actualGioRa - shiftEnd).TotalMinutes;

                            if (phutLamLho >= phutOTQuyDinh)
                            {
                                bcc.GhiChuSua = string.IsNullOrEmpty(bcc.GhiChuSua)
                                    ? $"Ghi nhận Tăng ca: {phutLamLho} phút"
                                    : bcc.GhiChuSua + $" | Tăng ca: {phutLamLho} phút";
                            }
                        }
                    }
                    else
                    {
                        bcc.GioRa = actualGioRa;
                        break;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Ra ca thành công!" });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        // =================================================================
        // [3] HÀM XỬ LÝ NHÂN VIÊN QUÊN RA CA QUA ĐÊM (MẤT TĂNG CA)
        // =================================================================
        private async Task AutoClosePastShifts(int idNhanVien)
        {
            var today = DateTime.Today;
            var caDangMoQuaKhu = await _context.BangChamCongs
                .Include(c => c.LichLamViec).ThenInclude(l => l.CaLamViec)
                .Where(c => c.LichLamViec.IdNhanVien == idNhanVien
                         && c.LichLamViec.NgayLam < today
                         && c.GioVao.HasValue
                         && !c.GioRa.HasValue)
                .ToListAsync();

            if (caDangMoQuaKhu.Any())
            {
                foreach (var ca in caDangMoQuaKhu)
                {
                    if (ca.LichLamViec?.CaLamViec != null)
                    {                        // Phạt: Ép giờ ra về đúng giờ kết thúc lý thuyết của ca đó (Mất OT)
                        ca.GioRa = ca.LichLamViec.NgayLam.Add(ca.LichLamViec.CaLamViec.GioKetThuc);

                        ca.GhiChuSua = string.IsNullOrEmpty(ca.GhiChuSua)
                            ? "Tự chốt (Quên Out ca qua đêm)"
                            : ca.GhiChuSua + " | Tự chốt qua đêm";
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task<ChamCongDashboardDto> GetDashboardDto(int idNhanVien)
        {
            var dto = new ChamCongDashboardDto();
            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            dto.TenNhanVien = nhanVien?.HoTen ?? "Nhân viên";

            var today = DateTime.Today;

            var lichHomNay = await _context.LichLamViecs
                .Include(l => l.CaLamViec)
                .Where(l => l.IdNhanVien == idNhanVien && l.NgayLam == today && l.TrangThai == "Đã duyệt")
                .OrderBy(l => l.CaLamViec.GioBatDau)
                .ToListAsync();

            if (!lichHomNay.Any())
            {
                dto.TrangThai = "KhongCoCa";
                return dto;
            }

            var listIdLich = lichHomNay.Select(l => l.IdLichLamViec).ToList();
            var chamCongs = await _context.BangChamCongs.Where(c => listIdLich.Contains(c.IdLichLamViec)).ToListAsync();

            var caDangLam = lichHomNay.FirstOrDefault(l => chamCongs.Any(c => c.IdLichLamViec == l.IdLichLamViec && c.GioVao.HasValue && !c.GioRa.HasValue));

            if (caDangLam != null)
            {
                var chuoiCa = GetChuoiCaLienTiep(lichHomNay, caDangLam);
                var chamCongHienTai = chamCongs.First(c => c.IdLichLamViec == caDangLam.IdLichLamViec && !c.GioRa.HasValue);

                dto.TrangThai = "DangTrongCa";
                dto.DangTrongCa = true;
                dto.TenCa = string.Join(" + ", chuoiCa.Select(c => c.CaLamViec.TenCa));
                dto.GioBatDauCa = caDangLam.CaLamViec.GioBatDau;
                dto.GioKetThucCa = chuoiCa.Last().CaLamViec.GioKetThuc;
                dto.LanVaoGanNhat = chamCongHienTai.GioVao;

                if (dto.LanVaoGanNhat.HasValue)
                    dto.TongGioLamHienTai = (decimal)(DateTime.Now - dto.LanVaoGanNhat.Value).TotalHours;
            }
            else
            {
                var caTiepTheo = lichHomNay.FirstOrDefault(l => !chamCongs.Any(c => c.IdLichLamViec == l.IdLichLamViec && c.GioVao.HasValue));

                if (caTiepTheo != null)
                {
                    var chuoiCa = GetChuoiCaLienTiep(lichHomNay, caTiepTheo);

                    dto.DangTrongCa = false;
                    dto.TenCa = string.Join(" + ", chuoiCa.Select(c => c.CaLamViec.TenCa));
                    dto.GioBatDauCa = caTiepTheo.CaLamViec.GioBatDau;
                    dto.GioKetThucCa = chuoiCa.Last().CaLamViec.GioKetThuc;

                    var settingVaoSom = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "HR_VaoCaSom_Phut");
                    int phutVaoSom = (settingVaoSom != null && int.TryParse(settingVaoSom.GiaTri, out int vs)) ? vs : 30;

                    var thoiDiemBatDau = caTiepTheo.NgayLam.Add(caTiepTheo.CaLamViec.GioBatDau);
                    if (DateTime.Now < thoiDiemBatDau.AddMinutes(-phutVaoSom))
                    {
                        dto.TrangThai = "ChuaDenGio";
                    }
                    else
                    {
                        dto.TrangThai = "ChoVaoCa";
                    }
                }
                else
                {
                    dto.TrangThai = "DaHoanThanh";
                    dto.DangTrongCa = false;
                    dto.TenCa = "Đã hoàn thành các ca trong ngày";
                }
            }

            var dauThangNay = new DateTime(today.Year, today.Month, 1);
            var settingTre = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatDiTre_Phut");
            var settingSom = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatRaSom_Phut");
            int phutTreChoPhep = (settingTre != null && int.TryParse(settingTre.GiaTri, out int t)) ? t : 10;
            int phutSomChoPhep = (settingSom != null && int.TryParse(settingSom.GiaTri, out int s)) ? s : 30;

            var lichSuThangNay = await _context.BangChamCongs
                .Include(c => c.LichLamViec).ThenInclude(l => l.CaLamViec).AsNoTracking()
                .Where(c => c.LichLamViec != null && c.LichLamViec.IdNhanVien == idNhanVien &&
                            c.LichLamViec.NgayLam >= dauThangNay && c.LichLamViec.NgayLam <= today)
                .ToListAsync();

            int soLanTre = 0, soLanSom = 0;
            foreach (var bc in lichSuThangNay)
            {
                if (bc.LichLamViec?.CaLamViec == null) continue;
                if (bc.GioVao.HasValue && (bc.GioVao.Value.TimeOfDay - bc.LichLamViec.CaLamViec.GioBatDau).TotalMinutes > phutTreChoPhep)
                    soLanTre++;
                if (bc.GioRa.HasValue && (bc.LichLamViec.CaLamViec.GioKetThuc - bc.GioRa.Value.TimeOfDay).TotalMinutes > phutSomChoPhep)
                    soLanSom++;
            }
            dto.SoLanDiTreThangNay = soLanTre;
            dto.SoLanVeSomThangNay = soLanSom;

            return dto;
        }

        [HttpGet("lich-su/{idNhanVien}")]
        public async Task<IActionResult> GetLichSuChamCong(int idNhanVien, [FromQuery] int thang, [FromQuery] int nam)
        {
            try
            {
                var settingTre = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatDiTre_Phut");
                var settingSom = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatRaSom_Phut");
                int phutTreChoPhep = (settingTre != null && int.TryParse(settingTre.GiaTri, out int t)) ? t : 10;
                int phutSomChoPhep = (settingSom != null && int.TryParse(settingSom.GiaTri, out int s)) ? s : 30;

                var start = new DateTime(nam, thang, 1);
                var end = start.AddMonths(1).AddDays(-1);

                var chamCongs = await _context.BangChamCongs
                    .Include(c => c.LichLamViec).ThenInclude(l => l.CaLamViec).AsNoTracking()
                    .Where(c => c.LichLamViec != null && c.LichLamViec.IdNhanVien == idNhanVien &&
                                c.LichLamViec.NgayLam >= start && c.LichLamViec.NgayLam <= end)
                    .OrderByDescending(c => c.LichLamViec.NgayLam)
                    .ThenBy(c => c.GioVao)
                    .ToListAsync();

                var items = new List<LichSuItemDto>();
                decimal tongGioThang = 0;
                int tongTre = 0, tongSom = 0;

                var grouped = chamCongs.GroupBy(c => new { c.LichLamViec.NgayLam, c.LichLamViec.CaLamViec.TenCa });

                foreach (var g in grouped)
                {
                    var firstVao = g.Min(x => x.GioVao);
                    var lastRa = g.Max(x => x.GioRa);
                    var caLam = g.First().LichLamViec.CaLamViec;

                    string treStr = "", somStr = "";

                    if (firstVao.HasValue && (firstVao.Value.TimeOfDay - caLam.GioBatDau).TotalMinutes > phutTreChoPhep)
                    {
                        treStr = $"Trễ {(int)(firstVao.Value.TimeOfDay - caLam.GioBatDau).TotalMinutes}p";
                        tongTre++;
                    }

                    if (lastRa.HasValue && (caLam.GioKetThuc - lastRa.Value.TimeOfDay).TotalMinutes > phutSomChoPhep)
                    {
                        somStr = $"Sớm {(int)(caLam.GioKetThuc - lastRa.Value.TimeOfDay).TotalMinutes}p";
                        tongSom++;
                    }

                    decimal gioLamCa = 0;
                    foreach (var c in g)
                    {
                        if (c.GioVao.HasValue && c.GioRa.HasValue)
                            gioLamCa += (decimal)(c.GioRa.Value - c.GioVao.Value).TotalHours;
                    }
                    tongGioThang += gioLamCa;

                    items.Add(new LichSuItemDto
                    {
                        Ngay = g.Key.NgayLam.ToString("dd/MM/yyyy"),
                        CaLamViec = g.Key.TenCa,
                        GioVaoNhanhNhat = firstVao?.ToString("HH:mm") ?? "--:--",
                        GioRaMuonNhat = lastRa?.ToString("HH:mm") ?? "--:--",
                        DiTre = treStr,
                        VeSom = somStr,
                        TongGioLam = gioLamCa,
                        SoLanRaVao = g.Count()
                    });
                }

                var thongKe = new ThongKeChamCongDto { TongGioLam = tongGioThang, SoLanDiTre = tongTre, SoLanVeSom = tongSom };
                return Ok(new LichSuChamCongPageDto { ThongKe = thongKe, LichSuChamCong = items });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        private List<LichLamViec> GetChuoiCaLienTiep(List<LichLamViec> cacCaTrongNgay, LichLamViec caBatDau)
        {
            var chuoiCa = new List<LichLamViec> { caBatDau };
            var current = caBatDau;
            var index = cacCaTrongNgay.FindIndex(l => l.IdLichLamViec == current.IdLichLamViec);

            if (index >= 0)
            {
                for (int i = index + 1; i < cacCaTrongNgay.Count; i++)
                {
                    var next = cacCaTrongNgay[i];
                    if ((next.CaLamViec.GioBatDau - current.CaLamViec.GioKetThuc).TotalMinutes <= 15)
                    {
                        chuoiCa.Add(next);
                        current = next;
                    }
                    else break;
                }
            }
            return chuoiCa;
        }
    }
}