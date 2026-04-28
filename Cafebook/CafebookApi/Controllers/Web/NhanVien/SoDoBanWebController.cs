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

        // 1. SỬA LỖI: Thêm logic chặn khách vãng lai chiếm bàn đã đặt trước (Giống WPF)
        [HttpPost("createorder/{idBan}")]
        public async Task<IActionResult> CreateOrder(int idBan)
        {
            var idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            var ban = await _context.Bans.FindAsync(idBan);
            if (ban == null) return NotFound("Không tìm thấy bàn.");
            if (ban.TrangThai != "Trống" && ban.TrangThai != "Đã đặt") return Conflict("Bàn này đang bận hoặc bảo trì.");

            // Kiểm tra xem bàn có người đặt trong 1.5 tiếng tới không
            var now = DateTime.Now;
            var khoangThoiGianAnToan = now.AddMinutes(90);

            var phieuDatSapToi = await _context.PhieuDatBans
                .Where(p => p.IdBan == idBan && p.ThoiGianDat > now && p.ThoiGianDat <= khoangThoiGianAnToan &&
                           (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận"))
                .OrderBy(p => p.ThoiGianDat)
                .FirstOrDefaultAsync();

            if (ban.TrangThai == "Trống" && phieuDatSapToi != null)
                return Conflict($"Bàn này đã được đặt trước vào lúc {phieuDatSapToi.ThoiGianDat:HH:mm}. Không đủ thời gian nhận khách mới!");

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

            // Nếu khách đặt trước tới, cập nhật phiếu đặt thành Đã đến
            if (phieuDatSapToi != null && ban.TrangThai == "Đã đặt") phieuDatSapToi.TrangThai = "Đã đến";

            await _context.SaveChangesAsync();
            return Ok(new { idHoaDon = hoaDon.IdHoaDon });
        }

        // 2. SỬA LỖI: Cho phép "Hủy Bảo Trì" nếu gửi chuỗi rỗng
        [HttpPost("reportproblem/{idBan}")]
        public async Task<IActionResult> BaoCaoSuCo(int idBan, [FromBody] BaoCaoSuCoWebRequestDto request)
        {
            var idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            var ban = await _context.Bans.FindAsync(idBan);
            if (ban == null) return NotFound("Không tìm thấy bàn.");
            if (ban.TrangThai == "Có khách") return Conflict("Không thể thao tác bàn đang có khách.");

            // MỚI: Nếu nhân viên bấm "Mở lại bàn" (chuỗi gửi lên là rỗng)
            if (string.IsNullOrWhiteSpace(request.GhiChuSuCo))
            {
                ban.TrangThai = "Trống";
                ban.GhiChu = null;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Bàn đã sẵn sàng sử dụng." });
            }

            // Nếu là báo cáo sự cố thật
            ban.TrangThai = "Bảo trì";
            ban.GhiChu = $"[Sự cố NV báo]: {request.GhiChuSuCo}";

            _context.ThongBaos.Add(new ThongBao
            {
                IdNhanVienTao = idNhanVien,
                NoiDung = $"Bàn {ban.SoBan} bị báo cáo sự cố: {request.GhiChuSuCo}",
                LoaiThongBao = "SuCoBan",
                IdLienQuan = idBan,
                ThoiGianTao = DateTime.Now,
                DaXem = false
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Báo cáo sự cố thành công." });
        }

        // 3. THÊM MỚI: API Gộp Bàn (Đang bị thiếu)
        [HttpPost("merge-table")]
        public async Task<IActionResult> MergeTable([FromBody] BanActionWebRequestDto dto)
        {
            if (dto.IdHoaDonNguon == dto.IdHoaDonDich) return BadRequest("Không thể gộp bàn vào chính nó.");
            if (!dto.IdHoaDonDich.HasValue) return BadRequest("Không xác định được hóa đơn đích.");

            var hoaDonNguon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == dto.IdHoaDonNguon);
            if (hoaDonNguon == null) return NotFound("Không tìm thấy hóa đơn nguồn.");

            var hoaDonDich = await _context.HoaDons.FirstOrDefaultAsync(h => h.IdHoaDon == dto.IdHoaDonDich.Value);
            if (hoaDonDich == null) return NotFound("Không tìm thấy hóa đơn đích.");

            var banDich = await _context.Bans.FindAsync(dto.IdBanDich);

            // Chuyển Chi Tiết Hóa Đơn & Phụ thu
            var chiTiets = await _context.ChiTietHoaDons.Where(c => c.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var ct in chiTiets) ct.IdHoaDon = dto.IdHoaDonDich.Value;

            var phuThus = await _context.ChiTietPhuThuHoaDons.Where(p => p.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var pt in phuThus) { pt.IdHoaDon = dto.IdHoaDonDich.Value; }

            var cheBiens = await _context.TrangThaiCheBiens.Where(t => t.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var t in cheBiens) { t.IdHoaDon = dto.IdHoaDonDich.Value; t.SoBan = banDich?.SoBan ?? t.SoBan; }

            // Xóa Khuyến mãi cũ
            var kms = await _context.HoaDonKhuyenMais.Where(hk => hk.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            _context.HoaDonKhuyenMais.RemoveRange(kms);

            // Cộng dồn tiền
            hoaDonDich.TongTienGoc += hoaDonNguon.TongTienGoc;
            hoaDonDich.TongPhuThu += hoaDonNguon.TongPhuThu;
            hoaDonDich.GiamGia += hoaDonNguon.GiamGia;

            if (hoaDonNguon.Ban != null) hoaDonNguon.Ban.TrangThai = "Trống";
            _context.HoaDons.Remove(hoaDonNguon);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Gộp bàn thành công." });
        }

        // 4. THÊM MỚI: API Tạo Đơn Tại Quầy/Mang Về
        [HttpPost("createorder-no-table")]
        public async Task<IActionResult> CreateOrderNoTable([FromBody] string loaiHoaDon)
        {
            var idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();
            if (loaiHoaDon != "Mang về" && loaiHoaDon != "Tại quán") return BadRequest("Loại không hợp lệ.");

            var hoaDon = new HoaDon
            {
                IdBan = null,
                IdNhanVien = idNhanVien,
                TrangThai = "Chưa thanh toán",
                LoaiHoaDon = loaiHoaDon,
                ThoiGianTao = DateTime.Now
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();
            return Ok(new { idHoaDon = hoaDon.IdHoaDon });
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