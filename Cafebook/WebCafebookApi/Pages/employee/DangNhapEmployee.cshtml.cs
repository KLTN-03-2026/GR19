using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CafebookModel.Model.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebCafebookApi.Pages.Employee
{
    [IgnoreAntiforgeryToken]
    public class DangNhapEmployeeModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DangNhapEmployeeModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập Tên đăng nhập, Email hoặc SĐT")]
            public string TenDangNhap { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [DataType(DataType.Password)]
            public string MatKhau { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.Remove("AvatarUrl");

            ReturnUrl = returnUrl ?? Url.Content("~/Employee/TongQuanView");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var apiRequest = new LoginRequest
                {
                    Username = Input.TenDangNhap,
                    Password = Input.MatKhau
                };

                var response = await httpClient.PostAsJsonAsync("api/shared/Auth/login-nhan-vien", apiRequest);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Token))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, apiResponse.IdNhanVien.ToString()),
                            new Claim(ClaimTypes.Name, apiResponse.HoTen),
                            new Claim(ClaimTypes.Role, apiResponse.TenVaiTro)
                        };

                        if (apiResponse.Quyen != null)
                        {
                            foreach (var quyen in apiResponse.Quyen)
                            {
                                claims.Add(new Claim("Permission", quyen));
                            }
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                            IsPersistent = true
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        string avatar = apiResponse.AnhDaiDien ?? "";
                        if (!string.IsNullOrEmpty(avatar) && !avatar.StartsWith("http"))
                        {
                            var apiBaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/');
                            if (!avatar.StartsWith("/")) avatar = "/" + avatar;
                            avatar = $"{apiBaseUrl}{avatar}";
                        }

                        HttpContext.Session.SetString("JwtToken", apiResponse.Token);
                        HttpContext.Session.SetString("AvatarUrl", avatar);

                        if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
                        {
                            return RedirectToPage("/Employee/TongQuanView");
                        }

                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if (errorJson.TryGetProperty("message", out var msg))
                        {
                            ErrorMessage = msg.GetString();
                            return Page();
                        }
                    }
                    catch { }
                }

                ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostLogout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Employee/DangNhapEmployee");
        }
    }
}