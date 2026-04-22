using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace CafebookApi.Services
{
    // Kế thừa BackgroundService để ASP.NET Core tự động chạy nó khi Server Start
    public class AutoCancelReservationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoCancelReservationService> _logger;

        public AutoCancelReservationService(IServiceProvider serviceProvider, ILogger<AutoCancelReservationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ hủy bàn tự động đã khởi động.");

            // Cài đặt Timer lặp lại mỗi 5 phút
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            try
            {
                // Vòng lặp vô tận, chỉ dừng khi Server bị tắt (stoppingToken)
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ProcessCancellationsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Dịch vụ hủy bàn tự động đã dừng.");
            }
        }

        private async Task ProcessCancellationsAsync()
        {
            // BẮT BUỘC: Tạo scope mới để lấy DbContext (vì DbContext là Scoped, còn BackgroundService là Singleton)
            using var scope = _serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

            try
            {
                var now = DateTime.Now;
                var timeLimit = now.AddMinutes(-15);
                var ignoreOldLimit = now.AddHours(-12); // Chặn lỗi AM/PM quá khứ

                var lateReservations = await _context.PhieuDatBans
                    .Include(p => p.Ban)
                    .Include(p => p.KhachHang)
                    .Where(p => (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận") &&
                                p.ThoiGianDat < timeLimit &&
                                p.ThoiGianDat > ignoreOldLimit)
                    .ToListAsync();

                if (lateReservations.Any())
                {
                    var smtp = await GetSmtpSettingsAsync(_context);
                    var settingsDict = await GetGeneralSettingsAsync(_context);

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

                        var emailKhach = phieu.KhachHang?.Email;
                        if (!string.IsNullOrEmpty(emailKhach) && !string.IsNullOrEmpty(smtp.username) && !string.IsNullOrEmpty(smtp.password))
                        {
                            var khachInfo = phieu.KhachHang ?? new KhachHang { HoTen = phieu.HoTenKhach ?? "Khách hàng", Email = emailKhach };
                            string soBanStr = phieu.Ban?.SoBan ?? "N/A";

                            // Gửi mail độc lập không block tiến trình quét
                            _ = Task.Run(() => SendCancellationEmailAsync(phieu, khachInfo, soBanStr, smtp.host, smtp.port, smtp.username, smtp.password, smtp.fromName, settingsDict));
                        }
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã tự động hủy {lateReservations.Count} bàn quá hạn.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi chạy quét hủy bàn: {ex.Message}");
            }
        }

        // ==========================================================
        // CÁC HÀM HELPER BÊ TỪ CONTROLLER SANG (Có truyền thêm _context)
        // ==========================================================
        private async Task<Dictionary<string, string>> GetGeneralSettingsAsync(CafebookDbContext _context)
        {
            return await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
        }

        private async Task<(string host, int port, string username, string password, string fromName)> GetSmtpSettingsAsync(CafebookDbContext _context)
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
                _logger.LogError($"Lỗi gửi email hủy bàn (chạy ngầm): {ex.Message}");
            }
        }
    }
}