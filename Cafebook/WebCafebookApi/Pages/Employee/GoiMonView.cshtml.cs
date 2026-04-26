using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection; 

namespace WebCafebookApi.Pages.Employee
{
    [Authorize]
    public class GoiMonViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector; 

        public int IdHoaDon { get; set; }
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string? JwtToken { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public GoiMonViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.HoaDon.Security");
        }

        public IActionResult OnGet(string? maHoaDon)
        {
            if (string.IsNullOrEmpty(maHoaDon))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin Hóa đơn.";
                return RedirectToPage("/Employee/SoDoBanView");
            }

            try
            {
                string rawId = _protector.Unprotect(maHoaDon);
                IdHoaDon = int.Parse(rawId);
            }
            catch (Exception)
            {
                ErrorMessage = "Mã đường dẫn không hợp lệ hoặc đã bị can thiệp!";
                return Page();
            }

            JwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(JwtToken)) return RedirectToPage("/Employee/DangNhapEmployee");

            var client = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{client.BaseAddress}api/web/nhanvien/goimonweb";

            return Page();
        }
    }
}