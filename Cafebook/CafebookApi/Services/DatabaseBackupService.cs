using CafebookApi.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CafebookApi.Services
{
    public class DatabaseBackupService : BackgroundService
    {
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DatabaseBackupService(ILogger<DatabaseBackupService> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ tự động Backup Database đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    TimeSpan gioDongCua = new TimeSpan(22, 0, 0);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();
                        var settingDongCua = await dbContext.CaiDats
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.TenCaiDat == "ThongTin_GioDongCua", stoppingToken);

                        if (settingDongCua != null && TimeSpan.TryParse(settingDongCua.GiaTri, out TimeSpan parsedTime))
                        {
                            gioDongCua = parsedTime;
                        }
                    }

                    TimeSpan offset = TimeSpan.FromHours(2);
                    TimeSpan backupTimeOfDay = gioDongCua.Add(offset);

                    if (backupTimeOfDay.TotalDays >= 1)
                    {
                        backupTimeOfDay = backupTimeOfDay.Subtract(TimeSpan.FromDays(1));
                    }

                    var now = DateTime.Now;
                    var nextRun = now.Date.Add(backupTimeOfDay); 

                    if (now > nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation($"[Lịch trình Backup] Giờ đóng cửa: {gioDongCua:hh\\:mm}. Sẽ chạy sao lưu vào: {nextRun:dd/MM/yyyy HH:mm:ss}");

                    await Task.Delay(delay, stoppingToken);

                    await PerformDatabaseBackupAsync();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Có lỗi xảy ra trong quá trình chạy lịch Backup Database.");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
        }

        private async Task PerformDatabaseBackupAsync()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("CafeBookConnectionString")
                                        ?? throw new Exception("Không tìm thấy chuỗi kết nối CafeBookConnectionString.");

                string fileName = $"CafebookDBbackup_{DateTime.Now:yyyyMMdd_HHmm}.bak";

                string contentRootPath = Directory.GetCurrentDirectory();
                string backupFolder = Path.Combine(contentRootPath, "DatabaseCafebook");

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                string backupPath = Path.Combine(backupFolder, fileName);

                string backupQuery = $@"
                    BACKUP DATABASE [CAFEBOOKDB] 
                    TO DISK = '{backupPath}' 
                    WITH INIT, FORMAT, 
                    MEDIANAME = 'CafebookBackup', 
                    NAME = 'Full Backup of CAFEBOOKDB';";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(backupQuery, connection))
                    {
                        command.CommandTimeout = 300;
                        await command.ExecuteNonQueryAsync();
                    }
                }

                _logger.LogInformation($"[Thành công] Đã sao lưu Database CAFEBOOKDB tại: {backupPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Thất bại] Lỗi khi thực thi lệnh BACKUP DATABASE.");
                throw;
            }
        }
    }
}