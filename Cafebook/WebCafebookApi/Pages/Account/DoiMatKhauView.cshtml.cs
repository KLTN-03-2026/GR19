using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages.Account
{
    [Authorize(Roles = "KhachHang")] // [Quy tắc 3] Bảo mật lớp 1
    public class DoiMatKhauViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public DoiMatKhauDto Input { get; set; } = new();

        [TempData]
        public string? SweetAlertMessage { get; set; }

        [TempData]
        public string? SweetAlertType { get; set; }

        public DoiMatKhauViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId) || userId == 0) return Challenge();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // [Quy tắc 2] Không hardcode localhost. Dùng cấu hình ApiClient
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var response = await httpClient.PutAsJsonAsync($"/api/web/khachhang/doi-mat-khau/{userId}", Input);

                if (response.IsSuccessStatusCode)
                {
                    await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                    return RedirectToPage("/Account/DangNhapView");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync();
                    return RedirectToPage("/Account/DangNhapView");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                SweetAlertMessage = error?.Message ?? "Không thể đổi mật khẩu lúc này.";
                SweetAlertType = "error";
            }
            catch (Exception) 
            {
                SweetAlertMessage = "Lỗi kết nối máy chủ. Vui lòng thử lại sau.";
                SweetAlertType = "error";
            }

            return Page();
        }

        private class ApiErrorResponse { public string? Message { get; set; } }
    }
}