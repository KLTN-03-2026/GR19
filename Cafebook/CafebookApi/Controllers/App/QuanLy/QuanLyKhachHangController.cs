using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-khachhang")]
    [ApiController]
    public class QuanLyKhachHangController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyKhachHangController(CafebookDbContext context)
        {
            _context = context;
        }

        private async Task AutoUnlockCheckAsync()
        {
            var expired = await _context.Set<KhachHang>()
                .Where(k => k.BiKhoa && k.ThoiGianMoKhoa.HasValue && k.ThoiGianMoKhoa.Value <= DateTime.Now && !k.DaXoa)
                .ToListAsync();
            if (expired.Any())
            {
                foreach (var kh in expired) { kh.BiKhoa = false; kh.LyDoKhoa = null; kh.ThoiGianMoKhoa = null; }
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await AutoUnlockCheckAsync();

            var data = await _context.Set<KhachHang>().AsNoTracking()
                .Where(k => !k.DaXoa)
                .OrderByDescending(k => k.IdKhachHang)
                .Select(k => new QuanLyKhachHangGridDto
                {
                    IdKhachHang = k.IdKhachHang,
                    HoTen = k.HoTen,
                    SoDienThoai = k.SoDienThoai ?? "N/A",
                    Email = k.Email ?? "N/A",
                    NgayTao = k.NgayTao,
                    DiemTichLuy = k.DiemTichLuy,
                    BiKhoa = k.BiKhoa,
                    TaiKhoanTam = k.TaiKhoanTam,
                    LoaiTaiKhoan = k.TaiKhoanTam ? "Khách vãng lai" : "Thành viên",
                    TrangThai = k.BiKhoa ? "Đã khóa" : "Hoạt động"
                }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            await AutoUnlockCheckAsync();
            var kh = await _context.Set<KhachHang>().AsNoTracking().FirstOrDefaultAsync(k => k.IdKhachHang == id && !k.DaXoa);
            if (kh == null) return NotFound();

            return Ok(new QuanLyKhachHangDetailDto
            {
                IdKhachHang = kh.IdKhachHang,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi,
                DiemTichLuy = kh.DiemTichLuy,
                TenDangNhap = kh.TenDangNhap,
                BiKhoa = kh.BiKhoa,
                LyDoKhoa = kh.LyDoKhoa,
                ThoiGianMoKhoa = kh.ThoiGianMoKhoa,
                TaiKhoanTam = kh.TaiKhoanTam,
                AnhDaiDien = kh.AnhDaiDien,
                NgayTao = kh.NgayTao
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var kh = await _context.Set<KhachHang>().FindAsync(id);
            if (kh == null || kh.DaXoa) return NotFound();
            kh.DaXoa = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/diem")]
        public async Task<IActionResult> UpdateDiem(int id, [FromBody] CapNhatDiemKhachHangDto req)
        {
            var kh = await _context.Set<KhachHang>().FindAsync(id);
            if (kh == null || kh.DaXoa) return NotFound();

            kh.DiemTichLuy += req.DiemThayDoi;
            if (kh.DiemTichLuy < 0) kh.DiemTichLuy = 0;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/khoa")]
        public async Task<IActionResult> LockAccount(int id, [FromBody] KhoaKhachHangRequestDto req)
        {
            var kh = await _context.Set<KhachHang>().FindAsync(id);
            if (kh == null || kh.DaXoa) return NotFound();

            kh.BiKhoa = true;
            kh.LyDoKhoa = req.LyDoKhoa;
            kh.ThoiGianMoKhoa = req.SoNgayKhoa.HasValue ? DateTime.Now.AddDays(req.SoNgayKhoa.Value) : null;

            var settings = await _context.Set<CaiDat>().AsNoTracking().ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(kh.Email))
            {
                string thoiGianStr = kh.ThoiGianMoKhoa.HasValue ? kh.ThoiGianMoKhoa.Value.ToString("dd/MM/yyyy HH:mm") : "Vĩnh viễn";

                // Lấy thông tin liên hệ từ bảng CaiDat (có giá trị mặc định phòng trường hợp chưa có trong DB)
                string supportEmail = settings.ContainsKey("LienHe_Email") ? settings["LienHe_Email"] : "cafebook.hotro@gmail.com";
                string supportPhone = settings.ContainsKey("ThongTin_SoDienThoai") ? settings["ThongTin_SoDienThoai"] : "Đang cập nhật";

                // Giao diện email thiết kế lại với phong cách Material Design - Cafebook
                // Giao diện email fix lỗi hiển thị text icon
                string body = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <link href=""https://fonts.googleapis.com/css2?family=Roboto:wght@400;500;700&display=swap"" rel=""stylesheet"">
    <style>
        body {{
            font-family: 'Roboto', Arial, sans-serif;
            background-color: #F7F3E9;
            color: #4E342E;
            margin: 0;
            padding: 40px 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #FFFFFF;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0,0,0,0.08);
        }}
        .email-header {{
            background-color: #5D4037;
            color: #FFF3E0;
            padding: 30px 20px;
            text-align: center;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 700;
            letter-spacing: 1px;
        }}
        .email-body {{
            padding: 30px;
            line-height: 1.6;
            font-size: 16px;
        }}
        .alert-card {{
            background-color: #FFF8E1;
            border-left: 5px solid #D84315;
            border-radius: 4px;
            padding: 20px;
            margin: 20px 0;
        }}
        .detail-row {{
            margin-bottom: 12px;
            display: flex;
            align-items: flex-start;
        }}
        .detail-row:last-child {{
            margin-bottom: 0;
        }}
        .icon-label {{
            color: #D84315;
            font-weight: 500;
            margin-right: 8px;
            display: inline-flex;
            align-items: center;
        }}
        .emoji-icon {{
            font-size: 18px;
            margin-right: 6px;
        }}
        .detail-value {{
            color: #3E2723;
            font-weight: 700;
        }}
        .contact-box {{
            background-color: #FAFAFA;
            border: 1px solid #E0E0E0;
            border-radius: 8px;
            padding: 15px;
            margin-top: 20px;
            text-align: center;
        }}
        .contact-box p {{
            margin: 5px 0;
            color: #5D4037;
        }}
        .email-footer {{
            background-color: #EFEBE9;
            padding: 20px;
            text-align: center;
            font-size: 13px;
            color: #8D6E63;
            border-top: 1px solid #D7CCC8;
        }}
        .btn-contact {{
            display: inline-block;
            margin-top: 15px;
            padding: 12px 24px;
            background-color: #D84315;
            color: #FFFFFF !important;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 500;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <h1>☕ CAFEBOOK</h1>
        </div>
        <div class=""email-body"">
            <p>Xin chào bạn,</p>
            <p>Chúng tôi rất tiếc phải thông báo rằng tài khoản Cafebook của bạn hiện đang bị tạm khóa. Dưới đây là thông tin chi tiết:</p>
            
            <div class=""alert-card"">
                <div class=""detail-row"">
                    <span class=""icon-label"">
                        <span class=""emoji-icon"">ℹ️</span> Lý do:
                    </span> 
                    <span class=""detail-value"">{req.LyDoKhoa}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""icon-label"">
                        <span class=""emoji-icon"">⏱️</span> Mở khóa dự kiến:
                    </span> 
                    <span class=""detail-value"">{thoiGianStr}</span>
                </div>
            </div>

            <p>Nếu bạn cho rằng đây là một sự nhầm lẫn hoặc cần hỗ trợ thêm, vui lòng liên hệ với bộ phận CSKH của chúng tôi để được giải đáp qua các kênh dưới đây:</p>
            
            <div class=""contact-box"">
                <p>📞 <strong>Hotline:</strong> {supportPhone}</p>
                <p>✉️ <strong>Email:</strong> {supportEmail}</p>
                <a href=""mailto:{supportEmail}"" class=""btn-contact"">Gửi Email Hỗ Trợ</a>
            </div>
        </div>
        <div class=""email-footer"">
            Trân trọng,<br>
            <strong>Đội ngũ Cafebook</strong><br><br>
            © {DateTime.Now.Year} Cafebook. Mang đến trải nghiệm trọn vẹn nhất.
        </div>
    </div>
</body>
</html>";

                _ = Task.Run(() => SendEmailBackground(kh.Email, "Thông báo khóa tài khoản", body, settings));
            }
            return Ok();
        }

        [HttpPost("{id}/mokhoa")]
        public async Task<IActionResult> UnlockAccount(int id)
        {
            var kh = await _context.Set<KhachHang>().FindAsync(id);
            if (kh == null || kh.DaXoa) return NotFound();

            kh.BiKhoa = false; kh.LyDoKhoa = null; kh.ThoiGianMoKhoa = null;
            await _context.SaveChangesAsync();
            return Ok();
        }

        private void SendEmailBackground(string toEmail, string subject, string body, Dictionary<string, string> settings)
        {
            try
            {
                var client = new SmtpClient(settings["Smtp_Host"], int.Parse(settings["Smtp_Port"]))
                {
                    Credentials = new NetworkCredential(settings["Smtp_Username"], settings["Smtp_Password"]),
                    EnableSsl = bool.Parse(settings["Smtp_EnableSsl"])
                };
                var mail = new MailMessage { From = new MailAddress(settings["Smtp_Username"], settings["Smtp_FromName"]), Subject = subject, Body = body, IsBodyHtml = true };
                mail.To.Add(toEmail);
                client.Send(mail);
            }
            catch { }
        }
    }
}