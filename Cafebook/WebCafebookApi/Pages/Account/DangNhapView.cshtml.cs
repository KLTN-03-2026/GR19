using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages.Account
{
    // Cấm trình duyệt lưu cache trang này
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DangNhapViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DangNhapViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }
        [TempData]
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập Tên đăng nhập, Email hoặc SĐT")]
            public string LoginIdentifier { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mã xác thực")]
            public string CaptchaResult { get; set; } = string.Empty;
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var captcha = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            HttpContext.Session.SetString("CaptchaCode", captcha);
            ViewData["CaptchaCode"] = captcha;
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            Response.Headers.Expires = "-1";
            Response.Headers.Pragma = "no-cache";

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            returnUrl ??= Url.Content("~/");

            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("KhachHang"))
            {
                return LocalRedirect(returnUrl);
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ReturnUrl = returnUrl;

            GenerateCaptcha();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Vui lòng nhập đầy đủ và hợp lệ các thông tin.";
                return RedirectToPage(new { returnUrl }); 
            }

            var expectedCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (string.IsNullOrEmpty(expectedCaptcha) || !string.Equals(expectedCaptcha, Input.CaptchaResult, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Mã xác thực không chính xác.";
                return RedirectToPage(new { returnUrl });
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var apiRequest = new DangNhapRequestDto
            {
                TenDangNhap = Input.LoginIdentifier,
                MatKhau = Input.Password
            };

            var response = await httpClient.PostAsJsonAsync("api/web/khachhang/dangnhap", apiRequest);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<DangNhapResponseDto>();
                if (apiResponse != null && apiResponse.Success && apiResponse.KhachHangData != null)
                {
                    var user = apiResponse.KhachHangData;
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdKhachHang.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap ?? user.Email ?? ""),
                new Claim(ClaimTypes.GivenName, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.MobilePhone, user.SoDienThoai ?? ""),
                new Claim(ClaimTypes.Role, "KhachHang")
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    HttpContext.Session.SetString("JwtToken", apiResponse.Token ?? "");
                    HttpContext.Session.SetString("AvatarUrl", user.AnhDaiDienUrl ?? "");

                    HttpContext.Session.Remove("CaptchaCode");

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ErrorMessage = apiResponse?.Message ?? "Lỗi không xác định từ API.";
                    return RedirectToPage(new { returnUrl });
                }
            }

            ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác.";
            return RedirectToPage(new { returnUrl });
        }
    }
}