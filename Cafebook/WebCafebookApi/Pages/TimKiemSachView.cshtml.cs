// Tập tin: WebCafebookApi/Pages/TimKiemSachView.cshtml.cs
// Tập tin: WebCafebookApi/Pages/TimKiemSachView.cshtml.cs
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

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

        public string PageTitle { get; set; } = "Thư Viện Sách";
        public string? PageDescription { get; set; }

        public List<TimKiemSachCardDto> SachList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public TimKiemSachViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;

            // Phải khớp tên Purpose với bên ChiTietSach
            _protectorSach = provider.CreateProtector("Cafebook.Sach.Id");
            _protectorTacGia = provider.CreateProtector("Cafebook.TacGia.Id");
            _protectorTheLoai = provider.CreateProtector("Cafebook.TheLoai.Id");
            _protectorNXB = provider.CreateProtector("Cafebook.NXB.Id");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var sb = new StringBuilder("api/web/timkiemsach?");
            bool hasValidParam = false;

            try
            {
                // Giải mã Token thành ID thật để gửi cho API
                if (!string.IsNullOrEmpty(TokenTacGia))
                {
                    int idTacGia = int.Parse(_protectorTacGia.Unprotect(TokenTacGia));
                    sb.Append($"idTacGia={idTacGia}");
                    hasValidParam = true;
                }
                else if (!string.IsNullOrEmpty(TokenTheLoai))
                {
                    int idTheLoai = int.Parse(_protectorTheLoai.Unprotect(TokenTheLoai));
                    sb.Append($"idTheLoai={idTheLoai}");
                    hasValidParam = true;
                }
                else if (!string.IsNullOrEmpty(TokenNXB))
                {
                    int idNXB = int.Parse(_protectorNXB.Unprotect(TokenNXB));
                    sb.Append($"idNXB={idNXB}");
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
            return _protectorSach.Protect(id.ToString());
        }
    }
}

