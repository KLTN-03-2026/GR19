using CafebookApi.Data;
using CafebookApi.Services;
using CafebookApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 0. TỰ ĐỘNG TẠO/ĐỌC FILE CẤU HÌNH KHÓA ADMIN (TOTP)
// ==========================================================
string currentDir = Directory.GetCurrentDirectory();
DirectoryInfo parentDir = Directory.GetParent(currentDir)!;
string configDirPath = Path.Combine(parentDir.FullName, "SettingCafebook");
string adminKeyFilePath = Path.Combine(configDirPath, "ApisecretKeyAdministrator.json");

if (!Directory.Exists(configDirPath))
{
    Directory.CreateDirectory(configDirPath);
}

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
    File.WriteAllText(adminKeyFilePath, System.Text.Json.JsonSerializer.Serialize(defaultAdminConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
}

// Nạp file cấu hình, tự động cập nhật khi file thay đổi
builder.Configuration.AddJsonFile(adminKeyFilePath, optional: false, reloadOnChange: true);

// ==========================================================
// ĐĂNG KÝ CÁC DỊCH VỤ NỀN
// ==========================================================
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<CafebookApi.Services.DatabaseBackupService>();
builder.Services.AddHostedService<CafebookApi.Services.AutoCancelOrderService>();
builder.Services.AddHostedService<CafebookApi.Services.AutoUnlockAccountService>();
builder.Services.AddHostedService<CafebookApi.Services.AutoCancelReservationService>();
builder.Services.AddHostedService<CafebookApi.Services.DailyReminderBackgroundService>();

// ==========================================================
// 1. KẾT NỐI DATABASE
// ==========================================================
var connectionString = builder.Configuration.GetConnectionString("CafeBookConnectionString");
builder.Services.AddDbContext<CafebookDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================================
// 2. CẤU HÌNH CORS (ĐÃ SỬA LỖI SIGNALR)
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

// ==========================================================
// 3. ĐĂNG KÝ CÁC DỊCH VỤ (DEPENDENCY INJECTION)
// ==========================================================
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
// 4. CẤU HÌNH BẢO MẬT JWT VÀ SIGNALR
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
// 5. CẤU HÌNH PIPELINE (MIDDLEWARE)
// ==========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();