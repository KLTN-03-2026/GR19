using CafebookApi.Data;
using EntityKhachHang = CafebookModel.Model.ModelEntities.KhachHang;
using CafebookModel.Model.ModelWeb.KhachHang;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Mail;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khachhang/dangky")]
    [ApiController]
    public class DangKyController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IMemoryCache _cache;

        public DangKyController(CafebookDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Đổi string thành string?
        private string MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@')) return email ?? string.Empty;
            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];
            if (name.Length <= 2) return $"{name[0]}***@{domain}";
            return $"{name.Substring(0, 2)}***@{domain}";
        }

        // Đổi string thành string?
        private string MaskPhone(string? phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7) return phone ?? string.Empty;
            return $"{phone.Substring(0, 3)}****{phone.Substring(phone.Length - 3)}";
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] DangKyRequestDto model)
        {
            // 0. Validate cơ bản
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.SoDienThoai) || string.IsNullOrEmpty(model.Password))
                return Ok(new DangKyResponseDto { Success = false, Message = "Vui lòng điền đầy đủ Email, SĐT và Mật khẩu." });

            // ====================================================================
            // 1. LẤY TOÀN BỘ DỮ LIỆU LIÊN QUAN TRONG 1 LẦN TRUY VẤN
            // ====================================================================
            var existingAccounts = await _context.KhachHangs
                .Where(k => k.Email == model.Email || k.SoDienThoai == model.SoDienThoai)
                .ToListAsync();

            // ====================================================================
            // 1.5. KIỂM TRA TÀI KHOẢN BỊ XÓA HOẶC BỊ KHÓA
            // ====================================================================
            // A. Kiểm tra tài khoản đã bị xóa
            var deletedAccount = existingAccounts.FirstOrDefault(k => k.DaXoa == true);
            if (deletedAccount != null)
            {
                string matchedType = deletedAccount.Email?.ToLower() == model.Email.ToLower() ? "Email" : "Số điện thoại";
                return Ok(new DangKyResponseDto
                {
                    Success = false,
                    Message = $"{matchedType} này thuộc về một hồ sơ đã bị xóa khỏi hệ thống. Vui lòng liên hệ quản trị viên để được hỗ trợ."
                });
            }

            // B. Kiểm tra tài khoản bị khóa và xử lý tự động mở khóa
            var lockedAccount = existingAccounts.FirstOrDefault(k => k.BiKhoa == true);
            if (lockedAccount != null)
            {
                if (lockedAccount.ThoiGianMoKhoa.HasValue && lockedAccount.ThoiGianMoKhoa.Value <= DateTime.Now)
                {
                    // Tự động gỡ khóa vì đã qua hạn
                    lockedAccount.BiKhoa = false;
                    lockedAccount.LyDoKhoa = null;
                    lockedAccount.ThoiGianMoKhoa = null;
                    _context.KhachHangs.Update(lockedAccount);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Vẫn đang bị khóa (Chưa tới hạn hoặc khóa vĩnh viễn)
                    string matchedType = lockedAccount.Email?.ToLower() == model.Email.ToLower() ? "Email" : "Số điện thoại";
                    string timeStr = lockedAccount.ThoiGianMoKhoa.HasValue
                        ? $"đến {lockedAccount.ThoiGianMoKhoa.Value:dd/MM/yyyy HH:mm}"
                        : "vĩnh viễn";

                    string reasonStr = !string.IsNullOrEmpty(lockedAccount.LyDoKhoa)
                        ? $" Lý do: {lockedAccount.LyDoKhoa}."
                        : "";

                    return Ok(new DangKyResponseDto
                    {
                        Success = false,
                        Message = $"{matchedType} này thuộc về tài khoản đang bị khóa {timeStr}.{reasonStr} Bạn không thể đăng ký lúc này."
                    });
                }
            }

            // ====================================================================
            // 2. PHÂN LOẠI TÀI KHOẢN (Chính thức & Tạm)
            // ====================================================================
            var officialByPhone = existingAccounts.FirstOrDefault(k => k.TaiKhoanTam == false && k.SoDienThoai == model.SoDienThoai);
            var officialByEmail = existingAccounts.FirstOrDefault(k => k.TaiKhoanTam == false && k.Email == model.Email);

            var tempByPhone = existingAccounts.FirstOrDefault(k => k.TaiKhoanTam == true && k.SoDienThoai == model.SoDienThoai);
            var tempByEmail = existingAccounts.FirstOrDefault(k => k.TaiKhoanTam == true && k.Email == model.Email);

            // ====================================================================
            // 3. ƯU TIÊN BẮT LỖI CHÉO CHO TÀI KHOẢN TẠM (Tối ưu UX)
            // ====================================================================
            if (tempByEmail != null && officialByPhone != null)
            {
                return Ok(new DangKyResponseDto
                {
                    Success = false,
                    Message = $"Email này là tài khoản lưu trú (SĐT gợi ý: {MaskPhone(tempByEmail.SoDienThoai)}). Tuy nhiên, SĐT bạn vừa nhập đã thuộc về một tài khoản chính thức khác. Vui lòng kiểm tra lại."
                });
            }

            if (tempByPhone != null && officialByEmail != null)
            {
                return Ok(new DangKyResponseDto
                {
                    Success = false,
                    Message = $"Số điện thoại này là tài khoản lưu trú (Email gợi ý: {MaskEmail(tempByPhone.Email)}). Tuy nhiên, Email bạn vừa nhập ({model.Email}) đã thuộc về một tài khoản chính thức khác."
                });
            }

            // ====================================================================
            // 4. LỚP KHIÊN BẢO VỆ TUYỆT ĐỐI TÀI KHOẢN CHÍNH THỨC
            // ====================================================================
            if (officialByPhone != null)
                return Ok(new DangKyResponseDto { Success = false, IsOfficialAccount = true, Message = "Số điện thoại này đã được đăng ký tài khoản chính thức. Vui lòng đăng nhập!" });

            if (officialByEmail != null)
                return Ok(new DangKyResponseDto { Success = false, IsOfficialAccount = true, Message = "Email này đã được đăng ký tài khoản chính thức. Vui lòng đăng nhập!" });

            // ====================================================================
            // 5. XÁC MINH CHÉO THÔNG TIN TÀI KHOẢN TẠM
            // ====================================================================
            EntityKhachHang? targetTempAccount = null;

            if (tempByPhone != null && tempByEmail != null && tempByPhone.IdKhachHang != tempByEmail.IdKhachHang)
            {
                return Ok(new DangKyResponseDto { Success = false, Message = "Thông কাহিনী không hợp lệ. Số điện thoại và Email này đang thuộc về hai hồ sơ lưu trú khác nhau." });
            }

            if (tempByPhone != null)
            {
                if (!string.IsNullOrEmpty(tempByPhone.Email) && tempByPhone.Email.ToLower() != model.Email.ToLower())
                    return Ok(new DangKyResponseDto { Success = false, Message = $"Số điện thoại này thuộc về khách hàng đã cung cấp Email {MaskEmail(tempByPhone.Email)}. Vui lòng nhập đúng Email." });
                targetTempAccount = tempByPhone;
            }
            else if (tempByEmail != null)
            {
                if (!string.IsNullOrEmpty(tempByEmail.SoDienThoai) && tempByEmail.SoDienThoai != model.SoDienThoai)
                    return Ok(new DangKyResponseDto { Success = false, Message = $"Email này thuộc về khách hàng đã dùng số điện thoại {MaskPhone(tempByEmail.SoDienThoai)} tại quán. Vui lòng nhập đúng SĐT." });
                targetTempAccount = tempByEmail;
            }

            // ====================================================================
            // 6. LƯU SESSION CACHE & GỬI OTP
            // ====================================================================
            int tempId = targetTempAccount?.IdKhachHang ?? 0;
            bool isNewAccount = targetTempAccount == null;

            string otp = new Random().Next(100000, 999999).ToString();
            var tempSession = new { TempId = tempId, Email = model.Email, SoDienThoai = model.SoDienThoai, Password = model.Password, OtpCode = otp };

            _cache.Set($"REG_SESSION_{model.Email}", tempSession, TimeSpan.FromMinutes(5));

            await SendOtpEmailAsync(model.Email, otp, isNewAccount);

            return Ok(new DangKyResponseDto
            {
                Success = true,
                RequireOtp = true,
                Message = isNewAccount ? "Mã xác thực OTP đã được gửi đến Email của bạn. Vui lòng kiểm tra." : "Chào mừng bạn quay lại! Vui lòng xác thực OTP để hoàn tất nâng cấp tài khoản.",
                TempId = tempId,
                TempEmail = model.Email,
                TempPhone = model.SoDienThoai
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto model)
        {
            // Lấy Session từ Cache thông qua Email khách gửi lên
            if (!_cache.TryGetValue($"REG_SESSION_{model.Email}", out dynamic? savedSession) || savedSession?.OtpCode != model.OtpCode)
                return Ok(new DangKyResponseDto { Success = false, Message = "Mã OTP không chính xác hoặc đã hết hạn (5 phút)." });

            EntityKhachHang? khachHang = null;

            if (savedSession?.TempId > 0)
            {
                // NÂNG CẤP TÀI KHOẢN TẠM
                khachHang = await _context.KhachHangs.FindAsync((int)savedSession.TempId);
                if (khachHang == null || !khachHang.TaiKhoanTam)
                    return Ok(new DangKyResponseDto { Success = false, Message = "Dữ liệu không hợp lệ hoặc đã được nâng cấp." });

                khachHang.Email = savedSession.Email;
                khachHang.SoDienThoai = savedSession.SoDienThoai;
                khachHang.MatKhau = savedSession.Password;
                if (khachHang.HoTen == khachHang.Email || string.IsNullOrEmpty(khachHang.HoTen)) khachHang.HoTen = savedSession.Email;

                khachHang.TaiKhoanTam = false;
                khachHang.NgayTao = DateTime.Now;
                _context.KhachHangs.Update(khachHang);
            }
            else
            {
                // TẠO TÀI KHOẢN MỚI HOÀN TOÀN
                khachHang = new EntityKhachHang
                {
                    HoTen = savedSession?.Email ?? string.Empty,
                    Email = savedSession?.Email ?? string.Empty,
                    SoDienThoai = savedSession?.SoDienThoai ?? string.Empty,
                    TenDangNhap = savedSession?.Email ?? string.Empty,
                    MatKhau = savedSession?.Password ?? string.Empty,
                    NgayTao = DateTime.Now,
                    TaiKhoanTam = false,
                    BiKhoa = false,
                    DiemTichLuy = 0
                };
                _context.KhachHangs.Add(khachHang);
            }

            await _context.SaveChangesAsync();
            _cache.Remove($"REG_SESSION_{model.Email}");

            return Ok(new DangKyResponseDto { Success = true });
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task SendOtpEmailAsync(string toEmail, string otpCode, bool isNewAccount)
        {
            var settings = await _context.CaiDats.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            string host = settings.GetValueOrDefault("Smtp_Host", "smtp.gmail.com");
            int port = int.Parse(settings.GetValueOrDefault("Smtp_Port", "587"));
            string username = settings.GetValueOrDefault("Smtp_Username", "");
            string password = settings.GetValueOrDefault("Smtp_Password", "");
            string fromName = settings.GetValueOrDefault("Smtp_FromName", "Cafebook Hỗ Trợ");
            bool enableSsl = bool.Parse(settings.GetValueOrDefault("Smtp_EnableSsl", "true"));
            string tenQuan = settings.GetValueOrDefault("ThongTin_TenQuan", "Cafebook");
            string diaChiQuan = settings.GetValueOrDefault("ThongTin_DiaChi", "Đang cập nhật");
            string supportPhone = settings.GetValueOrDefault("ThongTin_SoDienThoai", "Đang cập nhật");
            string supportEmail = settings.GetValueOrDefault("LienHe_Email", "cafebook.hotro@gmail.com");

            using var client = new SmtpClient(host, port) { Credentials = new NetworkCredential(username, password), EnableSsl = enableSsl };

            // Phân loại nội dung Email
            string emailSubject = isNewAccount ? $"[{tenQuan}] Xác thực đăng ký tài khoản mới" : $"[{tenQuan}] Mã xác thực nâng cấp tài khoản";
            string titleH1 = isNewAccount ? "XÁC THỰC ĐĂNG KÝ" : "NÂNG CẤP TÀI KHOẢN";
            string greetingDesc = isNewAccount
                ? "Chào mừng bạn đến với cộng đồng của chúng tôi! Để hoàn tất quá trình đăng ký tài khoản mới, vui lòng sử dụng mã xác thực dưới đây:"
                : "Chào bạn, hệ thống ghi nhận bạn đang yêu cầu nâng cấp tài khoản lưu trú thành tài khoản chính thức. Vui lòng sử dụng mã xác thực dưới đây:";

            string body = $@"
            <!DOCTYPE html>
            <html lang=""vi"">
            <head>
                <meta charset=""UTF-8"">
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #F7F3E9; color: #4E342E; margin: 0; padding: 20px; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                    .header {{ background-color: #5D4037; color: #FFF3E0; padding: 30px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 2px; text-transform: uppercase; }}
                    .content {{ padding: 30px; line-height: 1.6; font-size: 16px; text-align: center; }}
                    .otp-card {{ background-color: #FFF3E0; border: 2px dashed #D84315; border-radius: 12px; padding: 20px; margin: 25px auto; max-width: 250px; }}
                    .otp-code {{ font-size: 36px; font-weight: 700; color: #D84315; letter-spacing: 6px; margin: 10px 0; }}
                    .footer {{ background-color: #EFEBE9; padding: 20px; text-align: center; font-size: 13px; color: #8D6E63; border-top: 1px solid #D7CCC8; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>☕ {tenQuan.ToUpper()}</h1>
                    </div>
                    <div class=""content"">
                        <div style=""font-size: 20px; font-weight: bold; margin-bottom: 15px; color: #5D4037;"">🔐 {titleH1}</div>
                        <p style=""text-align: left;"">{greetingDesc}</p>
                
                        <div class=""otp-card"">
                            <div style=""font-size: 13px; color: #5D4037; text-transform: uppercase; font-weight: 600;"">Mã OTP của bạn</div>
                            <div class=""otp-code"">{otpCode}</div>
                        </div>
                
                        <p style=""text-align: left;"">⏳ Mã này chỉ có hiệu lực trong vòng <strong>5 phút</strong>.</p>
                        <div style=""font-size: 14px; color: #D32F2F; margin-top: 20px; padding: 15px; background-color: #FFEBEE; border-radius: 8px; border-left: 4px solid #D32F2F; text-align: left;"">
                            ⚠️ <strong>Bảo mật tài khoản:</strong> Tuyệt đối không chia sẻ mã này cho bất kỳ ai để đảm bảo an toàn cho dữ liệu của bạn.
                        </div>
                    </div>
                    <div class=""footer"">
                        <strong>Đội ngũ {tenQuan}</strong><br>
                        📍 Địa chỉ: {diaChiQuan}<br>
                        📞 Hotline: {supportPhone} | ✉️ {supportEmail}
                    </div>
                </div>
            </body>
            </html>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, fromName),
                Subject = emailSubject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await client.SendMailAsync(mailMessage);
        }
    }
}