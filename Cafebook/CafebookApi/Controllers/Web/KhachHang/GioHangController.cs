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
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khach-hang/gio-hang")]
    [ApiController]
    [Authorize(Roles = "KhachHang")] 
    public class GioHangController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public GioHangController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncCart([FromBody] GioHangSyncRequestDto request)
        {
            if (request.Items == null || !request.Items.Any())
                return Ok(new GioHangResponseDto());

            var productIds = request.Items.Select(x => x.IdSanPham).ToList();
            var products = await _context.SanPhams.AsNoTracking()
                .Where(p => productIds.Contains(p.IdSanPham))
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = new GioHangResponseDto
            {
                Items = request.Items.Select(item => {
                    var p = products.FirstOrDefault(x => x.IdSanPham == item.IdSanPham);
                    string anhUrl = "";
                    if (p != null && !string.IsNullOrEmpty(p.HinhAnh))
                    {
                        anhUrl = p.HinhAnh.StartsWith("/") ? $"{baseUrl}{p.HinhAnh}" : $"{baseUrl}{HinhAnhPaths.UrlFoods}/{p.HinhAnh}";
                    }
                    else { anhUrl = $"{baseUrl}{HinhAnhPaths.WebDefaultFoodIcon}"; }

                    return new GioHangItemDto
                    {
                        IdSanPham = item.IdSanPham,
                        TenSanPham = p?.TenSanPham ?? "Sản phẩm không tồn tại",
                        DonGia = p?.GiaBan ?? 0,
                        SoLuong = item.SoLuong,
                        HinhAnhUrl = anhUrl
                    };
                }).Where(x => x.DonGia > 0).ToList()
            };

            if (!string.IsNullOrEmpty(request.MaKhuyenMaiApDung))
            {
                var km = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhuyenMai == request.MaKhuyenMaiApDung);
                if (km != null)
                {
                    var (isEligible, reason, discount) = CheckEligibilityCart(km, result.TongTienHang, result.Items, DateTime.Now);
                    if (isEligible)
                    {
                        result.MaKhuyenMaiApDung = km.MaKhuyenMai;
                        result.TienGiamGia = discount;
                    }
                }
            }

            if (result.Items.Any())
            {
                var phiGiaoHangDb = await _context.Set<PhuThu>().AsNoTracking()
                    .FirstOrDefaultAsync(pt => pt.TenPhuThu == "Phí giao hàng Online");

                result.PhiGiaoHang = phiGiaoHangDb?.GiaTri ?? 0m;
            }

            return Ok(result);
        }

        // Tải danh sách Khuyến mãi cho Popup
        [HttpPost("khuyen-mai")]
        public async Task<IActionResult> GetAvailablePromotions([FromBody] List<GioHangItemDto> currentItems)
        {
            var tongTien = currentItems.Sum(x => x.ThanhTien);
            var now = DateTime.Now;
            var allKms = await _context.KhuyenMais.AsNoTracking().ToListAsync();
            var resultList = new List<GioHangKhuyenMaiDto>();

            foreach (var km in allKms)
            {
                if (!string.Equals(km.TrangThai, "Hoạt động", StringComparison.OrdinalIgnoreCase) || km.NgayBatDau > now || km.NgayKetThuc < now)
                    continue;

                var (isEligible, reason, discountValue) = CheckEligibilityCart(km, tongTien, currentItems, now);

                resultList.Add(new GioHangKhuyenMaiDto
                {
                    MaKhuyenMai = km.MaKhuyenMai,
                    TenChuongTrinh = km.TenChuongTrinh,
                    DieuKienApDung = string.IsNullOrEmpty(km.DieuKienApDung) ? km.MoTa : km.DieuKienApDung,
                    IsEligible = isEligible,
                    IneligibilityReason = reason,
                    CalculatedDiscount = discountValue
                });
            }

            return Ok(resultList.OrderByDescending(k => k.IsEligible).ThenByDescending(k => k.CalculatedDiscount).ToList());
        }

        // Logic check điều kiện (Điều chỉnh lại cho Giỏ hàng thay vì Hóa đơn)
        private (bool IsEligible, string? Reason, decimal CalculatedDiscount) CheckEligibilityCart(KhuyenMai km, decimal tongTien, List<GioHangItemDto> items, DateTime now)
        {
            if (km.SoLuongConLai.HasValue && km.SoLuongConLai <= 0) return (false, "Đã hết lượt sử dụng.", 0);
            if (km.HoaDonToiThieu.HasValue && km.HoaDonToiThieu > 0 && tongTien < km.HoaDonToiThieu.Value) return (false, $"Cần hóa đơn tối thiểu {km.HoaDonToiThieu.Value:N0} đ.", 0);
            if (km.GioBatDau.HasValue && km.GioKetThuc.HasValue && (now.TimeOfDay < km.GioBatDau || now.TimeOfDay > km.GioKetThuc)) return (false, $"Chỉ áp dụng trong khung giờ {km.GioBatDau} - {km.GioKetThuc}.", 0);

            if (!string.IsNullOrWhiteSpace(km.NgayTrongTuan))
            {
                string homNay = now.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Thứ 2",
                    DayOfWeek.Tuesday => "Thứ 3",
                    DayOfWeek.Wednesday => "Thứ 4",
                    DayOfWeek.Thursday => "Thứ 5",
                    DayOfWeek.Friday => "Thứ 6",
                    DayOfWeek.Saturday => "Thứ 7",
                    DayOfWeek.Sunday => "Chủ nhật",
                    _ => ""
                };
                if (!km.NgayTrongTuan.Contains(homNay, StringComparison.OrdinalIgnoreCase)) return (false, $"Chỉ áp dụng vào {km.NgayTrongTuan}.", 0);
            }

            decimal tongTienGocChoKM = tongTien;
            if (km.IdSanPhamApDung.HasValue)
            {
                if (!items.Any(c => c.IdSanPham == km.IdSanPhamApDung.Value))
                {
                    var sp = _context.SanPhams.Find(km.IdSanPhamApDung.Value);
                    return (false, $"Chỉ áp dụng khi mua '{sp?.TenSanPham}'.", 0);
                }
                tongTienGocChoKM = items.Where(c => c.IdSanPham == km.IdSanPhamApDung.Value).Sum(c => c.ThanhTien);
            }

            decimal discount = 0;
            if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
            {
                discount = tongTienGocChoKM * (km.GiaTriGiam / 100);
                if (km.GiamToiDa.HasValue && discount > km.GiamToiDa.Value) discount = km.GiamToiDa.Value;
            }
            else { discount = km.GiaTriGiam; }

            return (true, null, discount);
        }
    }
}