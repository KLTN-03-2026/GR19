using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages.Account
{
    public class DangXuatModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Xóa Cookie xác thực của ASP.NET Core
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Xóa toàn bộ Session (Token, Thông tin người dùng, Giỏ hàng...)
            HttpContext.Session.Clear();

            // 3. Chuyển hướng người dùng về Trang chủ
            return RedirectToPage("/TrangChuView");
        }
    }
}