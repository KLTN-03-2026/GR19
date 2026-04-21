using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages
{
    public class ChinhSachViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChinhSachViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ChinhSachDto ChinhSach { get; set; } = new ChinhSachDto();

        public async Task OnGetAsync()
        {
            // Sử dụng ApiClient cấu hình sẵn thay vì hardcode localhost
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.GetFromJsonAsync<ChinhSachDto>("api/web/khachhang/chinhsach/data");
                if (response != null)
                {
                    ChinhSach = response;
                }
            }
            catch (Exception)
            {
                // Giữ nguyên DTO mặc định để View không bị lỗi Null Reference
            }
        }
    }
}