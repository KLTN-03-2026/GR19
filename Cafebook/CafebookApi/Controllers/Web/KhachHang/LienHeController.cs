using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/lienhe")]
    [ApiController]
    public class LienHeController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IMemoryCache _cache; 

        public LienHeController(CafebookDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private string? GetSettingValue(List<CafebookModel.Model.ModelEntities.CaiDat> settings, string key)
        {
            var value = settings.FirstOrDefault(c => c.TenCaiDat == key)?.GiaTri;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetContactInfo()
        {
            try
            {
                var keysToFetch = new[] {
                    "ThongTin_TenQuan", "ThongTin_GioiThieu", "ThongTin_DiaChi", "ThongTin_SoDienThoai",
                    "ThongTin_GioMoCua", "ThongTin_GioDongCua", "ThongTin_ThuMoCua",
                    "LienHe_Email", "LienHe_Facebook", "LienHe_GoogleMapsEmbed",
                    "LienHe_Instagram", "LienHe_Website", "LienHe_X", "LienHe_Youtube", "LienHe_Zalo"
                };

                var settings = await _context.CaiDats
                    .Where(c => keysToFetch.Contains(c.TenCaiDat))
                    .ToListAsync();

                string thuMoCua = GetSettingValue(settings, "ThongTin_ThuMoCua") ?? "2,3,4,5,6,7,8";
                string formattedThu = thuMoCua == "2,3,4,5,6,7,8" ? "2 - CN" : thuMoCua.Replace("8", "CN");
                string gioMo = GetSettingValue(settings, "ThongTin_GioMoCua") ?? "07:00";
                string gioDong = GetSettingValue(settings, "ThongTin_GioDongCua") ?? "22:00";

                var dto = new LienHeDto
                {
                    TenQuan = GetSettingValue(settings, "ThongTin_TenQuan"),
                    GioiThieu = GetSettingValue(settings, "ThongTin_GioiThieu"),
                    DiaChi = GetSettingValue(settings, "ThongTin_DiaChi"),
                    SoDienThoai = GetSettingValue(settings, "ThongTin_SoDienThoai"),
                    EmailLienHe = GetSettingValue(settings, "LienHe_Email"),
                    GioHoatDong = $"Thứ {formattedThu}: {gioMo} - {gioDong}",
                    LinkFacebook = GetSettingValue(settings, "LienHe_Facebook"),
                    LinkInstagram = GetSettingValue(settings, "LienHe_Instagram"),
                    LinkGoogleMapsEmbed = GetSettingValue(settings, "LienHe_GoogleMapsEmbed"),
                    LinkZalo = GetSettingValue(settings, "LienHe_Zalo"),
                    LinkYoutube = GetSettingValue(settings, "LienHe_Youtube"),
                    LinkWebsite = GetSettingValue(settings, "LienHe_Website"),
                    LinkX = GetSettingValue(settings, "LienHe_X")
                };

                return Ok(dto);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Lỗi CSDL API GetContactInfo: {ex.Message}");
            }
        }

        [HttpPost("gui-gop-y")]
        public async Task<IActionResult> GuiGopY([FromBody] PhanHoiInputModel input)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
            var cacheKey = $"GopY_SpamCheck_{ipAddress}";

            if (_cache.TryGetValue(cacheKey, out _))
            {
                return StatusCode(429, "Bạn thao tác quá nhanh. Vui lòng đợi 1 phút trước khi gửi góp ý tiếp theo.");
            }

            if (string.IsNullOrWhiteSpace(input.Ten) || string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.NoiDung))
            {
                return BadRequest("Vui lòng cung cấp đủ thông tin Tên, Email và Nội dung.");
            }

            var gopY = new CafebookModel.Model.ModelEntities.GopY
            {
                HoTen = input.Ten,
                Email = input.Email,
                NoiDung = input.NoiDung,
                NgayTao = DateTime.Now,
                TrangThai = "Chưa đọc"
            };
            _context.GopYs.Add(gopY);
            await _context.SaveChangesAsync();

            var thongBao = new CafebookModel.Model.ModelEntities.ThongBao
            {
                NoiDung = $"Góp ý mới từ {input.Ten}: {input.NoiDung.Substring(0, Math.Min(input.NoiDung.Length, 30))}...",
                ThoiGianTao = DateTime.Now,
                LoaiThongBao = "GopY",
                IdLienQuan = gopY.IdGopY,
                DaXem = false
            };
            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();

            _cache.Set(cacheKey, true, TimeSpan.FromMinutes(1));

            return Ok(new { success = true, message = "Cảm ơn bạn! Chúng tôi đã nhận được góp ý." });
        }
    }
}