using Microsoft.AspNetCore.Http;
using System.IO;
using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/[controller]")]
    [ApiController]
    [Authorize(Roles = "KhachHang")]
    public class LichSuDonHangWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public LichSuDonHangWebController(CafebookDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<bool> CheckStoreWorkingHoursAsync()
        {
            var settings = await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            var gioMoStr = settings.GetValueOrDefault("ThongTin_GioMoCua", "07:00");
            var gioDongStr = settings.GetValueOrDefault("ThongTin_GioDongCua", "22:00");
            var thuMoCuaStr = settings.GetValueOrDefault("ThongTin_ThuMoCua", "2,3,4,5,6,7,8") ?? "";

            var now = DateTime.Now;
            int currentDay = now.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)now.DayOfWeek + 1;

            if (!thuMoCuaStr.Contains(currentDay.ToString())) return false;

            if (TimeSpan.TryParse(gioMoStr, out TimeSpan gioMo) && TimeSpan.TryParse(gioDongStr, out TimeSpan gioDong))
            {
                if (now.TimeOfDay < gioMo || now.TimeOfDay > gioDong) return false;
            }
            return true;
        }

        private string? GetFullImageUrl(string? relativeUrl, string type = "food")
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return type == "food" ? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{CafebookModel.Utils.HinhAnhPaths.WebDefaultFoodIcon}" : null;

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            string path = relativeUrl.TrimStart('/').Replace('\\', '/');

            if (!path.StartsWith("images/"))
            {
                if (type == "delivery") path = $"{CafebookModel.Utils.HinhAnhPaths.Urldelivery.TrimStart('/')}/{path}";
                else path = $"{CafebookModel.Utils.HinhAnhPaths.UrlFoods.TrimStart('/')}/{path}";
            }
            return $"{baseUrl}/{path}";
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory()
        {
            int userId = GetCurrentUserId();
            bool isStoreOpen = await CheckStoreWorkingHoursAsync();

            var orders = await _context.HoaDons
                .Where(h => h.IdKhachHang == userId && h.LoaiHoaDon == "Giao hàng")
                .OrderByDescending(h => h.ThoiGianTao)
                .Select(h => new LichSuDonHangWebDto
                {
                    IdHoaDon = h.IdHoaDon,
                    MaDonHang = $"#{h.IdHoaDon}",
                    ThoiGianTao = h.ThoiGianTao,
                    ThanhTien = h.ThanhTien,
                    TrangThaiGiaoHang = h.TrangThaiGiaoHang ?? "Chờ xác nhận",
                    TrangThaiThanhToan = h.TrangThai,
                    TenSanPham = h.ChiTietHoaDons.FirstOrDefault() != null ? h.ChiTietHoaDons.FirstOrDefault()!.SanPham.TenSanPham : "Sản phẩm",
                    SoLuongSanPhamKhac = h.ChiTietHoaDons.Count > 1 ? h.ChiTietHoaDons.Count - 1 : 0,
                    HinhAnhUrl = h.ChiTietHoaDons.FirstOrDefault() != null ? h.ChiTietHoaDons.FirstOrDefault()!.SanPham.HinhAnh : null,
                    IsStoreOpen = isStoreOpen
                }).ToListAsync();

            foreach (var order in orders) order.HinhAnhUrl = GetFullImageUrl(order.HinhAnhUrl);
            return Ok(orders);
        }

        [HttpPut("cancel/{idHoaDon}")]
        public async Task<IActionResult> CancelOrder(int idHoaDon, [FromBody] HuyDonHangRequestDto req)
        {
            int userId = GetCurrentUserId();
            var hoaDon = await _context.HoaDons.FindAsync(idHoaDon);

            if (hoaDon == null || hoaDon.IdKhachHang != userId) return Forbid();

            var allowCancel = new[] { "Chờ xác nhận" };
            if (!allowCancel.Contains(hoaDon.TrangThaiGiaoHang) && hoaDon.TrangThai != "Chờ thanh toán")
                return BadRequest(new { message = "Đơn hàng đã được quán chuẩn bị, không thể hủy vào lúc này." });

            hoaDon.TrangThai = "Đã hủy";
            hoaDon.TrangThaiGiaoHang = "Đã hủy";
            hoaDon.GhiChu = $"{hoaDon.GhiChu} | [Khách hủy]: {req.LyDoHuy}".Trim(' ', '|');

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hủy đơn hàng thành công." });
        }

        [HttpPost("repay/{idHoaDon}")]
        public async Task<IActionResult> RepayOrder(int idHoaDon, [FromQuery] string returnUrl)
        {
            if (!await CheckStoreWorkingHoursAsync())
                return BadRequest(new { message = "Cửa hàng hiện đang đóng cửa. Vui lòng thanh toán vào khung giờ hoạt động." });

            int userId = GetCurrentUserId();
            var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon && h.IdKhachHang == userId);

            if (hoaDon == null || hoaDon.TrangThai != "Chờ thanh toán")
                return BadRequest(new { message = "Đơn hàng không khả dụng để thanh toán lại." });

            var settings = await _context.CaiDats.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            var giaoDich = new CafebookModel.Model.ModelEntities.GiaoDichThanhToan
            {
                IdHoaDon = hoaDon.IdHoaDon,
                MaGiaoDichNgoai = $"REPAY_{hoaDon.IdHoaDon}_{DateTime.Now.Ticks}",
                CongThanhToan = "VNPAY",
                SoTien = hoaDon.ThanhTien,
                ThoiGianGiaoDich = DateTime.Now,
                TrangThai = "Đang xử lý"
            };
            _context.GiaoDichThanhToans.Add(giaoDich);
            await _context.SaveChangesAsync();

            VNPayHelper vnpay = new VNPayHelper();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", settings.GetValueOrDefault("VNPay_TmnCode", ""));
            vnpay.AddRequestData("vnp_Amount", ((int)hoaDon.ThanhTien * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan lai don hang {hoaDon.IdHoaDon}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", giaoDich.MaGiaoDichNgoai);

            string paymentUrl = vnpay.CreateRequestUrl(settings.GetValueOrDefault("VNPay_Url", ""), settings.GetValueOrDefault("VNPay_HashSecret", ""));

            return Ok(new { paymentUrl });
        }

        [HttpGet("detail/{idHoaDon}")]
        public async Task<IActionResult> GetOrderDetail(int idHoaDon)
        {
            int userId = GetCurrentUserId();
            bool isStoreOpen = await CheckStoreWorkingHoursAsync();

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons).ThenInclude(c => c.SanPham)
                .Include(h => h.KhachHang)
                .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

            if (hoaDon == null) return NotFound();
            if (hoaDon.IdKhachHang != userId) return Forbid();

            var trackingEvents = new List<TrackingEventDto>();

            if (hoaDon.TrangThai == "Đã hủy" || hoaDon.TrangThaiGiaoHang == "Đã hủy")
            {
                trackingEvents.Add(new TrackingEventDto { Timestamp = hoaDon.ThoiGianTao, Status = "Đặt hàng", Description = "Đã nhận đơn", IsCurrent = false });
                trackingEvents.Add(new TrackingEventDto { Timestamp = DateTime.Now, Status = "Đã hủy", Description = "Đơn bị hủy", IsCurrent = true });
            }
            else
            {
                trackingEvents.Add(new TrackingEventDto { Timestamp = hoaDon.ThoiGianTao, Status = "Đã đặt hàng", Description = "Chờ xác nhận", IsCurrent = hoaDon.TrangThaiGiaoHang == "Chờ xác nhận" });

                if (hoaDon.TrangThaiGiaoHang == "Chờ lấy hàng" || hoaDon.TrangThaiGiaoHang == "Đang giao" || hoaDon.TrangThaiGiaoHang == "Hoàn thành")
                    trackingEvents.Add(new TrackingEventDto { Timestamp = hoaDon.ThoiGianTao.AddMinutes(5), Status = "Đã xác nhận", Description = "Đang chuẩn bị", IsCurrent = hoaDon.TrangThaiGiaoHang == "Chờ lấy hàng" });

                if (hoaDon.TrangThaiGiaoHang == "Đang giao" || hoaDon.TrangThaiGiaoHang == "Hoàn thành")
                    trackingEvents.Add(new TrackingEventDto { Timestamp = hoaDon.ThoiGianTao.AddMinutes(15), Status = "Đang giao", Description = "Shipper đang lấy hàng", IsCurrent = hoaDon.TrangThaiGiaoHang == "Đang giao" });

                if (hoaDon.TrangThaiGiaoHang == "Hoàn thành")
                    trackingEvents.Add(new TrackingEventDto { Timestamp = hoaDon.ThoiGianThanhToan ?? DateTime.Now, Status = "Hoàn thành", Description = "Đã giao thành công", IsCurrent = true });
            }

            var dto = new DonHangChiTietWebDto
            {
                IdHoaDon = hoaDon.IdHoaDon,
                MaDonHang = $"#{hoaDon.IdHoaDon}",
                TrangThaiGiaoHang = hoaDon.TrangThaiGiaoHang ?? "Chờ xác nhận",
                TrangThaiThanhToan = hoaDon.TrangThai,
                PhuongThucThanhToan = hoaDon.PhuongThucThanhToan ?? "COD",
                ThoiGianTao = hoaDon.ThoiGianTao,
                HoTen = hoaDon.KhachHang?.HoTen ?? "Khách hàng",
                SoDienThoai = hoaDon.SoDienThoaiGiaoHang ?? hoaDon.KhachHang?.SoDienThoai ?? "N/A",
                DiaChiGiaoHang = hoaDon.DiaChiGiaoHang ?? hoaDon.KhachHang?.DiaChi ?? "N/A",
                TrackingEvents = trackingEvents.OrderBy(t => t.Timestamp).ToList(),
                Items = hoaDon.ChiTietHoaDons.Select(ct => new DonHangItemWebDto
                {
                    IdSanPham = ct.IdSanPham,
                    TenSanPham = ct.SanPham.TenSanPham,
                    HinhAnhUrl = GetFullImageUrl(ct.SanPham.HinhAnh),
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien,
                    DaDanhGia = _context.DanhGias.Any(d => d.idHoaDon == hoaDon.IdHoaDon && d.idSanPham == ct.IdSanPham)
                }).ToList(),
                TongTienHang = hoaDon.TongTienGoc,
                GiamGia = hoaDon.GiamGia,
                PhiGiaoHang = hoaDon.TongPhuThu,
                ThanhTien = hoaDon.ThanhTien,
                AnhXacNhanGiaoHangUrl = GetFullImageUrl(hoaDon.AnhGiaoHang, "delivery"),
                IsStoreOpen = isStoreOpen
            };

            return Ok(dto);
        }

        [HttpPost("danh-gia")]
        public async Task<IActionResult> SubmitReview([FromForm] int idHoaDon, [FromForm] int idSanPham, [FromForm] int soSao, [FromForm] string? binhLuan, IFormFile? hinhAnh)
        {
            int userId = GetCurrentUserId();
            var hoaDon = await _context.HoaDons.FindAsync(idHoaDon);

            if (hoaDon == null || hoaDon.IdKhachHang != userId) return Forbid();
            if (hoaDon.TrangThaiGiaoHang != "Hoàn thành")
                return BadRequest(new { message = "Chỉ được đánh giá đơn hàng đã hoàn thành." });

            bool exists = await _context.DanhGias.AnyAsync(d => d.idHoaDon == idHoaDon && d.idSanPham == idSanPham);
            if (exists) return BadRequest(new { message = "Sản phẩm này đã được bạn đánh giá." });

            string? imagePath = null;
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "danhgia");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                var fileName = $"dg_{userId}_{DateTime.Now.Ticks}{Path.GetExtension(hinhAnh.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(stream);
                }
                imagePath = $"images/danhgia/{fileName}";
            }

            var danhGia = new CafebookModel.Model.ModelEntities.DanhGia
            {
                idKhachHang = userId,
                idHoaDon = idHoaDon,
                idSanPham = idSanPham,
                SoSao = soSao,
                BinhLuan = binhLuan,
                HinhAnhURL = imagePath,
                NgayTao = DateTime.Now,
                TrangThai = "Hiển thị"
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cảm ơn bạn đã đánh giá sản phẩm!" });
        }
    }
}