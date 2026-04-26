using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Claims;
using MailKit.Net.Smtp;
using CafebookModel.Utils; 

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class PhanHoiKhachHangWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public PhanHoiKhachHangWebController(CafebookDbContext context)
        {
            _context = context;
        }

        private string GetFullImageUrl(string? path)
        {
            if (string.IsNullOrEmpty(path)) return "";

            string cleanPath = path.Replace('\\', '/');
            if (!cleanPath.StartsWith("/"))
            {
                cleanPath = "/" + cleanPath;
            }

            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{cleanPath}";
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews()
        {
            var data = await _context.DanhGias
                .Include(d => d.KhachHang)
                .Include(d => d.SanPham)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new DanhGiaWebDto
                {
                    IdDanhGia = d.idDanhGia,
                    TenKhachHang = d.KhachHang != null ? d.KhachHang.HoTen : "Khách Hàng Ẩn Danh",
                    TenSanPham = d.SanPham != null ? d.SanPham.TenSanPham : "Đánh giá chung về Quán",
                    SoSao = d.SoSao,
                    BinhLuan = d.BinhLuan,
                    HinhAnhUrl = d.HinhAnhURL,
                    NgayTao = d.NgayTao,
                    TrangThai = d.TrangThai ?? "Hiển thị",

                    DanhSachPhanHoi = _context.PhanHoiDanhGias
                        .Include(p => p.NhanVien)
                        .Where(p => p.idDanhGia == d.idDanhGia)
                        .Select(p => new PhanHoiReviewWebDto
                        {
                            TenNhanVien = p.NhanVien != null ? p.NhanVien.HoTen : "Nhân viên",
                            NoiDung = p.NoiDung,
                            NgayTao = p.NgayTao
                        }).ToList()
                }).ToListAsync();

            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.HinhAnhUrl))
                {
                    if (!item.HinhAnhUrl.Contains("images/danhgia"))
                    {
                        string fileName = item.HinhAnhUrl.TrimStart('\\', '/');
                        item.HinhAnhUrl = $"{HinhAnhPaths.UrlDanhGiaSanPham}/{fileName}"; 
            }

                     item.HinhAnhUrl = GetFullImageUrl(item.HinhAnhUrl);
        }
            }

            return Ok(data);
        }

        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetFeedbacks()
        {
            var data = await _context.GopYs
                .OrderByDescending(g => g.NgayTao)
                .Select(g => new GopYWebDto
                {
                    IdGopY = g.IdGopY,
                    HoTen = g.HoTen ?? "Khách hàng",
                    Email = g.Email ?? "",
                    NoiDung = g.NoiDung ?? "",
                    NgayTao = g.NgayTao,
                    TrangThai = g.TrangThai ?? "Chờ xử lý"
                }).ToListAsync();
            return Ok(data);
        }

        [HttpPost("reply-review/{id}")]
        public async Task<IActionResult> ReplyReview(int id, [FromBody] PhanHoiInputWebDto input)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // FIX CS0246: Sử dụng đúng Class Entity là PhanHoiDanhGia (Thay vì PhanHoi)
            var phanHoi = new PhanHoiDanhGia
            {
                idDanhGia = id,     // Chữ i viết thường theo DB
                idNhanVien = userId, // Chữ i viết thường theo DB
                NoiDung = input.NoiDung,
                NgayTao = DateTime.Now
            };

            // FIX CS1061: Sử dụng _context.PhanHoiDanhGias
            _context.PhanHoiDanhGias.Add(phanHoi);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi phản hồi thành công." });
        }

        [HttpPut("toggle-review/{id}")]
        public async Task<IActionResult> ToggleReview(int id)
        {
            var review = await _context.DanhGias.FindAsync(id);
            if (review == null) return NotFound();
            review.TrangThai = review.TrangThai == "Hiển thị" ? "Đã ẩn" : "Hiển thị";
            await _context.SaveChangesAsync();
            return Ok(new { status = review.TrangThai });
        }

        [HttpPost("reply-feedback/{id}")]
        public async Task<IActionResult> ReplyFeedback(int id, [FromBody] GopYReplyRequestDto req)
        {
            try
            {
                var gopY = await _context.GopYs.FindAsync(id);
                if (gopY == null) return NotFound(new { message = "Không tìm thấy góp ý." });
                if (gopY.TrangThai == "Đã xử lý") return Conflict(new { message = "Góp ý này đã được xử lý trước đó." });

                // 1. Lấy toàn bộ cấu hình từ bảng CaiDat
                var settings = await _context.CaiDats.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri ?? "");

                string smtpHost = settings.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
                int smtpPort = int.Parse(settings.GetValueOrDefault("Smtp_Port", "587"));
                string smtpUser = settings.GetValueOrDefault("Smtp_Username", "");
                string smtpPass = settings.GetValueOrDefault("Smtp_Password", "");
                string fromName = settings.GetValueOrDefault("Smtp_FromName", "Cafebook");

                string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
                string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
                string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                    return BadRequest(new { message = "Chưa cấu hình tài khoản gửi Email trong hệ thống." });

                // 2. Tạo MimeMessage
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, smtpUser));
                email.To.Add(new MailboxAddress(gopY.HoTen, gopY.Email));
                email.Subject = $"[{tenQuan}] Phản hồi về góp ý của bạn";

                // Sử dụng Template HTML tuyệt đẹp của bạn
                string body = $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F7F3E9; color: #4E342E; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #6F4E37; color: #FFFFFF; padding: 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 1px; text-transform: uppercase; }}
                        .content {{ padding: 30px; line-height: 1.6; }}
                        .alert-card {{ background-color: #EFEBE9; border-left: 6px solid #6F4E37; border-radius: 8px; padding: 20px; margin: 25px 0; color: #4E342E; font-style: italic; }}
                        .footer {{ background-color: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; line-height: 1.6; border-top: 1px solid #D7CCC8; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>☕ {tenQuan}</h1>
                        </div>
                        <div class=""content"">
                            <div style=""font-size: 18px; font-weight: bold; margin-bottom: 20px;"">Kính gửi {gopY.HoTen},</div>
                            <p>Đầu tiên, chúng tôi xin chân thành cảm ơn bạn đã dành thời gian gửi góp ý quý báu cho hệ thống. Dưới đây là phản hồi từ bộ phận Chăm sóc khách hàng:</p>
                            
                            <div style=""font-size: 16px; margin: 20px 0; white-space: pre-wrap;"">{req.NoiDungEmail}</div>

                            <p style=""font-size: 14px; color: #757575;"">Nội dung góp ý gốc của bạn:</p>
                            <div class=""alert-card"">""{gopY.NoiDung}""</div>
                            
                            <p>Rất mong tiếp tục nhận được sự ủng hộ của bạn trong thời gian tới.</p>
                        </div>
                        <div class=""footer"">
                            <strong>Đội ngũ {tenQuan}</strong><br>
                            📍 Địa chỉ: {diaChiQuan}<br>
                            📞 Hotline: {supportPhone} | ✉️ {smtpUser}<br>
                            © {DateTime.Now.Year} {tenQuan}. Mang đến trải nghiệm trọn vẹn nhất.
                        </div>
                    </div>
                </body>
                </html>";

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                // 3. Gửi Email qua SMTP
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                    await client.SendAsync(email);
                    await client.DisconnectAsync(true);
                }

                // 4. Đổi trạng thái thành Đã xử lý
                gopY.TrangThai = "Đã xử lý";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã gửi email và đóng góp ý thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống hoặc lỗi SMTP: " + ex.Message });
            }
        }
    }
}
