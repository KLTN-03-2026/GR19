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
    [Authorize] // Bắt buộc Token hợp lệ
    public class SoDoBanWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public SoDoBanWebController(CafebookDbContext context)
        {
            _context = context;
        }

        // Lấy ID Nhân viên an toàn từ Token (Chống IDOR)
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int userId);
            return userId;
        }

        [HttpGet("khuvuc-list")]
        public async Task<IActionResult> GetKhuVucList()
        {
            var data = await _context.KhuVucs
                .AsNoTracking()
                .Select(k => new KhuVucWebDto
                {
                    IdKhuVuc = k.IdKhuVuc,
                    TenKhuVuc = k.TenKhuVuc
                }).OrderBy(k => k.TenKhuVuc).ToListAsync();
            return Ok(data);
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetSoDoBan()
        {
            var now = DateTime.Now;
            var nowPlus10Minutes = now.AddMinutes(10);

            var data = await _context.Bans.AsNoTracking().Select(b => new
            {
                Ban = b,
                HoaDonHienTai = _context.HoaDons.Where(h => h.IdBan == b.IdBan && h.TrangThai == "Chưa thanh toán")
                    .Select(h => new { h.IdHoaDon, h.ThanhTien }).FirstOrDefault(),
                PhieuDatSapToi = _context.PhieuDatBans.Where(p => p.IdBan == b.IdBan && p.ThoiGianDat > now &&
                    (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận")).OrderBy(p => p.ThoiGianDat).FirstOrDefault()
            })
            .Select(data => new BanSoDoWebDto
            {
                IdBan = data.Ban.IdBan,
                SoBan = data.Ban.SoBan,
                TrangThai = (data.Ban.TrangThai == "Trống" && data.PhieuDatSapToi != null && data.PhieuDatSapToi.ThoiGianDat <= nowPlus10Minutes)
                             ? "Đã đặt" : data.Ban.TrangThai,
                GhiChu = data.Ban.GhiChu,
                IdKhuVuc = data.Ban.IdKhuVuc,
                IdHoaDonHienTai = data.HoaDonHienTai != null ? data.HoaDonHienTai.IdHoaDon : null,
                TongTienHienTai = data.HoaDonHienTai != null ? data.HoaDonHienTai.ThanhTien : 0,
                ThongTinDatBan = (data.Ban.TrangThai == "Trống" && data.PhieuDatSapToi != null) ? $"Đặt lúc: {data.PhieuDatSapToi.ThoiGianDat:HH:mm}" : null
            }).OrderBy(b => b.SoBan).ToListAsync();

            return Ok(data);
        }

        [HttpPost("createorder/{idBan}")]
        public async Task<IActionResult> CreateOrder(int idBan)
        {
            var idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            var ban = await _context.Bans.FindAsync(idBan);
            if (ban == null) return NotFound("Không tìm thấy bàn.");
            if (ban.TrangThai != "Trống" && ban.TrangThai != "Đã đặt") return Conflict("Bàn này đang bận hoặc bảo trì.");

            var hoaDon = new HoaDon
            {
                IdBan = idBan,
                IdNhanVien = idNhanVien,
                TrangThai = "Chưa thanh toán",
                LoaiHoaDon = "Tại quán",
                ThoiGianTao = DateTime.Now
            };

            _context.HoaDons.Add(hoaDon);
            ban.TrangThai = "Có khách";
            await _context.SaveChangesAsync();

            return Ok(new { idHoaDon = hoaDon.IdHoaDon });
        }

        [HttpPost("reportproblem/{idBan}")]
        public async Task<IActionResult> BaoCaoSuCo(int idBan, [FromBody] BaoCaoSuCoWebRequestDto request)
        {
            var idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            var ban = await _context.Bans.FindAsync(idBan);
            if (ban == null) return NotFound("Không tìm thấy bàn.");
            if (ban.TrangThai == "Có khách") return Conflict("Không thể báo cáo sự cố bàn đang có khách.");

            ban.TrangThai = "Bảo trì";
            ban.GhiChu = $"[Sự cố NV báo]: {request.GhiChuSuCo}";

            var thongBao = new ThongBao
            {
                IdNhanVienTao = idNhanVien,
                NoiDung = $"Bàn {ban.SoBan} vừa được báo cáo sự cố: {request.GhiChuSuCo}",
                LoaiThongBao = "SuCoBan",
                IdLienQuan = idBan,
                ThoiGianTao = DateTime.Now,
                DaXem = false
            };

            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Báo cáo sự cố thành công." });
        }

        [HttpPost("move-table")]
        public async Task<IActionResult> MoveTable([FromBody] BanActionWebRequestDto dto)
        {
            // Logic giữ nguyên như App, nhưng nằm riêng trong controller Web
            var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == dto.IdHoaDonNguon);
            if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn nguồn.");

            var banDich = await _context.Bans.FindAsync(dto.IdBanDich);
            if (banDich == null) return NotFound("Không tìm thấy bàn đích.");
            if (banDich.TrangThai != "Trống") return Conflict("Bàn đích đang bận.");

            if (hoaDon.Ban != null) hoaDon.Ban.TrangThai = "Trống";
            banDich.TrangThai = "Có khách";
            hoaDon.IdBan = dto.IdBanDich;

            var trangThaiCheBiens = await _context.TrangThaiCheBiens.Where(t => t.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var t in trangThaiCheBiens) t.SoBan = banDich.SoBan;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Chuyển bàn thành công." });
        }
    }
}