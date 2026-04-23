using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebCafebookApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WebCafebookApi.Pages.Account
{
    [Authorize(Roles = "KhachHang")]
    public class ThanhToanViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ThanhToanViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ThanhToanLoadDto PageData { get; set; } = new();

        [BindProperty]
        public ThanhToanSubmitDto Input { get; set; } = new();

        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? error)
        {
            if (!string.IsNullOrEmpty(error)) ErrorMessage = error;

            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var sessionCart = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new();
            var appliedPromo = HttpContext.Session.GetString("AppliedPromo");

            var cartRequest = new GioHangSyncRequestDto { Items = sessionCart, MaKhuyenMaiApDung = appliedPromo };

            try
            {
                var response = await httpClient.PostAsJsonAsync("api/web/khach-hang/thanh-toan/load", cartRequest);
                if (response.IsSuccessStatusCode)
                {
                    PageData = await response.Content.ReadFromJsonAsync<ThanhToanLoadDto>() ?? new();

                    if (string.IsNullOrEmpty(PageData.CartSummary.MaKhuyenMaiApDung) && !string.IsNullOrEmpty(appliedPromo))
                    {
                        HttpContext.Session.Remove("AppliedPromo");
                    }

                    if (!PageData.IsStoreOpen) ErrorMessage = PageData.StoreMessage;
                    if (PageData.CartSummary.Items.Count == 0) return RedirectToPage("/GioHangView");

                    Input.HoTen = PageData.KhachHang.HoTen;
                    Input.SoDienThoai = PageData.KhachHang.SoDienThoai;
                    Input.Email = PageData.KhachHang.Email;
                    Input.DiaChiGiaoHang = PageData.KhachHang.DiaChi;
                    Input.PhuongThucThanhToan = "COD";
                }
                else
                {
                    ErrorMessage = "Lỗi khi tải dữ liệu thanh toán.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối máy chủ: {ex.Message}";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.ReturnUrl");
            ModelState.Remove("Input.CartData");
            ModelState.Remove("Input.Email");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return await OnGetAsync("Vui lòng điền đầy đủ thông tin: " + string.Join(", ", errors));
            }

            var sessionCart = HttpContext.Session.Get<List<CartSessionItemDto>>(WebCafebookApi.Services.SessionExtensions.CartKey) ?? new();
            var appliedPromo = HttpContext.Session.GetString("AppliedPromo");

            Input.CartData = new GioHangSyncRequestDto { Items = sessionCart, MaKhuyenMaiApDung = appliedPromo };
            Input.ReturnUrl = $"{Request.Scheme}://{Request.Host}/ket-qua-thanh-toan/VNPay-Process";

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/web/khach-hang/thanh-toan/submit", Input);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ThanhToanResponseDto>();
                    HttpContext.Session.Remove(WebCafebookApi.Services.SessionExtensions.CartKey);
                    HttpContext.Session.Remove("AppliedPromo");

                    if (!string.IsNullOrEmpty(result?.PaymentUrl))
                    {
                        return Redirect(result.PaymentUrl.Trim());
                    }

                    var bytes = System.Text.Encoding.UTF8.GetBytes(result?.IdHoaDonMoi.ToString() ?? "0");
                    string encoded = System.Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace("=", "");

                    return RedirectToPage("/Account/ThanhToanThanhCongView", new { code = encoded });
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ThanhToanResponseDto>();
                    return await OnGetAsync(errorResult?.Message ?? "Đã xảy ra lỗi khi đặt hàng.");
                }
            }
            catch (System.Exception ex)
            {
                return await OnGetAsync($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public IActionResult OnPostApplyPromo(string maKhuyenMai)
        {
            HttpContext.Session.SetString("AppliedPromo", maKhuyenMai);
            return RedirectToPage();
        }

        public IActionResult OnPostRemovePromo()
        {
            HttpContext.Session.Remove("AppliedPromo");
            return RedirectToPage();
        }
    }
}