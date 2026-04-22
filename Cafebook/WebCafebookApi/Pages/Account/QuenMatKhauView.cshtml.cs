using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.ComponentModel.DataAnnotations;
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

        // Không cần truyền EmailService nữa
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
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // 1. Tạo mã xác nhận động
            string verificationCode = _random.Next(100000, 999999).ToString("D6");

            // 2. Gói dữ liệu gửi xuống API
            var req = new GuiMaXacNhanRequestDto
            {
                Email = Input.Email,
                MaXacNhan = verificationCode
            };

            try
            {
                // Dùng ApiClient động (Đọc từ file config)
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.PostAsJsonAsync("api/web/quenmatkhau/gui-ma", req);

                if (response.IsSuccessStatusCode)
                {
                    // Chỉ lưu Cache khi API xác nhận Email hợp lệ và đã gửi mail thành công
                    string cacheKey = $"ForgotPassword_{Input.Email}";
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    _cache.Set(cacheKey, verificationCode, cacheEntryOptions);

                    TempData["VerificationEmail"] = Input.Email;
                    return RedirectToPage("./XacNhanMaView");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    ModelState.AddModelError(string.Empty, error?.Message ?? "Có lỗi xảy ra, vui lòng thử lại.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối máy chủ: {ex.Message}");
                return Page();
            }
        }

        private class ApiErrorResponse { public string? Message { get; set; } }
    }
}