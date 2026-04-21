// Thay vì using CafebookModel.Model.ModelWeb;
using CafebookModel.Model.ModelWeb.KhachHang; // <-- TRỎ ĐÚNG DTO MỚI TẠO
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages
{
    public class ChiTietSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;

        public ChiTietSachViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.Sach.Id");
        }

        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        public ChiTietSachDto? Sach { get; set; } // <-- DÙNG DTO MỚI
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                ErrorMessage = "Đường dẫn không hợp lệ hoặc thiếu Token.";
                return Page();
            }

            int bookId;
            try
            {
                bookId = int.Parse(_protector.Unprotect(Token));
            }
            catch
            {
                ErrorMessage = "Đường dẫn đã bị can thiệp hoặc hết hạn sử dụng.";
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                // <-- GỌI VÀO API CONTROLLER MỚI (chitietsach)
                Sach = await httpClient.GetFromJsonAsync<ChiTietSachDto>($"api/web/chitietsach/{bookId}");
                if (Sach == null)
                {
                    ErrorMessage = "Không tìm thấy cuốn sách bạn yêu cầu.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối máy chủ Thư viện: {ex.Message}";
            }
            return Page();
        }

        public string EncryptId(int id)
        {
            return _protector.Protect(id.ToString());
        }
    }
}