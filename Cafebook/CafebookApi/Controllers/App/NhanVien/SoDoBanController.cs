// File: CafebookApi/Controllers/App/NhanVien/SoDoBanController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/sodoban")]
    [ApiController]
    public class SoDoBanController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public SoDoBanController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetSoDoBan()
        {
            await AutoCancelLateReservationsInternal();

            var now = DateTime.Now;
            var nowPlus10Minutes = now.AddMinutes(10);

            var data = await _context.Bans
               .AsNoTracking()
               .Select(b => new
               {
                   Ban = b,
                   HoaDonHienTai = _context.HoaDons
                       .Where(h => h.IdBan == b.IdBan && h.TrangThai == "Chưa thanh toán")
                       .Select(h => new { h.IdHoaDon, h.ThanhTien })
                       .FirstOrDefault(),

                   PhieuDatSapToi = _context.PhieuDatBans
                       .Where(p => p.IdBan == b.IdBan &&
                                   p.ThoiGianDat > now &&
                                   (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận"))
                       .OrderBy(p => p.ThoiGianDat)
                       .FirstOrDefault()
               })
               .Select(data => new BanSoDoDto
               {
                   IdBan = data.Ban.IdBan,
                   SoBan = data.Ban.SoBan,

                   TrangThai = (data.Ban.TrangThai == "Trống" &&
                                data.PhieuDatSapToi != null &&
                                data.PhieuDatSapToi.ThoiGianDat <= nowPlus10Minutes)
                               ? "Đã đặt"
                               : data.Ban.TrangThai,

                   GhiChu = data.Ban.GhiChu,
                   IdKhuVuc = data.Ban.IdKhuVuc,
                   IdHoaDonHienTai = data.HoaDonHienTai != null ? (int?)data.HoaDonHienTai.IdHoaDon : null,
                   TongTienHienTai = data.HoaDonHienTai != null ? data.HoaDonHienTai.ThanhTien : 0,
                   ThongTinDatBan = (data.Ban.TrangThai == "Trống" && data.PhieuDatSapToi != null)
                                    ? $"Đặt lúc: {data.PhieuDatSapToi.ThoiGianDat:HH:mm}"
                                    : null
               })
               .OrderBy(b => b.SoBan)
               .ToListAsync();

            return Ok(data);
        }

        [HttpPost("createorder/{idBan}/{idNhanVien}")]
        public async Task<IActionResult> CreateOrder(int idBan, int idNhanVien)
        {
            var ban = await _context.Bans.FindAsync(idBan);
            if (ban == null) return NotFound("Không tìm thấy bàn.");
            if (ban.TrangThai != "Trống" && ban.TrangThai != "Đã đặt")
                return Conflict("Bàn này đang bận hoặc đang bảo trì.");
            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound("Nhân viên không hợp lệ.");
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

        [HttpPost("reportproblem/{idBan}/{idNhanVien}")]
        public async Task<IActionResult> BaoCaoSuCo(int idBan, int idNhanVien, [FromBody] BaoCaoSuCoRequestDto request)
        {
            var ban = await _context.Bans.FindAsync(idBan);
            if (ban == null) return NotFound("Không tìm thấy bàn.");
            if (ban.TrangThai == "Có khách")
                return Conflict("Không thể báo cáo sự cố bàn đang có khách.");
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
            return Ok(new { message = "Báo cáo sự cố thành công. Bàn đã được khóa." });
        }

        [HttpPost("createorder-no-table/{idNhanVien}")]
        public async Task<IActionResult> CreateOrderNoTable(int idNhanVien, [FromBody] string loaiHoaDon)
        {
            if (loaiHoaDon != "Mang về" && loaiHoaDon != "Tại quán")
                return BadRequest("Loại hóa đơn không hợp lệ.");

            var nhanVien = await _context.NhanViens.FindAsync(idNhanVien);
            if (nhanVien == null) return NotFound("Nhân viên không hợp lệ.");
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
        public async Task<IActionResult> MoveTable([FromBody] BanActionRequestDto dto)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == dto.IdHoaDonNguon);
            if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn nguồn.");
            var banDich = await _context.Bans.FindAsync(dto.IdBanDich);
            if (banDich == null) return NotFound("Không tìm thấy bàn đích.");
            if (banDich.TrangThai != "Trống") return Conflict("Bàn đích đang bận, không thể chuyển đến.");

            if (hoaDon.Ban != null) hoaDon.Ban.TrangThai = "Trống";
            banDich.TrangThai = "Có khách";
            hoaDon.IdBan = dto.IdBanDich;

            // Cập nhật lại tên bàn cho bộ phận bếp (TrangThaiCheBien)
            var trangThaiCheBiens = await _context.TrangThaiCheBiens.Where(t => t.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var t in trangThaiCheBiens) t.SoBan = banDich.SoBan;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Chuyển bàn thành công." });
        }

        // === [ĐÃ FIX LỖI BIÊN DỊCH CS1061 VÀ CS0019] ===
        [HttpPost("merge-table")]
        public async Task<IActionResult> MergeTable([FromBody] BanActionRequestDto dto)
        {
            if (dto.IdHoaDonNguon == dto.IdHoaDonDich) return BadRequest("Không thể gộp bàn vào chính nó.");
            if (!dto.IdHoaDonDich.HasValue) return BadRequest("Không xác định được hóa đơn đích.");

            var hoaDonNguon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == dto.IdHoaDonNguon);
            if (hoaDonNguon == null) return NotFound("Không tìm thấy hóa đơn nguồn.");

            var hoaDonDich = await _context.HoaDons.FirstOrDefaultAsync(h => h.IdHoaDon == dto.IdHoaDonDich.Value);
            if (hoaDonDich == null) return NotFound("Không tìm thấy hóa đơn đích.");

            var banDich = await _context.Bans.FindAsync(dto.IdBanDich);

            // 1. Chuyển Chi Tiết Hóa Đơn
            var chiTietNguon = await _context.ChiTietHoaDons.Where(c => c.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var ct in chiTietNguon) ct.IdHoaDon = dto.IdHoaDonDich.Value;

            // 2. Chuyển Trạng Thái Chế Biến (Tránh lỗi FK_TrangThaiCheBien_HoaDon)
            var trangThaiCheBiens = await _context.TrangThaiCheBiens.Where(t => t.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var t in trangThaiCheBiens)
            {
                t.IdHoaDon = dto.IdHoaDonDich.Value;
                t.SoBan = banDich?.SoBan ?? t.SoBan;
            }

            // 3. Chuyển Phụ Thu (ĐÃ FIX HOA THƯỜNG IdHoaDon và IdPhuThu)
            var phuThus = await _context.ChiTietPhuThuHoaDons.Where(p => p.IdHoaDon == dto.IdHoaDonNguon).ToListAsync();
            foreach (var pt in phuThus)
            {
                var exists = await _context.ChiTietPhuThuHoaDons.AnyAsync(x => x.IdHoaDon == dto.IdHoaDonDich.Value && x.IdPhuThu == pt.IdPhuThu);
                if (!exists) pt.IdHoaDon = dto.IdHoaDonDich.Value;
                else _context.ChiTietPhuThuHoaDons.Remove(pt);
            }

            // 4. Xóa Khuyến mãi của hóa đơn nguồn (nếu có)
            await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.HoaDon_KhuyenMai WHERE idHoaDon = {dto.IdHoaDonNguon}");

            // 5. Cộng dồn tiền Gốc
            hoaDonDich.TongTienGoc += hoaDonNguon.TongTienGoc;
            hoaDonDich.TongPhuThu += hoaDonNguon.TongPhuThu;
            hoaDonDich.GiamGia += hoaDonNguon.GiamGia;

            // 6. Giải phóng bàn
            if (hoaDonNguon.Ban != null) hoaDonNguon.Ban.TrangThai = "Trống";

            // 7. Xóa hóa đơn nguồn
            _context.HoaDons.Remove(hoaDonNguon);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Gộp bàn thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi hệ thống khi gộp bàn: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpGet("khuvuc-list")]
        public async Task<IActionResult> GetKhuVucListForSoDoBan()
        {
            try
            {
                var data = await _context.KhuVucs
                    .AsNoTracking()
                    .Select(k => new KhuVucDto
                    {
                        IdKhuVuc = k.IdKhuVuc,
                        TenKhuVuc = k.TenKhuVuc
                    })
                    .OrderBy(k => k.TenKhuVuc)
                    .ToListAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server khi lấy khu vực: {ex.Message}");
            }
        }

        // ====================================================================
        // === TÍNH NĂNG TỰ ĐỘNG HỦY BÀN QUÁ HẠN & GỬI EMAIL (ĐỒNG BỘ) ===
        // ====================================================================
        private async Task AutoCancelLateReservationsInternal()
        {
            try
            {
                var now = DateTime.Now;
                var timeLimit = now.AddMinutes(-15);

                var lateReservations = await _context.PhieuDatBans
                    .Include(p => p.Ban)
                    .Include(p => p.KhachHang)
                    .Where(p => (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận") &&
                                p.ThoiGianDat < timeLimit)
                    .ToListAsync();

                if (lateReservations.Any())
                {
                    var smtp = await GetSmtpSettingsAsync();
                    var settingsDict = await GetGeneralSettingsAsync();

                    foreach (var phieu in lateReservations)
                    {
                        phieu.TrangThai = "Đã hủy";
                        phieu.GhiChu = string.IsNullOrEmpty(phieu.GhiChu)
                            ? "Tự động hủy do khách trễ 15p"
                            : phieu.GhiChu + " | Tự động hủy do trễ 15p";

                        if (phieu.Ban != null && phieu.Ban.TrangThai != "Có khách")
                        {
                            phieu.Ban.TrangThai = "Trống";
                        }

                        // Gửi Mail báo hủy
                        var emailKhach = phieu.KhachHang?.Email;
                        if (!string.IsNullOrEmpty(emailKhach) && !string.IsNullOrEmpty(smtp.username) && !string.IsNullOrEmpty(smtp.password))
                        {
                            var khachInfo = phieu.KhachHang ?? new KhachHang
                            {
                                HoTen = phieu.HoTenKhach ?? "Khách hàng",
                                Email = emailKhach ?? ""
                            };
                            string soBanStr = phieu.Ban?.SoBan ?? "N/A";

                            _ = Task.Run(() => SendCancellationEmailAsync(phieu, khachInfo, soBanStr, smtp.host, smtp.port, smtp.username, smtp.password, smtp.fromName, settingsDict));
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi auto-cancel (SoDoBan): {ex.Message}");
            }
        }

        private async Task<Dictionary<string, string>> GetGeneralSettingsAsync()
        {
            return await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
        }

        private async Task<(string host, int port, string username, string password, string fromName)> GetSmtpSettingsAsync()
        {
            var smtpHost = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Host");
            var smtpPort = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Port");
            var smtpUser = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Username");
            var smtpPass = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Password");
            var smtpName = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_FromName");

            string host = smtpHost?.GiaTri ?? "smtp.gmail.com";
            int port = int.TryParse(smtpPort?.GiaTri, out int p) ? p : 587;
            string username = smtpUser?.GiaTri ?? "";
            string password = smtpPass?.GiaTri ?? "";
            string fromName = smtpName?.GiaTri ?? "Cafebook Hỗ Trợ";

            return (host, port, username, password, fromName);
        }

        private async Task SendCancellationEmailAsync(PhieuDatBan phieu, KhachHang khach, string soBan, string host, int port, string username, string password, string fromName, Dictionary<string, string> settings)
        {
            string toEmail = khach.Email ?? "";
            if (string.IsNullOrEmpty(toEmail)) return;

            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, username));
                email.To.Add(new MailboxAddress(khach.HoTen, toEmail));
                email.Subject = $"[{tenQuan}] Thông báo hủy đặt bàn tự động";

                string body = $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F7F3E9; color: #4E342E; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #D32F2F; color: #FFFFFF; padding: 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 1px; text-transform: uppercase; }}
                        .content {{ padding: 30px; line-height: 1.6; }}
                        .alert-card {{ background-color: #FFEBEE; border-left: 6px solid #D32F2F; border-radius: 8px; padding: 20px; margin: 25px 0; color: #C62828; }}
                        .footer {{ background-color: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6; border-top: 1px solid #D7CCC8; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>☕ {tenQuan}</h1>
                        </div>
                        <div class=""content"">
                            <div style=""font-size: 18px; font-weight: bold; margin-bottom: 20px;"">Xin chào {khach.HoTen},</div>
                            <p>Chúng tôi gửi email này để thông báo về tình trạng phiếu đặt bàn của bạn vào lúc <strong>{phieu.ThoiGianDat:HH:mm} ngày {phieu.ThoiGianDat:dd/MM/yyyy}</strong>.</p>
                            
                            <div class=""alert-card"">
                                <strong>❌ Đã Hủy Tự Động</strong><br><br>
                                Rất tiếc vì bạn đã không thể đến đúng giờ. Hệ thống của chúng tôi đã tự động hủy bàn (Bàn: <strong>{soBan}</strong>) do vượt quá thời gian chờ quy định (15 phút) nhằm giải phóng không gian phục vụ các thực khách khác.
                            </div>
                            
                            <p>Chúng tôi rất hy vọng có cơ hội được phục vụ bạn vào một dịp khác gần nhất. Nếu bạn vẫn muốn ghé quán, vui lòng liên hệ lại Hotline hoặc đặt bàn mới nhé!</p>
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

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email hủy bàn (chạy ngầm): {ex.Message}");
            }
        }
    }
}