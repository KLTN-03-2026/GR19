using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachHang;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khach-hang/thanh-toan")]
    [ApiController]
    [Authorize]
    [Authorize(Roles = "KhachHang")]
    public class ThanhToanController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ThanhToanController(CafebookDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            int.TryParse(idClaim?.Value, out int id);
            return id;
        }

        private async Task<(bool isOpen, string message)> CheckStoreWorkingHours()
        {
            var settings = await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            var gioMoStr = settings.GetValueOrDefault("ThongTin_GioMoCua", "07:00");
            var gioDongStr = settings.GetValueOrDefault("ThongTin_GioDongCua", "22:00");
            var thuMoCuaStr = settings.GetValueOrDefault("ThongTin_ThuMoCua", "2,3,4,5,6,7,8") ?? "";

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            int currentDay = now.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)now.DayOfWeek + 1;
            if (!thuMoCuaStr.Contains(currentDay.ToString()))
                return (false, "Hôm nay cửa hàng tạm nghỉ, vui lòng quay lại vào ngày khác.");

            if (TimeSpan.TryParse(gioMoStr, out TimeSpan gioMo) && TimeSpan.TryParse(gioDongStr, out TimeSpan gioDong))
            {
                if (currentTime < gioMo || currentTime > gioDong)
                    return (false, $"Cửa hàng hiện đang đóng cửa. Giờ hoạt động từ {gioMoStr} đến {gioDongStr}.");
            }

            return (true, "");
        }

        [HttpPost("load")]
        public async Task<IActionResult> LoadCheckoutData([FromBody] GioHangSyncRequestDto cartRequest)
        {
            var idKhachHang = GetCurrentUserId();
            if (idKhachHang == 0) return Unauthorized();

            var khachHang = await _context.KhachHangs.FindAsync(idKhachHang);
            if (khachHang == null) return NotFound("Không tìm thấy khách hàng.");

            var storeStatus = await CheckStoreWorkingHours();
            var settings = await _context.CaiDats.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            decimal.TryParse(settings.GetValueOrDefault("DiemTichLuy_DoiVND", "1000"), out var tiLeDoiDiem);

            var gioHangCtrl = new GioHangController(_context);
            gioHangCtrl.ControllerContext = new ControllerContext { HttpContext = this.HttpContext };

            var cartDataResult = await gioHangCtrl.SyncCart(cartRequest ?? new GioHangSyncRequestDto());
            var cartResult = (cartDataResult as OkObjectResult)?.Value as GioHangResponseDto;

            var availablePromotions = new List<GioHangKhuyenMaiDto>();
            if (cartResult != null && cartResult.Items.Any())
            {
                var promoResponse = await gioHangCtrl.GetAvailablePromotions(cartResult.Items) as OkObjectResult;
                if (promoResponse?.Value is List<GioHangKhuyenMaiDto> promos)
                {
                    availablePromotions = promos;
                }
            }

            var dto = new ThanhToanLoadDto
            {
                IsStoreOpen = storeStatus.isOpen,
                StoreMessage = storeStatus.message,
                KhachHang = new KhachHangThanhToanDto
                {
                    IdKhachHang = khachHang.IdKhachHang,
                    HoTen = khachHang.HoTen,
                    SoDienThoai = khachHang.SoDienThoai ?? "",
                    Email = khachHang.Email ?? "",
                    DiaChi = khachHang.DiaChi ?? "",
                    DiemTichLuy = khachHang.DiemTichLuy
                },
                CartSummary = cartResult ?? new GioHangResponseDto(),
                TiLeDoiDiemVND = tiLeDoiDiem,
                AvailablePromotions = availablePromotions
            };

            return Ok(dto);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitOrder([FromBody] ThanhToanSubmitDto dto)
        {
            var storeStatus = await CheckStoreWorkingHours();
            if (!storeStatus.isOpen)
                return BadRequest(new ThanhToanResponseDto { Success = false, Message = storeStatus.message });

            var idKhachHang = GetCurrentUserId();
            if (idKhachHang == 0) return Unauthorized();

            var khachHang = await _context.KhachHangs.FindAsync(idKhachHang);
            if (khachHang == null) return NotFound("Không tìm thấy khách hàng.");

            var gioHangCtrl = new GioHangController(_context);
            gioHangCtrl.ControllerContext = new ControllerContext { HttpContext = this.HttpContext };

            var cartDataResult = await gioHangCtrl.SyncCart(dto.CartData ?? new GioHangSyncRequestDto());
            var cartData = (cartDataResult as OkObjectResult)?.Value as GioHangResponseDto;

            if (cartData == null || !cartData.Items.Any())
                return BadRequest(new ThanhToanResponseDto { Success = false, Message = "Giỏ hàng trống." });

            var settings = await _context.CaiDats.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            decimal.TryParse(settings.GetValueOrDefault("DiemTichLuy_DoiVND", "1000"), out var tiLeDoiDiem);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal giamGiaDiem = 0;
                if (dto.DiemSuDung > 0)
                {
                    if (khachHang.DiemTichLuy < dto.DiemSuDung)
                        return BadRequest(new ThanhToanResponseDto { Success = false, Message = "Điểm tích lũy không đủ." });

                    giamGiaDiem = dto.DiemSuDung * tiLeDoiDiem;
                    decimal maxAllowed = (cartData.TongTienHang - cartData.TienGiamGia) * 0.5m;
                    if (giamGiaDiem > maxAllowed) giamGiaDiem = maxAllowed;

                    int diemBiTru = (int)Math.Ceiling(giamGiaDiem / tiLeDoiDiem);
                    khachHang.DiemTichLuy -= diemBiTru;
                }

                bool isVNPay = dto.PhuongThucThanhToan == "VNPAY";

                var hoaDon = new HoaDon
                {
                    IdKhachHang = idKhachHang,
                    ThoiGianTao = DateTime.Now,
                    TrangThai = "Chờ thanh toán",
                    LoaiHoaDon = "Giao hàng",
                    DiaChiGiaoHang = dto.DiaChiGiaoHang,
                    SoDienThoaiGiaoHang = dto.SoDienThoai,
                    GhiChu = dto.GhiChu,
                    PhuongThucThanhToan = dto.PhuongThucThanhToan,
                    TrangThaiGiaoHang = isVNPay ? "Chờ thanh toán" : "Chờ xác nhận",
                    TongTienGoc = cartData.TongTienHang,
                    GiamGia = cartData.TienGiamGia + giamGiaDiem,
                    TongPhuThu = cartData.PhiGiaoHang
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync();

                foreach (var item in cartData.Items)
                {
                    _context.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        IdHoaDon = hoaDon.IdHoaDon,
                        IdSanPham = item.IdSanPham,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia
                    });
                }

                var giaoDich = new GiaoDichThanhToan
                {
                    IdHoaDon = hoaDon.IdHoaDon,
                    MaGiaoDichNgoai = $"WEB_{hoaDon.IdHoaDon}_{DateTime.Now.Ticks}",
                    CongThanhToan = dto.PhuongThucThanhToan ?? "COD",
                    SoTien = hoaDon.ThanhTien,
                    ThoiGianGiaoDich = DateTime.Now,
                    TrangThai = isVNPay ? "Đang xử lý" : "Thành công"
                };
                _context.GiaoDichThanhToans.Add(giaoDich);

                if (!isVNPay)
                {
                    var thongBao = new ThongBao
                    {
                        NoiDung = $"🔔 CÓ ĐƠN HÀNG MỚI! (Mã đơn: #{hoaDon.IdHoaDon} - Hình thức: Tiền mặt)",
                        ThoiGianTao = DateTime.Now,
                        LoaiThongBao = "DonHangMoi",
                        IdLienQuan = hoaDon.IdHoaDon,
                        DaXem = false
                    };
                    _context.Set<ThongBao>().Add(thongBao);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (isVNPay)
                {
                    string vnp_Returnurl = (dto.ReturnUrl ?? "").Trim();
                    string vnp_Url = (settings.GetValueOrDefault("VNPay_Url", "") ?? "").Trim();
                    string vnp_TmnCode = (settings.GetValueOrDefault("VNPay_TmnCode", "") ?? "").Trim();
                    string vnp_HashSecret = (settings.GetValueOrDefault("VNPay_HashSecret", "") ?? "").Trim();

                    VNPayHelper vnpay = new VNPayHelper();
                    vnpay.AddRequestData("vnp_Version", "2.1.0");
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                    vnpay.AddRequestData("vnp_Amount", ((int)hoaDon.ThanhTien * 100).ToString());
                    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {hoaDon.IdHoaDon}");
                    vnpay.AddRequestData("vnp_OrderType", "other");
                    vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                    vnpay.AddRequestData("vnp_TxnRef", giaoDich.MaGiaoDichNgoai);

                    string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                    return Ok(new ThanhToanResponseDto { Success = true, PaymentUrl = paymentUrl });
                }

                return Ok(new ThanhToanResponseDto { Success = true, Message = "Đặt hàng thành công!", IdHoaDonMoi = hoaDon.IdHoaDon });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ThanhToanResponseDto { Success = false, Message = $"Lỗi máy chủ: {ex.Message}" });
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn()
        {
            var vnpayData = Request.Query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value.ToString())).ToList();

            var settings = await _context.CaiDats.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            string vnp_HashSecret = (settings.GetValueOrDefault("VNPay_HashSecret", "") ?? "").Trim();

            VNPayHelper vnpay = new VNPayHelper();
            string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret, vnpayData);

            if (checkSignature)
            {
                string vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
                string vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();

                var giaoDich = await _context.GiaoDichThanhToans.FirstOrDefaultAsync(g => g.MaGiaoDichNgoai == vnp_TxnRef);
                if (giaoDich != null)
                {
                    var hoaDon = await _context.HoaDons.FindAsync(giaoDich.IdHoaDon);

                    if (vnp_ResponseCode == "00")
                    {
                        giaoDich.TrangThai = "Thành công";

                        if (hoaDon != null)
                        {
                            hoaDon.ThoiGianThanhToan = DateTime.Now;
                            hoaDon.TrangThai = "Đã thanh toán";
                            hoaDon.TrangThaiGiaoHang = "Chờ xác nhận";

                            var thongBao = new ThongBao
                            {
                                NoiDung = $"✅ ĐƠN HÀNG VNPAY ĐÃ THANH TOÁN (Mã đơn: #{hoaDon.IdHoaDon})",
                                ThoiGianTao = DateTime.Now,
                                LoaiThongBao = "DonHangMoi",
                                IdLienQuan = hoaDon.IdHoaDon,
                                DaXem = false
                            };
                            _context.Set<ThongBao>().Add(thongBao);
                        }

                        await _context.SaveChangesAsync();

                        var bytes = System.Text.Encoding.UTF8.GetBytes(hoaDon?.IdHoaDon.ToString() ?? "0");
                        string encoded = System.Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').Replace("=", "");

                        return Ok(new { success = true, encodedId = encoded });
                    }
                    else
                    {
                        giaoDich.TrangThai = vnp_ResponseCode == "24" ? "Đã hủy" : "Thất bại";
                        giaoDich.MaLoi = vnp_ResponseCode;
                        giaoDich.MoTaLoi = GetVNPayErrorDescription(vnp_ResponseCode);

                        if (hoaDon != null)
                        {
                            hoaDon.TrangThai = "Đã hủy";
                            hoaDon.TrangThaiGiaoHang = "Đã hủy";
                        }

                        await _context.SaveChangesAsync();

                        return Ok(new { success = false, message = giaoDich.MoTaLoi });
                    }
                }
                return Ok(new { success = false, message = "Không tìm thấy mã giao dịch trên hệ thống." });
            }
            return Ok(new { success = false, message = "Chữ ký thanh toán không hợp lệ (Sai HashSecret)." });
        }

        private string GetVNPayErrorDescription(string vnp_ResponseCode)
        {
            return vnp_ResponseCode switch
            {
                "24" => "Khách hàng hủy giao dịch thanh toán.",
                "11" => "Thẻ chưa đăng ký dịch vụ thanh toán trực tuyến.",
                "12" => "Thẻ hoặc tài khoản ngân hàng bị khóa.",
                "51" => "Tài khoản không đủ số dư để thực hiện thanh toán.",
                "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Khách hàng nhập sai mật khẩu thanh toán quá số lần quy định.",
                "99" => "Lỗi không xác định từ phía ngân hàng.",
                _ => $"Giao dịch thanh toán thất bại (Mã lỗi: {vnp_ResponseCode})."
            };
        }

        [HttpGet("order-summary/{id}")]
        public async Task<IActionResult> GetOrderSummary(int id)
        {
            var idKhachHang = GetCurrentUserId();
            if (idKhachHang == 0) return Unauthorized();

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.SanPham)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHoaDon == id && h.IdKhachHang == idKhachHang);

            if (hoaDon == null) return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền xem.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var dto = new ThanhToanThanhCongDto
            {
                IdHoaDonMoi = hoaDon.IdHoaDon,
                ThoiGianTao = hoaDon.ThoiGianTao,
                TrangThai = hoaDon.TrangThai,
                TongTienHang = hoaDon.TongTienGoc,
                GiamGia = hoaDon.GiamGia,
                PhiGiaoHang = hoaDon.TongPhuThu,
                ThanhTien = hoaDon.ThanhTien,
                PhuongThucThanhToan = hoaDon.PhuongThucThanhToan ?? "N/A",
                DiaChiGiaoHang = hoaDon.DiaChiGiaoHang ?? "N/A",
                SoDienThoai = hoaDon.SoDienThoaiGiaoHang ?? "N/A",

                Items = hoaDon.ChiTietHoaDons.Select(ct => new GioHangItemDto
                {
                    IdSanPham = ct.IdSanPham,
                    TenSanPham = ct.SanPham?.TenSanPham ?? "N/A",
                    DonGia = ct.DonGia,
                    SoLuong = ct.SoLuong,
                    HinhAnhUrl = ct.SanPham != null && !string.IsNullOrEmpty(ct.SanPham.HinhAnh)
                        ? (ct.SanPham.HinhAnh.StartsWith("/") ? $"{baseUrl}{ct.SanPham.HinhAnh}" : $"{baseUrl}{HinhAnhPaths.UrlFoods}/{ct.SanPham.HinhAnh}")
                        : $"{baseUrl}{HinhAnhPaths.WebDefaultFoodIcon}"
                }).ToList()
            };
            return Ok(dto);
        }
    }
}