using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebCafebookApi.Pages.Account
{
    [Authorize(Roles = "KhachHang")]
    public class ThongTinCaNhanModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public ThongTinCaNhanUpdateDto Input { get; set; } = new();

        [BindProperty]
        public IFormFile? AvatarFile { get; set; }

        public string AvatarHienTaiUrl { get; set; } = string.Empty;
        public bool IsEditMode { get; set; } = false;

        public ThongTinCaNhanModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);
            return userId;
        }

        public async Task<IActionResult> OnGetAsync(string? handler)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                await HttpContext.SignOutAsync();
                return RedirectToPage("/Account/DangNhapView");
            }

            if (handler == "Edit") IsEditMode = true;

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.GetAsync($"api/web/khachhang/thongtincanhan/{userId}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync();
                    return RedirectToPage("/Account/DangNhapView");
                }

                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<ThongTinCaNhanDto>();
                    if (profile != null)
                    {
                        Input.HoTen = profile.HoTen;
                        Input.SoDienThoai = profile.SoDienThoai;
                        Input.Email = profile.Email;
                        Input.DiaChi = profile.DiaChi;
                        Input.TenDangNhap = profile.TenDangNhap ?? "";

                        // ==========================================
                        // FIX LỖI CACHE ẢNH: Thêm đuôi thời gian (Ticks)
                        // ==========================================
                        string cleanUrl = profile.AnhDaiDienUrl.Split('?')[0]; // Xóa đuôi cũ (nếu có)
                        AvatarHienTaiUrl = cleanUrl + "?v=" + DateTime.Now.Ticks; // Ép trình duyệt nhận diện đây là link mới

                        // Cập nhật lại Session cho thanh Navbar (_LoginPartial)
                        HttpContext.Session.SetString("AvatarUrl", AvatarHienTaiUrl);
                        // ==========================================
                    }
                    return Page();
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải dữ liệu hồ sơ. Vui lòng thử lại sau.";
                    return Page();
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể kết nối đến máy chủ API.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                await HttpContext.SignOutAsync();
                return RedirectToPage("/Account/DangNhapView");
            }

            if (!ModelState.IsValid) return await OnGetAsync("Edit");

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(Input.HoTen), nameof(Input.HoTen));
            formData.Add(new StringContent(Input.SoDienThoai ?? ""), nameof(Input.SoDienThoai));
            formData.Add(new StringContent(Input.Email ?? ""), nameof(Input.Email));
            formData.Add(new StringContent(Input.DiaChi ?? ""), nameof(Input.DiaChi));
            formData.Add(new StringContent(Input.TenDangNhap), nameof(Input.TenDangNhap));

            if (AvatarFile != null)
            {
                formData.Add(new StreamContent(AvatarFile.OpenReadStream()), "avatarFile", AvatarFile.FileName);
            }

            try
            {
                var response = await httpClient.PutAsync($"api/web/khachhang/thongtincanhan/update/{userId}", formData);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HttpContext.SignOutAsync();
                    return RedirectToPage("/Account/DangNhapView");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    var result = await response.Content.ReadFromJsonAsync<UpdateResponse>();

                    await UpdateClaims(Input.HoTen, Input.TenDangNhap);
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cập nhật thất bại. Vui lòng kiểm tra lại dữ liệu.");
                    AvatarHienTaiUrl = HttpContext.Session.GetString("AvatarUrl") ?? "";
                    IsEditMode = true;
                    return Page();
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống khi cập nhật.";
                return await OnGetAsync("Edit");
            }
        }

        private async Task UpdateClaims(string newHoTen, string newTenDangNhap)
        {
            var user = User.Identity as ClaimsIdentity;
            if (user != null)
            {
                var oldGivenName = user.FindFirst(ClaimTypes.GivenName);
                if (oldGivenName != null) user.RemoveClaim(oldGivenName);
                user.AddClaim(new Claim(ClaimTypes.GivenName, newHoTen));

                var oldName = user.FindFirst(ClaimTypes.Name);
                if (oldName != null) user.RemoveClaim(oldName);
                user.AddClaim(new Claim(ClaimTypes.Name, newTenDangNhap));

                await HttpContext.SignOutAsync();
                await HttpContext.SignInAsync(new ClaimsPrincipal(user));
            }
        }

        private class UpdateResponse { public string newAvatarUrl { get; set; } = string.Empty; }
    }
}