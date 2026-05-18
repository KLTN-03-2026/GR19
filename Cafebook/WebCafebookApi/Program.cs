using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. TỰ ĐỘNG TÌM/TẠO THƯ MỤC CHUNG (SettingCafebook)
// ==========================================
string currentDir = Directory.GetCurrentDirectory();
DirectoryInfo parentDir = Directory.GetParent(currentDir)!;
string configDirPath = Path.Combine(parentDir.FullName, "SettingCafebook");

if (!Directory.Exists(configDirPath))
{
    Directory.CreateDirectory(configDirPath);
}

// ==========================================
// 2. CẤU HÌNH DATA PROTECTION (DÙNG CHUNG KEY VỚI BACKEND)
// ==========================================
string dataProtectionDirPath = Path.Combine(configDirPath, "SharedKeys");
if (!Directory.Exists(dataProtectionDirPath))
{
    Directory.CreateDirectory(dataProtectionDirPath);
}

var sharedKeyDir = new DirectoryInfo(dataProtectionDirPath);
builder.Services.AddDataProtection()
    .SetApplicationName("CafebookSystem") // Tên này phải y hệt bên Backend
    .PersistKeysToFileSystem(sharedKeyDir);

// ==========================================
// 3. TỰ ĐỘNG TẠO/ĐỌC FILE CẤU HÌNH WEB
// ==========================================
string configFilePath = Path.Combine(configDirPath, "WebConfig.json");
if (!File.Exists(configFilePath))
{
    var defaultConfig = new
    {
        ApiSettings = new
        {
            BaseUrl = ""
        }
    };
    File.WriteAllText(configFilePath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
}
builder.Configuration.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
string apiServerUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "";

builder.Services.AddHttpContextAccessor();

// ==========================================
// 4. CẤU HÌNH HTTP CLIENT ĐỘNG (ApiClient)
// ==========================================
builder.Services.AddHttpClient("ApiClient", (serviceProvider, client) =>
{
    client.BaseAddress = new Uri(apiServerUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
    if (httpContextAccessor != null)
    {
        var token = httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
});

builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// 5. CẤU HÌNH AUTHENTICATION (COOKIES)
// ==========================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthToken";
        options.LoginPath = "/dang-nhap";
        options.AccessDeniedPath = "/AccessDenied";
        options.LogoutPath = "/Account/DangXuat";

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var requestPath = context.Request.Path;

                if (requestPath.StartsWithSegments("/nhan-vien", StringComparison.OrdinalIgnoreCase) ||
                    requestPath.StartsWithSegments("/Employee", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                    context.Response.Redirect($"/nhan-vien/dang-nhap-nhan-vien?ReturnUrl={returnUrl}");
                }
                else
                {
                    context.Response.Redirect(context.RedirectUri);
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();

var app = builder.Build();

// ==========================================
// 6. PIPELINE MIDDLEWARE
// ==========================================
app.UseExceptionHandler("/loi-he-thong?code=500");
app.UseStatusCodePagesWithReExecute("/loi-he-thong", "?code={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();