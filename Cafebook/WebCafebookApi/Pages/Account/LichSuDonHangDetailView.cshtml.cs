using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Text;
using WebCafebookApi.Services;

namespace WebCafebookApi.Pages.Account
{
    [Authorize(Roles = "KhachHang")]
    public class LichSuDonHangDetailViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;

        public string ApiBaseUrl { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;

        public LichSuDonHangDetailViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.SanPham.Id");
        }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;

        public DonHangChiTietWebDto? OrderDetails { get; set; }

        [TempData] public string? ErrorMessage { get; set; }

        private int DecodeId(string encodedId)
        {
            if (string.IsNullOrWhiteSpace(encodedId)) return 0;
            if (int.TryParse(encodedId, out int rawId)) return rawId;

            try
            {
                var bytes = WebEncoders.Base64UrlDecode(encodedId);
                var str = Encoding.UTF8.GetString(bytes);
                if (int.TryParse(str, out int id)) return id;
                return 0;
            }
            catch { return 0; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            int realIdHoaDon = DecodeId(Id);
            if (realIdHoaDon == 0) { ErrorMessage = "Mã đơn hàng không hợp lệ."; return Page(); }

            JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";

            if (string.IsNullOrEmpty(JwtToken))
            {
                return Redirect($"/dang-nhap?ReturnUrl=/Account/LichSuDonHangDetailView/{Id}");
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{httpClient.BaseAddress}api/web/khachhang/lichsudonhangweb";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);

            try
            {
                OrderDetails = await httpClient.GetFromJsonAsync<DonHangChiTietWebDto>($"{ApiBaseUrl}/detail/{realIdHoaDon}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ErrorMessage = "Bạn không có quyền xem đơn hàng này.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ErrorMessage = "Không tìm thấy thông tin đơn hàng trên hệ thống.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối máy chủ: {ex.Message}";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostMuaLaiAsync(int idHoaDon)
        {
            JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";
            if (string.IsNullOrEmpty(JwtToken)) return RedirectToPage("/Account/DangNhapView");

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{httpClient.BaseAddress}api/web/khachhang/lichsudonhangweb";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);

            try
            {
                var order = await httpClient.GetFromJsonAsync<DonHangChiTietWebDto>($"{ApiBaseUrl}/detail/{idHoaDon}");
                if (order != null && order.Items.Any())
                {
                    // FIX CS0104: Gọi tường minh đường dẫn đến SessionExtensions
                    var cart = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new List<CartSessionItemDto>();

                    foreach (var orderItem in order.Items)
                    {
                        var existing = cart.FirstOrDefault(i => i.IdSanPham == orderItem.IdSanPham);
                        if (existing != null)
                            existing.SoLuong += orderItem.SoLuong;
                        else
                            cart.Add(new CartSessionItemDto { IdSanPham = orderItem.IdSanPham, SoLuong = orderItem.SoLuong });
                    }

                    // FIX CS0104: Gọi tường minh đường dẫn đến SessionExtensions
                    HttpContext.Session.Set(WebCafebookApi.Services.SessionExtensions.CartKey, cart);

                    TempData["CartMessage"] = "Đã thêm các sản phẩm từ đơn hàng cũ vào giỏ!";
                    return RedirectToPage("/GioHangView");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể thêm vào giỏ hàng: {ex.Message}";
                return await OnGetAsync();
            }

            ErrorMessage = "Không thể tìm thấy chi tiết đơn hàng để mua lại.";
            return await OnGetAsync();
        }

        public async Task<bool> KiemTraDaDanhGia(int idHoaDon, int? idSanPham, int? idSach)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var accessToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(accessToken)) return true;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                var response = await httpClient.GetAsync($"api/danhgia/kiemtra?idHoaDon={idHoaDon}&idSanPham={idSanPham}&idSach={idSach}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }
                return true;
            }
            catch (Exception) { return true; }
        }

        public string EncodeId(int id)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(id.ToString());
            return WebEncoders.Base64UrlEncode(plainTextBytes);
        }

        public string EncryptProductId(int id)
        {
            return _protector.Protect(id.ToString());
        }
    }
}