using CafebookModel.Model.ModelWeb.KhachVangLai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;

namespace WebCafebookApi.Pages
{
    [AllowAnonymous]
    public class HoTroViewModel : PageModel
    {
        private readonly IConfiguration _config;
        public HoTroViewModel(IConfiguration config) { _config = config; }

        public HoTroViewDto HoTroData { get; set; } = new HoTroViewDto();
        public string ApiBaseUrl { get; set; } = "";

        public IActionResult OnGet()
        {
            /*
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/khach-hang/ho-tro/");
            }
            */
            string? url = _config.GetSection("ApiSettings")["BaseUrl"] ?? _config["ApiBaseUrl"];
            ApiBaseUrl = string.IsNullOrWhiteSpace(url) ? "http://localhost:5202" : url.TrimEnd('/');

            string? guestSessionId = HttpContext.Session.GetString("GuestChatSession");
            if (string.IsNullOrEmpty(guestSessionId))
            {
                guestSessionId = "guest_fallback_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                HttpContext.Session.SetString("GuestChatSession", guestSessionId);
            }
            HoTroData.GuestSessionId = guestSessionId;
            return Page();
        }
    }
}