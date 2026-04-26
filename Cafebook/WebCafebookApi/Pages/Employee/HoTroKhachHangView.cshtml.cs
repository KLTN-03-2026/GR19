using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace WebCafebookApi.Pages.Employee
{
    [Authorize]
    public class HoTroKhachHangViewModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public HoTroKhachHangViewModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ApiBaseUrl { get; set; } = "";
        public string? JwtToken { get; set; }

        public void OnGet()
        {
            string? url = _configuration.GetSection("ApiSettings")["BaseUrl"];
            if (string.IsNullOrWhiteSpace(url)) url = _configuration["ApiBaseUrl"];

            ApiBaseUrl = string.IsNullOrWhiteSpace(url) ? "http://localhost:5202" : url.TrimEnd('/');
            JwtToken = HttpContext.Session.GetString("JwtToken");
        }
    }
}