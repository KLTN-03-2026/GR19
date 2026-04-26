using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class DangKyLichWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public DangKyLichWebController(CafebookDbContext context)
        {
            _context = context;
        }

        // Lấy cấu hình vẽ bảng Kanban
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            var moCuaStr = await _context.CaiDats.Where(c => c.TenCaiDat == "ThongTin_GioMoCua").Select(c => c.GiaTri).FirstOrDefaultAsync();
            var dongCuaStr = await _context.CaiDats.Where(c => c.TenCaiDat == "ThongTin_GioDongCua").Select(c => c.GiaTri).FirstOrDefaultAsync();
            var thuMoCuaStr = await _context.CaiDats.Where(c => c.TenCaiDat == "ThongTin_ThuMoCua").Select(c => c.GiaTri).FirstOrDefaultAsync();

            var config = new DangKyLichConfigWebDto
            {
                GioMoCua = !string.IsNullOrEmpty(moCuaStr) ? TimeSpan.Parse(moCuaStr) : new TimeSpan(7, 0, 0),
                GioDongCua = !string.IsNullOrEmpty(dongCuaStr) ? TimeSpan.Parse(dongCuaStr) : new TimeSpan(22, 0, 0),
                ThuMoCua = !string.IsNullOrEmpty(thuMoCuaStr)
                            ? thuMoCuaStr.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                            : new List<int> { 2, 3, 4, 5, 6, 7, 8 }
            };

            return Ok(config);
        }

        [HttpGet("weekly-shifts")]
        public async Task<IActionResult> GetWeeklyShifts([FromQuery] DateTime monday)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int idNhanVien)) return Unauthorized();

                var myInfo = await _context.NhanViens.FindAsync(idNhanVien);
                if (myInfo == null) return Unauthorized();

                var sunday = monday.AddDays(6);
                var today = DateTime.Now.Date;
                var timeNow = DateTime.Now.TimeOfDay;

                var list = await _context.NhuCauCaLams
                    .Include(n => n.CaLamViec)
                    .Include(n => n.VaiTro)
                    .Where(n => n.NgayLam.Date >= monday.Date && n.NgayLam.Date <= sunday.Date && n.IdVaiTro == myInfo.IdVaiTro)
                    .Select(n => new
                    {
                        NhuCau = n,
                        SoLuongDaDuyet = _context.LichLamViecs.Count(l => l.IdCa == n.IdCa && l.NgayLam.Date == n.NgayLam.Date && l.TrangThai == "Đã duyệt"),
                        LichCuaToi = _context.LichLamViecs.FirstOrDefault(l => l.IdNhanVien == idNhanVien && l.IdCa == n.IdCa && l.NgayLam.Date == n.NgayLam.Date)
                    })
                    .ToListAsync();

                var dtoList = list.Select(x => new NhuCauLichWebDto
                {
                    IdNhuCau = x.NhuCau.IdNhuCau,
                    NgayLam = x.NhuCau.NgayLam,
                    IdCa = x.NhuCau.IdCa,
                    TenCa = x.NhuCau.CaLamViec.TenCa,
                    GioBatDau = x.NhuCau.CaLamViec.GioBatDau,
                    GioKetThuc = x.NhuCau.CaLamViec.GioKetThuc,
                    TenVaiTroYeuCau = x.NhuCau.VaiTro.TenVaiTro,
                    SoLuongCan = x.NhuCau.SoLuongCan,
                    SoLuongDaDangKy = x.SoLuongDaDuyet,
                    GhiChu = x.NhuCau.GhiChu,
                    TrangThaiCuaToi = x.LichCuaToi?.TrangThai ?? "Chưa đăng ký",
                    IdLichCuaToi = x.LichCuaToi?.IdLichLamViec,
                    GhiChuCuaToi = x.LichCuaToi?.GhiChu,
                    IsQuaHan = (x.NhuCau.NgayLam.Date < today) ||
                               (x.NhuCau.NgayLam.Date == today && x.NhuCau.CaLamViec.GioBatDau <= timeNow)
                }).OrderBy(x => x.NgayLam).ThenBy(x => x.GioBatDau).ToList();

                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách: " + ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterShift([FromBody] DangKyCaRequestDto req)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int idNhanVien)) return Unauthorized();

                var nhuCau = await _context.NhuCauCaLams.Include(n => n.CaLamViec).FirstOrDefaultAsync(n => n.IdNhuCau == req.IdNhuCau);
                if (nhuCau == null) return NotFound(new { message = "Không tìm thấy nhu cầu ca làm này." });

                if (nhuCau.NgayLam.Date < DateTime.Now.Date || (nhuCau.NgayLam.Date == DateTime.Now.Date && nhuCau.CaLamViec.GioBatDau <= DateTime.Now.TimeOfDay))
                    return Conflict(new { message = "Ca làm việc này đã quá hạn đăng ký." });

                var exists = await _context.LichLamViecs.AnyAsync(l => l.IdNhanVien == idNhanVien && l.IdCa == nhuCau.IdCa && l.NgayLam.Date == nhuCau.NgayLam.Date);
                if (exists) return Conflict(new { message = "Bạn đã đăng ký ca làm này rồi." });

                var caCungNgay = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Where(l => l.IdNhanVien == idNhanVien && l.NgayLam.Date == nhuCau.NgayLam.Date)
                    .ToListAsync();

                foreach (var lich in caCungNgay)
                {
                    if (nhuCau.CaLamViec.GioBatDau < lich.CaLamViec.GioKetThuc && nhuCau.CaLamViec.GioKetThuc > lich.CaLamViec.GioBatDau)
                    {
                        return Conflict(new { message = $"Trùng thời gian với '{lich.CaLamViec.TenCa}' ({lich.CaLamViec.GioBatDau:hh\\:mm}-{lich.CaLamViec.GioKetThuc:hh\\:mm}) mà bạn đã đăng ký." });
                    }
                }

                var lichMoi = new LichLamViec
                {
                    IdNhanVien = idNhanVien,
                    IdCa = nhuCau.IdCa,
                    NgayLam = nhuCau.NgayLam.Date,
                    TrangThai = "Chờ duyệt",
                    GhiChu = req.GhiChuNhanVien
                };

                _context.LichLamViecs.Add(lichMoi);

                _context.ThongBaos.Add(new ThongBao
                {
                    IdNhanVienTao = idNhanVien,
                    NoiDung = $"Nhân viên vừa đăng ký ca mới ngày {nhuCau.NgayLam:dd/MM}.",
                    ThoiGianTao = DateTime.Now,
                    LoaiThongBao = "DangKyLichMoi",
                    DaXem = false
                });

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đăng ký thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Không thể lưu vào DB: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpDelete("cancel/{idLich}")]
        public async Task<IActionResult> CancelShift(int idLich)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int.TryParse(userIdStr, out int idNhanVien);

                var lich = await _context.LichLamViecs.FindAsync(idLich);
                if (lich == null) return NotFound(new { message = "Không tìm thấy lịch làm việc." });
                if (lich.IdNhanVien != idNhanVien) return Forbid();
                if (lich.TrangThai != "Chờ duyệt") return Conflict(new { message = "Chỉ có thể hủy ca đang chờ duyệt." });

                _context.LichLamViecs.Remove(lich);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã hủy yêu cầu đăng ký ca làm." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi hủy: " + ex.Message });
            }
        }
    }
}