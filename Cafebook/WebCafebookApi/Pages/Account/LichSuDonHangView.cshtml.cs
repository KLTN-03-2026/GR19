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
    public class LichSuDonHangViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;

        public List<LichSuDonHangWebDto> AllOrders { get; set; } = new();
        public List<LichSuDonHangWebDto> FilteredOrders { get; set; } = new();

        public string ApiBaseUrl { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;

        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)] public string StatusFilter { get; set; } = "Chờ xác nhận";
        [BindProperty(SupportsGet = true)] public string SearchQuery { get; set; } = string.Empty;

        public string[] StatusTabs { get; set; } = { "Chờ xác nhận", "Chờ thanh toán", "Chờ lấy hàng", "Đang giao", "Hoàn thành", "Đã Hủy", "Tất cả" };

        public LichSuDonHangViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.SanPham.Id");
        }

        public string EncryptProductId(int id) => _protector.Protect(id.ToString());

        public string EncodeId(int id)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(id.ToString());
            return WebEncoders.Base64UrlEncode(plainTextBytes);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            JwtToken = HttpContext.Session.GetString("JwtToken") ?? "";

            if (string.IsNullOrEmpty(JwtToken))
            {
                return Redirect("/dang-nhap?ReturnUrl=/Account/LichSuDonHangView");
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            ApiBaseUrl = $"{httpClient.BaseAddress}api/web/khachhang/lichsudonhangweb";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);

            try
            {
                var response = await httpClient.GetFromJsonAsync<List<LichSuDonHangWebDto>>($"{ApiBaseUrl}/history");
                if (response != null) AllOrders = response;
                FilterOrders();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi tải lịch sử đơn hàng: {ex.Message}";
            }
            return Page();
        }

        private void FilterOrders()
        {
            // Lọc theo Tab trạng thái
            if (StatusFilter == "Tất cả") FilteredOrders = AllOrders;
            else if (StatusFilter == "Đã Hủy") FilteredOrders = AllOrders.Where(o => o.TrangThaiThanhToan == "Đã hủy" || o.TrangThaiGiaoHang == "Đã hủy").ToList();
            else if (StatusFilter == "Chờ thanh toán") FilteredOrders = AllOrders.Where(o => o.TrangThaiThanhToan == "Chờ thanh toán" && o.TrangThaiThanhToan != "Đã hủy").ToList();
            else FilteredOrders = AllOrders.Where(o => o.TrangThaiGiaoHang == StatusFilter && o.TrangThaiThanhToan != "Đã hủy").ToList();

            // Lọc theo Từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                FilteredOrders = FilteredOrders.Where(o =>
                    o.MaDonHang.ToLower().Contains(q) ||
                    o.TenSanPham.ToLower().Contains(q)
                ).ToList();
            }
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
                    // Tránh lỗi CS0104 bằng việc gọi tường minh đường dẫn tuyệt đối
                    var cart = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new List<CartSessionItemDto>();

                    foreach (var item in order.Items)
                    {
                        var existing = cart.FirstOrDefault(i => i.IdSanPham == item.IdSanPham);
                        if (existing != null)
                            existing.SoLuong += item.SoLuong;
                        else
                            cart.Add(new CartSessionItemDto { IdSanPham = item.IdSanPham, SoLuong = item.SoLuong });
                    }

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
    }
}