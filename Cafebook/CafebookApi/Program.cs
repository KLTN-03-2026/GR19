using CafebookApi.Data;
using CafebookApi.Services;
using CafebookApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 0. KHỞI TẠO ĐƯỜNG DẪN THƯ MỤC CHUNG (SettingCafebook)
// ==========================================================
string currentDir = Directory.GetCurrentDirectory();
DirectoryInfo parentDir = Directory.GetParent(currentDir)!;
string configDirPath = Path.Combine(parentDir.FullName, "SettingCafebook");

if (!Directory.Exists(configDirPath))
{
    Directory.CreateDirectory(configDirPath);
}

// ==========================================================
// 1. CẤU HÌNH DATA PROTECTION (DÙNG CHUNG KEY VỚI FRONTEND)
// ==========================================================
string dataProtectionDirPath = Path.Combine(configDirPath, "SharedKeys");
if (!Directory.Exists(dataProtectionDirPath))
{
    Directory.CreateDirectory(dataProtectionDirPath);
}

var sharedKeyDir = new DirectoryInfo(dataProtectionDirPath);
builder.Services.AddDataProtection()
    .SetApplicationName("CafebookSystem") // Tên này phải y hệt bên Frontend
    .PersistKeysToFileSystem(sharedKeyDir);

// ==========================================================
// 2. TỰ ĐỘNG TẠO/ĐỌC FILE CẤU HÌNH KHÓA ADMIN (TOTP)
// ==========================================================
string adminKeyFilePath = Path.Combine(configDirPath, "ApisecretKeyAdministrator.json");
bool fileExists = File.Exists(adminKeyFilePath);
bool fileIsEmpty = fileExists && new FileInfo(adminKeyFilePath).Length == 0;

if (!fileExists || fileIsEmpty)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    var random = new Random();
    string randomSecretKey = new string(Enumerable.Repeat(chars, 16)
                               .Select(s => s[random.Next(s.Length)]).ToArray());

    var defaultAdminConfig = new
    {
        AdminSettings = new
        {
            SecretKey = randomSecretKey
        }
    };
    File.WriteAllText(adminKeyFilePath, JsonSerializer.Serialize(defaultAdminConfig, new JsonSerializerOptions { WriteIndented = true }));
}
builder.Configuration.AddJsonFile(adminKeyFilePath, optional: false, reloadOnChange: true);

// ==========================================================
// ĐĂNG KÝ CÁC DỊCH VỤ NỀN VÀ DATABASE
// ==========================================================
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<DatabaseBackupService>();
builder.Services.AddHostedService<AutoCancelOrderService>();
builder.Services.AddHostedService<AutoUnlockAccountService>();
builder.Services.AddHostedService<AutoCancelReservationService>();
builder.Services.AddHostedService<DailyReminderBackgroundService>();

var connectionString = builder.Configuration.GetConnectionString("CafeBookConnectionString");
builder.Services.AddDbContext<CafebookDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================================
// CẤU HÌNH CORS, DI, VÀ SWAGGER
// ==========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});

builder.Services.AddSignalR();
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<AiToolService>();

// ==========================================================
// CẤU HÌNH BẢO MẬT JWT VÀ SIGNALR
// ==========================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
        ValidAudience = builder.Configuration["Jwt:Audience"]! ?? builder.Configuration["Jwt:Issuer"]!,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==========================================================
// CẤU HÌNH PIPELINE (MIDDLEWARE)
// ==========================================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cafebook API v1");
    // Cài đặt RoutePrefix = string.Empty để hiển thị Swagger ngay khi truy cập domain (vd: https://cafebookapi.shushushu.id.vn/)
    options.RoutePrefix = string.Empty;
});

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();