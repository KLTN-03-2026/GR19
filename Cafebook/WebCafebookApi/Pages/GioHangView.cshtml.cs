using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebCafebookApi.Services;

namespace WebCafebookApi.Pages
{
    [Authorize(Roles = "KhachHang")]
    public class GioHangViewPageModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public GioHangViewPageModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public GioHangResponseDto Cart { get; set; } = new();
        public List<GioHangKhuyenMaiDto> AvailablePromotions { get; set; } = new();

        // Sửa lỗi cảnh báo CS0104
        private const string PromoKey = "AppliedPromo";

        public async Task<IActionResult> OnGetAsync()
        {
            var sessionCart = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new();
            var appliedPromo = HttpContext.Session.GetString(PromoKey);

            var request = new GioHangSyncRequestDto
            {
                Items = sessionCart,
                MaKhuyenMaiApDung = appliedPromo
            };

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("api/web/khach-hang/gio-hang/sync", request);

            if (response.IsSuccessStatusCode)
            {
                Cart = await response.Content.ReadFromJsonAsync<GioHangResponseDto>() ?? new();

                // Nếu giỏ hàng bị mất mã do không đủ điều kiện nữa thì xóa Session
                if (string.IsNullOrEmpty(Cart.MaKhuyenMaiApDung) && !string.IsNullOrEmpty(appliedPromo))
                    HttpContext.Session.Remove(PromoKey);

                // Lấy danh sách mã khuyến mãi để hiển thị Modal
                if (Cart.Items.Any())
                {
                    var promoResponse = await client.PostAsJsonAsync("api/web/khach-hang/gio-hang/khuyen-mai", Cart.Items);
                    if (promoResponse.IsSuccessStatusCode)
                        AvailablePromotions = await promoResponse.Content.ReadFromJsonAsync<List<GioHangKhuyenMaiDto>>() ?? new();
                }
            }

            return Page();
        }

        public IActionResult OnPostUpdate(int id, int delta)
        {
            var sessionData = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new();
            var item = sessionData.Find(x => x.IdSanPham == id);
            if (item != null)
            {
                item.SoLuong += delta;
                if (item.SoLuong <= 0) sessionData.Remove(item);
                HttpContext.Session.Set(WebCafebookApi.Services.SessionExtensions.CartKey, sessionData);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int id)
        {
            var sessionData = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new();
            sessionData.RemoveAll(x => x.IdSanPham == id);
            HttpContext.Session.Set(WebCafebookApi.Services.SessionExtensions.CartKey, sessionData);
            return RedirectToPage();
        }

        // HÀM MỚI: Xử lý Áp dụng và Gỡ mã khuyến mãi
        public IActionResult OnPostApplyPromo(string maKhuyenMai)
        {
            HttpContext.Session.SetString(PromoKey, maKhuyenMai);
            return RedirectToPage();
        }

        public IActionResult OnPostRemovePromo()
        {
            HttpContext.Session.Remove(PromoKey);
            return RedirectToPage();
        }
    }
}