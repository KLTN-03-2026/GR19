using CafebookModel.Model.ModelApp;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;

namespace WebCafebookApi.Pages
{
    public class ThucDonViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;
        public ThucDonDto? MenuResult { get; set; }
        public List<SelectListItem> LoaiSanPhamsList { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public int? LoaiId { get; set; }
        [BindProperty(SupportsGet = true)] public decimal? GiaMin { get; set; }
        [BindProperty(SupportsGet = true)] public decimal? GiaMax { get; set; }
        [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "ten_asc";
        [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;

        public ThucDonViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.SanPham.Id");
        }

        public async Task OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var filters = await httpClient.GetFromJsonAsync<List<ThucDonFilterDto>>("api/web/thucdon/filters");

                LoaiSanPhamsList.Add(new SelectListItem("Tất cả danh mục", "0"));
                if (filters != null)
                {
                    LoaiSanPhamsList.AddRange(filters.Select(f => new SelectListItem(f.Ten, f.Id.ToString())));
                }

                var queryString = $"?loaiId={LoaiId ?? 0}&search={Search}&sortBy={SortBy}&giaMin={GiaMin}&giaMax={GiaMax}&pageNum={PageNum}";
                MenuResult = await httpClient.GetFromJsonAsync<ThucDonDto>($"api/web/thucdon/search{queryString}");
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối API: {ex.Message}";
            }
        }

        public string EncryptId(int id)
        {
            return _protector.Protect(id.ToString());
        }
    }
}