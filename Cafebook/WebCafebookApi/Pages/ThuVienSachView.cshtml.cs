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
    public class ThuVienSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector; 

        public SachPhanTrangDto? SachResult { get; set; }
        public string? ErrorMessage { get; set; }

        public List<SelectListItem> TheLoaiList { get; set; } = new();
        public List<SelectListItem> TrangThaiList { get; set; } = new();
        public List<SelectListItem> SortList { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public int? TheLoai { get; set; }
        [BindProperty(SupportsGet = true)] public string? TrangThai { get; set; }
        [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "ten_asc";
        [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;

        public ThuVienSachViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.Sach.Id"); 
        }

        public async Task OnGetAsync()
        {
            // GỌI API BẰNG CẤU HÌNH ĐỘNG
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var filters = await httpClient.GetFromJsonAsync<SachFiltersDto>("api/web/thuvien/filters");
                TheLoaiList.Add(new SelectListItem("Tất cả thể loại", "0"));
                if (filters != null)
                {
                    TheLoaiList.AddRange(filters.TheLoais.Select(f => new SelectListItem(f.Ten, f.Id.ToString())));
                }

                TrangThaiList.Add(new SelectListItem("Tất cả", "all"));
                TrangThaiList.Add(new SelectListItem("Còn sách", "con_sach"));
                TrangThaiList.Add(new SelectListItem("Hết sách", "het_sach"));

                SortList.Add(new SelectListItem("Tên (A-Z)", "ten_asc"));
                SortList.Add(new SelectListItem("Tên (Z-A)", "ten_desc"));
                SortList.Add(new SelectListItem("Tiền cọc (Thấp-Cao)", "gia_asc"));
                SortList.Add(new SelectListItem("Tiền cọc (Cao-Thấp)", "gia_desc"));

                var queryString = $"?search={Search}&theLoaiId={TheLoai ?? 0}&trangThai={TrangThai}&sortBy={SortBy}&pageNum={PageNum}";
                SachResult = await httpClient.GetFromJsonAsync<SachPhanTrangDto>($"api/web/thuvien/search{queryString}");
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối máy chủ Thư viện: {ex.Message}";
            }
        }

        public string EncryptId(int id)
        {
            return _protector.Protect(id.ToString());
        }
    }
}