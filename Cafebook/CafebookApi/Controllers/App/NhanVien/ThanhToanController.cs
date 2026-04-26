using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.NhanVien;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/thanhtoan")]
    [ApiController]
    [Authorize]
    public class ThanhToanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private decimal _tiLeDoiDiem = 1000m;
        private decimal _tiLeNhanDiem = 10000m;
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public ThanhToanController(CafebookDbContext context)
        {
            _context = context;
        }

        private async Task LoadCaiDat()
        {
            _settings = await _context.CaiDats
                .Where(c =>
                    c.TenCaiDat == "DiemTichLuy_DoiVND" ||
                    c.TenCaiDat == "DiemTichLuy_NhanVND" ||
                    c.TenCaiDat == "ThongTin_TenQuan" ||
                    c.TenCaiDat == "ThongTin_DiaChi" ||
                    c.TenCaiDat == "ThongTin_SoDienThoai" ||
                    c.TenCaiDat == "Wifi_MatKhau" ||
                    c.TenCaiDat == "NganHang_SoTaiKhoan" ||
                    c.TenCaiDat == "NganHang_ChuTaiKhoan" ||
                    c.TenCaiDat == "NganHang_MaDinhDanhNganHang"||
                    c.TenCaiDat == "VNPay_Url" ||
                    c.TenCaiDat == "VNPay_TmnCode" ||
                    c.TenCaiDat == "VNPay_HashSecret")
                .AsNoTracking()
                .ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            decimal.TryParse(_settings.GetValueOrDefault("DiemTichLuy_DoiVND", "1000"), out _tiLeDoiDiem);
            decimal.TryParse(_settings.GetValueOrDefault("DiemTichLuy_NhanVND", "10000"), out _tiLeNhanDiem);
            if (_tiLeNhanDiem == 0) _tiLeNhanDiem = 10000;
            if (_tiLeDoiDiem == 0) _tiLeDoiDiem = 1000;
        }

        [HttpGet("load/{idHoaDon}")]
        public async Task<IActionResult> LoadThanhToanData(int idHoaDon)
        {
            await LoadCaiDat();

            var hoaDon = await _context.HoaDons
                .Include(h => h.Ban)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon);

            if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn.");

            var hoaDonInfo = new HoaDonInfoDto
            {
                IdHoaDon = hoaDon.IdHoaDon,
                SoBan = hoaDon.Ban?.SoBan ?? hoaDon.LoaiHoaDon,
                LoaiHoaDon = hoaDon.LoaiHoaDon,
                TongTienGoc = hoaDon.TongTienGoc,
                GiamGia = hoaDon.GiamGia,
                ThanhTien = hoaDon.ThanhTien,
            };

            var chiTietItems = await _context.ChiTietHoaDons
                .Where(c => c.IdHoaDon == idHoaDon)
                .AsNoTracking()
                .Select(c => new ChiTietDto
                {
                    IdChiTietHoaDon = c.IdChiTietHoaDon,
                    IdSanPham = c.IdSanPham,
                    TenSanPham = c.SanPham.TenSanPham,
                    SoLuong = c.SoLuong,
                    DonGia = c.DonGia,
                    ThanhTien = c.ThanhTien
                }).ToListAsync();

            var phuThusDaApDung = await _context.ChiTietPhuThuHoaDons
                .Where(pt => pt.IdHoaDon == idHoaDon)
                .AsNoTracking()
                .Select(pt => new PhuThuDto
                {
                    IdPhuThu = pt.IdPhuThu,
                    TenPhuThu = pt.PhuThu.TenPhuThu,
                    SoTien = pt.SoTien,
                    LoaiGiaTri = pt.PhuThu.LoaiGiaTri,
                    GiaTri = pt.PhuThu.GiaTri
                }).ToListAsync();

            var idPhuThuDaApDung = phuThusDaApDung.Select(p => p.IdPhuThu);
            var phuThusKhaDung = await _context.PhuThus
                .Where(p => !idPhuThuDaApDung.Contains(p.IdPhuThu))
                .AsNoTracking()
                .ToListAsync();

            var khuyenMaiLink = await _context.HoaDonKhuyenMais
                .AsNoTracking()
                .FirstOrDefaultAsync(hkm => hkm.IdHoaDon == idHoaDon);

            KhachHang? khachHang = null;
            if (hoaDon.IdKhachHang.HasValue)
            {
                khachHang = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(kh => kh.IdKhachHang == hoaDon.IdKhachHang.Value);
            }

            var khachHangsList = await _context.KhachHangs
                .AsNoTracking()
                .Select(kh => new KhachHangTimKiemDto
                {
                    IdKhachHang = kh.IdKhachHang,
                    DisplayText = kh.HoTen + (kh.SoDienThoai != null ? $" - {kh.SoDienThoai}" : ""),
                    KhachHangData = kh
                }).ToListAsync();

            return Ok(new ThanhToanViewDto
            {
                HoaDonInfo = hoaDonInfo,
                ChiTietItems = chiTietItems,
                IdKhuyenMaiDaApDung = khuyenMaiLink?.IdKhuyenMai,
                PhuThusDaApDung = phuThusDaApDung,
                PhuThusKhaDung = phuThusKhaDung,
                KhachHang = khachHang,
                KhachHangsList = khachHangsList,
                DiemTichLuy_DoiVND = _tiLeDoiDiem,
                DiemTichLuy_NhanVND = _tiLeNhanDiem,

                TenQuan = _settings.GetValueOrDefault("ThongTin_TenQuan", "CafeBook"),
                DiaChi = _settings.GetValueOrDefault("ThongTin_DiaChi", "N/A"),
                SoDienThoai = _settings.GetValueOrDefault("ThongTin_SoDienThoai", "N/A"),
                WifiMatKhau = _settings.GetValueOrDefault("Wifi_MatKhau", "N/A"),

                NganHang_SoTaiKhoan = _settings.GetValueOrDefault("NganHang_SoTaiKhoan", ""),
                NganHang_ChuTaiKhoan = _settings.GetValueOrDefault("NganHang_ChuTaiKhoan", ""),
                NganHang_MaDinhDanhNganHang = _settings.GetValueOrDefault("NganHang_MaDinhDanhNganHang", "")
            });
        }

        [HttpPost("find-or-create-customer")]
        public async Task<IActionResult> FindOrCreateCustomer([FromBody] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(null);
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.SoDienThoai == query);

            if (khachHang != null)
            {
                var khDto = new KhachHangTimKiemDto
                {
                    IdKhachHang = khachHang.IdKhachHang,
                    DisplayText = khachHang.HoTen + $" - {khachHang.SoDienThoai}",
                    KhachHangData = khachHang,
                    IsNew = false
                };
                return Ok(khDto);
            }

            if (IsValidPhone(query))
            {
                var newKhachHang = new KhachHang
                {
                    HoTen = $"Khách SĐT {query}",
                    SoDienThoai = query,
                    Email = $"{query}@temp.cafebook.com",
                    TenDangNhap = query,
                    MatKhau = "123456",
                    TaiKhoanTam = true,
                    NgayTao = DateTime.Now,
                    DiemTichLuy = 0,
                    BiKhoa = false
                };

                _context.KhachHangs.Add(newKhachHang);
                await _context.SaveChangesAsync();

                var newKhachHangDto = new KhachHangTimKiemDto
                {
                    IdKhachHang = newKhachHang.IdKhachHang,
                    DisplayText = newKhachHang.HoTen + $" - {newKhachHang.SoDienThoai}",
                    KhachHangData = newKhachHang,
                    IsNew = true
                };
                return Ok(newKhachHangDto);
            }
            else
            {
                return Ok(null);
            }
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

                if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn.");

                var now = DateTime.Now;
                var allKms = await _context.KhuyenMais.AsNoTracking().ToListAsync();
                var resultList = new List<KhuyenMaiHienThiThanhToanDto>();

                foreach (var km in allKms)
                {
                    var (isEligible, reason, discountValue) = CheckEligibility(km, hoaDon, now);

                    var dto = new KhuyenMaiHienThiThanhToanDto
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

        [HttpPost("pay")]
        public async Task<IActionResult> ProcessPayment([FromBody] ThanhToanRequestDto req)
        {
            await LoadCaiDat();

            var hoaDonGoc = await _context.HoaDons
                .Include(h => h.Ban)
                .Include(h => h.ChiTietHoaDons)
                .Include(h => h.ChiTietPhuThuHoaDons)
                .FirstOrDefaultAsync(h => h.IdHoaDon == req.IdHoaDonGoc);

            if (hoaDonGoc == null) return NotFound("Không tìm thấy hóa đơn gốc.");
            if (hoaDonGoc.TrangThai != "Chưa thanh toán") return Conflict("Hóa đơn này đã được xử lý.");

            var allItemsGocIds = hoaDonGoc.ChiTietHoaDons.Select(c => c.IdChiTietHoaDon).ToList();
            bool isFullPayment = allItemsGocIds.Count == req.IdChiTietTach.Count && allItemsGocIds.All(req.IdChiTietTach.Contains);

            HoaDon hoaDonThanhToan;

            var chiTietTach = hoaDonGoc.ChiTietHoaDons
                .Where(c => req.IdChiTietTach.Contains(c.IdChiTietHoaDon)).ToList();

            var phuThuDaApDungGoc = hoaDonGoc.ChiTietPhuThuHoaDons
                .Where(pt => req.IdPhuThuTach.Contains(pt.IdPhuThu)).ToList();
            var idPhuThuMoi = req.IdPhuThuTach.Where(id => !phuThuDaApDungGoc.Any(pt => pt.IdPhuThu == id)).ToList();
            var phuThuMoi = await _context.PhuThus.Where(p => idPhuThuMoi.Contains(p.IdPhuThu)).ToListAsync();

            if (isFullPayment)
            {
                hoaDonThanhToan = hoaDonGoc;
                if (req.IdKhachHang.HasValue)
                    hoaDonThanhToan.IdKhachHang = req.IdKhachHang;

                decimal tongTienGocMoi = chiTietTach.Sum(c => c.ThanhTien);
                hoaDonThanhToan.TongTienGoc = tongTienGocMoi;

                decimal tongPhuThuMoi = 0;

                foreach (var phuThu in phuThuDaApDungGoc)
                {
                    tongPhuThuMoi += phuThu.SoTien;
                }

                foreach (var ptMoi in phuThuMoi)
                {
                    decimal soTienPT = string.Equals(ptMoi.LoaiGiaTri, "%", StringComparison.OrdinalIgnoreCase)
                        ? (tongTienGocMoi * (ptMoi.GiaTri / 100))
                        : ptMoi.GiaTri;

                    _context.ChiTietPhuThuHoaDons.Add(new ChiTietPhuThuHoaDon
                    {
                        IdHoaDon = hoaDonThanhToan.IdHoaDon,
                        IdPhuThu = ptMoi.IdPhuThu,
                        SoTien = soTienPT
                    });
                    tongPhuThuMoi += soTienPT;
                }

                hoaDonThanhToan.TongPhuThu = tongPhuThuMoi;
            }
            else
            {
                hoaDonThanhToan = new HoaDon
                {
                    IdBan = hoaDonGoc.IdBan,
                    IdNhanVien = hoaDonGoc.IdNhanVien,
                    IdKhachHang = req.IdKhachHang,
                    ThoiGianTao = hoaDonGoc.ThoiGianTao,
                    LoaiHoaDon = hoaDonGoc.LoaiHoaDon,
                    GhiChu = $"Tách từ HĐ #{hoaDonGoc.IdHoaDon}"
                };
                _context.HoaDons.Add(hoaDonThanhToan);
                await _context.SaveChangesAsync();

                decimal tongTienTach = 0;
                foreach (var chiTiet in chiTietTach)
                {
                    chiTiet.IdHoaDon = hoaDonThanhToan.IdHoaDon;
                    tongTienTach += chiTiet.ThanhTien;
                    hoaDonGoc.ChiTietHoaDons.Remove(chiTiet);
                }
                hoaDonThanhToan.TongTienGoc = tongTienTach;

                decimal tongPhuThuTach = 0;
                foreach (var phuThu in phuThuDaApDungGoc)
                {
                    phuThu.IdHoaDon = hoaDonThanhToan.IdHoaDon;
                    tongPhuThuTach += phuThu.SoTien;
                    hoaDonGoc.ChiTietPhuThuHoaDons.Remove(phuThu);
                }
                foreach (var ptMoi in phuThuMoi)
                {
                    decimal soTienPT = string.Equals(ptMoi.LoaiGiaTri, "%", StringComparison.OrdinalIgnoreCase)
                        ? (tongTienTach * (ptMoi.GiaTri / 100))
                        : ptMoi.GiaTri;
                    _context.ChiTietPhuThuHoaDons.Add(new ChiTietPhuThuHoaDon
                    {
                        IdHoaDon = hoaDonThanhToan.IdHoaDon,
                        IdPhuThu = ptMoi.IdPhuThu,
                        SoTien = soTienPT
                    });
                    tongPhuThuTach += soTienPT;
                }
                hoaDonThanhToan.TongPhuThu = tongPhuThuTach;

                hoaDonGoc.TongTienGoc = hoaDonGoc.ChiTietHoaDons.Sum(c => c.ThanhTien);
                hoaDonGoc.TongPhuThu = hoaDonGoc.ChiTietPhuThuHoaDons.Sum(pt => pt.SoTien);
            }

            decimal giamGiaKM = 0;
            decimal giamGiaDiem = 0;

            if (req.IdKhuyenMai.HasValue && req.IdKhuyenMai > 0)
            {
                var km = await _context.KhuyenMais.FindAsync(req.IdKhuyenMai.Value);
                if (km != null)
                {
                    if (km.SoLuongConLai.HasValue && km.SoLuongConLai <= 0)
                    {
                        return BadRequest($"Thanh toán thất bại: Khuyến mãi '{km.TenChuongTrinh}' đã hết lượt sử dụng trong hệ thống. Vui lòng gỡ khuyến mãi này ra khỏi hóa đơn!");
                    }

                    giamGiaKM = await CalculateDiscount(km, hoaDonThanhToan.TongTienGoc, chiTietTach);

                    var existingLink = await _context.HoaDonKhuyenMais
                        .AsNoTracking()
                        .FirstOrDefaultAsync(hkm => hkm.IdHoaDon == hoaDonThanhToan.IdHoaDon && hkm.IdKhuyenMai == km.IdKhuyenMai);

                    if (existingLink == null)
                    {
                        _context.HoaDonKhuyenMais.Add(new HoaDon_KhuyenMai { IdHoaDon = hoaDonThanhToan.IdHoaDon, IdKhuyenMai = km.IdKhuyenMai });
                    }

                    if (km.SoLuongConLai.HasValue)
                    {
                        km.SoLuongConLai -= 1;
                    }
                }
            }

            KhachHang? khachHang = null;
            if (req.IdKhachHang.HasValue)
            {
                khachHang = await _context.KhachHangs.FindAsync(req.IdKhachHang.Value);
            }

            if (khachHang != null && req.DiemSuDung > 0)
            {
                if (khachHang.DiemTichLuy < req.DiemSuDung)
                    return BadRequest("Khách hàng không đủ điểm tích lũy.");

                giamGiaDiem = req.DiemSuDung * _tiLeDoiDiem;

                decimal tongTruocDiem = hoaDonThanhToan.TongTienGoc + hoaDonThanhToan.TongPhuThu - giamGiaKM;

                if (giamGiaDiem > tongTruocDiem)
                {
                    giamGiaDiem = tongTruocDiem;
                    req.DiemSuDung = (int)Math.Ceiling(giamGiaDiem / _tiLeDoiDiem);
                }

                khachHang.DiemTichLuy -= req.DiemSuDung;
                hoaDonThanhToan.IdKhachHang = khachHang.IdKhachHang;
            }

            hoaDonThanhToan.GiamGia = giamGiaKM + giamGiaDiem;

            // XÓA BỎ LỜI GỌI HÀM TRUKHO Ở ĐÂY

            hoaDonThanhToan.TrangThai = "Đã thanh toán";
            hoaDonThanhToan.ThoiGianThanhToan = DateTime.Now;
            hoaDonThanhToan.PhuongThucThanhToan = req.PhuongThucThanhToan;

            await _context.SaveChangesAsync();

            _context.GiaoDichThanhToans.Add(new GiaoDichThanhToan
            {
                IdHoaDon = hoaDonThanhToan.IdHoaDon,
                MaGiaoDichNgoai = $"HD_{hoaDonThanhToan.IdHoaDon}_{DateTime.Now:HHmmss}",
                CongThanhToan = req.PhuongThucThanhToan,
                SoTien = hoaDonThanhToan.ThanhTien,
                ThoiGianGiaoDich = (DateTime)hoaDonThanhToan.ThoiGianThanhToan,
                TrangThai = "Thành công"
            });

            int diemCongMoi = 0;
            int tongDiemSauThanhToan = 0;

            if (khachHang != null && _tiLeNhanDiem > 0)
            {
                if (req.DiemSuDung == 0)
                {
                    int diemMoi = (int)Math.Floor(hoaDonThanhToan.ThanhTien / _tiLeNhanDiem);
                    if (diemMoi > 0)
                    {
                        khachHang.DiemTichLuy += diemMoi;
                        diemCongMoi = diemMoi;
                    }
                }
                tongDiemSauThanhToan = khachHang.DiemTichLuy;
            }

            bool hoaDonGocDaThanhToanHet = false;

            if (isFullPayment || (hoaDonGoc.TongTienGoc == 0 && hoaDonGoc.TongPhuThu == 0))
            {
                hoaDonGocDaThanhToanHet = true;
                if (hoaDonGoc.Ban != null)
                {
                    hoaDonGoc.Ban.TrangThai = "Trống";
                }
                if (!isFullPayment)
                {
                    hoaDonGoc.TrangThai = "Đã hủy";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Thanh toán thành công!",
                isFullPayment = hoaDonGocDaThanhToanHet,
                idHoaDonDaThanhToan = hoaDonThanhToan.IdHoaDon,
                diemCong = diemCongMoi,
                tongDiemTichLuy = tongDiemSauThanhToan
            });
        }

        [HttpPost("vnpay-url")]
        public async Task<IActionResult> GenerateVNPayUrl([FromBody] VNPayUrlRequestDto req)
        {
            await LoadCaiDat();
            string vnp_Url = _settings.GetValueOrDefault("VNPay_Url", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
            string vnp_TmnCode = _settings.GetValueOrDefault("VNPay_TmnCode", "");
            string vnp_HashSecret = _settings.GetValueOrDefault("VNPay_HashSecret", "");

            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
                return BadRequest("Chưa cấu hình VNPAY (TmnCode, HashSecret) trong bảng CaiDat.");

            var vnpay = new CafebookModel.Utils.VNPayHelper();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(req.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan HD {req.IdHoaDonGoc}");
            vnpay.AddRequestData("vnp_OrderType", "other");

            vnpay.AddRequestData("vnp_ReturnUrl", "https://localhost/vnpay-app-return");
            vnpay.AddRequestData("vnp_TxnRef", $"{req.IdHoaDonGoc}_{DateTime.Now.Ticks}");

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Ok(new VNPayUrlResponseDto { PaymentUrl = paymentUrl });
        }
        #region Hàm Helper 

        private async Task<decimal> CalculateDiscount(KhuyenMai km, decimal tongTienGoc, ICollection<ChiTietHoaDon> chiTietList)
        {
            decimal tongTienGocChoKM = tongTienGoc;
            if (km.IdSanPhamApDung.HasValue)
            {
                tongTienGocChoKM = chiTietList
                    .Where(c => c.IdSanPham == km.IdSanPhamApDung.Value)
                    .Sum(c => c.ThanhTien);
            }

            decimal giamGia = 0;
            if (string.Equals(km.LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
            {
                giamGia = tongTienGocChoKM * (km.GiaTriGiam / 100);
                if (km.GiamToiDa.HasValue && giamGia > km.GiamToiDa.Value)
                {
                    giamGia = km.GiamToiDa.Value;
                }
            }
            else
            {
                giamGia = km.GiaTriGiam;
            }
            return await Task.FromResult(giamGia);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return phone.All(char.IsDigit) && phone.Length >= 9 && phone.Length <= 11;
        }

        #endregion
    }
}