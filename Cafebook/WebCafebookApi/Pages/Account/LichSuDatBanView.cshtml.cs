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
    public class LichSuDatBanViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PagedLichSuResponseDto PagedData { get; set; } = new PagedLichSuResponseDto();

        [TempData] public string? ErrorMessage { get; set; }
        [TempData] public string? SuccessMessage { get; set; }

        // --- BỘ LỌC ĐẦU VÀO ---
        [BindProperty(SupportsGet = true)] public int PageIndex { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? Status { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }

        public LichSuDatBanViewModel(IHttpClientFactory httpClientFactory)
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

            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                // Xây dựng Query String truyền lên API
                var queryParams = new List<string> { $"page={PageIndex}" };
                if (!string.IsNullOrEmpty(Search)) queryParams.Add($"search={Search}");
                if (!string.IsNullOrEmpty(Status)) queryParams.Add($"status={Status}");
                if (FromDate.HasValue) queryParams.Add($"fromDate={FromDate.Value:yyyy-MM-dd}");
                if (ToDate.HasValue) queryParams.Add($"toDate={ToDate.Value:yyyy-MM-dd}");

                string queryString = string.Join("&", queryParams);
                var response = await httpClient.GetAsync($"api/web/khach-hang/lich-su-dat-ban?{queryString}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync();
                    return RedirectToPage("/Account/DangNhapView");
                }

                if (response.IsSuccessStatusCode)
                {
                    PagedData = await response.Content.ReadFromJsonAsync<PagedLichSuResponseDto>()
                                ?? new PagedLichSuResponseDto();
                }
                else
                {
                    ErrorMessage = "Không thể tải lịch sử đặt bàn. Vui lòng thử lại sau.";
                }

                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "Lỗi kết nối đến máy chủ. Vui lòng kiểm tra mạng.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostHuyBanAsync(int idPhieu, string lyDoHuy)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.PutAsJsonAsync($"api/web/khach-hang/lich-su-dat-ban/huy/{idPhieu}", lyDoHuy);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đã gửi yêu cầu hủy bàn thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy bàn. Vui lòng liên hệ hotline.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối máy chủ.";
            }
            return RedirectToPage();
        }
    }
}