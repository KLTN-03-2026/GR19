using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using CafebookModel.Utils;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/giaohang")]
    [ApiController]
    [Authorize]
    public class GiaoHangController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public GiaoHangController(CafebookDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "IdNhanVien" || c.Type == ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int idNhanVien)) return idNhanVien;
            return 0;
        }

        private async Task<Dictionary<string, string>> GetGeneralSettingsAsync()
        {
            return await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri ?? "");
        }

        [HttpGet("load")]
        public async Task<IActionResult> LoadGiaoHangData([FromQuery] string? search, [FromQuery] string? status, [FromQuery] DateTime? date)
        {
            try
            {
                DateTime filterDate = date ?? DateTime.Today;

                var query = _context.HoaDons.AsNoTracking()
                    .Where(h => h.LoaiHoaDon == "Giao hàng" && h.ThoiGianTao.Date == filterDate.Date)
                    .Include(h => h.KhachHang)
                    .Include(h => h.NhanVienGiaoHang)
                    .OrderByDescending(h => h.ThoiGianTao)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && status != "Tất cả")
                {
                    if (status == "Đã hủy")
                        query = query.Where(h => h.TrangThai == "Đã hủy" || h.TrangThaiGiaoHang == "Đã hủy");
                    else if (status == "Đã giao")
                        query = query.Where(h => h.TrangThaiGiaoHang == "Hoàn thành" || h.TrangThaiGiaoHang == "Đã giao");
                    else
                        query = query.Where(h => h.TrangThaiGiaoHang == status);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    int.TryParse(search, out int idSearch);
                    query = query.Where(h =>
                        (h.KhachHang != null && h.KhachHang.HoTen.ToLower().Contains(searchLower)) ||
                        (h.SoDienThoaiGiaoHang != null && h.SoDienThoaiGiaoHang.Contains(search)) ||
                        (h.KhachHang != null && h.KhachHang.SoDienThoai != null && h.KhachHang.SoDienThoai.Contains(search)) ||
                        h.IdHoaDon == idSearch
                    );
                }

                var donGiaoHang = await query.Select(h => new GiaoHangItemDto
                {
                    IdHoaDon = h.IdHoaDon,
                    ThoiGianTao = h.ThoiGianTao,
                    TenKhachHang = h.KhachHang != null ? h.KhachHang.HoTen : (h.DiaChiGiaoHang ?? "Khách vãng lai"),
                    SoDienThoaiGiaoHang = h.SoDienThoaiGiaoHang ?? (h.KhachHang != null ? h.KhachHang.SoDienThoai : ""),
                    DiaChiGiaoHang = h.DiaChiGiaoHang ?? (h.KhachHang != null ? h.KhachHang.DiaChi : ""),
                    ThanhTien = h.ThanhTien,
                    TrangThaiThanhToan = h.TrangThai,
                    PhuongThucThanhToan = h.PhuongThucThanhToan ?? "COD",
                    TrangThaiGiaoHang = h.TrangThaiGiaoHang,
                    IdNguoiGiaoHang = h.IdNguoiGiaoHang,
                    TenNguoiGiaoHang = h.NhanVienGiaoHang != null ? h.NhanVienGiaoHang.HoTen : null,
                    GhiChu = h.GhiChu,
                    IdNhanVien = h.IdNhanVien,
                    AnhGiaoHang = h.AnhGiaoHang
                }).ToListAsync();

                var nguoiGiaoHang = await _context.NhanViens.AsNoTracking()
                    .Where(n => n.TrangThaiLamViec == "Đang làm việc")
                    .Select(n => new NguoiGiaoHangDto
                    {
                        IdNguoiGiaoHang = n.IdNhanVien,
                        TenNguoiGiaoHang = n.HoTen
                    }).ToListAsync();

                return Ok(new GiaoHangViewDto { DonGiaoHang = donGiaoHang, NguoiGiaoHangSanSang = nguoiGiaoHang });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpPost("update/{idHoaDon}")]
        public async Task<IActionResult> UpdateGiaoHang(int idHoaDon, [FromBody] GiaoHangUpdateRequestDto dto)
        {
            try
            {
                int idNhanVien = GetCurrentUserId();
                var hoaDon = await _context.HoaDons.Include(h => h.KhachHang).FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

                if (hoaDon == null || hoaDon.LoaiHoaDon != "Giao hàng") return BadRequest("Đơn hàng không hợp lệ.");

                string? trangThaiCu = hoaDon.TrangThaiGiaoHang;

                if (dto.TrangThaiGiaoHang == "Đã giao") dto.TrangThaiGiaoHang = "Hoàn thành";
                hoaDon.TrangThaiGiaoHang = dto.TrangThaiGiaoHang ?? hoaDon.TrangThaiGiaoHang;
                hoaDon.IdNguoiGiaoHang = dto.IdNguoiGiaoHang ?? hoaDon.IdNguoiGiaoHang;

                if (dto.TrangThaiGiaoHang == "Đang chuẩn bị" && (trangThaiCu == "Chờ xác nhận" || trangThaiCu == null))
                {
                    await CreateOrUpdateCheBienItems(idHoaDon, idNhanVien);
                    var settingsDict = await GetGeneralSettingsAsync();
                    if (hoaDon.KhachHang != null && !string.IsNullOrEmpty(hoaDon.KhachHang.Email))
                    {
                        _ = SendStatusEmailAsync(hoaDon.KhachHang.Email, hoaDon.KhachHang.HoTen, idHoaDon, "Đang chuẩn bị", null, settingsDict);
                    }
                }
                if (dto.TrangThaiGiaoHang == "Đã hủy" || dto.TrangThaiGiaoHang == "Trả hàng")
                {
                    hoaDon.TrangThai = "Đã hủy";
                }

                if (dto.TrangThaiGiaoHang == "Hoàn thành" && hoaDon.TrangThai != "Đã thanh toán")
                {
                    hoaDon.TrangThai = "Đã thanh toán";
                    hoaDon.ThoiGianThanhToan = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật thành công." });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpPost("confirm-all-pending")]
        public async Task<IActionResult> ConfirmAllPendingOrders()
        {
            var idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            try
            {
                var donHangCho = await _context.HoaDons.Include(h => h.KhachHang)
                    .Where(h => h.LoaiHoaDon == "Giao hàng" && (h.TrangThaiGiaoHang == "Chờ xác nhận" || h.TrangThaiGiaoHang == null))
                    .ToListAsync();

                if (!donHangCho.Any()) return Ok(new { message = "Không có đơn nào 'Chờ xác nhận'." });

                var settingsDict = await GetGeneralSettingsAsync();
                int count = 0;

                foreach (var hoaDon in donHangCho)
                {
                    hoaDon.TrangThaiGiaoHang = "Đang chuẩn bị";
                    await CreateOrUpdateCheBienItems(hoaDon.IdHoaDon, idNhanVien);

                    if (hoaDon.KhachHang != null && !string.IsNullOrEmpty(hoaDon.KhachHang.Email))
                    {
                        _ = SendStatusEmailAsync(hoaDon.KhachHang.Email, hoaDon.KhachHang.HoTen, hoaDon.IdHoaDon, "Đang chuẩn bị", null, settingsDict);
                    }
                    count++;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Đã chuyển {count} đơn hàng sang Bếp thành công." });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi máy chủ: {ex.Message}"); }
        }

        [HttpGet("print-data/{idHoaDon}")]
        public async Task<IActionResult> GetPrintData(int idHoaDon)
        {
            var settings = await GetGeneralSettingsAsync();
            var hoaDon = await _context.HoaDons.AsNoTracking()
                 .Include(h => h.Ban).Include(h => h.NhanVienTao).Include(h => h.KhachHang)
                 .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.SanPham)
                 .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

            if (hoaDon == null) return NotFound();

            var dto = new PhieuGoiMonPrintDto
            {
                IdPhieu = hoaDon.IdHoaDon.ToString(),
                TenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook"),
                DiaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "N/A"),
                SdtQuan = settings.GetValueOrDefault("ThongTin_SoDienThoai", "N/A"),
                NgayTao = hoaDon.ThoiGianTao,
                TenNhanVien = hoaDon.NhanVienTao?.HoTen ?? "Web Online",
                SoBan = hoaDon.LoaiHoaDon,
                GhiChu = $"KH: {hoaDon.KhachHang?.HoTen ?? "Khách vãng lai"}\nSĐT: {hoaDon.SoDienThoaiGiaoHang}\nĐịa chỉ: {hoaDon.DiaChiGiaoHang}\nGhi chú: {hoaDon.GhiChu}",

                ChiTiet = hoaDon.ChiTietHoaDons.Select(ct => new ChiTietDto
                {
                    TenSanPham = ct.SanPham.TenSanPham,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }).ToList(),

                TongTienGoc = hoaDon.TongTienGoc,
                GiamGia = hoaDon.GiamGia,
                ThanhTien = hoaDon.ThanhTien
            };
            return Ok(dto);
        }

        private async Task<int> CreateOrUpdateCheBienItems(int idHoaDon, int idNhanVien)
        {
            var chiTietItems = await _context.ChiTietHoaDons.Include(c => c.SanPham).Where(c => c.IdHoaDon == idHoaDon).ToListAsync();
            int itemsAdded = 0;
            var now = DateTime.Now;

            foreach (var item in chiTietItems)
            {
                bool daTonTai = await _context.TrangThaiCheBiens.AnyAsync(cb => cb.IdChiTietHoaDon == item.IdChiTietHoaDon);
                if (!daTonTai)
                {
                    _context.TrangThaiCheBiens.Add(new TrangThaiCheBien
                    {
                        IdChiTietHoaDon = item.IdChiTietHoaDon,
                        IdHoaDon = item.IdHoaDon,
                        IdSanPham = item.IdSanPham,
                        TenMon = item.SanPham.TenSanPham,
                        SoBan = "Giao hàng",
                        SoLuong = item.SoLuong,
                        GhiChu = item.GhiChu,
                        NhomIn = item.SanPham.NhomIn,
                        TrangThai = "Chờ làm",
                        ThoiGianGoi = now
                    });
                    itemsAdded++;
                }
            }

            if (itemsAdded > 0)
            {
                _context.ThongBaos.Add(new ThongBao
                {
                    IdNhanVienTao = idNhanVien > 0 ? idNhanVien : (int?)null,
                    NoiDung = $"Đơn Giao Hàng #{idHoaDon} cần chuẩn bị.",
                    ThoiGianTao = DateTime.Now,
                    LoaiThongBao = "PhieuGoiMon",
                    IdLienQuan = idHoaDon,
                    DaXem = false
                });
            }
            return itemsAdded;
        }

        private async Task SendStatusEmailAsync(string email, string hoTen, int idHoaDon, string status, string? shipperName, Dictionary<string, string> settings)
        {
            try
            {
                string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
                string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
                string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");
                string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");

                string noiDungChinh;
                string iconTrangThai;

                if (status == "Đang chuẩn bị")
                {
                    iconTrangThai = "🍳";
                    noiDungChinh = $"Đơn hàng <strong>#{idHoaDon}</strong> của bạn đã được tiếp nhận và đang được bộ phận Bếp chuẩn bị. Chúng tôi sẽ giao hàng trong thời gian sớm nhất!";
                }
                else
                {
                    iconTrangThai = "🛵";
                    noiDungChinh = $"Đơn hàng <strong>#{idHoaDon}</strong> của bạn đã được giao cho shipper <strong>{shipperName}</strong>. Vui lòng chú ý điện thoại để nhận hàng nhé!";
                }

                string body = $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F7F3E9; color: #4E342E; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #5D4037; color: #FFF3E0; padding: 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 1px; text-transform: uppercase; }}
                        .content {{ padding: 30px; line-height: 1.6; font-size: 16px; }}
                        .status-card {{ background-color: #FFF3E0; border-left: 6px solid #D84315; border-radius: 8px; padding: 20px; margin: 25px 0; }}
                        .footer {{ background-color: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6; border-top: 1px solid #D7CCC8; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>☕ {tenQuan}</h1>
                        </div>
                        <div class=""content"">
                            <div style=""font-size: 18px; font-weight: bold; margin-bottom: 20px;"">Xin chào {hoTen},</div>
                            <p>Trạng thái đơn giao hàng của bạn vừa được cập nhật:</p>
                    
                            <div class=""status-card"">
                                <div style=""font-size: 18px; margin-bottom: 10px;"">{iconTrangThai} <strong>Cập nhật trạng thái</strong></div>
                                <div>{noiDungChinh}</div>
                            </div>
                    
                            <p>Cảm ơn bạn đã đặt món tại <strong>{tenQuan}</strong>. Chúc bạn có một bữa ăn ngon miệng!</p>
                        </div>
                        <div class=""footer"">
                            <strong>Đội ngũ {tenQuan}</strong><br>
                            📍 Địa chỉ: {diaChiQuan}<br>
                            📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                            © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                        </div>
                    </div>
                </body>
                </html>";

                string host = settings.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
                string username = settings.GetValueOrDefault("Smtp_Username", "");
                string password = settings.GetValueOrDefault("Smtp_Password", "");
                string fromName = settings.GetValueOrDefault("Smtp_FromName", "Cafebook Hỗ Trợ");
                int port = int.TryParse(settings.GetValueOrDefault("Smtp_Port", "587"), out int p) ? p : 587;
                bool enableSsl = bool.TryParse(settings.GetValueOrDefault("Smtp_EnableSsl", "true"), out bool s) ? s : true;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;

                var mailMessage = new MailMessage { From = new MailAddress(username, fromName), Subject = $"[{tenQuan}] Cập nhật đơn hàng #{idHoaDon}", Body = body, IsBodyHtml = true };
                mailMessage.To.Add(new MailAddress(email));

                using var smtpClient = new SmtpClient(host, port) { Credentials = new NetworkCredential(username, password), EnableSsl = enableSsl };
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex) { Console.WriteLine($"Lỗi gửi mail hệ thống: {ex.Message}"); }
        }
    }
}