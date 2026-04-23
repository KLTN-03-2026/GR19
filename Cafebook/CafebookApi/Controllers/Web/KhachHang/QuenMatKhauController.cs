using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // THÊM THƯ VIỆN NÀY
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/quenmatkhau")]
    [ApiController]
    [AllowAnonymous]
    public class QuenMatKhauController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IMemoryCache _cache; // KHAI BÁO CACHE

        public QuenMatKhauController(CafebookDbContext context, IMemoryCache cache) // BƠM CACHE VÀO ĐÂY
        {
            _context = context;
            _cache = cache;
        }

        [HttpPost("gui-ma")]
        public async Task<IActionResult> GuiMaXacNhan([FromBody] GuiMaXacNhanRequestDto req)
        {
            // ====================================================================
            // 0. CHỐNG SPAM GỬI MAIL (COOLDOWN 60 GIÂY)
            // ====================================================================
            string cooldownKey = $"QMK_Cooldown_{req.Email}";
            if (_cache.TryGetValue(cooldownKey, out _))
            {
                return Ok(new { success = false, message = "Bạn thao tác quá nhanh. Vui lòng đợi 60 giây trước khi yêu cầu gửi lại mã." });
            }

            // ====================================================================
            // 1. KIỂM TRA KHÁCH HÀNG & LOGIC MỞ KHÓA TỰ ĐỘNG
            // ====================================================================
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == req.Email);

            if (khachHang == null)
                return Ok(new { success = false, message = "Email này chưa được đăng ký trong hệ thống." });

            if (khachHang.DaXoa)
                return Ok(new { success = false, message = "Tài khoản của bạn đã bị xóa. Vui lòng liên hệ quản trị viên." });

            if (khachHang.BiKhoa)
            {
                if (khachHang.ThoiGianMoKhoa.HasValue && khachHang.ThoiGianMoKhoa.Value <= DateTime.Now)
                {
                    khachHang.BiKhoa = false;
                    khachHang.LyDoKhoa = null;
                    khachHang.ThoiGianMoKhoa = null;
                    _context.KhachHangs.Update(khachHang);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    string timeStr = khachHang.ThoiGianMoKhoa.HasValue ? $"đến {khachHang.ThoiGianMoKhoa.Value:dd/MM/yyyy HH:mm}" : "vĩnh viễn";
                    return Ok(new { success = false, message = $"Tài khoản đang bị khóa {timeStr}. Không thể khôi phục mật khẩu lúc này." });
                }
            }

            if (khachHang.TaiKhoanTam)
                return Ok(new { success = false, message = "Đây là tài khoản Tạm (chưa kích hoạt). Vui lòng chuyển sang trang Đăng Ký để nâng cấp tài khoản." });

            // ====================================================================
            // 2. LẤY CẤU HÌNH SMTP TỪ DB (Giữ nguyên code cũ của bạn)
            // ====================================================================
            var settings = await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            string host = settings.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
            int port = int.TryParse(settings.GetValueOrDefault("Smtp_Port"), out int p) ? p : 587;
            string username = settings.GetValueOrDefault("Smtp_Username", "");
            string password = settings.GetValueOrDefault("Smtp_Password", "");
            string fromName = settings.GetValueOrDefault("Smtp_FromName", "Cafebook Hỗ Trợ");

            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Ok(new { success = false, message = "Hệ thống gửi mail chưa được cấu hình." });
            }

            // ====================================================================
            // 3. GỬI EMAIL
            // ====================================================================
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(fromName, username));
                emailMessage.To.Add(new MailboxAddress(khachHang.HoTen ?? "Khách hàng", khachHang.Email ?? ""));
                emailMessage.Subject = $"[{tenQuan}] Mã xác nhận khôi phục mật khẩu";

                string body = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F7F3E9; color: #4E342E; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #5D4037; color: #FFF3E0; padding: 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 2px; text-transform: uppercase; }}
                        .content {{ padding: 30px; line-height: 1.6; font-size: 16px; text-align: center; }}
                        .otp-card {{ background-color: #FFF3E0; border: 2px dashed #D84315; border-radius: 12px; padding: 20px; margin: 25px auto; max-width: 250px; }}
                        .otp-code {{ font-size: 36px; font-weight: 700; color: #D84315; letter-spacing: 6px; margin: 10px 0; }}
                        .footer {{ background-color: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; border-top: 1px solid #D7CCC8; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>☕ {tenQuan.ToUpper()}</h1>
                        </div>
                        <div class='content'>
                            <div style='font-size: 20px; font-weight: bold; margin-bottom: 15px; color: #5D4037;'>🔐 KHÔI PHỤC MẬT KHẨU</div>
                            <p style='text-align: left;'>Xin chào <strong>{khachHang.HoTen}</strong>,</p>
                            <p style='text-align: left;'>Bạn vừa yêu cầu đặt lại mật khẩu tại hệ thống của chúng tôi. Vui lòng sử dụng mã xác thực dưới đây để tiếp tục:</p>
                    
                            <div class='otp-card'>
                                <div style='font-size: 13px; color: #5D4037; text-transform: uppercase; font-weight: 600;'>Mã OTP của bạn</div>
                                <div class='otp-code'>{req.MaXacNhan}</div>
                            </div>
                    
                            <p style='text-align: left;'>⏳ Mã này chỉ có hiệu lực trong vòng <strong>5 phút</strong>.</p>
                            <div style='font-size: 14px; color: #D32F2F; margin-top: 20px; padding: 15px; background-color: #FFEBEE; border-radius: 8px; border-left: 4px solid #D32F2F; text-align: left;'>
                                ⚠️ <strong>Bảo mật tài khoản:</strong> Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này để bảo vệ tài khoản. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.
                            </div>
                        </div>
                        <div class='footer'>
                            <strong>Đội ngũ {tenQuan}</strong><br>
                            📍 Địa chỉ: {diaChiQuan}<br>
                            📞 Hotline: {supportPhone} | ✉️ {supportEmail}
                        </div>
                    </div>
                </body>
                </html>";

                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(emailMessage);
                await smtp.DisconnectAsync(true);

                return Ok(new { success = true, message = "Mã xác nhận đã được gửi đến Email của bạn." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Lỗi hệ thống khi gửi mail: {ex.Message}" });
            }
        }

        // ====================================================================
        // 4. API MỚI: ĐẶT LẠI MẬT KHẨU
        // ====================================================================
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto req)
        {
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == req.Email);

            if (khachHang == null)
            {
                return BadRequest("Không tìm thấy tài khoản.");
            }

            khachHang.MatKhau = req.NewPassword;

            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật mật khẩu thành công." });
        }
    }
}