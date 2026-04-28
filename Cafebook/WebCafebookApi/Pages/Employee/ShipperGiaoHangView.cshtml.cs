using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            JwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(JwtToken))
            {
                string currentUrl = Request.Path + Request.QueryString;
                return RedirectToPage("/Employee/DangNhapEmployee", new { ReturnUrl = currentUrl });
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{client.BaseAddress}api/web/nhanvien/giaohangweb";

            return Page();
        }
    }
}