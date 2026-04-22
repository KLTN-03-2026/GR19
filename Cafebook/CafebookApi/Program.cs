using CafebookApi.Data;
// using CafebookApi.Services; // (Bỏ comment khi tạo xong VNPayService, AiToolService)
// using CafebookApi.Hubs;     // (Bỏ comment khi tạo xong ChatHub)
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<CafebookApi.Services.AutoCancelReservationService>();
// ==========================================================
// 1. KẾT NỐI DATABASE
// ==========================================================
var connectionString = builder.Configuration.GetConnectionString("CafeBookConnectionString");
builder.Services.AddDbContext<CafebookDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================================
// 2. CẤU HÌNH CORS (Bảo mật truy cập chéo)
// ==========================================================
builder.Services.AddCors(options =>
{
    // Policy chung cho App WPF và Mobile gọi API (Cho phép tất cả)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Policy chặt chẽ hơn dành riêng cho Web Frontend gọi SignalR (Bắt buộc AllowCredentials)
    options.AddPolicy("SignalRPolicy", policy =>
    {
        // Thay bằng port Web thực tế của bạn (VD: localhost:5156, localhost:3000)
        policy.WithOrigins("http://localhost:5202")
              .AllowAnyHeader()
              .AllowAnyMethod()
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

// Thêm SignalR cho chức năng Chat / Thông báo realtime
builder.Services.AddSignalR();

// Đăng ký các Service nghiệp vụ (Sẽ tạo ở bước sau)
// builder.Services.AddScoped<AiToolService>();
// builder.Services.AddScoped<VNPayService>(); // <-- Chuẩn bị sẵn cho VNPay

// ==========================================================
// 4. CẤU HÌNH BẢO MẬT JWT
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

// Chỉ dùng UseCors 1 lần với Policy chung cho toàn bộ API
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Gắn Policy riêng cho endpoint SignalR để tránh lỗi khi Web kết nối
// app.MapHub<ChatHub>("/chathub").RequireCors("SignalRPolicy");

app.Run();