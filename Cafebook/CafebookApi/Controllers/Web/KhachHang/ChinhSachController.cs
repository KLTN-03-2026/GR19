using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/chinhsach")]
    [ApiController]
    [AllowAnonymous]
    public class ChinhSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ChinhSachController(CafebookDbContext context)
        {
            _context = context;
        }

        private decimal GetSettingValue(List<CaiDat> settings, string key, decimal defaultValue)
        {
            var valueString = settings.FirstOrDefault(c => c.TenCaiDat == key)?.GiaTri;
            if (decimal.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        private string GetSettingString(List<CaiDat> settings, string key, string defaultValue = "")
        {
            return settings.FirstOrDefault(c => c.TenCaiDat == key)?.GiaTri ?? defaultValue;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetChinhSachData()
        {
            var keysToFetch = new[] {
                "Sach_PhiThue", "Sach_PhiTraTreMoiNgay","Sach_SoNgayMuonToiDa", "DiemTichLuy_NhanVND", "DiemTichLuy_DoiVND",
                "ThongTin_TenQuan", "ThongTin_GioiThieu", "ThongTin_DiaChi", "ThongTin_SoDienThoai",
                "ThongTin_GioMoCua", "ThongTin_GioDongCua", "ThongTin_ThuMoCua", "LienHe_Email"
            };

            var settings = await _context.CaiDats
                .Where(c => keysToFetch.Contains(c.TenCaiDat))
                .ToListAsync();

            var dto = new ChinhSachDto
            {
                PhiThue = GetSettingValue(settings, "Sach_PhiThue", 15000),
                PhiTraTreMoiNgay = GetSettingValue(settings, "Sach_PhiTraTreMoiNgay", 5000),
                SoNgayMuonToiDa = GetSettingString(settings, "Sach_SoNgayMuonToiDa", "7"),
                DiemNhanVND = GetSettingValue(settings, "DiemTichLuy_NhanVND", 10000),
                DiemDoiVND = GetSettingValue(settings, "DiemTichLuy_DoiVND", 1000),

                TenQuan = GetSettingString(settings, "ThongTin_TenQuan", "Cafebook"),
                GioiThieu = GetSettingString(settings, "ThongTin_GioiThieu", "Ốc đảo tri thức và không gian bình yên."),
                DiaChi = GetSettingString(settings, "ThongTin_DiaChi", ""),
                SoDienThoai = GetSettingString(settings, "ThongTin_SoDienThoai", ""),
                GioMoCua = GetSettingString(settings, "ThongTin_GioMoCua", "07:00"),
                GioDongCua = GetSettingString(settings, "ThongTin_GioDongCua", "22:00"),
                ThuMoCua = GetSettingString(settings, "ThongTin_ThuMoCua", "2,3,4,5,6,7,8"),
                Email = GetSettingString(settings, "LienHe_Email", "")
            };

            return Ok(dto);
        }
    }
}