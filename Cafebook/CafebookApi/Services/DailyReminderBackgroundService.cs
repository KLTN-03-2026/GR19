using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace CafebookApi.Services
{
    public class DailyReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyReminderBackgroundService> _logger;

        public DailyReminderBackgroundService(IServiceProvider serviceProvider, ILogger<DailyReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // CẤU HÌNH GIỜ CHẠY: Cài đặt chạy vào 08:00:00 sáng mỗi ngày
                var now = DateTime.Now;
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);

                // Nếu hiện tại đã qua 8h sáng, thì lên lịch cho 8h sáng ngày mai
                if (now >= nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }

                var delay = nextRunTime - now;

                _logger.LogInformation($"[DailyReminder] Hệ thống đang đếm ngược. Lần quét mail nhắc nhở tiếp theo vào lúc: {nextRunTime:dd/MM/yyyy HH:mm:ss}");

                // Ngủ đông Service cho đến giờ chạy để không tốn CPU
                await Task.Delay(delay, stoppingToken);

                // Khi thức dậy, kiểm tra xem App có đang bị tắt không, nếu không thì chạy
                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await DoWorkAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[DailyReminder] Lỗi hệ thống khi chạy tự động: {ex.Message}");
                    }
                }
            }
        }

        private async Task DoWorkAsync()
        {
            _logger.LogInformation("[DailyReminder] Đang bắt đầu quét các phiếu thuê sắp trễ hạn...");

            // Tạo một Scope mới để gọi DbContext an toàn
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

            // 1. Lấy thông tin cấu hình (SMTP, Tên quán...)
            var settingsDict = await context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            string tenQuan = settingsDict.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settingsDict.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportEmail = settingsDict.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");
            string supportPhone = settingsDict.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

            // 2. Tìm tất cả các sách CÓ HẸN TRẢ VÀO NGÀY MAI
            var ngayMai = DateTime.Today.AddDays(1);

            var phieuSapTre = await context.PhieuThueSachs.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietPhieuThues).ThenInclude(ct => ct.Sach)
                .Where(p => p.ChiTietPhieuThues.Any(ct => ct.NgayTraThucTe == null && ct.NgayHenTra.Date == ngayMai))
                .ToListAsync();

            int mailSentCount = 0;

            // 3. Duyệt qua và gửi Email
            foreach (var phieu in phieuSapTre)
            {
                var khach = phieu.KhachHang;
                if (khach == null || string.IsNullOrWhiteSpace(khach.Email)) continue;

                var sachList = phieu.ChiTietPhieuThues
                    .Where(ct => ct.NgayTraThucTe == null && ct.NgayHenTra.Date == ngayMai)
                    .ToList();

                if (!sachList.Any()) continue;

                string body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; background-color: #F7F3E9; padding: 20px;"">
                    <div style=""max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden;"">
                        <div style=""background: #D84315; color: #FFF; padding: 25px; text-align: center;""><h2>☕ {tenQuan.ToUpper()} THÔNG BÁO</h2></div>
                        <div style=""padding: 30px;"">
                            <p>Xin chào <strong>{khach.HoTen}</strong>,</p>
                            <p>{tenQuan} nhắc nhẹ bạn các cuốn sách sau sẽ <strong>đến hạn trả vào NGÀY MAI</strong>:</p>
                            <div style=""background: #FFF3E0; border-left: 5px solid #D84315; padding: 15px; margin: 20px 0;"">
                                {string.Join("", sachList.Select(s => $@"
                                    <div style='border-bottom: 1px solid #EEE; padding: 10px 0;'>
                                        <strong>📖 {s.Sach.TenSach}</strong><br>
                                        <small style='color: #666;'>Độ mới lúc thuê: {s.DoMoiKhiThue ?? 100}% | Ghi chú: {(string.IsNullOrWhiteSpace(s.GhiChuKhiThue) ? "-" : s.GhiChuKhiThue)}</small><br>
                                        <small style='color: #D84315;'>Hạn trả: {s.NgayHenTra:dd/MM/yyyy}</small>
                                    </div>"))}
                            </div>
                            <p>Vui lòng mang sách đến trả đúng hạn để tránh phát sinh phí trễ hạn nhé. Chúc bạn một ngày tốt lành!</p>
                        </div>
                        <div style=""background: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6;"">
                            <strong>Đội ngũ {tenQuan}</strong><br>
                            📍 Địa chỉ: {diaChiQuan}<br>
                            📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                        </div>
                    </div>
                </body>
                </html>";

                string subject = $"[{tenQuan}] Nhắc hẹn trả sách vào ngày mai (#{phieu.IdPhieuThueSach})";

                try
                {
                    await SendEmailAsync(khach.Email, subject, body, settingsDict);
                    mailSentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Lỗi gửi mail cho {khach.Email}: {ex.Message}");
                }
            }

            _logger.LogInformation($"[DailyReminder] Hoàn tất! Đã gửi thành công {mailSentCount} email nhắc nhở.");
        }

        private async Task SendEmailAsync(string emailTo, string subject, string htmlBody, Dictionary<string, string> settings)
        {
            string host = settings.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
            string username = settings.GetValueOrDefault("Smtp_Username", "");
            string password = settings.GetValueOrDefault("Smtp_Password", "");
            string fromName = settings.GetValueOrDefault("Smtp_FromName", "Cafebook Hỗ Trợ");
            int port = int.TryParse(settings.GetValueOrDefault("Smtp_Port", "587"), out int p) ? p : 587;
            bool enableSsl = bool.TryParse(settings.GetValueOrDefault("Smtp_EnableSsl", "true"), out bool s) ? s : true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(emailTo));

            using var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}