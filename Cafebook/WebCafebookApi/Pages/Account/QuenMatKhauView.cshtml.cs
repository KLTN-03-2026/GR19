using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Http; // Đảm bảo có thư viện này để dùng Session
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages.Account
{
    public class QuenMatKhauViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();

        public QuenMatKhauViewModel(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mã xác thực.")]
            public string CaptchaResult { get; set; } = string.Empty;
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var captcha = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[_random.Next(s.Length)]).ToArray());

            HttpContext.Session.SetString("CaptchaCode", captcha);
            ViewData["CaptchaCode"] = captcha;
        }

        public void OnGet()
        {
            GenerateCaptcha();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                GenerateCaptcha();
                return Page();
            }

            var expectedCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (string.IsNullOrEmpty(expectedCaptcha) || !string.Equals(expectedCaptcha, Input.CaptchaResult, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.CaptchaResult", "Mã xác thực không chính xác.");
                GenerateCaptcha(); 
                return Page();
            }

            string verificationCode = _random.Next(100000, 999999).ToString("D6");

            var req = new GuiMaXacNhanRequestDto
            {
                Email = Input.Email,
                MaXacNhan = verificationCode
            };

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.PostAsJsonAsync("api/web/quenmatkhau/gui-ma", req);

                if (response.IsSuccessStatusCode)
                {
                    string cacheKey = $"ForgotPassword_{Input.Email}";
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(cacheKey, verificationCode, cacheEntryOptions);

                    HttpContext.Session.Remove("CaptchaCode");

                    TempData["VerificationEmail"] = Input.Email;
                    return RedirectToPage("./XacNhanMaView");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    ModelState.AddModelError(string.Empty, error?.Message ?? "Có lỗi xảy ra, vui lòng thử lại.");
                    GenerateCaptcha(); 
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối máy chủ: {ex.Message}");
                GenerateCaptcha(); 
                return Page();
            }
        }

        private class ApiErrorResponse { public string? Message { get; set; } }
    }
}