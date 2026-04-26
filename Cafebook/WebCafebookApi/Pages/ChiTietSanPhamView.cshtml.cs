using CafebookModel.Model.ModelWeb.KhachHang; 
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebCafebookApi.Services;
using MySessionExt = WebCafebookApi.Services.SessionExtensions;

namespace WebCafebookApi.Pages
{
    public class ChiTietSanPhamViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;

        public ChiTietSanPhamViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.SanPham.Id");
        }

        [BindProperty(SupportsGet = true)] public string? Token { get; set; }
        [BindProperty] public int SoLuong { get; set; } = 1;
        [BindProperty] public string? EncryptedId { get; set; }

        public ChiTietSanPhamDto? SanPham { get; set; }
        public string? ErrorMessage { get; set; }
        public List<DanhGiaChiTietDto> DanhSachDanhGia { get; set; } = new();
        public double SaoTrungBinh { get; set; } = 0;
        public int TongSoDanhGia { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token)) { ErrorMessage = "Token không hợp lệ."; return Page(); }

            int productId;
            try
            {
                productId = int.Parse(_protector.Unprotect(Token));
                EncryptedId = Token;
            }
            catch { ErrorMessage = "Đường dẫn không hợp lệ."; return Page(); }

            var client = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                // GỌI ĐÚNG CONTROLLER MỚI TẠO
                SanPham = await client.GetFromJsonAsync<ChiTietSanPhamDto>($"api/web/chitietsanpham/{productId}");
            }
            catch { ErrorMessage = "Không tìm thấy sản phẩm."; return Page(); }

            if (SanPham == null) return Page();

            try
            {
                DanhSachDanhGia = await client.GetFromJsonAsync<List<DanhGiaChiTietDto>>($"api/web/chitietsanpham/{productId}/danhgia") ?? new();
                if (DanhSachDanhGia.Any())
                {
                    SaoTrungBinh = DanhSachDanhGia.Average(d => d.SoSao);
                    TongSoDanhGia = DanhSachDanhGia.Count;
                }
            }
            catch { /* Bỏ qua nếu lỗi đánh giá */ }

            return Page();
        }

        public IActionResult OnPostAddToCart()
        {
            if (User.Identity?.IsAuthenticated != true || !User.IsInRole("KhachHang"))
            {
                return RedirectToPage("/Account/DangNhapView", new { returnUrl = Url.Page("/ChiTietSanPhamView", new { token = EncryptedId }) });
            }

            if (string.IsNullOrEmpty(EncryptedId)) return RedirectToPage("/ThucDonView");

            int idSanPham;
            try { idSanPham = int.Parse(_protector.Unprotect(EncryptedId)); }
            catch { return RedirectToPage("/ThucDonView"); }

            var cart = HttpContext.Session.Get<List<CartItemDto>>(MySessionExt.CartKey) ?? new List<CartItemDto>();

            var existingItem = cart.FirstOrDefault(i => i.IdSanPham == idSanPham);

            if (existingItem != null)
            {
                existingItem.SoLuong += this.SoLuong;
                TempData["CartMessage"] = $"Đã cập nhật số lượng.";
            }
            else
            {
                cart.Add(new CartItemDto { IdSanPham = idSanPham, SoLuong = this.SoLuong });
                TempData["CartMessage"] = "Đã thêm sản phẩm vào giỏ!";
            }

            HttpContext.Session.Set(MySessionExt.CartKey, cart);
            return RedirectToPage("/ChiTietSanPhamView", new { Token = EncryptedId });
        }

        public string EncryptId(int id) => _protector.Protect(id.ToString());
    }
}