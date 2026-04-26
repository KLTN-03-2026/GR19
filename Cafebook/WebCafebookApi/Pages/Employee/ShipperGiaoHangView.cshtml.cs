using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages.Employee
{
    [Authorize]
    public class ShipperGiaoHangViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public string ApiBaseUrl { get; set; } = string.Empty;
        public string? JwtToken { get; set; }

        public ShipperGiaoHangViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet()
        {
            // Kiểm tra Token, nếu mất Session thì đá về trang Đăng nhập
            JwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(JwtToken)) return RedirectToPage("/Employee/DangNhapEmployee");

            var client = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{client.BaseAddress}api/web/nhanvien/giaohangweb";

            return Page();
        }
    }
}