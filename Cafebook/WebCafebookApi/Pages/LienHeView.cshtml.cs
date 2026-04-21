using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages
{
    public class LienHeViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LienHeViewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public LienHeDto Info { get; set; } = new LienHeDto();

        [BindProperty]
        public PhanHoiInputModel PhanHoiInput { get; set; } = new PhanHoiInputModel();

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.GetFromJsonAsync<LienHeDto>("api/web/lienhe/info");
                if (response != null)
                {
                    Info = response;
                }
            }
            catch (Exception)
            {
                Info = new LienHeDto();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.PostAsJsonAsync("api/web/lienhe/gui-gop-y", PhanHoiInput);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cảm ơn bạn! Chúng tôi đã nhận được góp ý và sẽ phản hồi sớm nhất.";
                    ModelState.Clear();
                    PhanHoiInput = new PhanHoiInputModel();
                }
                else if ((int)response.StatusCode == 429) 
                {
                    ErrorMessage = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    ErrorMessage = "Có lỗi xảy ra khi gửi góp ý. Vui lòng thử lại sau.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối máy chủ: {ex.Message}";
            }

            await OnGetAsync();
            return Page();
        }
    }
}