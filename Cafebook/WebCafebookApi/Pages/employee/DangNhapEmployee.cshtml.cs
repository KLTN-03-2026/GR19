using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CafebookModel.Model.Shared; // Tuân thủ Quy tắc 1 & 5: Dùng đúng DTO Shared
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebCafebookApi.Pages.employee
{
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
            // Reset Authentication và Session
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.Remove("AvatarUrl");

            ReturnUrl = returnUrl ?? Url.Content("~/Employee/Dashboard");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/Employee/Dashboard");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Quy tắc 2: Dùng IHttpClientFactory cấu hình sẵn
                var httpClient = _httpClientFactory.CreateClient("ApiClient");

                // Map dữ liệu từ View Input sang API DTO
                var apiRequest = new LoginRequest
                {
                    Username = Input.TenDangNhap,
                    Password = Input.MatKhau
                };

                // Gọi Endpoint API dùng chung
                var response = await httpClient.PostAsJsonAsync("api/shared/Auth/login-nhan-vien", apiRequest);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Token))
                    {
                        // Quy tắc 3: Cấu hình Claims an toàn
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, apiResponse.IdNhanVien.ToString()),
                            new Claim(ClaimTypes.Name, apiResponse.HoTen),
                            new Claim(ClaimTypes.Role, apiResponse.TenVaiTro)
                        };

                        // Add danh sách quyền để kiểm tra phân quyền (Authorization)
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

                        // Quy tắc 6: Lưu JWT để đính kèm vào HttpClient các request sau
                        HttpContext.Session.SetString("JwtToken", apiResponse.Token);
                        HttpContext.Session.SetString("AvatarUrl", apiResponse.AnhDaiDien ?? "");

                        return LocalRedirect(ReturnUrl);
                    }
                }
                else
                {
                    // Đọc nội dung lỗi API trả về (401 Unauthorized, 403 Forbidden)
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
                    catch
                    {
                        // Bỏ qua lỗi parse Json
                    }
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
    }
}