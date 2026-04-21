using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebCafebookApi.Pages
{
    public class DatBanViewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataProtector _protector;

        public TimeSpan OpeningTime { get; set; } = new(6, 0, 0);
        public TimeSpan ClosingTime { get; set; } = new(23, 0, 0);

        public DatBanViewModel(IHttpClientFactory httpClientFactory, IDataProtectionProvider provider)
        {
            _httpClientFactory = httpClientFactory;
            _protector = provider.CreateProtector("Cafebook.Table.Id");
        }

        [BindProperty(SupportsGet = true)]
        public SearchModel Search { get; set; } = new();

        public BookingInfoModel Booking { get; set; } = new();

        public List<KhuVucBanDto> KhuVucList { get; set; } = new();
        public List<int> AvailableTableIds { get; set; } = new();
        public bool IsSearched { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? SearchSuccessMessage { get; set; }
        public bool IsLoggedInUserMissingEmail { get; set; } = false;

        public class SearchModel
        {
            [Required(ErrorMessage = "Vui lòng chọn ngày")]
            [DataType(DataType.Date)]
            public DateTime Date { get; set; } = DateTime.Today;

            [Required(ErrorMessage = "Vui lòng chọn giờ")]
            public string Time { get; set; } = "09:00";

            // FIX: Đổi thành int? để ModelState bắt đúng câu thông báo lỗi tùy chỉnh
            [Required(ErrorMessage = "Vui lòng nhập số người")]
            [Range(1, 50, ErrorMessage = "Số người từ 1-50")]
            public int? People { get; set; } = 2;
        }

        public class BookingInfoModel
        {
            [Required(ErrorMessage = "Vui lòng chọn bàn hợp lệ.")]
            public string SelectedTableToken { get; set; } = string.Empty;
            public string? SelectedTableNumber { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập SĐT")]
            [Phone(ErrorMessage = "SĐT không hợp lệ")]
            public string Phone { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập Email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            public string? Note { get; set; }
        }

        public async Task OnGetAsync()
        {
            PopulateBookingInfoForUser();
            await LoadOpeningHoursAsync();
            await LoadAllTablesAsync();

            if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(Booking.Email))
                IsLoggedInUserMissingEmail = true;

            if (!Request.Query.ContainsKey("handler"))
            {
                // FIX: Xử lý thông minh khi khách vào lúc khuya
                var now = DateTime.Now;
                // Nếu quán đã đóng cửa hoặc chỉ còn 1 tiếng là đóng -> Chuyển lịch sang NGÀY MAI
                if (now.TimeOfDay >= ClosingTime.Subtract(TimeSpan.FromHours(1)))
                {
                    Search.Date = DateTime.Today.AddDays(1);
                    Search.Time = OpeningTime.ToString(@"hh\:mm");
                }
                else if (Search.Date == DateTime.Today)
                {
                    Search.Time = GetDefaultStartTime(OpeningTime, ClosingTime);
                }
                else
                {
                    Search.Time = OpeningTime.ToString(@"hh\:mm");
                }
            }

            if (Request.Query.ContainsKey("handler") && Request.Query["handler"] == "TimKiem")
                await OnPostTimKiemAsync();
        }

        public async Task<IActionResult> OnPostTimKiemAsync()
        {
            PopulateBookingInfoForUser();
            await LoadOpeningHoursAsync();
            await LoadAllTablesAsync();

            if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(Booking.Email))
                IsLoggedInUserMissingEmail = true;

            if (!ModelState.IsValid)
            {
                IsSearched = false;
                ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu tìm kiếm không hợp lệ.";
                return Page();
            }

            if (!TimeSpan.TryParse(Search.Time, out TimeSpan time))
            {
                ModelState.AddModelError("Search.Time", "Giờ không hợp lệ, vui lòng nhập dạng HH:mm");
                IsSearched = false;
                ErrorMessage = "Giờ không hợp lệ, vui lòng nhập dạng HH:mm";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var req = new TimBanRequestDto { NgayDat = Search.Date, GioDat = time, SoNguoi = Search.People ?? 2 };

            var response = await client.PostAsJsonAsync("api/web/datban/tim-ban", req);
            IsSearched = true;

            if (response.IsSuccessStatusCode)
            {
                var availableTables = await response.Content.ReadFromJsonAsync<List<BanTrongDto>>() ?? new();
                AvailableTableIds = availableTables.Select(b => b.IdBan).ToList();

                if (!AvailableTableIds.Any()) ErrorMessage = "Rất tiếc, không còn bàn trống phù hợp với lựa chọn của bạn.";
                else SearchSuccessMessage = $"Đã tìm thấy {AvailableTableIds.Count} bàn trống phù hợp.";
            }
            else
            {
                ErrorMessage = await response.Content.ReadAsStringAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBookAsync([Bind(Prefix = "Booking")] BookingInfoModel bookingForm)
        {
            Booking = bookingForm;

            if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(Booking.FullName)) PopulateBookingInfoForUser();
            if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(Booking.Email)) IsLoggedInUserMissingEmail = true;

            await LoadOpeningHoursAsync();
            await LoadAllTablesAsync();
            IsSearched = true;

            if (!TimeSpan.TryParse(Search.Time, out TimeSpan time))
            {
                ModelState.AddModelError("Search.Time", "Giờ không hợp lệ.");
                ErrorMessage = "Giờ đặt bàn không hợp lệ.";
                await PopulateAvailableTablesAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Thông tin không hợp lệ.";
                await PopulateAvailableTablesAsync();
                return Page();
            }

            int idBanThucTe;
            try
            {
                idBanThucTe = int.Parse(_protector.Unprotect(Booking.SelectedTableToken));
            }
            catch
            {
                ErrorMessage = "Lỗi bảo mật: Không thể xác minh bàn bạn đã chọn.";
                await PopulateAvailableTablesAsync();
                return Page();
            }

            var req = new DatBanWebRequestDto
            {
                IdBan = idBanThucTe,
                NgayDat = Search.Date,
                GioDat = time,
                SoLuongKhach = Search.People ?? 2,
                HoTen = Booking.FullName,
                SoDienThoai = Booking.Phone,
                Email = Booking.Email,
                GhiChu = Booking.Note
            };

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("api/web/datban/tao-yeu-cau", req);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Yêu cầu đặt bàn thành công! Vui lòng chờ xác nhận.";
                IsSearched = false;
                AvailableTableIds.Clear();
                ModelState.Clear();

                Search = new SearchModel();
                await LoadOpeningHoursAsync();
                Search.Time = (Search.Date == DateTime.Today) ? GetDefaultStartTime(OpeningTime, ClosingTime) : OpeningTime.ToString(@"hh\:mm");

                Booking = new BookingInfoModel();
                PopulateBookingInfoForUser();
                if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(Booking.Email)) IsLoggedInUserMissingEmail = true;
            }
            else
            {
                ErrorMessage = await response.Content.ReadAsStringAsync();
                await PopulateAvailableTablesAsync();
            }

            return Page();
        }

        private async Task PopulateAvailableTablesAsync()
        {
            if (TimeSpan.TryParse(Search.Time, out TimeSpan time))
            {
                var req = new TimBanRequestDto { NgayDat = Search.Date, GioDat = time, SoNguoi = Search.People ?? 2 };
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.PostAsJsonAsync("api/web/datban/tim-ban", req);
                if (response.IsSuccessStatusCode)
                {
                    var tables = await response.Content.ReadFromJsonAsync<List<BanTrongDto>>() ?? new();
                    AvailableTableIds = tables.Select(b => b.IdBan).ToList();
                }
            }
        }

        private void PopulateBookingInfoForUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (string.IsNullOrEmpty(Booking.FullName) && string.IsNullOrEmpty(Booking.Phone))
                {
                    Booking.FullName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity.Name ?? "";
                    Booking.Phone = User.FindFirstValue(ClaimTypes.MobilePhone) ?? "";
                    Booking.Email = User.FindFirstValue(ClaimTypes.Email) ?? "";
                }
            }
        }

        private async Task LoadOpeningHoursAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var hours = await client.GetFromJsonAsync<OpeningHoursDto>("api/web/datban/get-opening-hours");
                if (hours != null)
                {
                    OpeningTime = hours.Open;
                    ClosingTime = hours.Close;
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi tải giờ: " + ex.Message); }
        }

        private async Task LoadAllTablesAsync()
        {
            if (!KhuVucList.Any())
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("ApiClient");
                    KhuVucList = await client.GetFromJsonAsync<List<KhuVucBanDto>>("api/web/datban/get-all-tables-by-area") ?? new();
                }
                catch (Exception ex) { ErrorMessage = "Lỗi khi tải danh sách bàn: " + ex.Message; }
            }
        }

        private string GetDefaultStartTime(TimeSpan open, TimeSpan close)
        {
            var now = DateTime.Now;
            var nowPlus10Min = now.AddMinutes(10);
            int minutesToAdd = (nowPlus10Min.Minute % 30 == 0) ? 0 : (30 - (nowPlus10Min.Minute % 30));
            var defaultTime = nowPlus10Min.AddMinutes(minutesToAdd);

            if (defaultTime.TimeOfDay < open || defaultTime.TimeOfDay >= close) return open.ToString(@"hh\:mm");
            return defaultTime.ToString("HH:mm");
        }

        public string EncryptTableId(int id)
        {
            return _protector.Protect(id.ToString());
        }
    }
}