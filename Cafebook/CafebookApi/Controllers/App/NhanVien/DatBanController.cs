using CafebookApi.Data;
using CafebookModel.Model.ModelApp.NhanVien.DatBan;
using CafebookModel.Model.ModelEntities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Text.RegularExpressions;

namespace AppCafebookApi.Controllers.app.NhanVien
{
    [Route("api/app/datban")]
    [ApiController]
    [Authorize]
    public class DatBanController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IConfiguration _config;

        private const int ReservationSlotHours = 2;
        private const int ReservationBufferMinutes = 5;

        private class OpeningHours
        {
            public TimeSpan Open { get; set; } = new TimeSpan(6, 0, 0);
            public TimeSpan Close { get; set; } = new TimeSpan(23, 0, 0);
            public bool IsValid { get; set; } = false;
        }

        public DatBanController(CafebookDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        #region GET Endpoints
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<PhieuDatBanDto>>> GetDatBanList()
        {

            var list = await _context.PhieuDatBans
                .AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.Ban).ThenInclude(b => b.KhuVuc)
                .OrderByDescending(p => p.ThoiGianDat)
                .Select(p => new PhieuDatBanDto
                {
                    IdPhieuDatBan = p.IdPhieuDatBan,
                    TenKhachHang = p.KhachHang != null ? p.KhachHang.HoTen : p.HoTenKhach,
                    SoDienThoai = p.KhachHang != null ? p.KhachHang.SoDienThoai : p.SdtKhach,
                    Email = p.KhachHang != null ? p.KhachHang.Email : null,
                    IdBan = p.IdBan,
                    SoBan = p.Ban.SoBan,
                    SoGhe = p.Ban.SoGhe,
                    TenKhuVuc = p.Ban.KhuVuc != null ? p.Ban.KhuVuc.TenKhuVuc : "N/A",
                    ThoiGianDat = p.ThoiGianDat,
                    SoLuongKhach = p.SoLuongKhach,
                    TrangThai = p.TrangThai,
                    GhiChu = p.GhiChu,
                    IdKhachHang = p.IdKhachHang
                }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("available-bans")]
        public async Task<ActionResult<IEnumerable<BanDatBanDto>>> GetAvailableBans()
        {
            var bans = await _context.Bans
                .AsNoTracking()
                .Include(b => b.KhuVuc)
                .Where(b => b.TrangThai == "Trống" || b.TrangThai == "Đã đặt")
                .Select(b => new BanDatBanDto
                {
                    IdBan = b.IdBan,
                    SoBan = b.SoBan,
                    TenKhuVuc = b.KhuVuc != null ? b.KhuVuc.TenKhuVuc : "N/A",
                    SoGhe = b.SoGhe,
                    IdKhuVuc = b.IdKhuVuc
                }).ToListAsync();
            return Ok(bans);
        }

        [HttpGet("khuvuc")]
        public async Task<ActionResult<IEnumerable<KhuVucDatBanDto>>> GetKhuVucList()
        {
            var list = await _context.KhuVucs
                .AsNoTracking()
                .Select(k => new KhuVucDatBanDto
                {
                    IdKhuVuc = k.IdKhuVuc,
                    TenKhuVuc = k.TenKhuVuc
                }).ToListAsync();

            return Ok(list);
        }

        [HttpGet("search-customer")]
        public async Task<ActionResult<IEnumerable<KhachHangLookupDto>>> SearchCustomer([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query)) return Ok(new List<KhachHangLookupDto>());
            var queryLower = query.ToLower();
            var results = await _context.KhachHangs
                .AsNoTracking()
                .Where(kh => (kh.SoDienThoai != null && kh.SoDienThoai.Contains(query)) ||
                             (kh.Email != null && kh.Email.ToLower().Contains(queryLower)) ||
                             kh.HoTen.ToLower().Contains(queryLower))
                .Select(kh => new KhachHangLookupDto
                {
                    IdKhachHang = kh.IdKhachHang,
                    HoTen = kh.HoTen,
                    SoDienThoai = kh.SoDienThoai ?? "",
                    Email = kh.Email
                })
                .Take(10).ToListAsync();
            return Ok(results);
        }

        [HttpGet("opening-hours")]
        public async Task<ActionResult<string>> GetOpeningHours()
        {
            var openSetting = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(cd => cd.TenCaiDat == "ThongTin_GioMoCua");
            var closeSetting = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(cd => cd.TenCaiDat == "ThongTin_GioDongCua");

            string openTime = openSetting?.GiaTri ?? "07:00";
            string closeTime = closeSetting?.GiaTri ?? "22:00";

            return Ok($"{openTime} - {closeTime}");
        }
        #endregion

        #region POST/PUT Endpoints 

        [HttpPost("create-staff")]
        public async Task<IActionResult> CreateDatBanStaff(PhieuDatBanCreateUpdateDto dto)
        {
            KhachHang? khachHang = null;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.TenKhachHang) || string.IsNullOrWhiteSpace(dto.SoDienThoai))
                {
                    return BadRequest("Tên khách hàng và Số điện thoại là bắt buộc.");
                }

                string? sdt = dto.SoDienThoai;
                string? email = dto.Email;

                if (!string.IsNullOrWhiteSpace(sdt)) { khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt); }
                if (khachHang == null && !string.IsNullOrWhiteSpace(email)) { khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == email); }

                if (khachHang == null)
                {
                    if (!string.IsNullOrWhiteSpace(sdt) && await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdt)) return Conflict("Số điện thoại này đã tồn tại.");
                    if (!string.IsNullOrWhiteSpace(email) && await _context.KhachHangs.AnyAsync(k => k.Email == email)) return Conflict("Email này đã tồn tại.");

                    string tenDangNhap = !string.IsNullOrWhiteSpace(sdt) ? sdt : (!string.IsNullOrWhiteSpace(email) ? email : $"temp_{Guid.NewGuid().ToString("N")[..12]}");

                    var newKH = new KhachHang
                    {
                        HoTen = dto.TenKhachHang,
                        SoDienThoai = sdt,
                        Email = email,
                        NgayTao = DateTime.Now,
                        DiemTichLuy = 0,
                        BiKhoa = false,
                        TenDangNhap = tenDangNhap,
                        MatKhau = "123456",
                        TaiKhoanTam = true
                    };
                    _context.KhachHangs.Add(newKH);
                    await _context.SaveChangesAsync();
                    khachHang = newKH;
                }
                else
                {
                    khachHang.HoTen = dto.TenKhachHang;
                    khachHang.Email = email;
                }

                var ban = await _context.Bans.FindAsync(dto.IdBan);
                if (ban == null) return BadRequest("Bàn không tồn tại.");
                if (ban.TrangThai == "Có khách") return BadRequest("Bàn đang có khách, không thể đặt.");

                var openingHours = await GetAndParseOpeningHours();
                if (!IsTimeValid(dto.ThoiGianDat, openingHours))
                {
                    return BadRequest($"Giờ đặt ({dto.ThoiGianDat:HH:mm}) nằm ngoài giờ mở cửa ({openingHours.Open:hh\\:mm} - {openingHours.Close:hh\\:mm}).");
                }
                if (dto.SoLuongKhach > ban.SoGhe)
                {
                    return BadRequest($"Số lượng khách ({dto.SoLuongKhach}) vượt quá số ghế của bàn ({ban.SoGhe}).");
                }

                DateTime newSlotStart = dto.ThoiGianDat;
                DateTime newSlotEnd = dto.ThoiGianDat.AddHours(ReservationSlotHours);

                var conflict = await _context.PhieuDatBans
                    .Where(p => p.IdBan == dto.IdBan && (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận"))
                    .ToListAsync();

                foreach (var p in conflict)
                {
                    DateTime existingSlotStart = p.ThoiGianDat.AddMinutes(-ReservationBufferMinutes);
                    DateTime existingSlotEnd = p.ThoiGianDat.AddHours(ReservationSlotHours).AddMinutes(ReservationBufferMinutes);

                    if (newSlotStart < existingSlotEnd && existingSlotStart < newSlotEnd)
                    {
                        return Conflict($"Xung đột! Bàn này đã có phiếu đặt lúc {p.ThoiGianDat:HH:mm} (có 5 phút đệm).");
                    }
                }

                var phieu = new PhieuDatBan
                {
                    IdKhachHang = khachHang?.IdKhachHang,
                    IdBan = dto.IdBan,
                    HoTenKhach = dto.TenKhachHang,
                    SdtKhach = dto.SoDienThoai,
                    ThoiGianDat = dto.ThoiGianDat,
                    SoLuongKhach = dto.SoLuongKhach,
                    GhiChu = dto.GhiChu,
                    TrangThai = dto.TrangThai
                };
                _context.PhieuDatBans.Add(phieu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Gửi Mail Xác Nhận (Chỉ giữ lại mail này)
                var emailNguoiNhan = khachHang?.Email ?? dto.Email;
                if (!string.IsNullOrEmpty(emailNguoiNhan) && dto.TrangThai == "Đã xác nhận")
                {
                    var smtp = await GetSmtpSettingsAsync();
                    var settingsDict = await GetGeneralSettingsAsync();
                    if (!string.IsNullOrEmpty(smtp.username) && !string.IsNullOrEmpty(smtp.password))
                    {
                        var khachInfo = new KhachHang { HoTen = dto.TenKhachHang, Email = emailNguoiNhan };
                        var phieuMail = new PhieuDatBan { ThoiGianDat = phieu.ThoiGianDat, SoLuongKhach = phieu.SoLuongKhach, GhiChu = phieu.GhiChu, TrangThai = phieu.TrangThai };
                        string soBanMail = ban.SoBan;

                        _ = Task.Run(() => SendConfirmationEmailAsync(phieuMail, khachInfo, soBanMail, smtp.host, smtp.port, smtp.username, smtp.password, smtp.fromName, settingsDict));
                    }
                }
                return Ok(new { idPhieuDatBan = phieu.IdPhieuDatBan });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi nội bộ: {ex.Message} \nChi tiết: {ex.InnerException?.Message}");
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDatBan(int id, PhieuDatBanCreateUpdateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phieu = await _context.PhieuDatBans
                    .Include(p => p.KhachHang)
                    .Include(p => p.Ban)
                    .FirstOrDefaultAsync(p => p.IdPhieuDatBan == id);

                if (phieu == null) return NotFound("Không tìm thấy phiếu đặt.");

                var openingHours = await GetAndParseOpeningHours();
                if (!IsTimeValid(dto.ThoiGianDat, openingHours))
                {
                    return BadRequest($"Giờ đặt ({dto.ThoiGianDat:HH:mm}) nằm ngoài giờ mở cửa ({openingHours.Open:hh\\:mm} - {openingHours.Close:hh\\:mm}).");
                }

                string oldTrangThai = phieu.TrangThai;

                KhachHang? khachHang = null;
                if (string.IsNullOrWhiteSpace(dto.TenKhachHang) || string.IsNullOrWhiteSpace(dto.SoDienThoai))
                {
                    return BadRequest("Tên khách hàng và Số điện thoại là bắt buộc.");
                }
                string? sdt = dto.SoDienThoai;
                string? email = dto.Email;

                if (!string.IsNullOrWhiteSpace(sdt)) { khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt); }
                if (khachHang == null && !string.IsNullOrWhiteSpace(email)) { khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == email); }

                if (khachHang == null)
                {
                    if (!string.IsNullOrWhiteSpace(sdt) && await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdt)) { return Conflict("Số điện thoại này đã tồn tại."); }
                    if (!string.IsNullOrWhiteSpace(email) && await _context.KhachHangs.AnyAsync(k => k.Email == email)) { return Conflict("Email này đã tồn tại."); }

                    string tenDangNhap = !string.IsNullOrWhiteSpace(sdt) ? sdt : (!string.IsNullOrWhiteSpace(email) ? email : $"temp_{Guid.NewGuid().ToString("N")[..12]}");

                    var newKH = new KhachHang
                    {
                        HoTen = dto.TenKhachHang,
                        SoDienThoai = sdt,
                        Email = email,
                        NgayTao = DateTime.Now,
                        DiemTichLuy = 0,
                        BiKhoa = false,
                        TenDangNhap = tenDangNhap,
                        MatKhau = "123456",
                        TaiKhoanTam = true
                    };
                    _context.KhachHangs.Add(newKH);
                    await _context.SaveChangesAsync();
                    khachHang = newKH;
                }
                else
                {
                    khachHang.HoTen = dto.TenKhachHang;
                    khachHang.Email = email;
                }

                phieu.IdKhachHang = khachHang?.IdKhachHang;
                phieu.HoTenKhach = dto.TenKhachHang;
                phieu.SdtKhach = dto.SoDienThoai;

                if (phieu.IdBan != dto.IdBan)
                {
                    var oldBan = await _context.Bans.FindAsync(phieu.IdBan);
                    if (oldBan != null) oldBan.TrangThai = "Trống";
                    var newBan = await _context.Bans.FindAsync(dto.IdBan);
                    if (newBan == null) return BadRequest("Bàn mới không tồn tại.");
                    if (newBan.TrangThai == "Có khách") return BadRequest("Bàn mới đang có khách.");
                    if (dto.SoLuongKhach > newBan.SoGhe)
                    {
                        return BadRequest($"Số lượng khách ({dto.SoLuongKhach}) vượt quá số ghế của bàn mới ({newBan.SoGhe}).");
                    }
                    phieu.IdBan = dto.IdBan;
                }

                DateTime newSlotStart = dto.ThoiGianDat;
                DateTime newSlotEnd = dto.ThoiGianDat.AddHours(ReservationSlotHours);
                var conflict = await _context.PhieuDatBans
                    .Where(p => p.IdBan == dto.IdBan && p.IdPhieuDatBan != id && (p.TrangThai == "Đã xác nhận" || p.TrangThai == "Chờ xác nhận"))
                    .ToListAsync();

                foreach (var p in conflict)
                {
                    DateTime existingSlotStart = p.ThoiGianDat.AddMinutes(-ReservationBufferMinutes);
                    DateTime existingSlotEnd = p.ThoiGianDat.AddHours(ReservationSlotHours).AddMinutes(ReservationBufferMinutes);
                    if (newSlotStart < existingSlotEnd && existingSlotStart < newSlotEnd)
                    {
                        return Conflict($"Xung đột! Bàn này đã có phiếu đặt lúc {p.ThoiGianDat:HH:mm} (có 5 phút đệm).");
                    }
                }

                phieu.ThoiGianDat = dto.ThoiGianDat;
                phieu.SoLuongKhach = dto.SoLuongKhach;
                phieu.GhiChu = dto.GhiChu;
                phieu.TrangThai = dto.TrangThai;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Gửi Mail khi được duyệt thành "Đã xác nhận"
                if (oldTrangThai == "Chờ xác nhận" && dto.TrangThai == "Đã xác nhận")
                {
                    var emailNguoiNhan = khachHang?.Email ?? dto.Email;
                    if (!string.IsNullOrEmpty(emailNguoiNhan) && phieu.Ban != null)
                    {
                        var smtp = await GetSmtpSettingsAsync();
                        var settingsDict = await GetGeneralSettingsAsync();
                        if (!string.IsNullOrEmpty(smtp.username) && !string.IsNullOrEmpty(smtp.password))
                        {
                            var khachInfo = new KhachHang { HoTen = dto.TenKhachHang, Email = emailNguoiNhan };
                            var phieuMail = new PhieuDatBan { ThoiGianDat = phieu.ThoiGianDat, SoLuongKhach = phieu.SoLuongKhach, GhiChu = phieu.GhiChu, TrangThai = phieu.TrangThai };
                            string soBanMail = phieu.Ban.SoBan;

                            _ = Task.Run(() => SendConfirmationEmailAsync(phieuMail, khachInfo, soBanMail, smtp.host, smtp.port, smtp.username, smtp.password, smtp.fromName, settingsDict));
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi nội bộ: {ex.Message} \nChi tiết: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("xacnhan-den")]
        public async Task<ActionResult<XacNhanKhachDenResponseDto>> XacNhanKhachDen(XacNhanKhachDenRequestDto dto)
        {
            var phieu = await _context.PhieuDatBans
                .Include(p => p.Ban)
                .FirstOrDefaultAsync(p => p.IdPhieuDatBan == dto.IdPhieuDatBan);

            if (phieu == null) return NotFound("Phiếu đặt không tồn tại.");
            if (phieu.Ban == null) return BadRequest("Bàn không hợp lệ.");
            var ban = phieu.Ban;
            if (ban.TrangThai == "Có khách")
            {
                return BadRequest($"Bàn {ban.SoBan} đã có khách. Vui lòng kiểm tra lại.");
            }
            var hoaDon = new HoaDon
            {
                IdBan = phieu.IdBan,
                IdNhanVien = dto.IdNhanVien,
                IdKhachHang = phieu.IdKhachHang,
                ThoiGianTao = DateTime.Now,
                TrangThai = "Chưa thanh toán",
                LoaiHoaDon = "Tại quán",
                TongTienGoc = 0,
                GiamGia = 0,
                TongPhuThu = 0
            };
            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();
            phieu.TrangThai = "Khách đã đến";
            ban.TrangThai = "Có khách";
            await _context.SaveChangesAsync();
            return Ok(new XacNhanKhachDenResponseDto { IdHoaDon = hoaDon.IdHoaDon });
        }

        [HttpPost("huy/{id}")]
        public async Task<IActionResult> HuyDatBan(int id)
        {
            var phieu = await _context.PhieuDatBans.Include(p => p.Ban)
                        .FirstOrDefaultAsync(p => p.IdPhieuDatBan == id);
            if (phieu == null) return NotFound("Phiếu đặt không tồn tại.");
            if (phieu.TrangThai == "Đã hủy" || phieu.TrangThai == "Khách đã đến")
            {
                return BadRequest("Không thể hủy phiếu ở trạng thái này.");
            }
            phieu.TrangThai = "Đã hủy";
            if (phieu.Ban != null)
            {
                phieu.Ban.TrangThai = "Trống";
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        #endregion

        #region Helpers (Khách hàng, Email, Giờ)

        private async Task<Dictionary<string, string>> GetGeneralSettingsAsync()
        {
            return await _context.CaiDats.AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
        }

        private async Task<(string host, int port, string username, string password, string fromName)> GetSmtpSettingsAsync()
        {
            var smtpHost = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Host");
            var smtpPort = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Port");
            var smtpUser = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Username");
            var smtpPass = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_Password");
            var smtpName = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "Smtp_FromName");

            string host = smtpHost?.GiaTri ?? "smtp.gmail.com";
            int port = int.TryParse(smtpPort?.GiaTri, out int p) ? p : 587;
            string username = smtpUser?.GiaTri ?? "";
            string password = smtpPass?.GiaTri ?? "";
            string fromName = smtpName?.GiaTri ?? "Cafebook Hỗ Trợ";

            return (host, port, username, password, fromName);
        }

        // HÀM EMAIL: CHỈ GIỮ LẠI XÁC NHẬN ĐẶT BÀN
        private async Task SendConfirmationEmailAsync(PhieuDatBan phieu, KhachHang khach, string soBan, string host, int port, string username, string password, string fromName, Dictionary<string, string> settings)
        {
            string toEmail = khach.Email ?? "";
            if (string.IsNullOrEmpty(toEmail)) return;

            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, username));
                email.To.Add(new MailboxAddress(khach.HoTen, toEmail));
                email.Subject = $"[{tenQuan}] Xác nhận đặt bàn thành công";

                string body = $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F7F3E9; color: #4E342E; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #5D4037; color: #FFF3E0; padding: 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 26px; letter-spacing: 2px; text-transform: uppercase; }}
                        .content {{ padding: 30px; line-height: 1.6; }}
                        .info-card {{ background-color: #FFFDE7; border-left: 6px solid #D84315; border-radius: 8px; padding: 20px; margin: 25px 0; }}
                        .footer {{ background-color: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6; border-top: 1px solid #D7CCC8; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>☕ {tenQuan}</h1>
                        </div>
                        <div class=""content"">
                            <div style=""font-size: 18px; font-weight: bold; margin-bottom: 20px;"">Xin chào {khach.HoTen},</div>
                            <p>Cảm ơn bạn đã tin tưởng lựa chọn <strong>{tenQuan}</strong>. Yêu cầu đặt bàn của bạn đã được ghi nhận thành công với các thông tin chi tiết dưới đây:</p>
                            
                            <div class=""info-card"">
                                <div style=""margin-bottom: 12px;""><span style=""color: #D84315; font-weight: 600;"">🪑 Bàn số:</span> <strong>{soBan}</strong></div>
                                <div style=""margin-bottom: 12px;""><span style=""color: #D84315; font-weight: 600;"">📅 Thời gian:</span> <strong>{phieu.ThoiGianDat:HH:mm} ngày {phieu.ThoiGianDat:dd/MM/yyyy}</strong></div>
                                <div style=""margin-bottom: 12px;""><span style=""color: #D84315; font-weight: 600;"">👥 Số khách:</span> <strong>{phieu.SoLuongKhach} người</strong></div>
                                <div><span style=""color: #D84315; font-weight: 600;"">📝 Ghi chú:</span> <strong>{phieu.GhiChu ?? "Không có"}</strong></div>
                            </div>
                            <p>Rất mong được đón tiếp bạn tại không gian ấm cúng của quán!</p>
                        </div>
                        <div class=""footer"">
                            <strong>Đội ngũ {tenQuan}</strong><br>
                            📍 Địa chỉ: {diaChiQuan}<br>
                            📞 Hotline: {supportPhone} | ✉️ {supportEmail}<br>
                            © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                        </div>
                    </div>
                </body>
                </html>";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email (chạy ngầm): {ex.Message}");
            }
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
            return timeOfDay >= hours.Open && timeOfDay <= hours.Close;
        }
        #endregion
    }
}