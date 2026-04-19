using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/chamcong")]
    [ApiController]
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
                return Ok(await GetDashboardDto(idNhanVien));
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpPost("clock-in/{idNhanVien}")]
        public async Task<IActionResult> ClockIn(int idNhanVien)
        {
            try
            {
                var homNay = DateTime.Today;
                var now = DateTime.Now;

                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .FirstOrDefaultAsync(l => l.IdNhanVien == idNhanVien && l.NgayLam == homNay && l.TrangThai == "Đã duyệt");

                if (lichLamViec == null || lichLamViec.CaLamViec == null) return BadRequest("Bạn không có ca làm việc hôm nay.");

                // ĐIỀU KIỆN 1: CHỈ CHO PHÉP VÀO CA TRƯỚC TỐI ĐA 30 PHÚT
                var gioBatDauCa = homNay.Add(lichLamViec.CaLamViec.GioBatDau);
                var gioSomNhatDuocVao = gioBatDauCa.AddMinutes(-30);

                if (now < gioSomNhatDuocVao)
                {
                    return BadRequest($"Chưa đến giờ điểm danh. Hệ thống chỉ mở trước giờ vào ca 30 phút (Mở lúc {gioSomNhatDuocVao:HH:mm}).");
                }

                var phienDangMo = await _context.BangChamCongs
                    .FirstOrDefaultAsync(c => c.IdLichLamViec == lichLamViec.IdLichLamViec && c.GioRa == null);

                if (phienDangMo != null) return BadRequest("Bạn đang ở trong ca, vui lòng Ra Ca trước khi Vào Ca mới.");

                _context.BangChamCongs.Add(new BangChamCong
                {
                    IdLichLamViec = lichLamViec.IdLichLamViec,
                    GioVao = now
                });

                await _context.SaveChangesAsync();
                return Ok(await GetDashboardDto(idNhanVien));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("clock-out/{idNhanVien}")]
        public async Task<IActionResult> ClockOut(int idNhanVien)
        {
            try
            {
                var homNay = DateTime.Today;

                var phienDangMo = await _context.BangChamCongs
                    .Include(c => c.LichLamViec)
                    .FirstOrDefaultAsync(c =>
                        c.LichLamViec != null &&
                        c.LichLamViec.IdNhanVien == idNhanVien &&
                        c.LichLamViec.NgayLam == homNay &&
                        c.GioRa == null);

                if (phienDangMo == null || phienDangMo.GioVao == null)
                {
                    return BadRequest("Bạn chưa vào ca hoặc đã trả ca rồi.");
                }

                phienDangMo.GioRa = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(await GetDashboardDto(idNhanVien));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("lich-su/{idNhanVien}")]
        public async Task<IActionResult> GetLichSuChamCong(int idNhanVien, [FromQuery] int thang, [FromQuery] int nam)
        {
            try
            {
                var ngayDauThang = new DateTime(nam, thang, 1);
                var ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);

                // Lấy thông số từ Database Cài Đặt (Fallback mặc định: Trễ 10p, Về sớm 30p)
                var settingTre = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatDiTre_Phut");
                var settingSom = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatRaSom_Phut");

                int phutTreChoPhep = (settingTre != null && int.TryParse(settingTre.GiaTri, out int t)) ? t : 10;
                int phutSomChoPhep = (settingSom != null && int.TryParse(settingSom.GiaTri, out int s)) ? s : 30;

                var lichSuGrouped = await _context.BangChamCongs
                    .Include(c => c.LichLamViec).ThenInclude(l => l.CaLamViec)
                    .Where(c => c.LichLamViec != null && c.LichLamViec.IdNhanVien == idNhanVien &&
                                c.LichLamViec.NgayLam >= ngayDauThang && c.LichLamViec.NgayLam <= ngayCuoiThang)
                    .AsNoTracking()
                    .ToListAsync();

                var lichSuDto = lichSuGrouped
                    .GroupBy(c => c.LichLamViec)
                    .Select(g =>
                    {
                        var lich = g.Key;
                        var minGioVao = g.Min(c => c.GioVao);
                        var maxGioRa = g.Max(c => c.GioRa);
                        var tongGio = g.Sum(c => c.SoGioLam ?? 0);
                        int soPhien = g.Count();

                        // TÍNH ĐI TRỄ (Dựa trên lần vào ca Đầu Tiên)
                        string diTreText = "";
                        if (minGioVao.HasValue && lich.CaLamViec != null)
                        {
                            var timeTrangThai = minGioVao.Value.TimeOfDay - lich.CaLamViec.GioBatDau;
                            if (timeTrangThai.TotalMinutes > phutTreChoPhep) diTreText = $"{timeTrangThai.TotalMinutes:N0} phút";
                        }

                        // TÍNH VỀ SỚM (Dựa trên lần ra ca Cuối Cùng)
                        string veSomText = "";
                        if (maxGioRa.HasValue && lich.CaLamViec != null)
                        {
                            var timeVeSom = lich.CaLamViec.GioKetThuc - maxGioRa.Value.TimeOfDay;
                            if (timeVeSom.TotalMinutes > phutSomChoPhep) veSomText = $"{timeVeSom.TotalMinutes:N0} phút";
                        }

                        return new LichSuItemDto
                        {
                            Ngay = lich.NgayLam.ToString("dd/MM/yyyy"),
                            CaLamViec = lich.CaLamViec?.TenCa ?? "N/A",
                            GioVaoNhanhNhat = minGioVao?.ToString("HH:mm") ?? "--",
                            GioRaMuonNhat = maxGioRa?.ToString("HH:mm") ?? "Đang làm",
                            DiTre = diTreText,
                            VeSom = veSomText,
                            TongGioLam = tongGio,
                            SoLanRaVao = soPhien
                        };
                    })
                    .OrderByDescending(x => x.Ngay)
                    .ToList();

                return Ok(new LichSuChamCongPageDto
                {
                    LichSuChamCong = lichSuDto,
                    ThongKe = new ThongKeChamCongDto
                    {
                        TongGioLam = lichSuDto.Sum(l => l.TongGioLam),
                        SoLanDiTre = lichSuDto.Count(l => !string.IsNullOrEmpty(l.DiTre)),
                        SoLanVeSom = lichSuDto.Count(l => !string.IsNullOrEmpty(l.VeSom))
                    }
                });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        private async Task<ChamCongDashboardDto> GetDashboardDto(int idNhanVien)
        {
            var homNay = DateTime.Today;
            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);

            var dto = new ChamCongDashboardDto { TenNhanVien = nhanVien?.HoTen ?? "N/A" };

            var lichLamViec = await _context.LichLamViecs.Include(l => l.CaLamViec).AsNoTracking()
                .FirstOrDefaultAsync(l => l.IdNhanVien == idNhanVien && l.NgayLam == homNay && l.TrangThai == "Đã duyệt");

            if (lichLamViec == null || lichLamViec.CaLamViec == null)
            {
                dto.TrangThai = "KhongCoCa";
                dto.TenCa = "Không có lịch làm";
                return dto;
            }

            dto.TenCa = lichLamViec.CaLamViec.TenCa;
            dto.GioBatDauCa = lichLamViec.CaLamViec.GioBatDau;
            dto.GioKetThucCa = lichLamViec.CaLamViec.GioKetThuc;

            var cacPhien = await _context.BangChamCongs.AsNoTracking()
                .Where(c => c.IdLichLamViec == lichLamViec.IdLichLamViec).ToListAsync();

            if (!cacPhien.Any())
            {
                dto.TrangThai = "ChuaChamCong";
                dto.DangTrongCa = false;
            }
            else
            {
                dto.TongGioLamHienTai = cacPhien.Sum(c => c.SoGioLam ?? 0);
                dto.LanVaoGanNhat = cacPhien.Max(c => c.GioVao);
                dto.LanRaGanNhat = cacPhien.Max(c => c.GioRa);

                var phienDangMo = cacPhien.FirstOrDefault(c => c.GioRa == null);
                if (phienDangMo != null)
                {
                    dto.TrangThai = "DaChamCong";
                    dto.DangTrongCa = true;
                    dto.LanVaoGanNhat = phienDangMo.GioVao;
                }
                else
                {
                    dto.TrangThai = "DaTraCa";
                    dto.DangTrongCa = false;
                }
            }

            // Tải dữ liệu về RAM để đếm nhanh số lần đi trễ / về sớm trong tháng
            var dauThangNay = new DateTime(homNay.Year, homNay.Month, 1);

            var settingTre = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatDiTre_Phut");
            var settingSom = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatRaSom_Phut");
            int phutTreChoPhep = (settingTre != null && int.TryParse(settingTre.GiaTri, out int t)) ? t : 10;
            int phutSomChoPhep = (settingSom != null && int.TryParse(settingSom.GiaTri, out int s)) ? s : 30;

            var lichSuThangNay = await _context.BangChamCongs
                .Include(c => c.LichLamViec).ThenInclude(l => l.CaLamViec).AsNoTracking()
                .Where(c => c.LichLamViec != null && c.LichLamViec.IdNhanVien == idNhanVien &&
                            c.LichLamViec.NgayLam >= dauThangNay && c.LichLamViec.NgayLam <= homNay)
                .ToListAsync();

            var groupedByCa = lichSuThangNay.GroupBy(c => c.LichLamViec);

            dto.SoLanDiTreThangNay = groupedByCa.Count(g => {
                var minVao = g.Min(c => c.GioVao);
                return minVao.HasValue && g.Key.CaLamViec != null &&
                       (minVao.Value.TimeOfDay - g.Key.CaLamViec.GioBatDau).TotalMinutes > phutTreChoPhep;
            });

            dto.SoLanVeSomThangNay = groupedByCa.Count(g => {
                var maxRa = g.Max(c => c.GioRa);
                return maxRa.HasValue && g.Key.CaLamViec != null &&
                       (g.Key.CaLamViec.GioKetThuc - maxRa.Value.TimeOfDay).TotalMinutes > phutSomChoPhep;
            });

            return dto;
        }
    }
}