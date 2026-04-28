// Vị trí lưu: WebCafebookApi/Program.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. TỰ ĐỘNG TÌM/TẠO THƯ MỤC & FILE CONFIG
// ==========================================
string currentDir = Directory.GetCurrentDirectory();
DirectoryInfo parentDir = Directory.GetParent(currentDir)!;
string configDirPath = Path.Combine(parentDir.FullName, "SettingCafebook");
string configFilePath = Path.Combine(configDirPath, "WebConfig.json");

if (!Directory.Exists(configDirPath))
{
    Directory.CreateDirectory(configDirPath);
}

if (!File.Exists(configFilePath))
{
    var defaultConfig = new
    {
        ApiSettings = new
        {
            BaseUrl = "http://localhost:5202"
        }
    };
    File.WriteAllText(configFilePath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
}
builder.Configuration.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
string apiServerUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5202";

builder.Services.AddHttpContextAccessor();
// ==========================================
// 2. CẤU HÌNH HTTP CLIENT ĐỘNG (ApiClient)
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

app.UseExceptionHandler("/loi-he-thong?code=500");
app.UseExceptionHandler("/loi-he-thong?code=400");
app.UseStatusCodePagesWithReExecute("/loi-he-thong", "?code={0}");

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();