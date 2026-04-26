using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages.Employee
{
    [Authorize]
    public class TongQuanViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TongQuanViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public TongQuanDto DashboardData { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var response = await httpClient.GetAsync("api/web/nhanvien/TongQuan/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TongQuanDto>();
                    if (data != null)
                    {
                        DashboardData = data;
                    }
                    return Page();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Remove("JwtToken");
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToPage("/Employee/Login");
                }

                ErrorMessage = "Không thể tải dữ liệu tổng quan.";
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "Mất kết nối đến máy chủ API.";
                return Page();
            }
        }
    }
}