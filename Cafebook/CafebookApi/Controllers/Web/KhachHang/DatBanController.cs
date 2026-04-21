using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/datban")]
    [ApiController]
    public class DatBanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private const int SlotDurationHours = 2;

        private class OpeningHours
        {
            public TimeSpan Open { get; set; } = new TimeSpan(6, 0, 0);
            public TimeSpan Close { get; set; } = new TimeSpan(23, 0, 0);
            public bool IsValid { get; set; } = false;
        }

        public DatBanController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-all-tables-by-area")]
        public async Task<IActionResult> GetAllTablesByArea()
        {
            var khuVucList = await _context.KhuVucs
                .Include(k => k.Bans)
                .Where(k => k.Bans.Any(b => b.TrangThai != "Tạm ngưng" && b.TrangThai != "Bảo trì"))
                .OrderBy(k => k.IdKhuVuc)
                .Select(k => new KhuVucBanDto
                {
                    IdKhuVuc = k.IdKhuVuc,
                    TenKhuVuc = k.TenKhuVuc,
                    BanList = k.Bans
                                .Where(b => b.TrangThai != "Tạm ngưng" && b.TrangThai != "Bảo trì")
                                .OrderBy(b => b.SoBan)
                                .Select(b => new BanTrongDto
                                {
                                    IdBan = b.IdBan,
                                    SoBan = b.SoBan,
                                    SoGhe = b.SoGhe,
                                    KhuVuc = k.TenKhuVuc,
                                    MoTa = b.GhiChu
                                }).ToList()
                })
                .ToListAsync();

            return Ok(khuVucList);
        }

        [HttpGet("get-opening-hours")]
        public async Task<ActionResult<OpeningHoursDto>> GetOpeningHours()
        {
            var hours = await GetAndParseOpeningHours();
            return Ok(new OpeningHoursDto { Open = hours.Open, Close = hours.Close });
        }

        [HttpPost("tim-ban")]
        public async Task<IActionResult> TimBanTrong([FromBody] TimBanRequestDto req)
        {
            DateTime requestedStart = req.NgayDat.Date.Add(req.GioDat);
            DateTime requestedEnd = requestedStart.AddHours(SlotDurationHours);
            var openingHours = await GetAndParseOpeningHours();

            if (requestedStart < DateTime.Now.AddMinutes(10))
                return BadRequest("Vui lòng đặt bàn trước ít nhất 10 phút.");

            if (!IsTimeValid(requestedStart, openingHours))
                return BadRequest($"Giờ đặt ({requestedStart:HH:mm}) nằm ngoài giờ mở cửa ({openingHours.Open:hh\\:mm} - {openingHours.Close:hh\\:mm}).");

            var suitableTables = await _context.Bans
                .Include(b => b.KhuVuc)
                .Where(b => b.SoGhe >= req.SoNguoi && b.TrangThai != "Bảo trì" && b.TrangThai != "Tạm ngưng")
                .ToListAsync();

            var existingBookings = await _context.PhieuDatBans
                .Where(p => p.ThoiGianDat.Date == req.NgayDat.Date &&
                            (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Khách đã đến"))
                .ToListAsync();

            var availableTables = new List<BanTrongDto>();

            foreach (var table in suitableTables)
            {
                bool isBusy = existingBookings.Any(booking =>
                {
                    if (booking.IdBan != table.IdBan) return false;
                    DateTime bookingStart = booking.ThoiGianDat;
                    DateTime bookingEnd = bookingStart.AddHours(SlotDurationHours);
                    return requestedStart < bookingEnd && bookingStart < requestedEnd;
                });

                if (!isBusy)
                {
                    availableTables.Add(new BanTrongDto
                    {
                        IdBan = table.IdBan,
                        SoBan = table.SoBan,
                        SoGhe = table.SoGhe,
                        KhuVuc = table.KhuVuc?.TenKhuVuc ?? "Chung",
                        MoTa = table.GhiChu
                    });
                }
            }

            return Ok(availableTables.OrderBy(t => t.SoGhe).ThenBy(t => t.SoBan));
        }

        [HttpPost("tao-yeu-cau")]
        public async Task<IActionResult> TaoYeuCauDatBan([FromBody] DatBanWebRequestDto req)
        {
            int? idKhachHang = null;
            // ĐÃ FIX: CS0118 và CS8600
            CafebookModel.Model.ModelEntities.KhachHang? khachHang = null;

            if (string.IsNullOrWhiteSpace(req.HoTen) || string.IsNullOrWhiteSpace(req.SoDienThoai) || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Vui lòng cung cấp đủ Họ tên, Số điện thoại và Email liên hệ.");

            if (User.Identity?.IsAuthenticated == true)
            {
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(claimId, out int id))
                {
                    var user = await _context.KhachHangs.FindAsync(id);
                    if (user != null && user.SoDienThoai == req.SoDienThoai && user.Email == req.Email)
                    {
                        khachHang = user;
                    }
                }
            }

            if (khachHang == null)
            {
                var khachTheoSDT = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == req.SoDienThoai);
                var khachTheoEmail = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == req.Email);

                if (khachTheoSDT != null && khachTheoEmail != null)
                {
                    if (khachTheoSDT.IdKhachHang != khachTheoEmail.IdKhachHang)
                        return Conflict("SĐT và Email này thuộc về 2 tài khoản khác nhau. Vui lòng kiểm tra lại.");
                    else
                    {
                        khachHang = khachTheoSDT;
                        khachHang.HoTen = req.HoTen;
                        _context.KhachHangs.Update(khachHang);
                    }
                }
                else if (khachTheoSDT != null)
                {
                    khachHang = khachTheoSDT;
                    khachHang.HoTen = req.HoTen;
                    khachHang.Email = req.Email;
                    _context.KhachHangs.Update(khachHang);
                }
                else if (khachTheoEmail != null)
                {
                    khachHang = khachTheoEmail;
                    khachHang.HoTen = req.HoTen;
                    khachHang.SoDienThoai = req.SoDienThoai;
                    _context.KhachHangs.Update(khachHang);
                }
                else
                {
                    if (await _context.KhachHangs.AnyAsync(k => k.TenDangNhap == req.SoDienThoai))
                        return Conflict("Tên đăng nhập (SĐT) này đã được sử dụng.");

                    // ĐÃ FIX: CS0118
                    khachHang = new CafebookModel.Model.ModelEntities.KhachHang
                    {
                        HoTen = req.HoTen,
                        SoDienThoai = req.SoDienThoai,
                        Email = req.Email,
                        NgayTao = DateTime.Now,
                        DiemTichLuy = 0,
                        BiKhoa = false,
                        TenDangNhap = req.SoDienThoai,
                        MatKhau = Guid.NewGuid().ToString("N")[..8],
                        TaiKhoanTam = true
                    };
                    _context.KhachHangs.Add(khachHang);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                idKhachHang = khachHang.IdKhachHang;
            }
            catch (DbUpdateException ex)
            {
                return Conflict($"Lỗi CSDL khi cập nhật/tạo khách hàng: {ex.InnerException?.Message ?? ex.Message}");
            }

            DateTime requestedStart = req.NgayDat.Date.Add(req.GioDat);
            var openingHours = await GetAndParseOpeningHours();

            if (requestedStart < DateTime.Now.AddMinutes(10))
                return Conflict("Giờ đặt quá gần. Vui lòng chọn thời gian sau 10 phút nữa.");

            if (!IsTimeValid(requestedStart, openingHours))
                return BadRequest($"Giờ đặt ({requestedStart:HH:mm}) nằm ngoài giờ mở cửa ({openingHours.Open:hh\\:mm} - {openingHours.Close:hh\\:mm}).");

            bool isConflict = await _context.PhieuDatBans.AnyAsync(p =>
                p.IdBan == req.IdBan &&
                (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Khách đã đến") &&
                requestedStart < p.ThoiGianDat.AddHours(SlotDurationHours) &&
                p.ThoiGianDat < requestedStart.AddHours(SlotDurationHours)
            );

            if (isConflict) return Conflict("Rất tiếc, bàn này vừa có người đặt vào khung giờ bạn chọn. Vui lòng chọn bàn khác.");

            var phieuMoi = new PhieuDatBan
            {
                IdBan = req.IdBan,
                IdKhachHang = idKhachHang,
                HoTenKhach = req.HoTen,
                SdtKhach = req.SoDienThoai,
                ThoiGianDat = requestedStart,
                SoLuongKhach = req.SoLuongKhach,
                GhiChu = req.GhiChu,
                TrangThai = "Chờ xác nhận"
            };
            _context.PhieuDatBans.Add(phieuMoi);

            var banInfo = await _context.Bans.FindAsync(req.IdBan);
            var thongBao = new ThongBao
            {
                NoiDung = $"Đơn đặt bàn mới từ Web: {req.HoTen} - Bàn {banInfo?.SoBan} lúc {requestedStart:HH:mm dd/MM}",
                LoaiThongBao = "DatBan",
                IdLienQuan = phieuMoi.IdPhieuDatBan,
                ThoiGianTao = DateTime.Now,
                DaXem = false
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Yêu cầu đặt bàn của bạn đã được gửi. Chúng tôi sẽ sớm xác nhận!", idPhieu = phieuMoi.IdPhieuDatBan });
        }

        [HttpGet("get-customer-info")]
        public async Task<IActionResult> GetCustomerInfoByPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return BadRequest();

            var khachInfo = await _context.KhachHangs
                .Where(k => k.SoDienThoai == phone && k.TaiKhoanTam == false)
                .Select(k => new { k.HoTen, k.Email })
                .FirstOrDefaultAsync();

            if (khachInfo == null) return NotFound();

            return Ok(khachInfo);
        }

        private async Task<OpeningHours> GetAndParseOpeningHours()
        {
            var openSetting = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(cd => cd.TenCaiDat == "ThongTin_GioMoCua");
            var closeSetting = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(cd => cd.TenCaiDat == "ThongTin_GioDongCua");

            string openTime = openSetting?.GiaTri ?? "07:00";
            string closeTime = closeSetting?.GiaTri ?? "22:00";

            return ParseOpeningHours($"{openTime} - {closeTime}");
        }

        private OpeningHours ParseOpeningHours(string settingValue)
        {
            var hours = new OpeningHours();
            try
            {
                var match = Regex.Match(settingValue, @"(\d{2}:\d{2})\s*-\s*(\d{2}:\d{2})");
                if (match.Success)
                {
                    if (TimeSpan.TryParse(match.Groups[1].Value, out TimeSpan open)) hours.Open = open;
                    if (TimeSpan.TryParse(match.Groups[2].Value, out TimeSpan close)) hours.Close = close;
                    hours.IsValid = true;
                }
            }
            catch { }
            return hours;
        }

        private bool IsTimeValid(DateTime thoiGianDat, OpeningHours hours)
        {
            var timeOfDay = thoiGianDat.TimeOfDay;
            return timeOfDay >= hours.Open && timeOfDay < hours.Close;
        }
    }
}