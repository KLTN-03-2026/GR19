using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages.Account
{
    [Authorize(Roles = "KhachHang")]
    public class LichSuThueSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PagedLichSuThueSachResponseDto PagedData { get; set; } = new();

        [TempData] public string? ErrorMessage { get; set; }

        // Bộ lọc từ URL
        [BindProperty(SupportsGet = true)] public int PageIndex { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? Status { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }

        public LichSuThueSachViewModel(IHttpClientFactory httpClientFactory)
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
            if (userId == 0)
            {
                await HttpContext.SignOutAsync();
                return RedirectToPage("/Account/DangNhapView");
            }

            // Nguyên tắc 2: Dùng ApiClient
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var queryParams = new List<string> { $"page={PageIndex}" };
                if (!string.IsNullOrEmpty(Search)) queryParams.Add($"search={Search}");
                if (!string.IsNullOrEmpty(Status)) queryParams.Add($"status={Status}");
                if (FromDate.HasValue) queryParams.Add($"fromDate={FromDate.Value:yyyy-MM-dd}");
                if (ToDate.HasValue) queryParams.Add($"toDate={ToDate.Value:yyyy-MM-dd}");

                string queryString = string.Join("&", queryParams);
                var response = await httpClient.GetAsync($"api/web/khach-hang/lich-su-thue-sach?{queryString}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync();
                    return RedirectToPage("/Account/DangNhapView");
                }

                if (response.IsSuccessStatusCode)
                {
                    PagedData = await response.Content.ReadFromJsonAsync<PagedLichSuThueSachResponseDto>() ?? new();
                }
                else
                {
                    ErrorMessage = "Không thể tải lịch sử thuê sách.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Lỗi kết nối đến máy chủ. Vui lòng kiểm tra mạng.";
            }

            return Page();
        }
    }
}