using CafebookApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CafebookApi.Services
{
    public class AutoUnlockAccountService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoUnlockAccountService> _logger;

        public AutoUnlockAccountService(IServiceProvider serviceProvider, ILogger<AutoUnlockAccountService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ tự động mở khóa tài khoản khách hàng đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

                        var expired = await context.KhachHangs
                            .Where(k => k.BiKhoa && k.ThoiGianMoKhoa.HasValue && k.ThoiGianMoKhoa.Value <= DateTime.Now && !k.DaXoa)
                            .ToListAsync(stoppingToken);

                        if (expired.Any())
                        {
                            foreach (var kh in expired)
                            {
                                kh.BiKhoa = false;
                                kh.LyDoKhoa = null;
                                kh.ThoiGianMoKhoa = null;
                            }
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"[AutoUnlock] Đã tự động mở khóa cho {expired.Count} tài khoản khách hàng đến hạn.");
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Dịch vụ tự động mở khóa tài khoản đã dừng an toàn.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AutoUnlock] Lỗi khi chạy tự động mở khóa tài khoản.");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }
        }
    }
}