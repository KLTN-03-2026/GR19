using CafebookModel.Model.ModelWeb.KhachHang; 
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CafebookModel.Utils;

namespace WebCafebookApi.Pages
{
    public class ChiTietSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protectorSach;
        private readonly IDataProtector _protectorTacGia;
        private readonly IDataProtector _protectorTheLoai;
        private readonly IDataProtector _protectorNXB;

        public ChiTietSachViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protectorSach = provider.CreateProtector("Cafebook.Sach.Id");
            _protectorTacGia = provider.CreateProtector("Cafebook.TacGia.Id");
            _protectorTheLoai = provider.CreateProtector("Cafebook.TheLoai.Id");
            _protectorNXB = provider.CreateProtector("Cafebook.NXB.Id");
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
                bookId = int.Parse(_protectorSach.UnprotectFromUrlSafe(Token)); // <-- Đổi hàm
            }
            catch
            {
                ErrorMessage = "Đường dẫn đã bị can thiệp hoặc hết hạn sử dụng.";
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
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

        public string EncryptId(int id) => _protectorSach.ProtectToUrlSafe(id.ToString());
        public string EncryptTacGia(int id) => _protectorTacGia.ProtectToUrlSafe(id.ToString());
        public string EncryptTheLoai(int id) => _protectorTheLoai.ProtectToUrlSafe(id.ToString());
        public string EncryptNXB(int id) => _protectorNXB.ProtectToUrlSafe(id.ToString());
    }
}