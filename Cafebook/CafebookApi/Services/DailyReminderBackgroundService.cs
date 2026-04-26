using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
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
            _logger.LogInformation("Dịch vụ gửi mail nhắc trả sách tự động đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);

                if (now >= nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }

                var delay = nextRunTime - now;
                _logger.LogInformation($"[DailyReminder] Hệ thống ngủ đông. Lần quét tiếp theo vào lúc: {nextRunTime:dd/MM/yyyy HH:mm:ss}");

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    await DoWorkAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("[DailyReminder] Dịch vụ đã được lệnh dừng an toàn.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[DailyReminder] Lỗi hệ thống: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[DailyReminder] Đang quét các phiếu thuê đến hạn vào ngày mai...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

            var ngayMai = DateTime.Today.AddDays(1);

            var phieuSapTre = await context.PhieuThueSachs.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietPhieuThues.Where(ct => ct.NgayTraThucTe == null && ct.NgayHenTra.Date == ngayMai))
                    .ThenInclude(ct => ct.Sach)
                .Where(p => p.ChiTietPhieuThues.Any(ct => ct.NgayTraThucTe == null && ct.NgayHenTra.Date == ngayMai))
                .ToListAsync(stoppingToken);

            if (!phieuSapTre.Any())
            {
                _logger.LogInformation("[DailyReminder] Không có sách nào đến hạn trả vào ngày mai.");
                return;
            }

            var settingsDict = await context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri, stoppingToken);

            string tenQuan = settingsDict.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settingsDict.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportEmail = settingsDict.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");
            string supportPhone = settingsDict.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

            string host = settingsDict.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
            string? portStr = settingsDict.GetValueOrDefault("Smtp_Port");
            int port = int.TryParse(portStr, out int p) ? p : 587;
            string username = settingsDict.GetValueOrDefault("Smtp_Username", "");
            string password = settingsDict.GetValueOrDefault("Smtp_Password", "");
            string fromName = settingsDict.GetValueOrDefault("Smtp_FromName", "Cafebook Hỗ Trợ");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("[DailyReminder] Chưa cấu hình SMTP. Hủy quá trình gửi mail.");
                return;
            }

            int mailSentCount = 0;

            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls, stoppingToken);
                await smtpClient.AuthenticateAsync(username, password, stoppingToken);

                foreach (var phieu in phieuSapTre)
                {
                    var khach = phieu.KhachHang;
                    if (khach == null || string.IsNullOrWhiteSpace(khach.Email)) continue;

                    var sachList = phieu.ChiTietPhieuThues.ToList();
                    if (!sachList.Any()) continue;

                    string subject = $"[{tenQuan}] Nhắc hẹn trả sách vào ngày mai (#{phieu.IdPhieuThueSach})";
                    string body = GenerateHtmlBody(tenQuan, diaChiQuan, supportPhone, supportEmail, khach, sachList);

                    var email = new MimeMessage();
                    email.From.Add(new MailboxAddress(fromName, username));
                    email.To.Add(new MailboxAddress(khach.HoTen, khach.Email));
                    email.Subject = subject;
                    email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                    try
                    {
                        await smtpClient.SendAsync(email, stoppingToken);
                        mailSentCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"[DailyReminder] Lỗi gửi mail cho {khach.Email}: {ex.Message}");
                    }
                }

                await smtpClient.DisconnectAsync(true, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DailyReminder] Lỗi kết nối SMTP Server: {ex.Message}");
            }

            _logger.LogInformation($"[DailyReminder] Hoàn tất! Đã gửi thành công {mailSentCount} email nhắc nhở.");
        }

        private string GenerateHtmlBody(string tenQuan, string diaChiQuan, string supportPhone, string supportEmail, KhachHang khach, List<ChiTietPhieuThue> sachList)
        {
            string danhSachSachHtml = string.Join("", sachList.Select(s => $@"
                <div style='border-bottom: 1px solid #EEE; padding: 10px 0;'>
                    <strong>📖 {s.Sach.TenSach}</strong><br>
                    <small style='color: #666;'>Độ mới lúc thuê: {s.DoMoiKhiThue ?? 100}% | Ghi chú: {(string.IsNullOrWhiteSpace(s.GhiChuKhiThue) ? "-" : s.GhiChuKhiThue)}</small><br>
                    <small style='color: #D84315;'>Hạn trả: {s.NgayHenTra:dd/MM/yyyy}</small>
                </div>"));

            return $@"
            <html>
            <body style=""font-family: Arial, sans-serif; background-color: #F7F3E9; padding: 20px;"">
                <div style=""max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden;"">
                    <div style=""background: #D84315; color: #FFF; padding: 25px; text-align: center;""><h2>☕ {tenQuan.ToUpper()} THÔNG BÁO</h2></div>
                    <div style=""padding: 30px;"">
                        <p>Xin chào <strong>{khach.HoTen}</strong>,</p>
                        <p>{tenQuan} nhắc nhẹ bạn các cuốn sách sau sẽ <strong>đến hạn trả vào NGÀY MAI</strong>:</p>
                        <div style=""background: #FFF3E0; border-left: 5px solid #D84315; padding: 15px; margin: 20px 0;"">
                            {danhSachSachHtml}
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
        }
    }
}