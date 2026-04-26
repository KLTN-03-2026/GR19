using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.DataProtection;

namespace WebCafebookApi.Pages.Employee
{
    [Authorize]
    public class SoDoBanViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<KhuVucWebDto> KhuVucList { get; set; } = new();
        public string? JwtToken { get; set; }
        public string ApiBaseUrl { get; set; } = string.Empty;

        private readonly IDataProtector _protector;

        [TempData]
        public string? ErrorMessage { get; set; }

        public SoDoBanViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.HoaDon.Security"); 
        }

        public async Task<IActionResult> OnGetAsync()
        {
            JwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(JwtToken)) return RedirectToPage("/Employee/DangNhapEmployee");

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{httpClient.BaseAddress}api/web/nhanvien/sodobanweb";

            try
            {
                var response = await httpClient.GetFromJsonAsync<List<KhuVucWebDto>>("api/web/nhanvien/sodobanweb/khuvuc-list");
                if (response != null) KhuVucList = response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Remove("JwtToken");
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn.";
                return RedirectToPage("/Employee/DangNhapEmployee");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể tải cấu trúc khu vực. Chi tiết: " + ex.Message;
            }

            return Page();
        }

        public IActionResult OnGetEncryptUrl(int id)
        {
            var hash = _protector.Protect(id.ToString());
            return new JsonResult(new { url = $"/nhan-vien/goi-mon/{hash}" });
        }
    }
}