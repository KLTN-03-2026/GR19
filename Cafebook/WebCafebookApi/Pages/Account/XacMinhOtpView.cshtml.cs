using System.ComponentModel.DataAnnotations;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages.Account
{
    public class XacMinhOtpViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public XacMinhOtpViewModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [BindProperty] public OtpInputModel Input { get; set; } = new();
        public string? ReturnUrl { get; set; }

        [TempData] public string? OtpMessage { get; set; }

        public class OtpInputModel
        {
            public int TempId { get; set; }
            public string TempPassword { get; set; } = string.Empty;
            [Required(ErrorMessage = "Vui lòng nhập mã OTP")] public string OtpCode { get; set; } = string.Empty;
            [Required] public string EditableEmail { get; set; } = string.Empty;
            [Required] public string EditablePhone { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            // Bắt buộc phải có TempId từ trang Đăng Ký chuyển qua
            if (TempData["TempId"] == null) return RedirectToPage("/Account/DangKyView");

            ReturnUrl = returnUrl;

            // Lấy dữ liệu đẩy xuống giao diện
            Input.TempId = Convert.ToInt32(TempData["TempId"]);
            Input.TempPassword = TempData["TempPassword"]?.ToString() ?? "";
            Input.EditableEmail = TempData["TempEmail"]?.ToString() ?? "";
            Input.EditablePhone = TempData["TempPhone"]?.ToString() ?? "";

            // Lấy Message để hiển thị, TempData tự Keep nhờ thuộc tính [TempData] ở trên
            OtpMessage = TempData["OtpMessage"]?.ToString();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid) return Page();

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var apiRequest = new VerifyOtpRequestDto
            {
                TempId = Input.TempId,
                Email = Input.EditableEmail,
                SoDienThoai = Input.EditablePhone,
                Password = Input.TempPassword,
                OtpCode = Input.OtpCode
            };

            var response = await httpClient.PostAsJsonAsync("api/web/khachhang/dangky/verify-otp", apiRequest);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<DangKyResponseDto>();
                if (apiResponse != null && apiResponse.Success)
                {
                    // XÁC THỰC XONG -> VỀ ĐĂNG NHẬP
                    TempData["ErrorMessage"] = "Xác thực thành công! Tài khoản đã được kích hoạt. Vui lòng đăng nhập.";
                    return RedirectToPage("/Account/DangNhapView", new { returnUrl = returnUrl });
                }
                ModelState.AddModelError(string.Empty, apiResponse?.Message ?? "Mã OTP không đúng hoặc đã hết hạn.");
            }
            return Page();
        }
    }
}