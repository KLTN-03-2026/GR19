using System.ComponentModel.DataAnnotations;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages.Account
{
    public class DangKyViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DangKyViewModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [BindProperty] public InputModel Input { get; set; } = new();
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "SĐT không hợp lệ (Phải đủ 10 số và bắt đầu bằng 03, 05, 07, 08, 09).")]
            public string SoDienThoai { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập Email.")]
            [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null) { ReturnUrl = returnUrl; }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid) return Page();

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var apiRequest = new DangKyRequestDto { Email = Input.Email, SoDienThoai = Input.SoDienThoai, Password = Input.Password };

            var response = await httpClient.PostAsJsonAsync("api/web/khachhang/dangky", apiRequest);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<DangKyResponseDto>();
                if (apiResponse != null)
                {
                    if (apiResponse.Success)
                    {
                        TempData["TempId"] = apiResponse.TempId;
                        TempData["TempPassword"] = Input.Password;
                        TempData["TempEmail"] = apiResponse.TempEmail;
                        TempData["TempPhone"] = apiResponse.TempPhone;
                        TempData["OtpMessage"] = apiResponse.Message;

                        return RedirectToPage("/Account/XacMinhOtpView", new { returnUrl = returnUrl });
                    }
                    else if (apiResponse.IsOfficialAccount)
                    {
                        TempData["ErrorMessage"] = apiResponse.Message;
                        return RedirectToPage("/Account/DangNhapView", new { returnUrl = returnUrl });
                    }

                    ModelState.AddModelError(string.Empty, apiResponse.Message ?? "Lỗi đăng ký.");
                }
            }
            return Page();
        }
    }
}