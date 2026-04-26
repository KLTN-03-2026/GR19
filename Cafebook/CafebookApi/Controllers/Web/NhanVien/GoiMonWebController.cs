using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class GoiMonWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public GoiMonWebController(CafebookDbContext context)
        {
            _context = context;
        }

        // Quy tắc 3: Lấy ID Nhân viên từ Token
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(claim, out int id);
            return id;
        }

        // Quy tắc 2: Tự động nhận diện Domain sinh link ảnh
        private string GetFullImageUrl(string? relativePath)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            if (string.IsNullOrEmpty(relativePath)) return $"{baseUrl}/anhmacdinh/foods/default-food-icon.png";
            return $"{baseUrl}{relativePath.Replace('\\', '/')}";
        }

        [HttpGet("load/{idHoaDon}")]
        public async Task<IActionResult> LoadGoiMonData(int idHoaDon)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
            if (hoaDon == null) return NotFound(new { message = "Không tìm thấy hóa đơn." });

            var currentPromo = await _context.HoaDonKhuyenMais.FirstOrDefaultAsync(hk => hk.IdHoaDon == idHoaDon);

            var chiTietItems = await _context.ChiTietHoaDons.Include(c => c.SanPham)
                .Where(c => c.IdHoaDon == idHoaDon)
                .Select(c => new ChiTietWebDto
                {
                    IdChiTietHoaDon = c.IdChiTietHoaDon,
                    IdSanPham = c.IdSanPham,
                    // FIX CS8601: Thêm ?? string.Empty để chống Null
                    TenSanPham = c.SanPham.TenSanPham ?? "Chưa có tên",
                    SoLuong = c.SoLuong,
                    DonGia = c.DonGia,
                    ThanhTien = c.ThanhTien,
                    GhiChu = c.GhiChu
                }).ToListAsync();

            var sanPhams = await _context.SanPhams.Where(s => s.TrangThaiKinhDoanh == true)
                .Select(s => new SanPhamWebDto
                {
                    IdSanPham = s.IdSanPham,
                    // FIX CS8601
                    TenSanPham = s.TenSanPham ?? "Chưa có tên",
                    DonGia = s.GiaBan,
                    HinhAnh = s.HinhAnh ?? "",
                    IdDanhMuc = s.IdDanhMuc
                }).ToListAsync();

            foreach (var sp in sanPhams) sp.HinhAnh = GetFullImageUrl(sp.HinhAnh);

            var danhMucs = await _context.DanhMucs.OrderBy(d => d.TenDanhMuc)
                .Select(d => new DanhMucWebDto
                {
                    IdDanhMuc = d.IdDanhMuc,
                    // FIX CS8601
                    TenLoaiSP = d.TenDanhMuc ?? "Khác"
                }).ToListAsync();

            var now = DateTime.Now;
            var khuyenMais = await _context.KhuyenMais
                .Where(k => k.TrangThai == "Hoạt động" && k.NgayBatDau <= now && k.NgayKetThuc >= now)
                .Select(k => new KhuyenMaiWebDto
                {
                    IdKhuyenMai = k.IdKhuyenMai,
                    // FIX CS8601
                    TenKhuyenMai = k.TenChuongTrinh ?? "Khuyến mãi"
                }).ToListAsync();
            khuyenMais.Insert(0, new KhuyenMaiWebDto { IdKhuyenMai = 0, TenKhuyenMai = "-- Không áp dụng --" });

            return Ok(new GoiMonViewWebDto
            {
                HoaDonInfo = new HoaDonInfoWebDto
                {
                    IdHoaDon = hoaDon.IdHoaDon,
                    // FIX CS8601: Bọc thêm lớp dự phòng cuối cùng
                    SoBan = hoaDon.Ban?.SoBan ?? hoaDon.LoaiHoaDon ?? "Tại quán",
                    LoaiHoaDon = hoaDon.LoaiHoaDon ?? "Tại quán",
                    TongTienGoc = hoaDon.TongTienGoc,
                    GiamGia = hoaDon.GiamGia,
                    ThanhTien = hoaDon.ThanhTien,
                    IdKhuyenMai = currentPromo?.IdKhuyenMai
                },
                ChiTietItems = chiTietItems,
                SanPhams = sanPhams,
                DanhMucs = danhMucs,
                KhuyenMais = khuyenMais
            });
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromBody] AddItemWebRequest req)
        {
            var hoaDon = await _context.HoaDons.FindAsync(req.IdHoaDon);
            if (hoaDon == null) return NotFound(new { message = "Hóa đơn không tồn tại." });
            if (hoaDon.TrangThai == "Đã thanh toán") return Conflict(new { message = "Hóa đơn đã thanh toán." });

            var sanPham = await _context.SanPhams.FindAsync(req.IdSanPham);
            if (sanPham == null) return NotFound(new { message = "Sản phẩm không tồn tại." });

            var existingItemDb = await _context.ChiTietHoaDons.FirstOrDefaultAsync(c =>
                c.IdHoaDon == req.IdHoaDon && c.IdSanPham == req.IdSanPham && c.GhiChu == req.GhiChu);

            ChiTietWebDto resultDto;
            if (existingItemDb != null)
            {
                existingItemDb.SoLuong += req.SoLuong;
                await _context.SaveChangesAsync();
                resultDto = MapToChiTietWebDto(existingItemDb, sanPham);
            }
            else
            {
                var newItem = new ChiTietHoaDon { IdHoaDon = req.IdHoaDon, IdSanPham = req.IdSanPham, SoLuong = req.SoLuong, DonGia = sanPham.GiaBan, GhiChu = req.GhiChu };
                _context.ChiTietHoaDons.Add(newItem);
                await _context.SaveChangesAsync();
                resultDto = MapToChiTietWebDto(newItem, sanPham);
            }

            await UpdateHoaDonTotals(hoaDon);
            var updatedHoaDonInfo = await GetHoaDonInfo(req.IdHoaDon);
            return Ok(new { updatedHoaDonInfo, newItem = resultDto });
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateSoLuongWebRequest req)
        {
            var item = await _context.ChiTietHoaDons.Include(c => c.HoaDon).FirstOrDefaultAsync(c => c.IdChiTietHoaDon == req.IdChiTietHoaDon);
            if (item == null) return NotFound(new { message = "Không tìm thấy món." });
            if (item.HoaDon.TrangThai == "Đã thanh toán") return Conflict(new { message = "Hóa đơn đã thanh toán." });

            if (req.SoLuongMoi <= 0) _context.ChiTietHoaDons.Remove(item);
            else item.SoLuong = req.SoLuongMoi;

            await _context.SaveChangesAsync();
            await UpdateHoaDonTotals(item.HoaDon);
            return Ok(await GetHoaDonInfo(item.HoaDon.IdHoaDon));
        }

        [HttpDelete("delete-item/{idChiTiet}")]
        public async Task<IActionResult> DeleteItem(int idChiTiet)
        {
            var item = await _context.ChiTietHoaDons.Include(c => c.HoaDon).FirstOrDefaultAsync(c => c.IdChiTietHoaDon == idChiTiet);
            if (item == null) return NotFound(new { message = "Không tìm thấy món." });

            int idHD = item.IdHoaDon;
            _context.ChiTietHoaDons.Remove(item);
            await _context.SaveChangesAsync();

            var hd = await _context.HoaDons.FindAsync(idHD);
            await UpdateHoaDonTotals(hd!);
            return Ok(await GetHoaDonInfo(idHD));
        }

        [HttpPost("send-to-kitchen/{idHoaDon}")]
        public async Task<IActionResult> SendToKitchen(int idHoaDon)
        {
            int idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
            if (hoaDon == null) return NotFound(new { message = "Không tìm thấy hóa đơn" });

            var chiTietItems = await _context.ChiTietHoaDons.Include(c => c.SanPham).Where(c => c.IdHoaDon == idHoaDon).ToListAsync();

            // FIX CS8601
            string soBan = hoaDon.Ban?.SoBan ?? hoaDon.LoaiHoaDon ?? "Tại quán";

            int itemsAdded = 0;
            foreach (var item in chiTietItems)
            {
                if (!await _context.TrangThaiCheBiens.AnyAsync(cb => cb.IdChiTietHoaDon == item.IdChiTietHoaDon))
                {
                    _context.TrangThaiCheBiens.Add(new TrangThaiCheBien
                    {
                        IdChiTietHoaDon = item.IdChiTietHoaDon,
                        IdHoaDon = item.IdHoaDon,
                        IdSanPham = item.IdSanPham,
                        // FIX CS8601
                        TenMon = item.SanPham.TenSanPham ?? "Chưa có tên",
                        SoBan = soBan,
                        SoLuong = item.SoLuong,
                        GhiChu = item.GhiChu,
                        NhomIn = item.SanPham.NhomIn,
                        TrangThai = "Chờ làm",
                        ThoiGianGoi = DateTime.Now
                    });
                    itemsAdded++;
                }
            }
            if (itemsAdded > 0)
            {
                _context.ThongBaos.Add(new ThongBao
                {
                    IdNhanVienTao = idNhanVien,
                    NoiDung = $"Phiếu món mới cho [{soBan}].",
                    ThoiGianTao = DateTime.Now,
                    LoaiThongBao = "PhieuGoiMon",
                    IdLienQuan = idHoaDon,
                    DaXem = false
                });
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = $"Đã gửi {itemsAdded} món mới vào bếp." });
        }

        // --- Helper Methods (Quy tắc 8) ---
        private ChiTietWebDto MapToChiTietWebDto(ChiTietHoaDon c, SanPham s) => new()
        {
            IdChiTietHoaDon = c.IdChiTietHoaDon,
            IdSanPham = c.IdSanPham,
            // FIX CS8601
            TenSanPham = s.TenSanPham ?? "Chưa có tên",
            SoLuong = c.SoLuong,
            DonGia = c.DonGia,
            ThanhTien = c.SoLuong * c.DonGia,
            GhiChu = c.GhiChu
        };

        private async Task UpdateHoaDonTotals(HoaDon hoaDon)
        {
            if (hoaDon != null)
            {
                var tongGocMoi = await _context.ChiTietHoaDons.Where(c => c.IdHoaDon == hoaDon.IdHoaDon).SumAsync(c => c.ThanhTien);
                hoaDon.TongTienGoc = tongGocMoi;
            }
        }

        private async Task<HoaDonInfoWebDto> GetHoaDonInfo(int idHoaDon)
        {
            var hd = await _context.HoaDons.Include(h => h.Ban).AsNoTracking().FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
            var km = await _context.HoaDonKhuyenMais.AsNoTracking().FirstOrDefaultAsync(hk => hk.IdHoaDon == idHoaDon);
            return new HoaDonInfoWebDto
            {
                IdHoaDon = hd!.IdHoaDon,
                // FIX CS8601
                SoBan = hd.Ban?.SoBan ?? hd.LoaiHoaDon ?? "Tại quán",
                LoaiHoaDon = hd.LoaiHoaDon ?? "Tại quán",
                TongTienGoc = hd.TongTienGoc,
                GiamGia = hd.GiamGia,
                ThanhTien = hd.ThanhTien,
                IdKhuyenMai = km?.IdKhuyenMai
            };
        }

        [HttpGet("khuyenmai-available/{idHoaDon}")]
        public async Task<IActionResult> GetAvailableKhuyenMai(int idHoaDon)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

                if (hoaDon == null) return NotFound(new { message = "Không tìm thấy hóa đơn." });

                var now = DateTime.Now;
                var allKms = await _context.KhuyenMais.AsNoTracking().ToListAsync();
                var resultList = new List<KhuyenMaiHienThiGoiMonWebDto>();

                foreach (var km in allKms)
                {
                    var (isEligible, reason, discountValue) = CheckEligibility(km, hoaDon, now);

                    resultList.Add(new KhuyenMaiHienThiGoiMonWebDto
                    {
                        IdKhuyenMai = km.IdKhuyenMai,
                        // FIX CS8601
                        MaKhuyenMai = km.MaKhuyenMai ?? "NO_CODE",
                        TenChuongTrinh = km.TenChuongTrinh ?? "Khuyến mãi",
                        DieuKienApDung = string.IsNullOrEmpty(km.DieuKienApDung) ? (km.MoTa ?? "Không có mô tả") : km.DieuKienApDung,
                        LoaiGiamGia = km.LoaiGiamGia ?? "TienMat",
                        GiaTriGiam = km.GiaTriGiam,
                        GiamToiDa = km.GiamToiDa,
                        IsEligible = isEligible,
                        IneligibilityReason = reason,
                        CalculatedDiscount = discountValue
                    });
                }

                return Ok(resultList
                    .OrderByDescending(k => k.IsEligible)
                    .ThenByDescending(k => k.CalculatedDiscount)
                    .ThenBy(k => k.TenChuongTrinh));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("apply-promotion")]
        public async Task<IActionResult> ApplyPromotion([FromBody] ApplyPromotionWebRequest req)
        {
            try
            {
                var hoaDon = await _context.HoaDons.FindAsync(req.IdHoaDon);
                if (hoaDon == null) return NotFound(new { message = "Hóa đơn không tồn tại." });

                var existingPromos = _context.HoaDonKhuyenMais.Where(hk => hk.IdHoaDon == req.IdHoaDon);
                _context.HoaDonKhuyenMais.RemoveRange(existingPromos);

                if (req.IdKhuyenMai == null || req.IdKhuyenMai == 0)
                {
                    hoaDon.GiamGia = 0; 
                }
                else
                {
                    var km = await _context.KhuyenMais.FindAsync(req.IdKhuyenMai);
                    if (km == null) return NotFound(new { message = "Khuyến mãi không tồn tại." });

                    _context.HoaDonKhuyenMais.Add(new HoaDon_KhuyenMai
                    {
                        IdHoaDon = req.IdHoaDon,
                        IdKhuyenMai = km.IdKhuyenMai
                    });

                    if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal tongTienGocChoKM = hoaDon.TongTienGoc;

                        if (km.IdSanPhamApDung.HasValue)
                        {
                            tongTienGocChoKM = await _context.ChiTietHoaDons
                                .Where(c => c.IdHoaDon == req.IdHoaDon && c.IdSanPham == km.IdSanPhamApDung)
                                .SumAsync(c => c.ThanhTien);
                        }

                        decimal giamGia = tongTienGocChoKM * (km.GiaTriGiam / 100);
                        if (km.GiamToiDa.HasValue && giamGia > km.GiamToiDa.Value)
                        {
                            giamGia = km.GiamToiDa.Value;
                        }
                        hoaDon.GiamGia = giamGia;
                    }
                    else
                    {
                        hoaDon.GiamGia = km.GiaTriGiam; 
                    }
                }

                await _context.SaveChangesAsync();
                var updatedHoaDonInfo = await GetHoaDonInfo(req.IdHoaDon);
                return Ok(updatedHoaDonInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        private (bool IsEligible, string? Reason, decimal CalculatedDiscount) CheckEligibility(KhuyenMai km, HoaDon hoaDon, DateTime now)
        {
            decimal calculatedDiscount = 0m;

            if (!string.Equals(km.TrangThai, "Hoạt động", StringComparison.OrdinalIgnoreCase)) return (false, "Khuyến mãi đã bị tạm ngưng.", 0);
            if (km.NgayBatDau > now) return (false, $"Chưa tới ngày (Bắt đầu từ {km.NgayBatDau:dd/MM}).", 0);
            if (km.NgayKetThuc < now) return (false, $"Đã hết hạn (Kết thúc lúc {km.NgayKetThuc:dd/MM}).", 0);
            if (km.SoLuongConLai.HasValue && km.SoLuongConLai <= 0) return (false, "Đã hết lượt sử dụng.", 0);
            if (km.HoaDonToiThieu.HasValue && km.HoaDonToiThieu > 0 && hoaDon.TongTienGoc < km.HoaDonToiThieu.Value) return (false, $"Cần hóa đơn tối thiểu {km.HoaDonToiThieu.Value:N0} đ.", 0);
            if (km.GioBatDau.HasValue && km.GioKetThuc.HasValue && (now.TimeOfDay < km.GioBatDau || now.TimeOfDay > km.GioKetThuc)) return (false, $"Chỉ áp dụng trong khung giờ {km.GioBatDau:hh\\:mm} - {km.GioKetThuc:hh\\:mm}.", 0);

            if (!string.IsNullOrWhiteSpace(km.NgayTrongTuan))
            {
                string homNay = now.DayOfWeek switch { DayOfWeek.Monday => "2", DayOfWeek.Tuesday => "3", DayOfWeek.Wednesday => "4", DayOfWeek.Thursday => "5", DayOfWeek.Friday => "6", DayOfWeek.Saturday => "7", _ => "8" };
                var ngayHopLe = km.NgayTrongTuan.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim());
                if (!ngayHopLe.Contains(homNay)) return (false, "Không áp dụng vào ngày hôm nay.", 0);
            }

            decimal tongTienGocChoKM = hoaDon.TongTienGoc;
            if (km.IdSanPhamApDung.HasValue)
            {
                if (hoaDon.ChiTietHoaDons == null || !hoaDon.ChiTietHoaDons.Any(c => c.IdSanPham == km.IdSanPhamApDung.Value)) return (false, "Hóa đơn không có sản phẩm được áp dụng KM này.", 0);
                tongTienGocChoKM = hoaDon.ChiTietHoaDons.Where(c => c.IdSanPham == km.IdSanPhamApDung.Value).Sum(c => c.ThanhTien);
            }

            if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
            {
                calculatedDiscount = tongTienGocChoKM * (km.GiaTriGiam / 100);
                if (km.GiamToiDa.HasValue && calculatedDiscount > km.GiamToiDa.Value) calculatedDiscount = km.GiamToiDa.Value;
            }
            else { calculatedDiscount = km.GiaTriGiam; }

            return (true, null, calculatedDiscount);
        }
    }
}