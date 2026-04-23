using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelEntities;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.NhanVien
{
    [Route("api/app/nhanvien/thuesach")]
    [ApiController]
    [Authorize]
    public class ThueSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ThueSachController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.CaiDats.AsNoTracking()
                .Where(c => c.TenCaiDat.StartsWith("Sach_") || c.TenCaiDat.StartsWith("DiemTichLuy_") || c.TenCaiDat.StartsWith("NganHang_"))
                .ToListAsync();

            var dto = new CaiDatThueSachDto
            {
                PhiThue = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_PhiThue")?.GiaTri ?? "5000"),
                PhiTraTreMoiNgay = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_PhiTraTreMoiNgay")?.GiaTri ?? "2000"),
                SoNgayMuonToiDa = int.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_SoNgayMuonToiDa")?.GiaTri ?? "7"),
                DiemPhieuThue = int.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_DiemPhieuThue")?.GiaTri ?? "1"),
                PointToVND = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "DiemTichLuy_DoiVND")?.GiaTri ?? "1000"),
                BankId = settings.FirstOrDefault(c => c.TenCaiDat == "NganHang_MaDinhDanhNganHang")?.GiaTri ?? "",
                BankAccount = settings.FirstOrDefault(c => c.TenCaiDat == "NganHang_SoTaiKhoan")?.GiaTri ?? "",
                BankAccountName = settings.FirstOrDefault(c => c.TenCaiDat == "NganHang_ChuTaiKhoan")?.GiaTri ?? "",
                PhatGiamDoMoi1Percent = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_PhatGiamDoMoi1Percent")?.GiaTri ?? "2000")
            };
            return Ok(dto);
        }

        [HttpGet("phieuthue")]
        public async Task<IActionResult> GetPhieuThue([FromQuery] string? search, [FromQuery] string status = "Đang Thuê")
        {
            var query = _context.PhieuThueSachs.AsNoTracking().AsQueryable();
            if (status == "Đang Thuê" || status == "Đã Trả") { query = query.Where(p => p.TrangThai == status); }

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                int.TryParse(search, out int phieuId);
                query = query.Where(p => p.IdPhieuThueSach == phieuId || p.KhachHang.HoTen.ToLower().Contains(searchLower) || (p.KhachHang.SoDienThoai != null && p.KhachHang.SoDienThoai.Contains(search)));
            }

            var phieus = await query.Include(p => p.KhachHang).Include(p => p.ChiTietPhieuThues).OrderByDescending(p => p.NgayThue).ToListAsync();
            var dtos = phieus.Select(p => new PhieuThueGridDto
            {
                IdPhieuThueSach = p.IdPhieuThueSach,
                HoTenKH = p.KhachHang.HoTen,
                SoDienThoaiKH = p.KhachHang.SoDienThoai,
                NgayThue = p.NgayThue,
                NgayHenTra = p.ChiTietPhieuThues.Where(ct => ct.NgayTraThucTe == null).Select(ct => (DateTime?)ct.NgayHenTra).Min() ?? p.NgayThue,
                SoLuongSach = p.ChiTietPhieuThues.Count(ct => ct.NgayTraThucTe == null),
                TongTienCoc = p.TongTienCoc,
                TrangThai = p.TrangThai,
                TinhTrang = (p.TrangThai == "Đã Trả") ? "Hoàn tất" : (p.ChiTietPhieuThues.Any(ct => ct.NgayTraThucTe == null && ct.NgayHenTra < DateTime.Today) ? "Trễ Hạn" : "Đúng Hạn")
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("chitiet/{idPhieu}")]
        public async Task<IActionResult> GetChiTietPhieu(int idPhieu)
        {
            var phieu = await _context.PhieuThueSachs.AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietPhieuThues).ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(p => p.IdPhieuThueSach == idPhieu);

            if (phieu == null) return NotFound();

            var dsIdPhieuTra = await _context.PhieuTraSachs.AsNoTracking()
                .Where(pt => pt.IdPhieuThueSach == idPhieu)
                .OrderByDescending(pt => pt.NgayTra)
                .Select(pt => pt.IdPhieuTra)
                .ToListAsync();

            var settings = await GetSettingsInternal();
            var now = DateTime.Now;

            var dto = new PhieuThueChiTietDto
            {
                IdPhieuThueSach = phieu.IdPhieuThueSach,
                HoTenKH = phieu.KhachHang.HoTen,
                SoDienThoaiKH = phieu.KhachHang.SoDienThoai,
                EmailKH = phieu.KhachHang.Email,
                DiemTichLuyKH = phieu.KhachHang.DiemTichLuy,
                NgayThue = phieu.NgayThue,
                TrangThaiPhieu = phieu.TrangThai,
                DsIdPhieuTra = dsIdPhieuTra,
                SachDaThue = phieu.ChiTietPhieuThues.Select(ct => {
                    bool treHan = ct.NgayTraThucTe == null && ct.NgayHenTra < now.Date;
                    int daysLate = treHan ? (int)(now.Date - ct.NgayHenTra).TotalDays : 0;
                    return new ChiTietSachThueDto
                    {
                        IdPhieuThueSach = ct.IdPhieuThueSach,
                        IdSach = ct.IdSach,
                        TenSach = ct.Sach.TenSach,
                        NgayHenTra = ct.NgayHenTra,
                        TienCoc = ct.TienCoc,
                        TienPhat = daysLate * settings.PhiTraTreMoiNgay,
                        TinhTrang = ct.NgayTraThucTe != null ? "Đã Trả" : (treHan ? $"Trễ {daysLate} ngày" : "Đang Thuê"),
                        DoMoiKhiThue = ct.DoMoiKhiThue ?? 100,
                        GhiChuKhiThue = ct.GhiChuKhiThue
                    };
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpGet("search-khachhang")]
        public async Task<IActionResult> SearchKhachHang([FromQuery] string query)
        {
            var queryLower = query.ToLower();
            var khachHangs = await _context.KhachHangs.AsNoTracking()
                .Where(kh => kh.HoTen.ToLower().Contains(queryLower) || (kh.SoDienThoai != null && kh.SoDienThoai.Contains(query)))
                .Take(10)
                .Select(kh => new KhachHangSearchDto
                {
                    IdKhachHang = kh.IdKhachHang,
                    HoTen = kh.HoTen,
                    SoDienThoai = kh.SoDienThoai,
                    DiemTichLuy = kh.DiemTichLuy,
                    Email = kh.Email
                })
                .ToListAsync();
            return Ok(khachHangs);
        }

        [HttpGet("search-sach")]
        public async Task<IActionResult> SearchSach([FromQuery] string query)
        {
            var queryLower = query.ToLower();
            int.TryParse(query, out int sachId);

            var sachs = await _context.Sachs.AsNoTracking()
                .Where(s => s.SoLuongHienCo > 0 && (s.TenSach.ToLower().Contains(queryLower) || s.IdSach == sachId))
                .Include(s => s.SachTacGias).ThenInclude(stg => stg.TacGia)
                .Take(10)
                .Select(s => new SachTimKiemDto
                {
                    IdSach = s.IdSach,
                    TenSach = s.TenSach,
                    TacGia = string.Join(", ", s.SachTacGias.Select(stg => stg.TacGia.TenTacGia)),
                    SoLuongHienCo = s.SoLuongHienCo,
                    GiaBia = s.GiaBia ?? 0
                })
                .ToListAsync();
            return Ok(sachs);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePhieuThue([FromBody] PhieuThueRequestDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var settings = await GetSettingsInternal();
                int khachHangId;

                if (dto.KhachHangInfo == null || string.IsNullOrWhiteSpace(dto.KhachHangInfo.HoTen))
                    return BadRequest("Tên khách hàng là bắt buộc.");

                KhachHang? khach = null;
                string? sdt = dto.KhachHangInfo.SoDienThoai;
                string? email = dto.KhachHangInfo.Email;

                if (!string.IsNullOrWhiteSpace(sdt)) khach = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt);
                if (khach == null && !string.IsNullOrWhiteSpace(email)) khach = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == email);

                if (khach == null)
                {
                    if (!string.IsNullOrWhiteSpace(sdt) && await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdt || k.TenDangNhap == sdt)) return Conflict("Số điện thoại này đã tồn tại.");
                    if (!string.IsNullOrWhiteSpace(email) && await _context.KhachHangs.AnyAsync(k => k.Email == email)) return Conflict("Email này đã tồn tại.");

                    string tenDangNhap = !string.IsNullOrWhiteSpace(sdt) ? sdt : (!string.IsNullOrWhiteSpace(email) ? email : $"temp_{Guid.NewGuid().ToString("N")[..12]}");

                    khach = new KhachHang
                    {
                        HoTen = dto.KhachHangInfo.HoTen,
                        SoDienThoai = sdt,
                        Email = email,
                        NgayTao = DateTime.Now,
                        DiemTichLuy = 0,
                        BiKhoa = false,
                        TenDangNhap = tenDangNhap,
                        MatKhau = "123456",
                        TaiKhoanTam = true
                    };
                    _context.KhachHangs.Add(khach);
                    await _context.SaveChangesAsync();
                }
                khachHangId = khach.IdKhachHang;

                decimal tongCoc = dto.SachCanThue.Sum(s => s.TienCoc);
                var phieuThue = new PhieuThueSach
                {
                    IdKhachHang = khachHangId,
                    IdNhanVien = dto.IdNhanVien,
                    NgayThue = DateTime.Now,
                    TrangThai = "Đang Thuê",
                    TongTienCoc = tongCoc
                };
                _context.PhieuThueSachs.Add(phieuThue);
                await _context.SaveChangesAsync();

                var sachGuiMail = new List<ChiTietPrintDto>();

                foreach (var sachThue in dto.SachCanThue)
                {
                    var sach = await _context.Sachs.FindAsync(sachThue.IdSach);
                    if (sach == null || sach.SoLuongHienCo <= 0)
                    {
                        await transaction.RollbackAsync();
                        return Conflict($"Sách '{sach?.TenSach ?? "ID: " + sachThue.IdSach}' đã hết hàng.");
                    }
                    sach.SoLuongHienCo--;

                    var chiTiet = new ChiTietPhieuThue
                    {
                        IdPhieuThueSach = phieuThue.IdPhieuThueSach,
                        IdSach = sachThue.IdSach,
                        NgayHenTra = dto.NgayHenTra,
                        TienCoc = sachThue.TienCoc,
                        DoMoiKhiThue = sachThue.DoMoiKhiThue > 0 ? sachThue.DoMoiKhiThue : 100,
                        GhiChuKhiThue = string.IsNullOrWhiteSpace(sachThue.GhiChuKhiThue) ? "Bình thường" : sachThue.GhiChuKhiThue,
                        TienPhatTraTre = null,
                        NgayTraThucTe = null
                    };
                    _context.ChiTietPhieuThues.Add(chiTiet);

                    sachGuiMail.Add(new ChiTietPrintDto
                    {
                        TenSach = sach.TenSach,
                        DoMoi = chiTiet.DoMoiKhiThue.Value,
                        GhiChu = chiTiet.GhiChuKhiThue
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (!string.IsNullOrWhiteSpace(dto.KhachHangInfo.Email))
                {
                    var dictSettings = await GetGeneralSettingsAsync();
                    _ = SendConfirmationEmailAsync(dto.KhachHangInfo.Email, dto.KhachHangInfo.HoTen, sachGuiMail, dto.NgayHenTra, phieuThue.IdPhieuThueSach, tongCoc, dictSettings);
                }

                return Ok(new { IdPhieuThueSach = phieuThue.IdPhieuThueSach, TongTienCoc = tongCoc });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi nội bộ: {ex.Message}");
            }
        }

        [HttpPost("extend")]
        public async Task<IActionResult> ExtendRental([FromBody] GiaHanRequestDto dto)
        {
            var phieu = await _context.Set<PhieuThueSach>()
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietPhieuThues).ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(p => p.IdPhieuThueSach == dto.IdPhieuThueSach);

            if (phieu == null) return NotFound("Không tìm thấy phiếu.");

            var settingsDict = await GetGeneralSettingsAsync();
            string tenQuan = settingsDict.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settingsDict.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportEmail = settingsDict.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");
            string supportPhone = settingsDict.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

            if (dto.NgayHenTraMoi.Date <= DateTime.Today)
                return BadRequest("Ngày gia hạn phải lớn hơn ngày hiện tại.");

            bool isExtended = false;
            var sachGiaHanList = new List<string>();

            foreach (var ct in phieu.ChiTietPhieuThues.Where(c => c.NgayTraThucTe == null))
            {
                ct.NgayHenTra = dto.NgayHenTraMoi.Date;
                sachGiaHanList.Add(ct.Sach.TenSach);
                isExtended = true;
            }

            if (!isExtended) return BadRequest("Phiếu này đã trả hết sách.");
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(phieu.KhachHang.Email))
            {
                string body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; background-color: #F7F3E9; margin: 0; padding: 20px;"">
                    <div style=""max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);"">
                        <div style=""background: #5D4037; color: #FFF; padding: 20px; text-align: center;""><h2>☕ {tenQuan.ToUpper()}</h2></div>
                        <div style=""padding: 30px;"">
                            <h3 style=""color: #2E7D32;"">✨ Gia hạn thành công!</h3>
                            <p>Xin chào <strong>{phieu.KhachHang.HoTen}</strong>,</p>
                            <p>Phiếu thuê <strong>#{phieu.IdPhieuThueSach}</strong> đã được gia hạn thành công.</p>
                            <div style=""background: #F1F8E9; border-left: 5px solid #4CAF50; padding: 15px; margin: 20px 0;"">
                                📅 <strong>Hạn trả mới: {dto.NgayHenTraMoi:dd/MM/yyyy}</strong>
                            </div>
                            <p><strong>📚 Sách được gia hạn:</strong></p>
                            <ul style=""list-style: none; padding: 0;"">
                                {string.Join("", sachGiaHanList.Select(s => $"<li style='padding: 5px 0;'>📖 {s}</li>"))}
                            </ul>
                        </div>
                        <div style=""background: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6;"">
                            <strong>Đội ngũ {tenQuan}</strong><br>
                            📍 Địa chỉ: {diaChiQuan}<br>
                            📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                            © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                        </div>
                    </div>
                </body>
                </html>";

                _ = SendEmailHelper(phieu.KhachHang.Email, $"[{tenQuan}] Xác nhận gia hạn thành công #{phieu.IdPhieuThueSach}", body, settingsDict);
            }
            return Ok(new { Message = "Gia hạn thành công." });
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnSach([FromBody] TraSachRequestDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var settings = await GetSettingsInternal();
                var phieu = await _context.PhieuThueSachs
                    .Include(p => p.ChiTietPhieuThues)
                    .FirstOrDefaultAsync(p => p.IdPhieuThueSach == dto.IdPhieuThueSach);

                if (phieu == null) return NotFound("Không tìm thấy phiếu thuê.");
                var khach = await _context.KhachHangs.FindAsync(phieu.IdKhachHang);
                if (khach == null) return NotFound("Không tìm thấy khách hàng.");

                decimal mucPhatPerPercent = decimal.Parse((await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "Sach_PhatGiamDoMoi1Percent"))?.GiaTri ?? "2000");

                decimal totalPhat = 0;
                decimal totalCoc = 0;
                decimal totalPhiThue = 0;
                int sachDaTra = 0;
                var now = DateTime.Now;

                var phieuTra = new PhieuTraSach
                {
                    IdPhieuThueSach = phieu.IdPhieuThueSach,
                    IdNhanVien = dto.IdNhanVien,
                    NgayTra = now,
                    ChiTietPhieuTras = new List<ChiTietPhieuTra>()
                };

                var sachTraMailList = new List<string>();

                foreach (var item in dto.DanhSachTra)
                {
                    var ct = phieu.ChiTietPhieuThues.FirstOrDefault(c => c.IdSach == item.IdSach && c.NgayTraThucTe == null);
                    if (ct == null) continue;

                    var sach = await _context.Sachs.FindAsync(item.IdSach);
                    if (sach != null) sach.SoLuongHienCo++;

                    ct.NgayTraThucTe = now;

                    // 1. Phạt Trễ
                    decimal tienPhatTre = 0;
                    if (ct.NgayHenTra < now.Date)
                    {
                        int daysLate = (int)(now.Date - ct.NgayHenTra).TotalDays;
                        tienPhatTre = daysLate * settings.PhiTraTreMoiNgay;
                    }
                    ct.TienPhatTraTre = tienPhatTre;

                    // 2. Phạt Hư Hỏng (Khấu hao độ mới)
                    decimal tienPhatHuHong = 0;
                    int doMoiKhiThue = ct.DoMoiKhiThue ?? 100;
                    if (item.DoMoiKhiTra < doMoiKhiThue)
                    {
                        int giamPercent = doMoiKhiThue - item.DoMoiKhiTra;
                        tienPhatHuHong = giamPercent * mucPhatPerPercent;
                    }

                    // 3. Cộng dồn
                    decimal tongPhatCuonNay = tienPhatTre + tienPhatHuHong;
                    totalPhat += tongPhatCuonNay;
                    totalCoc += ct.TienCoc;
                    totalPhiThue += settings.PhiThue;
                    sachDaTra++;

                    // 4. Lưu
                    phieuTra.ChiTietPhieuTras.Add(new ChiTietPhieuTra
                    {
                        IdSach = item.IdSach,
                        TienPhat = tienPhatTre,
                        TienPhatHuHong = tienPhatHuHong,
                        DoMoiKhiTra = item.DoMoiKhiTra,
                        GhiChuKhiTra = item.GhiChuKhiTra
                    });

                    // 5. Build HTML Email
                    if (sach != null)
                    {
                        string phatTreText = tienPhatTre > 0 ? $"<br><small style='color: red;'>- Phạt trễ: {tienPhatTre:N0} đ</small>" : "";
                        string phatHuHongText = tienPhatHuHong > 0 ? $"<br><small style='color: red;'>- Khấu hao hư hỏng ({doMoiKhiThue}% -> {item.DoMoiKhiTra}%): {tienPhatHuHong:N0} đ</small>" : "";
                        string ghiChuText = $"<br><small style='color: #555;'>- Độ mới lúc trả: {item.DoMoiKhiTra}% | Ghi chú: {(string.IsNullOrWhiteSpace(item.GhiChuKhiTra) ? "-" : item.GhiChuKhiTra)}</small>";

                        sachTraMailList.Add($"<li style='padding: 10px 0; border-bottom: 1px solid #EEE;'><strong>📖 {sach.TenSach}</strong> {ghiChuText} {phatTreText} {phatHuHongText}</li>");
                    }
                }

                if (sachDaTra == 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Không có sách nào hợp lệ để trả.");
                }

                if (!phieu.ChiTietPhieuThues.Any(ct => ct.NgayTraThucTe == null)) phieu.TrangThai = "Đã Trả";

                int diem = settings.DiemPhieuThue;
                khach.DiemTichLuy += diem;

                phieuTra.TongPhiThue = totalPhiThue;
                phieuTra.TongTienPhat = totalPhat;
                phieuTra.TongTienCocHoan = totalCoc;
                phieuTra.DiemTichLuy = diem;
                _context.PhieuTraSachs.Add(phieuTra);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (!string.IsNullOrWhiteSpace(khach.Email))
                {
                    var dictSettings = await GetGeneralSettingsAsync();
                    decimal hoanTra = totalCoc - totalPhiThue - totalPhat;
                    _ = SendReturnEmailAsync(khach.Email, khach.HoTen, phieuTra.IdPhieuTra, phieu.IdPhieuThueSach, totalPhiThue, totalPhat, hoanTra, diem, sachTraMailList, dictSettings);
                }

                var response = new TraSachResponseDto
                {
                    IdPhieuTra = phieuTra.IdPhieuTra,
                    SoSachDaTra = sachDaTra,
                    TongPhiThue = totalPhiThue,
                    TongTienPhat = totalPhat,
                    TongTienCoc = totalCoc,
                    TongHoanTra = totalCoc - totalPhiThue - totalPhat,
                    DiemTichLuy = diem
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi nội bộ: {ex.Message}");
            }
        }

        [HttpGet("phieutra")]
        public async Task<IActionResult> GetPhieuTra([FromQuery] string? search)
        {
            var query = _context.PhieuTraSachs.AsNoTracking().Include(pt => pt.NhanVien).AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                int.TryParse(search, out int phieuId);
                query = query.Where(pt => pt.IdPhieuTra == phieuId || pt.IdPhieuThueSach == phieuId);
            }

            var phieuTra = await query.OrderByDescending(pt => pt.NgayTra).Take(100)
                .Select(pt => new PhieuTraGridDto
                {
                    IdPhieuTra = pt.IdPhieuTra,
                    IdPhieuThueSach = pt.IdPhieuThueSach,
                    NgayTra = pt.NgayTra,
                    TenNhanVien = pt.NhanVien.HoTen,
                    TongHoanTra = pt.TongTienCocHoan - pt.TongTienPhat - pt.TongPhiThue
                }).ToListAsync();
            return Ok(phieuTra);
        }

        [HttpPost("send-reminder/{idPhieu}")]
        public async Task<IActionResult> SendReminder(int idPhieu)
        {
            var phieu = await _context.Set<PhieuThueSach>().AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietPhieuThues).ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(p => p.IdPhieuThueSach == idPhieu);

            if (phieu == null || string.IsNullOrEmpty(phieu.KhachHang.Email)) return NotFound();

            var settingsDict = await GetGeneralSettingsAsync();
            var sachChuaTra = phieu.ChiTietPhieuThues.Where(ct => ct.NgayTraThucTe == null).ToList();

            if (!sachChuaTra.Any()) return BadRequest("Khách hàng không có sách nào sắp trễ hạn.");

            string body = BuildReminderTemplate(phieu.KhachHang.HoTen, sachChuaTra, settingsDict);
            string tenQuan = settingsDict.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");

            await SendEmailHelper(phieu.KhachHang.Email, $"[{tenQuan}] Nhắc hạn trả sách (#{idPhieu})", body, settingsDict);

            return Ok(new { Message = "Đã gửi mail nhắc nhở." });
        }

        [HttpGet("print-data/{idPhieu}")]
        public async Task<IActionResult> GetPrintData(int idPhieu)
        {
            var phieu = await _context.PhieuThueSachs.AsNoTracking()
                .Include(p => p.KhachHang).Include(p => p.NhanVien).Include(p => p.ChiTietPhieuThues).ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(p => p.IdPhieuThueSach == idPhieu);
            if (phieu == null) return NotFound();

            var settings = await _context.CaiDats.AsNoTracking().ToListAsync();
            var settingsThue = await GetSettingsInternal();

            var dto = new PhieuThuePrintDto
            {
                IdPhieu = $"#{phieu.IdPhieuThueSach}",
                TenQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_TenQuan")?.GiaTri ?? "Cafebook",
                DiaChiQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_DiaChi")?.GiaTri ?? "N/A",
                SdtQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_SoDienThoai")?.GiaTri ?? "N/A",
                NgayTao = phieu.NgayThue,
                TenNhanVien = phieu.NhanVien?.HoTen ?? "N/A",
                TenKhachHang = phieu.KhachHang.HoTen,
                SdtKhachHang = phieu.KhachHang.SoDienThoai ?? "N/A",
                NgayHenTra = phieu.ChiTietPhieuThues.Min(ct => ct.NgayHenTra),

                ChiTiet = phieu.ChiTietPhieuThues.Select(ct => new ChiTietPrintDto
                {
                    TenSach = ct.Sach.TenSach,
                    DoMoi = ct.DoMoiKhiThue ?? 100,
                    GhiChu = string.IsNullOrWhiteSpace(ct.GhiChuKhiThue) ? "-" : ct.GhiChuKhiThue,
                    TienCoc = ct.TienCoc
                }).ToList(),

                TongTienCoc = phieu.TongTienCoc,
                TongPhiThue = phieu.ChiTietPhieuThues.Count * settingsThue.PhiThue
            };
            return Ok(dto);
        }

        [HttpGet("print-data/tra/{idPhieuTra}")]
        public async Task<IActionResult> GetPrintDataTra(int idPhieuTra)
        {
            var phieuTra = await _context.PhieuTraSachs.AsNoTracking()
                .Include(pt => pt.NhanVien).Include(pt => pt.PhieuThueSach.KhachHang).Include(pt => pt.PhieuThueSach.ChiTietPhieuThues)
                .Include(pt => pt.ChiTietPhieuTras).ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(pt => pt.IdPhieuTra == idPhieuTra);

            if (phieuTra == null) return NotFound();

            var settings = await _context.CaiDats.AsNoTracking().ToListAsync();
            var khach = phieuTra.PhieuThueSach?.KhachHang;

            var dto = new PhieuTraPrintDto
            {
                IdPhieuTra = $"#{phieuTra.IdPhieuTra}",
                IdPhieuThue = $"#{phieuTra.IdPhieuThueSach}",
                TenQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_TenQuan")?.GiaTri ?? "Cafebook",
                DiaChiQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_DiaChi")?.GiaTri ?? "N/A",
                SdtQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_SoDienThoai")?.GiaTri ?? "N/A",
                NgayTra = phieuTra.NgayTra,
                TenNhanVien = phieuTra.NhanVien?.HoTen ?? "N/A",
                TenKhachHang = khach?.HoTen ?? "N/A",
                SdtKhachHang = khach?.SoDienThoai ?? "N/A",
                DiemTichLuy = phieuTra.DiemTichLuy,

                ChiTiet = phieuTra.ChiTietPhieuTras.Select(ct => new ChiTietTraPrintDto
                {
                    TenSach = ct.Sach.TenSach,
                    DoMoi = ct.DoMoiKhiTra ?? 100,
                    GhiChu = string.IsNullOrWhiteSpace(ct.GhiChuKhiTra) ? "-" : ct.GhiChuKhiTra,
                    TienPhat = ct.TienPhat + (ct.TienPhatHuHong ?? 0),
                    TienCoc = phieuTra.PhieuThueSach?.ChiTietPhieuThues.FirstOrDefault(cts => cts.IdSach == ct.IdSach)?.TienCoc ?? 0
                }).ToList(),

                TongTienCoc = phieuTra.TongTienCocHoan,
                TongPhiThue = phieuTra.TongPhiThue,
                TongTienPhat = phieuTra.TongTienPhat,
                TongHoanTra = phieuTra.TongTienCocHoan - phieuTra.TongPhiThue - phieuTra.TongTienPhat
            };
            return Ok(dto);
        }

        private async Task<CaiDatThueSachDto> GetSettingsInternal()
        {
            var settings = await _context.CaiDats.AsNoTracking().Where(c => c.TenCaiDat.StartsWith("Sach_") || c.TenCaiDat.StartsWith("DiemTichLuy_") || c.TenCaiDat.StartsWith("NganHang_")).ToListAsync();
            return new CaiDatThueSachDto
            {
                PhiThue = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_PhiThue")?.GiaTri ?? "5000"),
                PhiTraTreMoiNgay = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_PhiTraTreMoiNgay")?.GiaTri ?? "2000"),
                SoNgayMuonToiDa = int.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_SoNgayMuonToiDa")?.GiaTri ?? "7"),
                DiemPhieuThue = int.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_DiemPhieuThue")?.GiaTri ?? "1"),
                PointToVND = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "DiemTichLuy_DoiVND")?.GiaTri ?? "1000"),
                BankId = settings.FirstOrDefault(c => c.TenCaiDat == "NganHang_MaDinhDanhNganHang")?.GiaTri ?? "",
                BankAccount = settings.FirstOrDefault(c => c.TenCaiDat == "NganHang_SoTaiKhoan")?.GiaTri ?? "",
                BankAccountName = settings.FirstOrDefault(c => c.TenCaiDat == "NganHang_ChuTaiKhoan")?.GiaTri ?? "",
                PhatGiamDoMoi1Percent = decimal.Parse(settings.FirstOrDefault(c => c.TenCaiDat == "Sach_PhatGiamDoMoi1Percent")?.GiaTri ?? "2000")
            };
        }

        private async Task<Dictionary<string, string>> GetGeneralSettingsAsync()
        {
            return await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
        }

        private async Task SendConfirmationEmailAsync(string email, string hoTen, List<ChiTietPrintDto> tenSachs, DateTime ngayHenTra, int idPhieu, decimal tongCoc, Dictionary<string, string> settings)
        {
            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

            decimal phiThue = decimal.Parse(settings.GetValueOrDefault("Sach_PhiThue", "5000"));
            decimal phiPhat = decimal.Parse(settings.GetValueOrDefault("Sach_PhiTraTreMoiNgay", "2000"));
            decimal tongPhiThue = phiThue * tenSachs.Count;

            string body = $@"
            <html>
            <body style=""font-family: Arial, sans-serif; background-color: #F7F3E9; padding: 20px;"">
                <div style=""max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);"">
                    <div style=""background: #5D4037; color: #FFF; padding: 25px; text-align: center;"">
                        <h2 style=""margin:0;"">☕ {tenQuan.ToUpper()}</h2>
                        <p style=""margin:5px 0 0 0; font-size:14px; opacity:0.8;"">HÓA ĐƠN THUÊ SÁCH ĐIỆN TỬ</p>
                    </div>
                    <div style=""padding: 30px;"">
                        <p>Chào <strong>{hoTen}</strong>, cảm ơn bạn đã sử dụng dịch vụ tại {tenQuan}.</p>
                        <div style=""background: #FFFDE7; border-left: 5px solid #FBC02D; padding: 15px; margin: 20px 0;"">
                            📝 Mã phiếu: <strong>#{idPhieu}</strong><br>
                            📅 Hạn trả: <strong style=""color: #D84315;"">{ngayHenTra:dd/MM/yyyy}</strong>
                        </div>
                        
                        <h4 style=""color: #5D4037; border-bottom: 1px solid #eee; padding-bottom: 5px;"">📚 CHI TIẾT SÁCH MƯỢN</h4>
                        <div style=""background: #FAFAFA; padding: 15px; border-radius: 8px; border: 1px solid #E0E0E0; margin-bottom: 20px;"">
                            {string.Join("", tenSachs.Select(s => $@"
                                <div style='margin-bottom: 5px; padding-bottom: 5px; border-bottom: 1px dashed #ccc;'>
                                    <strong>📖 {s.TenSach}</strong><br>
                                    <small style='color: #666;'>Độ mới: {s.DoMoi}% | Ghi chú: {(string.IsNullOrWhiteSpace(s.GhiChu) ? "-" : s.GhiChu)}</small>
                                </div>"))}
                        </div>

                        <h4 style=""color: #5D4037; border-bottom: 1px solid #eee; padding-bottom: 5px;"">💰 THÔNG TIN THANH TOÁN</h4>
                        <table style=""width: 100%; border-collapse: collapse; margin-bottom: 20px;"">
                            <tr>
                                <td style=""padding: 8px 0; color: #555;"">Số lượng sách:</td>
                                <td style=""padding: 8px 0; text-align: right; font-weight: bold;"">{tenSachs.Count} cuốn</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; color: #555;"">Đơn giá thuê:</td>
                                <td style=""padding: 8px 0; text-align: right;"">{phiThue:N0} đ/cuốn</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; color: #555;"">Tổng phí thuê (tạm tính):</td>
                                <td style=""padding: 8px 0; text-align: right; color: #1976D2; font-weight: bold;"">{tongPhiThue:N0} đ</td>
                            </tr>
                            <tr style=""border-top: 2px solid #eee;"">
                                <td style=""padding: 12px 0; color: #333; font-weight: bold; font-size: 16px;"">TỔNG TIỀN CỌC ĐÃ NHẬN:</td>
                                <td style=""padding: 12px 0; text-align: right; color: #388E3C; font-weight: bold; font-size: 16px;"">{tongCoc:N0} đ</td>
                            </tr>
                        </table>

                        <div style=""background: #E3F2FD; border-radius: 8px; padding: 15px; font-size: 13px; color: #0D47A1;"">
                            <strong style=""display:block; margin-bottom: 5px;"">📌 ĐIỀU KHOẢN MƯỢN SÁCH:</strong>
                            - Phí thuê <b>{phiThue:N0} đ/cuốn</b> sẽ được trừ vào tiền cọc khi trả sách.<br>
                            - Vui lòng trả sách đúng hạn trước ngày <b>{ngayHenTra:dd/MM/yyyy}</b>.<br>
                            - Quá hạn sẽ tính phí phạt <b>{phiPhat:N0} đ/ngày/cuốn</b>.<br>
                            - Quán sẽ kiểm tra đánh giá độ mới của sách khi trả. Nếu bị giảm % so với lúc thuê, mức phạt khấu hao là <b>2.000 đ/1%</b> giảm.<br>
                            - Số tiền cọc còn lại (sau khi trừ phí thuê và phí phạt) sẽ được hoàn trả đầy đủ cho quý khách.
                        </div>
                    </div>
                    <div style=""background: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6;"">
                        <strong>Đội ngũ {tenQuan}</strong><br>
                        📍 Địa chỉ: {diaChiQuan}<br>
                        📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                        © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                    </div>
                </div>
            </body>
            </html>";

            await SendEmailHelper(email, $"[{tenQuan}] Hóa đơn & Xác nhận thuê sách (#{idPhieu})", body, settings);
        }

        private async Task SendReturnEmailAsync(string email, string hoTen, int idPhieuTra, int idPhieuThue, decimal phiThue, decimal tienPhat, decimal hoanTra, int diemCung, List<string> dsSachTra, Dictionary<string, string> settings)
        {
            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

            string sachHtml = string.Join("", dsSachTra);

            string body = $@"
            <html>
            <body style=""font-family: Arial, sans-serif; background-color: #F7F3E9; padding: 20px;"">
                <div style=""max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden;"">
                    <div style=""background: #5D4037; color: #FFF; padding: 25px; text-align: center;""><h2>☕ {tenQuan.ToUpper()}</h2></div>
                    <div style=""padding: 30px;"">
                        <h3 style=""color: #2E7D32;"">✅ Trả sách thành công!</h3>
                        <p>Chào <strong>{hoTen}</strong>, cảm ơn bạn đã trả sách tại {tenQuan}.</p>
                        <div style=""background: #E8F5E9; border-left: 5px solid #4CAF50; padding: 15px; margin: 20px 0;"">
                            Mã phiếu trả: <strong>#{idPhieuTra}</strong> (Thuộc phiếu: #{idPhieuThue})<br>
                            Thời gian trả: {DateTime.Now:dd/MM/yyyy HH:mm}
                        </div>
                        <p><strong>📚 Sách đã trả:</strong></p>
                        <ul style=""list-style: none; padding: 0;"">
                            {sachHtml}
                        </ul>
                        <div style=""background: #FAFAFA; padding: 15px; border-radius: 8px; border: 1px solid #E0E0E0; margin-top: 15px;"">
                            <strong>💰 Chi tiết thanh toán:</strong><br>
                            - Phí thuê: {phiThue:N0} đ<br>
                            - Tổng tiền phạt (Gồm Phạt trễ & Khấu hao): {tienPhat:N0} đ<br>
                            - <strong style=""color: #D84315;"">Tiền cọc hoàn trả: {hoanTra:N0} đ</strong><br>
                            - Điểm tích lũy cộng thêm: <strong style=""color: #4CAF50;"">+{diemCung} điểm</strong>
                        </div>
                    </div>
                    <div style=""background: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6;"">
                        <strong>Đội ngũ {tenQuan}</strong><br>
                        📍 Địa chỉ: {diaChiQuan}<br>
                        📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                        © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                    </div>
                </div>
            </body>
            </html>";

            await SendEmailHelper(email, $"[{tenQuan}] Xác nhận trả sách thành công (#{idPhieuTra})", body, settings);
        }

        private string BuildReminderTemplate(string hoTen, List<ChiTietPhieuThue> dsSach, Dictionary<string, string> settings)
        {
            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

            return $@"
        <html>
        <body style=""font-family: Arial, sans-serif; background-color: #F7F3E9; padding: 20px;"">
            <div style=""max-width: 600px; margin: 0 auto; background: #fff; border-radius: 12px; overflow: hidden;"">
                <div style=""background: #D84315; color: #FFF; padding: 25px; text-align: center;""><h2>☕ {tenQuan.ToUpper()} REMINDER</h2></div>
                <div style=""padding: 30px;"">
                    <p>Xin chào <strong>{hoTen}</strong>,</p>
                    <p>{tenQuan} nhắc nhẹ bạn các cuốn sách sau sắp/đã đến hạn trả:</p>
                    <div style=""background: #FFF3E0; border-left: 5px solid #D84315; padding: 15px; margin: 20px 0;"">
                        {string.Join("", dsSach.Select(s => $@"
                            <div style='border-bottom: 1px solid #EEE; padding: 10px 0;'>
                                <strong>📖 {s.Sach.TenSach}</strong><br>
                                <small style='color: #666;'>Độ mới lúc thuê: {s.DoMoiKhiThue ?? 100}% | Ghi chú: {(string.IsNullOrWhiteSpace(s.GhiChuKhiThue) ? "-" : s.GhiChuKhiThue)}</small><br>
                                <small style='color: #D84315;'>Hạn trả: {s.NgayHenTra:dd/MM/yyyy}</small>
                            </div>"))}
                    </div>
                </div>
                <div style=""background: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6;"">
                    <strong>Đội ngũ {tenQuan}</strong><br>
                    📍 Địa chỉ: {diaChiQuan}<br>
                    📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                    © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                </div>
            </div>
        </body>
        </html>";
        }

        private async Task SendEmailHelper(string emailTo, string subject, string htmlBody, Dictionary<string, string> settings)
        {
            try
            {
                string host = settings.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
                string username = settings.GetValueOrDefault("Smtp_Username", "");
                string password = settings.GetValueOrDefault("Smtp_Password", "");
                string fromName = settings.GetValueOrDefault("Smtp_FromName", "Cafebook Hỗ Trợ");
                int port = int.TryParse(settings.GetValueOrDefault("Smtp_Port", "587"), out int p) ? p : 587;
                bool enableSsl = bool.TryParse(settings.GetValueOrDefault("Smtp_EnableSsl", "true"), out bool s) ? s : true;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;

                var mailMessage = new MailMessage { From = new MailAddress(username, fromName), Subject = subject, Body = htmlBody, IsBodyHtml = true };
                mailMessage.To.Add(new MailAddress(emailTo));

                using var smtpClient = new SmtpClient(host, port) { Credentials = new NetworkCredential(username, password), EnableSsl = enableSsl };
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi mail hệ thống: {ex.Message}");
            }
        }
    }
}