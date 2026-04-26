using Microsoft.EntityFrameworkCore;

namespace CafebookApi.Services
{
    public class AutoCancelOrderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public AutoCancelOrderService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<CafebookApi.Data.CafebookDbContext>();
                    var limit = DateTime.Now.AddMinutes(-30); // Quá 30p không thanh toán
                    var orders = await context.HoaDons
                        .Where(h => h.TrangThai == "Chờ thanh toán" && h.ThoiGianTao < limit)
                        .ToListAsync();

                    foreach (var o in orders) { o.TrangThai = "Đã hủy"; o.TrangThaiGiaoHang = "Đã hủy"; o.GhiChu += " | Hệ thống tự động hủy đơn quá hạn."; }
                    await context.SaveChangesAsync();
                }
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}