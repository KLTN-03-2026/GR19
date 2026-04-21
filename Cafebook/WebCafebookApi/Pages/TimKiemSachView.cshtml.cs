// Tập tin: WebCafebookApi/Pages/TimKiemSachView.cshtml.cs
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages
{
    public class TimKiemSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;

        [BindProperty(SupportsGet = true)] public int? IdTacGia { get; set; }
        [BindProperty(SupportsGet = true)] public int? IdTheLoai { get; set; }
        [BindProperty(SupportsGet = true)] public int? IdNXB { get; set; }

        public string PageTitle { get; set; } = "Thư Viện Sách";
        public string? PageDescription { get; set; }

        public List<TimKiemSachCardDto> SachList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public TimKiemSachViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.Sach.Id");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var sb = new StringBuilder("api/web/timkiemsach?");

            if (IdTacGia.HasValue) sb.Append($"idTacGia={IdTacGia.Value}");
            else if (IdTheLoai.HasValue) sb.Append($"idTheLoai={IdTheLoai.Value}");
            else if (IdNXB.HasValue) sb.Append($"idNXB={IdNXB.Value}");
            else
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
            return _protector.Protect(id.ToString());
        }
    }
}