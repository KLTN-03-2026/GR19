// Tập tin: CafebookApi/Controllers/App/NhanVien/GoiMonController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class GoiMonController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly string _baseUrl;

        public GoiMonController(CafebookDbContext context, IConfiguration config)
        {
            _context = context;
            _baseUrl = config.GetValue<string>("Kestrel:Endpoints:Http:Url") ?? "http://127.0.0.1:5202";
        }

        private string GetFullImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return $"{_baseUrl}/images/default-food-icon.png";
            return $"{_baseUrl}{relativePath.Replace(Path.DirectorySeparatorChar, '/')}";
        }

        // ========================================================
        // HÀM KIỂM TRA ĐỒNG BỘ BẾP (Quyết định mở khóa Thanh toán)
        // ========================================================
        private async Task<bool> CheckChoPhepThanhToan(int idHoaDon)
        {
            // Lấy danh sách món trong Hóa Đơn hiện tại (kèm theo Số lượng và Ghi chú)
            var chiTiets = await _context.ChiTietHoaDons.AsNoTracking()
                .Where(c => c.IdHoaDon == idHoaDon)
                .Select(c => new { c.IdChiTietHoaDon, c.SoLuong, c.GhiChu }).ToListAsync();

            if (!chiTiets.Any()) return false; // Không có món nào thì chắc chắn không cho thanh toán

            // Lấy danh sách món đang nằm dưới Bếp
            var cheBiens = await _context.TrangThaiCheBiens.AsNoTracking()
                .Where(cb => cb.IdHoaDon == idHoaDon)
                .Select(cb => new { cb.IdChiTietHoaDon, cb.SoLuong, cb.GhiChu }).ToListAsync();

            // Nếu số món khác nhau (Vd: Bạn vừa Xóa 1 món ở màn hình Gọi Món nhưng Bếp chưa biết) -> Khóa
            if (chiTiets.Count != cheBiens.Count) return false;

            // Kiểm tra từng món một
            foreach (var ct in chiTiets)
            {
                var cb = cheBiens.FirstOrDefault(x => x.IdChiTietHoaDon == ct.IdChiTietHoaDon);
                // Khóa thanh toán nếu: Món chưa gửi bếp, Sai số lượng, Sai ghi chú
                if (cb == null || cb.SoLuong != ct.SoLuong || cb.GhiChu != ct.GhiChu) return false;
            }

            return true; // Tất cả đã đồng bộ hoàn hảo 100%
        }

        [HttpGet("load/{idHoaDon}")]
        public async Task<IActionResult> LoadGoiMonData(int idHoaDon)
        {
            try
            {
                var hoaDon = await _context.HoaDons.Include(h => h.Ban).AsNoTracking().FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
                if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn.");

                var currentPromotion = await _context.HoaDonKhuyenMais.AsNoTracking().FirstOrDefaultAsync(hk => hk.IdHoaDon == idHoaDon);

                // GỌI HÀM KIỂM TRA ĐỂ LẤY TRẠNG THÁI MỞ KHÓA
                bool choPhepThanhToan = await CheckChoPhepThanhToan(idHoaDon);

                var hoaDonInfo = new HoaDonInfoDto
                {
                    IdHoaDon = hoaDon.IdHoaDon,
                    SoBan = hoaDon.Ban?.SoBan ?? hoaDon.LoaiHoaDon,
                    LoaiHoaDon = hoaDon.LoaiHoaDon,
                    TongTienGoc = hoaDon.TongTienGoc,
                    GiamGia = hoaDon.GiamGia,
                    ThanhTien = hoaDon.ThanhTien,
                    IdKhuyenMai = currentPromotion?.IdKhuyenMai,

                    // SỬA LỖI CHÍNH Ở ĐÂY: Gán cờ mở khóa để gửi về WPF
                    ChoPhepThanhToan = choPhepThanhToan
                };

                var chiTietItems = await _context.ChiTietHoaDons
                    .Where(c => c.IdHoaDon == idHoaDon)
                    .Select(c => new ChiTietDto
                    {
                        IdChiTietHoaDon = c.IdChiTietHoaDon,
                        IdSanPham = c.IdSanPham,
                        TenSanPham = c.SanPham.TenSanPham,
                        SoLuong = c.SoLuong,
                        DonGia = c.DonGia,
                        ThanhTien = c.ThanhTien,
                        GhiChu = c.GhiChu
                    }).ToListAsync();

                var rawSanPhams = await _context.SanPhams.Where(s => s.TrangThaiKinhDoanh == true).AsNoTracking()
                    .Select(s => new { s.IdSanPham, s.TenSanPham, s.GiaBan, s.HinhAnh, s.IdDanhMuc }).ToListAsync();

                var sanPhams = rawSanPhams.Select(s => new SanPhamDto
                {
                    IdSanPham = s.IdSanPham,
                    TenSanPham = s.TenSanPham,
                    DonGia = s.GiaBan,
                    HinhAnh = GetFullImageUrl(s.HinhAnh),
                    IdDanhMuc = s.IdDanhMuc
                }).ToList();

                var danhMucs = await _context.DanhMucs.OrderBy(d => d.TenDanhMuc).AsNoTracking()
                    .Select(d => new DanhMucDto { IdDanhMuc = d.IdDanhMuc, TenLoaiSP = d.TenDanhMuc }).ToListAsync();

                // Lấy danh sách rỗng cho khuyến mãi để tối ưu băng thông lúc load (WPF tự gọi riêng sau)
                var dto = new GoiMonViewDto { HoaDonInfo = hoaDonInfo, ChiTietItems = chiTietItems, SanPhams = sanPhams, DanhMucs = danhMucs, KhuyenMais = new List<KhuyenMaiDto>() };
                return Ok(dto);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        private async Task UpdateHoaDonTotals(HoaDon hoaDon)
        {
            if (hoaDon != null)
            {
                var tongGocMoi = await _context.ChiTietHoaDons.Where(c => c.IdHoaDon == hoaDon.IdHoaDon).SumAsync(c => c.ThanhTien);
                hoaDon.TongTienGoc = tongGocMoi;
            }
        }

        private async Task ReApplyPromotion(HoaDon hoaDon)
        {
            var currentPromoLink = await _context.HoaDonKhuyenMais.FirstOrDefaultAsync(hk => hk.IdHoaDon == hoaDon.IdHoaDon);
            if (currentPromoLink == null) { hoaDon.GiamGia = 0; return; }
            var km = await _context.KhuyenMais.FindAsync(currentPromoLink.IdKhuyenMai);
            if (km == null || (km.HoaDonToiThieu.HasValue && hoaDon.TongTienGoc < km.HoaDonToiThieu.Value))
            { hoaDon.GiamGia = 0; _context.HoaDonKhuyenMais.Remove(currentPromoLink); return; }

            decimal tongTienGocChoKM = hoaDon.TongTienGoc;
            if (km.IdSanPhamApDung.HasValue)
                tongTienGocChoKM = await _context.ChiTietHoaDons.Where(c => c.IdHoaDon == hoaDon.IdHoaDon && c.IdSanPham == km.IdSanPhamApDung).SumAsync(c => c.ThanhTien);

            if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
            {
                decimal giamGia = tongTienGocChoKM * (km.GiaTriGiam / 100);
                if (km.GiamToiDa.HasValue && giamGia > km.GiamToiDa.Value) giamGia = km.GiamToiDa.Value;
                hoaDon.GiamGia = giamGia;
            }
            else { hoaDon.GiamGia = km.GiaTriGiam; }
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromBody] AddItemRequest req)
        {
            try
            {
                var hoaDon = await _context.HoaDons.FindAsync(req.IdHoaDon);
                if (hoaDon == null) return NotFound("Hóa đơn không tồn tại.");
                if (hoaDon.TrangThai == "Đã thanh toán") return Conflict("Hóa đơn đã thanh toán.");
                var sanPham = await _context.SanPhams.FindAsync(req.IdSanPham);
                if (sanPham == null) return NotFound("Sản phẩm không tồn tại.");

                var dinhLuongList = await _context.DinhLuongs.Include(d => d.NguyenLieu).Include(d => d.DonViSuDung).Where(d => d.IdSanPham == req.IdSanPham).ToListAsync();
                if (dinhLuongList.Any())
                {
                    var existingItemQty = (await _context.ChiTietHoaDons.FirstOrDefaultAsync(c => c.IdHoaDon == req.IdHoaDon && c.IdSanPham == req.IdSanPham && c.GhiChu == req.GhiChu))?.SoLuong ?? 0;

                    foreach (var dl in dinhLuongList)
                    {
                        // ========================================================
                        // LOGIC KIỂM TRA KHO CHUẨN XÁC (Đồng bộ với hàm TruKho)
                        // ========================================================
                        decimal heSoQuyDoi = (dl.DonViSuDung != null && dl.DonViSuDung.GiaTriQuyDoi > 0) ? dl.DonViSuDung.GiaTriQuyDoi : 1m;
                        decimal luongCanDungMotSP = 0;

                        if (dl.DonViSuDung != null && dl.DonViSuDung.LaDonViCoBan)
                        {
                            // Nếu là đơn vị cơ bản -> Lấy thẳng số lượng
                            luongCanDungMotSP = dl.SoLuongSuDung;
                        }
                        else
                        {
                            // Nếu là đơn vị quy đổi -> Phải CHIA cho hệ số
                            luongCanDungMotSP = dl.SoLuongSuDung / heSoQuyDoi;
                        }

                        decimal luongCanDungTong = luongCanDungMotSP * (existingItemQty + req.SoLuong);

                        if (dl.NguyenLieu.TonKho < luongCanDungTong)
                            return Conflict($"Hết hàng: '{dl.NguyenLieu.TenNguyenLieu}'. Không đủ nguyên liệu để thêm.");
                    }
                }

                ChiTietDto resultDto;
                var existingItemDb = await _context.ChiTietHoaDons.FirstOrDefaultAsync(c => c.IdHoaDon == req.IdHoaDon && c.IdSanPham == req.IdSanPham && c.GhiChu == req.GhiChu);
                if (existingItemDb != null)
                {
                    existingItemDb.SoLuong += req.SoLuong;
                    await _context.SaveChangesAsync();
                    resultDto = new ChiTietDto { IdChiTietHoaDon = existingItemDb.IdChiTietHoaDon, IdSanPham = existingItemDb.IdSanPham, TenSanPham = sanPham.TenSanPham, SoLuong = existingItemDb.SoLuong, DonGia = existingItemDb.DonGia, ThanhTien = existingItemDb.SoLuong * existingItemDb.DonGia, GhiChu = existingItemDb.GhiChu };
                }
                else
                {
                    var newItem = new ChiTietHoaDon { IdHoaDon = req.IdHoaDon, IdSanPham = req.IdSanPham, SoLuong = req.SoLuong, DonGia = sanPham.GiaBan, GhiChu = req.GhiChu };
                    _context.ChiTietHoaDons.Add(newItem);
                    await _context.SaveChangesAsync();
                    resultDto = new ChiTietDto { IdChiTietHoaDon = newItem.IdChiTietHoaDon, IdSanPham = newItem.IdSanPham, TenSanPham = sanPham.TenSanPham, SoLuong = newItem.SoLuong, DonGia = newItem.DonGia, ThanhTien = newItem.SoLuong * newItem.DonGia, GhiChu = newItem.GhiChu };
                }

                await UpdateHoaDonTotals(hoaDon); await ReApplyPromotion(hoaDon); await _context.SaveChangesAsync();
                var updatedHoaDonInfo = await GetHoaDonInfo(req.IdHoaDon);
                return Ok(new { updatedHoaDonInfo, newItem = resultDto });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateSoLuongRequest req)
        {
            try
            {
                var item = await _context.ChiTietHoaDons.Include(c => c.HoaDon).FirstOrDefaultAsync(c => c.IdChiTietHoaDon == req.IdChiTietHoaDon);
                if (item == null) return NotFound("Không tìm thấy món.");
                var hoaDon = item.HoaDon;
                if (hoaDon.TrangThai == "Đã thanh toán") return Conflict("Hóa đơn đã thanh toán.");

                // Chỉ kiểm tra kho khi người dùng TĂNG số lượng món
                if (req.SoLuongMoi > item.SoLuong)
                {
                    var dinhLuongList = await _context.DinhLuongs.Include(d => d.NguyenLieu).Include(d => d.DonViSuDung).Where(d => d.IdSanPham == item.IdSanPham).ToListAsync();
                    foreach (var dl in dinhLuongList)
                    {
                        // ========================================================
                        // LOGIC KIỂM TRA KHO CHUẨN XÁC
                        // ========================================================
                        decimal heSoQuyDoi = (dl.DonViSuDung != null && dl.DonViSuDung.GiaTriQuyDoi > 0) ? dl.DonViSuDung.GiaTriQuyDoi : 1m;
                        decimal luongCanDungMotSP = 0;

                        if (dl.DonViSuDung != null && dl.DonViSuDung.LaDonViCoBan)
                        {
                            luongCanDungMotSP = dl.SoLuongSuDung;
                        }
                        else
                        {
                            luongCanDungMotSP = dl.SoLuongSuDung / heSoQuyDoi;
                        }

                        decimal luongCanDungTong = luongCanDungMotSP * req.SoLuongMoi; // Check trên tổng SL mới

                        if (dl.NguyenLieu.TonKho < luongCanDungTong)
                            return Conflict($"Hết hàng: '{dl.NguyenLieu.TenNguyenLieu}'. Không thể tăng số lượng.");
                    }
                }

                if (req.SoLuongMoi <= 0) _context.ChiTietHoaDons.Remove(item);
                else item.SoLuong = req.SoLuongMoi;

                await _context.SaveChangesAsync();
                await UpdateHoaDonTotals(hoaDon); await ReApplyPromotion(hoaDon); await _context.SaveChangesAsync();
                var updatedHoaDonInfo = await GetHoaDonInfo(hoaDon.IdHoaDon);
                return Ok(updatedHoaDonInfo);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("delete-item/{idChiTiet}")]
        public async Task<IActionResult> DeleteItem(int idChiTiet)
        {
            try
            {
                var item = await _context.ChiTietHoaDons.Include(c => c.HoaDon).FirstOrDefaultAsync(c => c.IdChiTietHoaDon == idChiTiet);
                if (item == null) return NotFound("Không tìm thấy món.");
                var hoaDon = item.HoaDon;
                if (hoaDon.TrangThai == "Đã thanh toán") return Conflict("Hóa đơn đã thanh toán.");

                int idHoaDon = item.IdHoaDon;
                _context.ChiTietHoaDons.Remove(item);
                await _context.SaveChangesAsync();
                await UpdateHoaDonTotals(hoaDon); await ReApplyPromotion(hoaDon); await _context.SaveChangesAsync();
                var updatedHoaDonInfo = await GetHoaDonInfo(idHoaDon);
                return Ok(updatedHoaDonInfo);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPut("apply-promotion")]
        public async Task<IActionResult> ApplyPromotion([FromBody] ApplyPromotionRequest req)
        {
            try
            {
                var hoaDon = await _context.HoaDons.FindAsync(req.IdHoaDon);
                if (hoaDon == null) return NotFound("Hóa đơn không tồn tại.");

                var existingPromos = _context.HoaDonKhuyenMais.Where(hk => hk.IdHoaDon == req.IdHoaDon);
                _context.HoaDonKhuyenMais.RemoveRange(existingPromos);

                if (req.IdKhuyenMai == null || req.IdKhuyenMai == 0) hoaDon.GiamGia = 0;
                else
                {
                    var km = await _context.KhuyenMais.FindAsync(req.IdKhuyenMai);
                    if (km == null) return NotFound("Khuyến mãi không tồn tại.");
                    _context.HoaDonKhuyenMais.Add(new HoaDon_KhuyenMai { IdHoaDon = req.IdHoaDon, IdKhuyenMai = km.IdKhuyenMai });
                    if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal tongTienGocChoKM = hoaDon.TongTienGoc;
                        if (km.IdSanPhamApDung.HasValue) tongTienGocChoKM = await _context.ChiTietHoaDons.Where(c => c.IdHoaDon == req.IdHoaDon && c.IdSanPham == km.IdSanPhamApDung).SumAsync(c => c.ThanhTien);
                        decimal giamGia = tongTienGocChoKM * (km.GiaTriGiam / 100);
                        if (km.GiamToiDa.HasValue && giamGia > km.GiamToiDa.Value) giamGia = km.GiamToiDa.Value;
                        hoaDon.GiamGia = giamGia;
                    }
                    else { hoaDon.GiamGia = km.GiaTriGiam; }
                }

                await _context.SaveChangesAsync();
                var updatedHoaDonInfo = await GetHoaDonInfo(req.IdHoaDon);
                return Ok(updatedHoaDonInfo);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        private async Task<HoaDonInfoDto> GetHoaDonInfo(int idHoaDon)
        {
            var updatedHoaDon = await _context.HoaDons.Include(h => h.Ban).AsNoTracking().FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
            if (updatedHoaDon == null) throw new Exception("Không thể tìm thấy hóa đơn sau khi cập nhật.");
            var currentPromotion = await _context.HoaDonKhuyenMais.AsNoTracking().FirstOrDefaultAsync(hk => hk.IdHoaDon == idHoaDon);
            return new HoaDonInfoDto
            {
                IdHoaDon = updatedHoaDon.IdHoaDon,
                SoBan = updatedHoaDon.Ban?.SoBan ?? updatedHoaDon.LoaiHoaDon,
                LoaiHoaDon = updatedHoaDon.LoaiHoaDon,
                TongTienGoc = updatedHoaDon.TongTienGoc,
                GiamGia = updatedHoaDon.GiamGia,
                ThanhTien = updatedHoaDon.ThanhTien,
                IdKhuyenMai = currentPromotion?.IdKhuyenMai
            };
        }

        // SỬA LỖI: Khi thay đổi số lượng món, TrangThaiCheBien giờ đây sẽ nhận biết và được cập nhật số lượng theo.
        private async Task<int> CreateOrUpdateCheBienItems(int idHoaDon)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
            if (hoaDon == null) return 0;
            var chiTietItems = await _context.ChiTietHoaDons.Include(c => c.SanPham).Where(c => c.IdHoaDon == idHoaDon).ToListAsync();
            string soBan = hoaDon.Ban?.SoBan ?? hoaDon.LoaiHoaDon;
            int itemsUpdated = 0;
            var now = DateTime.Now;

            foreach (var item in chiTietItems)
            {
                var existingCB = await _context.TrangThaiCheBiens.FirstOrDefaultAsync(cb => cb.IdChiTietHoaDon == item.IdChiTietHoaDon);
                if (existingCB == null)
                {
                    _context.TrangThaiCheBiens.Add(new TrangThaiCheBien
                    {
                        IdChiTietHoaDon = item.IdChiTietHoaDon,
                        IdHoaDon = item.IdHoaDon,
                        IdSanPham = item.IdSanPham,
                        TenMon = item.SanPham.TenSanPham,
                        SoBan = soBan,
                        SoLuong = item.SoLuong,
                        GhiChu = item.GhiChu,
                        NhomIn = item.SanPham.NhomIn,
                        TrangThai = "Chờ làm",
                        ThoiGianGoi = now
                    });
                    itemsUpdated++;
                }
                else if (existingCB.SoLuong != item.SoLuong || existingCB.GhiChu != item.GhiChu)
                {
                    existingCB.SoLuong = item.SoLuong;
                    existingCB.GhiChu = item.GhiChu;
                    itemsUpdated++;
                }
            }
            if (itemsUpdated > 0) await _context.SaveChangesAsync();
            return itemsUpdated;
        }

        [HttpPost("send-to-kitchen/{idHoaDon}")]
        public async Task<IActionResult> SendToKitchen(int idHoaDon)
        {
            try
            {
                int itemsAdded = await CreateOrUpdateCheBienItems(idHoaDon);
                return Ok(new { message = $"Đã gửi {itemsAdded} món mới vào bếp." });
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi khi gửi bếp: {ex.Message}"); }
        }

        [HttpPut("cancel-order/{idHoaDon}")]
        public async Task<IActionResult> CancelOrder(int idHoaDon)
        {
            try
            {
                var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
                if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn.");

                hoaDon.TrangThai = "Đã hủy";
                if (hoaDon.Ban != null) hoaDon.Ban.TrangThai = "Trống";

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("print-and-notify-kitchen/{idHoaDon}/{idNhanVien}")]
        public async Task<IActionResult> PrintAndNotifyKitchen(int idHoaDon, int idNhanVien)
        {
            try
            {
                // 1. Nhận kết quả xem có bao nhiêu món THỰC SỰ được thêm mới hoặc cập nhật
                int itemsUpdated = await CreateOrUpdateCheBienItems(idHoaDon);

                // 2. CHỈ tạo thông báo nếu có sự thay đổi đẩy xuống bếp
                if (itemsUpdated > 0)
                {
                    var hoaDon = await _context.HoaDons.Include(h => h.Ban).FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);
                    string soBan = hoaDon?.Ban?.SoBan ?? hoaDon?.LoaiHoaDon ?? "Hóa đơn";

                    _context.ThongBaos.Add(new ThongBao
                    {
                        IdNhanVienTao = idNhanVien,
                        NoiDung = $"Phiếu gọi món mới cho [{soBan}].",
                        ThoiGianTao = DateTime.Now,
                        LoaiThongBao = "PhieuGoiMon",
                        IdLienQuan = idHoaDon,
                        DaXem = false
                    });

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Đã gửi phiếu gọi món và thông báo cho bếp." });
                }

                // 3. Nếu không có món nào mới (chỉ là in lại phiếu), trả về OK nhưng KHÔNG ghi thông báo
                return Ok(new { message = "Đã in phiếu (Không có món mới cần thông báo bếp)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi in và gửi bếp: {ex.Message}");
            }
        }

        [HttpGet("print-data/{idHoaDon}")]
        public async Task<IActionResult> GetPrintData(int idHoaDon)
        {
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.Ban)
                    .Include(h => h.ChiTietHoaDons).ThenInclude(ct => ct.SanPham)
                    .AsNoTracking().FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

                if (hoaDon == null) return NotFound();

                var nhanVien = await _context.NhanViens.FindAsync(hoaDon.IdNhanVien);
                string tenNhanVien = nhanVien?.HoTen ?? "N/A";

                var settings = await _context.CaiDats.ToListAsync();
                var dto = new PhieuGoiMonPrintDto
                {
                    IdPhieu = $"HD{hoaDon.IdHoaDon:D6}",
                    TenQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_TenQuan")?.GiaTri ?? "Cafebook",
                    DiaChiQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_DiaChi")?.GiaTri ?? "N/A",
                    SdtQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_SoDienThoai")?.GiaTri ?? "N/A",
                    NgayTao = hoaDon.ThoiGianTao,
                    TenNhanVien = tenNhanVien,
                    SoBan = hoaDon.Ban?.SoBan ?? hoaDon.LoaiHoaDon,
                    ChiTiet = hoaDon.ChiTietHoaDons.Select(ct => new ChiTietDto { TenSanPham = ct.SanPham.TenSanPham, SoLuong = ct.SoLuong, DonGia = ct.DonGia, ThanhTien = ct.ThanhTien, GhiChu = ct.GhiChu }).ToList(),
                    TongTienGoc = hoaDon.TongTienGoc,
                    GiamGia = hoaDon.GiamGia,
                    ThanhTien = hoaDon.ThanhTien
                };
                return Ok(dto);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // =========================================================================
        // API ĐỘC LẬP: Lấy danh sách Khuyến Mãi (Dành riêng cho module Gọi Món)
        // =========================================================================
        [HttpGet("khuyenmai-available/{idHoaDon}")]
        public async Task<IActionResult> GetAvailableKhuyenMai(int idHoaDon)
        {
            try
            {
                // QUAN TRỌNG: Phải Include ChiTietHoaDons để check IdSanPhamApDung
                var hoaDon = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

                if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn.");

                var now = DateTime.Now;
                var allKms = await _context.KhuyenMais.AsNoTracking().ToListAsync();
                var resultList = new List<KhuyenMaiHienThiGoiMonDto>();

                foreach (var km in allKms)
                {
                    // Lọc qua hàm Helper kiểm tra điều kiện toàn diện
                    var (isEligible, reason, discountValue) = CheckEligibility(km, hoaDon, now);

                    var dto = new KhuyenMaiHienThiGoiMonDto
                    {
                        IdKhuyenMai = km.IdKhuyenMai,
                        MaKhuyenMai = km.MaKhuyenMai,
                        TenChuongTrinh = km.TenChuongTrinh,
                        DieuKienApDung = string.IsNullOrEmpty(km.DieuKienApDung) ? km.MoTa : km.DieuKienApDung,
                        LoaiGiamGia = km.LoaiGiamGia,
                        GiaTriGiam = km.GiaTriGiam,
                        GiamToiDa = km.GiamToiDa,
                        IsEligible = isEligible,
                        IneligibilityReason = reason,
                        CalculatedDiscount = discountValue
                    };
                    resultList.Add(dto);
                }

                return Ok(resultList
                    .OrderByDescending(k => k.IsEligible)
                    .ThenByDescending(k => k.CalculatedDiscount)
                    .ThenBy(k => k.TenChuongTrinh));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Hàm Helper: Kiểm tra điều kiện KM dùng chung
        private (bool IsEligible, string? Reason, decimal CalculatedDiscount) CheckEligibility(KhuyenMai km, HoaDon hoaDon, DateTime now)
        {
            decimal calculatedDiscount = 0m;

            if (!string.Equals(km.TrangThai, "Hoạt động", StringComparison.OrdinalIgnoreCase))
                return (false, "Khuyến mãi đã bị tạm ngưng.", 0);
            if (km.NgayBatDau > now)
                return (false, $"Chưa tới ngày (Bắt đầu từ {km.NgayBatDau:dd/MM}).", 0);
            if (km.NgayKetThuc < now)
                return (false, $"Đã hết hạn (Kết thúc lúc {km.NgayKetThuc:dd/MM}).", 0);
            if (km.SoLuongConLai.HasValue && km.SoLuongConLai <= 0)
                return (false, "Đã hết lượt sử dụng.", 0);
            if (km.HoaDonToiThieu.HasValue && km.HoaDonToiThieu > 0 && hoaDon.TongTienGoc < km.HoaDonToiThieu.Value)
                return (false, $"Cần hóa đơn tối thiểu {km.HoaDonToiThieu.Value:N0} đ.", 0);
            if (km.GioBatDau.HasValue && km.GioKetThuc.HasValue && (now.TimeOfDay < km.GioBatDau || now.TimeOfDay > km.GioKetThuc))
                return (false, $"Chỉ áp dụng trong khung giờ {km.GioBatDau:hh\\:mm} - {km.GioKetThuc:hh\\:mm}.", 0);

            // LOGIC CHECK NGÀY 2,3,4,5,6,7,8
            if (!string.IsNullOrWhiteSpace(km.NgayTrongTuan))
            {
                string homNay = "";
                switch (now.DayOfWeek)
                {
                    case DayOfWeek.Monday: homNay = "2"; break;
                    case DayOfWeek.Tuesday: homNay = "3"; break;
                    case DayOfWeek.Wednesday: homNay = "4"; break;
                    case DayOfWeek.Thursday: homNay = "5"; break;
                    case DayOfWeek.Friday: homNay = "6"; break;
                    case DayOfWeek.Saturday: homNay = "7"; break;
                    case DayOfWeek.Sunday: homNay = "8"; break;
                }
                var ngayHopLe = km.NgayTrongTuan.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim());
                if (!ngayHopLe.Contains(homNay))
                {
                    return (false, "Không áp dụng vào ngày hôm nay.", 0);
                }
            }

            // Tính toán giá trị giảm & Kiểm tra Sản phẩm áp dụng
            decimal tongTienGocChoKM = hoaDon.TongTienGoc;
            if (km.IdSanPhamApDung.HasValue)
            {
                if (hoaDon.ChiTietHoaDons == null || !hoaDon.ChiTietHoaDons.Any(c => c.IdSanPham == km.IdSanPhamApDung.Value))
                {
                    return (false, "Hóa đơn không có sản phẩm được áp dụng KM này.", 0);
                }
                tongTienGocChoKM = hoaDon.ChiTietHoaDons
                    .Where(c => c.IdSanPham == km.IdSanPhamApDung.Value)
                    .Sum(c => c.ThanhTien);
            }

            if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
            {
                calculatedDiscount = tongTienGocChoKM * (km.GiaTriGiam / 100);
                if (km.GiamToiDa.HasValue && calculatedDiscount > km.GiamToiDa.Value)
                {
                    calculatedDiscount = km.GiamToiDa.Value;
                }
            }
            else
            {
                calculatedDiscount = km.GiaTriGiam;
            }

            return (true, null, calculatedDiscount);
        }
    }
}