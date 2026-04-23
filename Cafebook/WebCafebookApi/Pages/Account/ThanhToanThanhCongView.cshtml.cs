using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages.Account
{
    public class VNPayVerifyResult
    {
        public bool Success { get; set; }
        public string? EncodedId { get; set; }
        public string? Message { get; set; }
    }

    [Authorize(Roles = "KhachHang")]
    public class ThanhToanThanhCongViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ThanhToanThanhCongViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string? Code { get; set; }

        public ThanhToanThanhCongDto? HoaDon { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            if (Code == "VNPay-Process" && Request.Query.ContainsKey("vnp_ResponseCode"))
            {
                try
                {
                    var response = await httpClient.GetAsync($"api/web/khach-hang/thanh-toan/vnpay-return{Request.QueryString.Value}");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<VNPayVerifyResult>();
                        if (result != null && result.Success)
                        {
                            return Redirect($"/ket-qua-thanh-toan/{result.EncodedId}");
                        }
                        else
                        {
                            ErrorMessage = result?.Message ?? "Giao dịch thanh toán thất bại.";
                            return Page();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Lỗi kết nối xác thực VNPay: {ex.Message}";
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(Code))
            {
                ErrorMessage = "Đường dẫn không hợp lệ.";
                return Page();
            }

            int idHoaDon = 0;
            try
            {
                string incoming = Code.Replace('-', '+').Replace('_', '/');
                switch (incoming.Length % 4)
                {
                    case 2: incoming += "=="; break;
                    case 3: incoming += "="; break;
                }
                var bytes = Convert.FromBase64String(incoming);
                idHoaDon = int.Parse(Encoding.UTF8.GetString(bytes));
            }
            catch
            {
                ErrorMessage = "Mã đơn hàng đã bị sai lệch.";
                return Page();
            }

            try
            {
                HoaDon = await httpClient.GetFromJsonAsync<ThanhToanThanhCongDto>($"api/web/khach-hang/thanh-toan/order-summary/{idHoaDon}");
                if (HoaDon == null) ErrorMessage = "Không thể tải thông tin đơn hàng.";
            }
            catch (Exception)
            {
                ErrorMessage = $"Đơn hàng không tồn tại hoặc bạn không có quyền xem.";
            }

            return Page();
        }
    }
}