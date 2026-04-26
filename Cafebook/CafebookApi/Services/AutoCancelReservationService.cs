using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace CafebookApi.Services
{
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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    TimeSpan gioMoCua = new TimeSpan(7, 0, 0);
                    TimeSpan gioDongCua = new TimeSpan(22, 0, 0);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

                        var settings = await context.CaiDats.AsNoTracking()
                            .Where(c => c.TenCaiDat == "ThongTin_GioMoCua" || c.TenCaiDat == "ThongTin_GioDongCua")
                            .ToListAsync(stoppingToken);

                        var moCuaStr = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioMoCua")?.GiaTri;
                        var dongCuaStr = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioDongCua")?.GiaTri;

                        if (TimeSpan.TryParse(moCuaStr, out TimeSpan parsedMo)) gioMoCua = parsedMo;
                        if (TimeSpan.TryParse(dongCuaStr, out TimeSpan parsedDong)) gioDongCua = parsedDong;

                        var now = DateTime.Now;
                        var timeOfDay = now.TimeOfDay;
                        bool isClosed = timeOfDay > gioDongCua.Add(TimeSpan.FromMinutes(30)) || timeOfDay < gioMoCua;

                        if (isClosed)
                        {
                            var nextRun = now.Date.Add(gioMoCua);
                            if (now > nextRun) nextRun = nextRun.AddDays(1);
                            var delay = nextRun - now;

                            _logger.LogInformation($"[AutoCancelReservation] Quán đã đóng cửa. Hệ thống ngủ đông đến: {nextRun:dd/MM/yyyy HH:mm:ss}");
                            await Task.Delay(delay, stoppingToken);
                            continue; 
                        }

                        await ProcessCancellationsAsync(context);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Dịch vụ hủy bàn tự động đã dừng an toàn.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[AutoCancelReservation] Lỗi: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); 
                }
            }
        }

        private async Task ProcessCancellationsAsync(CafebookDbContext _context)
        {
            var now = DateTime.Now;
            var timeLimit = now.AddMinutes(-15);
            var ignoreOldLimit = now.AddHours(-12); 

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

                        _ = Task.Run(() => SendCancellationEmailAsync(phieu, khachInfo, soBanStr, smtp.host, smtp.port, smtp.username, smtp.password, smtp.fromName, settingsDict));
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"[AutoCancelReservation] Đã giải phóng và hủy tự động {lateReservations.Count} bàn quá hạn.");
            }
        }

        // ==========================================================
        // CÁC HÀM HELPER 
        // ==========================================================
        private async Task<Dictionary<string, string>> GetGeneralSettingsAsync(CafebookDbContext _context)
        {
            return await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
        }

        private async Task<(string host, int port, string username, string password, string fromName)> GetSmtpSettingsAsync(CafebookDbContext _context)
        {
            var keysToFetch = new[] { "Smtp_Host", "Smtp_Port", "Smtp_Username", "Smtp_Password", "Smtp_FromName" };

            var smtpSettings = await _context.CaiDats.AsNoTracking()
                .Where(c => keysToFetch.Contains(c.TenCaiDat))
                .ToListAsync();

            string host = smtpSettings.FirstOrDefault(c => c.TenCaiDat == "Smtp_Host")?.GiaTri ?? "smtp.gmail.com";
            string? portStr = smtpSettings.FirstOrDefault(c => c.TenCaiDat == "Smtp_Port")?.GiaTri;
            int port = int.TryParse(portStr, out int p) ? p : 587;

            string username = smtpSettings.FirstOrDefault(c => c.TenCaiDat == "Smtp_Username")?.GiaTri ?? "";
            string password = smtpSettings.FirstOrDefault(c => c.TenCaiDat == "Smtp_Password")?.GiaTri ?? "";
            string fromName = smtpSettings.FirstOrDefault(c => c.TenCaiDat == "Smtp_FromName")?.GiaTri ?? "Cafebook Hỗ Trợ";

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
                _logger.LogError($"[AutoCancelReservation] Lỗi gửi email hủy bàn: {ex.Message}");
            }
        }
    }
}