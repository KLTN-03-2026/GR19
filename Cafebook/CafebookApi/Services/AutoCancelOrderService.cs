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
    public class AutoCancelOrderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoCancelOrderService> _logger;

        public AutoCancelOrderService(IServiceProvider serviceProvider, ILogger<AutoCancelOrderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ tự động hủy đơn hàng quá hạn đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    TimeSpan gioMoCua = new TimeSpan(7, 0, 0);
                    TimeSpan gioDongCua = new TimeSpan(22, 0, 0);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<CafebookApi.Data.CafebookDbContext>();

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

                            _logger.LogInformation($"[AutoCancel] Quán đã đóng cửa. Hệ thống ngủ đông đến: {nextRun:dd/MM/yyyy HH:mm:ss}");
                            await Task.Delay(delay, stoppingToken);
                            continue; 
                        }

                        var limit = now.AddMinutes(-30);

                        var orders = await context.HoaDons
                            .Include(h => h.GiaoDichThanhToans)
                            .Where(h => (h.TrangThai == "Chờ thanh toán" || h.TrangThai == "Chờ xác nhận")
                                     && h.ThoiGianTao < limit)
                            .ToListAsync(stoppingToken);

                        int cancelCount = 0;
                        foreach (var o in orders)
                        {
                            if (o.TrangThai == "Chờ xác nhận" && (o.PhuongThucThanhToan == "COD" || o.PhuongThucThanhToan == "Tiền mặt"))
                            {
                                continue;
                            }
                            o.TrangThai = "Đã hủy";
                            if (o.LoaiHoaDon == "Giao hàng")
                            {
                                o.TrangThaiGiaoHang = "Đã hủy";
                            }
                            o.GhiChu += " | Hệ thống tự động hủy đơn quá hạn chưa thanh toán.";

                            var pendingTransactions = o.GiaoDichThanhToans.Where(g => g.TrangThai == "Đang xử lý").ToList();
                            foreach (var gd in pendingTransactions)
                            {
                                gd.TrangThai = "Đã hủy";
                                gd.MaLoi = "24"; 
                                gd.MoTaLoi = "Hệ thống tự động hủy do quá thời gian thanh toán.";
                            }

                            cancelCount++;
                        }

                        if (cancelCount > 0)
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"[AutoCancel] Đã dọn dẹp và tự động hủy {cancelCount} đơn hàng quá hạn.");
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AutoCancel] Lỗi khi chạy tự động hủy đơn.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}