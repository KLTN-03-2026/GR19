using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CafebookModel.Utils;

namespace WebCafebookApi.Pages
{
    public class TimKiemSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IDataProtector _protectorSach;
        private readonly IDataProtector _protectorTacGia;
        private readonly IDataProtector _protectorTheLoai;
        private readonly IDataProtector _protectorNXB;

        [BindProperty(SupportsGet = true)] public string? TokenTacGia { get; set; }
        [BindProperty(SupportsGet = true)] public string? TokenTheLoai { get; set; }
        [BindProperty(SupportsGet = true)] public string? TokenNXB { get; set; }
        [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int CurrentPage { get; set; } = 1;

        public string PageTitle { get; set; } = "Thư Viện Sách";
        public string? PageDescription { get; set; }

        public List<TimKiemSachCardDto> SachList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public TimKiemSachViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;

            _protectorSach = provider.CreateProtector("Cafebook.Sach.Id");
            _protectorTacGia = provider.CreateProtector("Cafebook.TacGia.Id");
            _protectorTheLoai = provider.CreateProtector("Cafebook.TheLoai.Id");
            _protectorNXB = provider.CreateProtector("Cafebook.NXB.Id");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var sb = new StringBuilder($"api/web/timkiemsach?pageNum={PageNum}");
            bool hasValidParam = false;

            try
            {
                if (!string.IsNullOrEmpty(TokenTacGia))
                {
                    // Xóa dòng Replace
                    int idTacGia = int.Parse(_protectorTacGia.UnprotectFromUrlSafe(TokenTacGia)); // <-- Đổi hàm
                    sb.Append($"&idTacGia={idTacGia}");
                    hasValidParam = true;
                }
                else if (!string.IsNullOrEmpty(TokenTheLoai))
                {
                    // Xóa dòng Replace
                    int idTheLoai = int.Parse(_protectorTheLoai.UnprotectFromUrlSafe(TokenTheLoai)); // <-- Đổi hàm
                    sb.Append($"&idTheLoai={idTheLoai}");
                    hasValidParam = true;
                }
                else if (!string.IsNullOrEmpty(TokenNXB))
                {
                    // Xóa dòng Replace
                    int idNXB = int.Parse(_protectorNXB.UnprotectFromUrlSafe(TokenNXB)); // <-- Đổi hàm
                    sb.Append($"&idNXB={idNXB}");
                    hasValidParam = true;
                }
            }
            catch
            {
                ErrorMessage = "Đường dẫn không hợp lệ hoặc đã bị can thiệp.";
                return Page();
            }

            if (!hasValidParam)
            {
                return RedirectToPage("/ThuVienSachView");
            }

            try
            {
                var result = await httpClient.GetFromJsonAsync<TimKiemSachResultDto>(sb.ToString());
                if (result != null)
                {
                    PageTitle = result.TieuDeTrang;
                    PageDescription = result.MoTaTrang;
                    SachList = result.SachList;
                    TotalPages = result.TotalPages;
                    CurrentPage = result.CurrentPage;
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
            return Page();
        }

        public string EncryptId(int id)
        {
            return _protectorSach.ProtectToUrlSafe(id.ToString()); // <-- Đổi hàm
        }
    }
}

