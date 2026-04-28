using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace WebCafebookApi.Pages.Account
{
    [Authorize]
    [Authorize(Roles = "KhachHang")]
    public class HoTroKHViewModel : PageModel
    {
        private readonly IConfiguration _config;
        public HoTroKHViewModel(IConfiguration config) { _config = config; }

        public HoTroKHViewDto HoTroData { get; set; } = new HoTroKHViewDto();
        public string ApiBaseUrl { get; set; } = "";
        public string? JwtToken { get; set; }

        // BIẾN MỚI ĐỂ LƯU PHIÊN KHÁCH VÃNG LAI
        public string GuestSessionId { get; set; } = "";

        public void OnGet()
        {
            string? url = _config.GetSection("ApiSettings")["BaseUrl"] ?? _config["ApiBaseUrl"];
            ApiBaseUrl = string.IsNullOrWhiteSpace(url) ? "http://localhost:5202" : url.TrimEnd('/');

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            HoTroData.IdKhachHang = id;
            HoTroData.TenKhachHang = User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue(ClaimTypes.Name) ?? "Khách hàng";
            JwtToken = HttpContext.Session.GetString("JwtToken");

            // LẤY SESSION CHAT TRƯỚC KHI ĐĂNG NHẬP
            GuestSessionId = HttpContext.Session.GetString("GuestChatSession") ?? "";
        }
    }
}