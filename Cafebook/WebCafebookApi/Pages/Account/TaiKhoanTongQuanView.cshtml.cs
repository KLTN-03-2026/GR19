using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;

namespace WebCafebookApi.Pages.Account
{
    [Authorize(Roles = "KhachHang")]
    public class TaiKhoanTongQuanViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TaiKhoanTongQuanDto Overview { get; set; } = new();
        public string HoTen { get; set; } = string.Empty;

        public TaiKhoanTongQuanViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);
            return userId;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToPage("/Account/DangNhapView");

            HoTen = User.FindFirstValue(ClaimTypes.GivenName) ?? "Khách hàng";

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.GetAsync($"api/web/taikhoantongquan/{userId}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync();

                    return RedirectToPage("/Account/DangNhapView");
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TaiKhoanTongQuanDto>();
                    if (result != null)
                    {
                        Overview = result;
                    }
                }

                return Page();
            }
            catch (System.Exception)
            {
                return Page();
            }
        }
    }
}