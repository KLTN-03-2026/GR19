using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages.Employee
{
    [Authorize]
    public class PhanHoiKhachHangViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string? JwtToken { get; set; }

        public PhanHoiKhachHangViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
            JwtToken = HttpContext.Session.GetString("JwtToken");
            var client = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{client.BaseAddress}api/web/nhanvien/PhanHoiKhachHangWeb";
        }
    }
}