// Vị trí lưu: WebCafebookApi/Pages/TrangChuView.cshtml.cs
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.DataProtection;

namespace WebCafebookApi.Pages
{
    public class TrangChuViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _sanPhamProtector;
        private readonly IDataProtector _sachProtector;

        public TrangChuViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _sanPhamProtector = provider.CreateProtector("Cafebook.SanPham.Id");
            _sachProtector = provider.CreateProtector("Cafebook.Sach.Id");
        }

        public ThongTinChungDto? Info { get; set; }
        public List<KhuyenMaiDto> Promotions { get; set; } = new();
        public List<SanPhamDto> MonNoiBat { get; set; } = new();
        public List<SachDto> SachNoiBat { get; set; } = new();

        public async Task OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.GetFromJsonAsync<TrangChuDto>("api/web/trangchu/data");
                if (response != null)
                {
                    Info = response.Info;
                    Promotions = response.Promotions;
                    MonNoiBat = response.MonNoiBat;
                    SachNoiBat = response.SachNoiBat;
                }
            }
            catch (Exception)
            {
                Info = new ThongTinChungDto();
            }
        }

        public string EncryptSanPhamId(int id)
        {
            return _sanPhamProtector.Protect(id.ToString());
        }

        public string EncryptSachId(int id)
        {
            return _sachProtector.Protect(id.ToString());
        }
    }
}