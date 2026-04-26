using CafebookApi.Data;
using CafebookApi.Services;
using CafebookApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<CafebookApi.Services.AutoCancelOrderService>();
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
    // Gộp chung thành 1 Policy mạnh nhất, bẻ khóa mọi chặn chéo Port
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // <--- CHÌA KHÓA: Chấp nhận mọi web client (cổng 5175, 5166, v.v.)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();           // <--- BẮT BUỘC: Để SignalR WebSockets hoạt động
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

// Thêm SignalR cho chức năng Chat / Thông báo realtime
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

    // ======================================================
    // FIX: BẮT TOKEN CHO SIGNALR (ĐỌC TỪ QUERY STRING)
    // ======================================================
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Nếu request là gửi tới Hub (đường dẫn bắt đầu bằng /chatHub)
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                // Lấy token từ Query String gán vào Context
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

// CỰC KỲ QUAN TRỌNG: Kích hoạt CORS ngay giữa Routing và Auth
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Gắn Hub SignalR (Viết hoa chữ cái theo đúng chuẩn /chatHub ở phía Client)
app.MapHub<ChatHub>("/chatHub");

app.Run();