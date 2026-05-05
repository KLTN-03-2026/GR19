using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

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

        [BindProperty]
        public CapNhatThongTinWebDto UpdateModel { get; set; } = new();

        [BindProperty]
        public DoiMatKhauWebDto PasswordModel { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            return await LoadDashboardDataAsync();
        }

        private async Task<IActionResult> LoadDashboardDataAsync()
        {
            var jwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/Employee/DangNhapEmployee", new { ReturnUrl = Request.Path + Request.QueryString });
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                var response = await httpClient.GetAsync("api/web/nhanvien/TongQuan/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TongQuanDto>();
                    if (data != null)
                    {
                        DashboardData = data;
                        // Gán sẵn dữ liệu vào form Cập nhật
                        UpdateModel.HoTen = data.ThongTin.HoTen;
                        UpdateModel.SoDienThoai = data.ThongTin.SoDienThoai ?? "";
                        UpdateModel.Email = data.ThongTin.Email;
                        UpdateModel.DiaChi = data.ThongTin.DiaChi;
                    }
                    return Page();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Remove("JwtToken");
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToPage("/Employee/DangNhapEmployee");
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

        public async Task<IActionResult> OnPostUpdateInfoAsync()
        {
            if (!ModelState.IsValid) return await LoadDashboardDataAsync();

            var jwtToken = HttpContext.Session.GetString("JwtToken");
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await httpClient.PutAsJsonAsync("api/web/nhanvien/TongQuan/update-info", UpdateModel);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = "Cập nhật thông tin thất bại.";
            return await LoadDashboardDataAsync();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid) return await LoadDashboardDataAsync();

            var jwtToken = HttpContext.Session.GetString("JwtToken");
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await httpClient.PostAsJsonAsync("api/web/nhanvien/TongQuan/change-password", PasswordModel);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToPage();
            }

            var errorStr = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = string.IsNullOrEmpty(errorStr) ? "Đổi mật khẩu thất bại." : errorStr;
            return await LoadDashboardDataAsync();
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một ảnh.";
                return RedirectToPage();
            }

            var jwtToken = HttpContext.Session.GetString("JwtToken");
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            using var content = new MultipartFormDataContent();
            using var fileStream = avatarFile.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(avatarFile.ContentType);
            content.Add(streamContent, "avatarFile", avatarFile.FileName);

            var response = await httpClient.PostAsync("api/web/nhanvien/TongQuan/upload-avatar", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Tải ảnh đại diện thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Tải ảnh lên thất bại.";
            }

            return RedirectToPage();
        }
    }
}