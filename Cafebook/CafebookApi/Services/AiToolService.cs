using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CafebookModel.Utils;
using System.Data.Common; 

namespace CafebookApi.Services
{
    public class AiToolService
    {
        private readonly CafebookDbContext _context;
        private readonly IDataProtector _protectorSanPham;
        private readonly IDataProtector _protectorSach;
        private readonly IDataProtector _protectorHoaDon;

        private readonly IDataProtector _protectorTacGia;
        private readonly IDataProtector _protectorTheLoai;
        private readonly IDataProtector _protectorNXB;

        private readonly IMemoryCache _cache;

        private const int SlotDurationHours = 2;

        public const string LinkTaiKhoan = "/tai-khoan/tong-quan";
        public const string LinkThongTinCaNhan = "/tai-khoan/thong-tin-ca-nhan";
        public const string LinkLichSuThue = "/tai-khoan/lich-su-thue-sach";
        public const string LinkLichSuDatBan = "/tai-khoan/lich-su-dat-ban";
        public const string LinkLichSuDonHang = "/tai-khoan/lich-su-don-hang";
        public const string LinkDoiMatKhau = "/tai-khoan/doi-mat-khau";
        public const string LinkThongTinSach = "/thu-vien-sach/tim-kiem";
        public const string LinkLienHe = "/lien-he";
        public const string LinkDangNhap = "/dang-nhap";
        public const string LinkDatBan = "/dat-ban";
        public const string LinkGioHang = "/gio-hang";
        public const string LinkThucDon = "/san-pham";
        public const string LinkThuVienSach = "/thu-vien-sach";
        public const string LinkChinhSach = "/chinh-sach-dieu-khoan";

        public const string LinkDetailSach = "/chi-tiet-sach/";
        public const string LinkDetailSP = "/chi-tiet-san-pham/";
        public const string LinkChiTietDonHang = "/tai-khoan/lich-su-don-hang/";

        public AiToolService(CafebookDbContext context, IDataProtectionProvider provider, IMemoryCache cache)
        {
            _context = context;
            _protectorSanPham = provider.CreateProtector("Cafebook.SanPham.Id");
            _protectorSach = provider.CreateProtector("Cafebook.Sach.Id");
            _protectorHoaDon = provider.CreateProtector("Cafebook.HoaDon.Id");

            _protectorTacGia = provider.CreateProtector("Cafebook.TacGia.Id");
            _protectorTheLoai = provider.CreateProtector("Cafebook.TheLoai.Id");
            _protectorNXB = provider.CreateProtector("Cafebook.NXB.Id");

            _cache = cache;
        }

        public async Task<object> GetBanChayAsync()
        {
            // BƯỚC 1: LẤY TOP 3 SẢN PHẨM & TẠO NÚT BẤM
            var topSanPhamIds = await _context.ChiTietHoaDons
                .GroupBy(c => c.IdSanPham)
                .OrderByDescending(g => g.Sum(c => c.SoLuong))
                .Take(3)
                .Select(g => g.Key)
                .ToListAsync();

            var topSanPham = await _context.SanPhams
                .Where(s => topSanPhamIds.Contains(s.IdSanPham))
                .ToListAsync();

            string spMessage = "";
            foreach (var sp in topSanPham)
            {
                // Mã hóa ID thành chuỗi an toàn cho URL
                string token = _protectorSanPham.ProtectToUrlSafe(sp.IdSanPham.ToString());
                spMessage += $"- {sp.TenSanPham} \n[BUTTON: Xem món này | {LinkDetailSP}?token={token}]\n";
            }

            // BƯỚC 2: LẤY TOP 3 SÁCH & TẠO NÚT BẤM
            var topSachIds = await _context.ChiTietPhieuThues
                .GroupBy(c => c.IdSach)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToListAsync();

            var topSach = await _context.Sachs
                .Where(s => topSachIds.Contains(s.IdSach))
                .ToListAsync();

            string sachMessage = "";
            foreach (var sach in topSach)
            {
                string token = _protectorSach.ProtectToUrlSafe(sach.IdSach.ToString());
                sachMessage += $"- {sach.TenSach} \n[BUTTON: Xem sách này | {LinkDetailSach}?token={token}]\n";
            }

            if (string.IsNullOrEmpty(spMessage) && string.IsNullOrEmpty(sachMessage))
            {
                return new { Message = "Dạ hiện tại quán mới mở nên chưa có thống kê bán chạy. Mời bạn tham khảo qua thực đơn nha!\n[BUTTON: Xem Thực Đơn | /san-pham]" };
            }

            return new
            {
                Message = $"Dạ đây là những món và sách đang cực hot, được mọi người yêu thích nhất tại Cafebook ạ:\n\n" +
                          $"🍵 **Nước uống Best-seller:**\n{spMessage}\n" +
                          $"📖 **Sách mượn nhiều nhất:**\n{sachMessage}\n" +
                          $"Bạn ưng ý món nào thì click vào nút bên trên để xem chi tiết nhé!"
            };
        }

        public async Task<object> QueryDatabaseAsync(string sqlQuery)
        {
            try
            {
                if (!sqlQuery.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("DROP", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("DELETE", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("INSERT", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("HoaDon", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("KhachHang", StringComparison.OrdinalIgnoreCase) ||
                    sqlQuery.Contains("NhanVien", StringComparison.OrdinalIgnoreCase))
                {
                    return new { Message = "Dạ, câu hỏi này nằm ngoài quyền hạn tra cứu của mình. Mình chỉ có thể hỗ trợ bạn tìm hiểu về thông tin công khai (Sách, Thức uống, Tác giả...). Bạn thông cảm nhé!" };
                }

                // Thực thi SQL trực tiếp
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = sqlQuery;
                command.CommandType = System.Data.CommandType.Text;

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                var results = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    results.Add(row);
                }

                if (!results.Any())
                    return new { Message = "Dạ mình đã tìm kỹ nhưng không thấy dữ liệu nào phù hợp với yêu cầu của bạn ạ." };

                return new
                {
                    Data = results,
                    Message = "Dưới đây là dữ liệu thô từ Database. Hãy hành văn lại một cách ấm áp, thân thiện để trả lời khách hàng."
                };
            }
            catch (Exception)
            {
                return new { Message = $"Dạ hệ thống truy vấn tạm thời gặp chút khó khăn. Bạn có thể thử hỏi lại theo cách khác giúp mình nhé." };
            }
        }


        public async Task<object> GetTrangThaiTaiKhoanAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.AsNoTracking()
                .FirstOrDefaultAsync(k => k.IdKhachHang == idKhachHang);

            if (kh == null) return new { Message = "Dạ, mình không tìm thấy thông tin tài khoản của bạn trên hệ thống." };
            if (kh.BiKhoa)
            {
                string lyDo = string.IsNullOrEmpty(kh.LyDoKhoa) ? "Vi phạm quy định của quán." : kh.LyDoKhoa;
                string thoiGian = kh.ThoiGianMoKhoa.HasValue
                    ? kh.ThoiGianMoKhoa.Value.ToString("dd/MM/yyyy HH:mm")
                    : "Chưa xác định, vui lòng liên hệ Admin.";

                return new
                {
                    Message = $"Dạ, tài khoản của bạn hiện đang trong trạng thái **Tạm khóa**. \n\n" +
                              $"⚠️ **Lý do:** {lyDo} \n" +
                              $"⏳ **Thời gian mở lại dự kiến:** {thoiGian} \n\n" +
                              $"Bạn vui lòng liên hệ trực tiếp hotline hoặc fanpage để được hỗ trợ mở khóa sớm nhé! [BUTTON: Liên hệ hỗ trợ | {LinkLienHe}]"
                };
            }

            return new { Message = "Tài khoản của bạn vẫn đang hoạt động bình thường trên hệ thống ạ. Bạn có cần mình hỗ trợ tra cứu thông tin gì khác không?" };
        }

        public async Task<object> KiemTraKhoaTaiKhoanNhanhAsync(string inputSearch)
        {
            var kh = await _context.KhachHangs.AsNoTracking()
                .FirstOrDefaultAsync(k => k.SoDienThoai == inputSearch || k.Email == inputSearch || k.TenDangNhap == inputSearch);

            if (kh == null) return new { Message = $"Dạ, mình không tìm thấy tài khoản nào khớp với '{inputSearch}'. Bạn kiểm tra lại SĐT hoặc Email nhé!" };

            if (kh.BiKhoa)
            {
                string lyDo = string.IsNullOrEmpty(kh.LyDoKhoa) ? "Vi phạm quy định của quán." : kh.LyDoKhoa;
                string thoiGian = kh.ThoiGianMoKhoa.HasValue ? kh.ThoiGianMoKhoa.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa xác định";
                return new
                {
                    Message = $"Tài khoản **{kh.HoTen}** hiện đang bị **Tạm khóa**.\n\n⚠️ Lý do: {lyDo}\n⏳ Thời gian mở lại: {thoiGian}\n\n[BUTTON: Liên hệ Admin | {LinkLienHe}]"
                };
            }

            return new { Message = $"Tài khoản **{kh.HoTen}** hiện tại đang hoạt động bình thường, không bị khóa ạ!" };
        }
        // ==========================================
        // CÁC HÀM CŨ GIỮ NGUYÊN
        // ==========================================

        public Task<object> GetYeuCauDangNhapAsync(string tenTinhNang)
        {
            string msg = $"Bạn cần đăng nhập để sử dụng tính năng này.\n[BUTTON: Tới trang Đăng nhập | {LinkDangNhap}]";
            return Task.FromResult<object>(new { Message = msg });
        }

        public Task<object> HuongDanDatBanAsync(int? idKhachHang)
        {
            string msg = "";
            if (!idKhachHang.HasValue || idKhachHang == 0)
            {
                msg = $"Để đặt bàn, bạn vui lòng ấn vào nút bên dưới để vào trang Đặt Bàn -> Chọn ngày, khoảng giờ và số người muốn đặt -> Nhấn Kiểm tra bàn -> Chọn bàn theo ý thích -> Nhập thông tin (Tên, SĐT, Email) và xác nhận đặt bàn nhé.\n[BUTTON: Đặt bàn trực tuyến | {LinkDatBan}]";
            }
            else
            {
                msg = $"Để đặt bàn, bạn vui lòng ấn vào nút bên dưới để vào trang Đặt Bàn -> Chọn ngày, khoảng giờ và số người muốn đặt -> Nhấn Kiểm tra bàn -> Chọn bàn theo ý thích -> Nhập ghi chú (nếu bạn đặt hộ thì điền thông tin người nhận) và xác nhận đặt bàn nhé.\n[BUTTON: Đặt bàn trực tuyến | {LinkDatBan}]";
            }
            return Task.FromResult<object>(new { Message = msg });
        }

        public Task<object> GetHuongDanHeThongAsync(string chuDe)
        {
            string topic = chuDe.ToLower();
            string hdResponse = "Tính năng này hiện có thể truy cập dễ dàng qua thanh Menu chính của trang web.";

            if (topic.Contains("quên") || topic.Contains("lấy lại"))
                hdResponse = $"Nếu quên mật khẩu, bạn hãy ra màn hình đăng nhập -> Chọn 'Quên mật khẩu' -> Nhập Email -> Nhận mã 6 số -> Nhập mật khẩu mới và lưu lại nhé.\n[BUTTON: Tới trang Đăng nhập | {LinkDangNhap}]";
            else if (topic.Contains("mật khẩu") || topic.Contains("doimatkhau") || topic.Contains("đổi"))
                hdResponse = $"Để đổi mật khẩu, bạn vui lòng truy cập vào phần Cài đặt Tài khoản -> Đổi mật khẩu.\n[BUTTON: Đổi mật khẩu | {LinkDoiMatKhau}]";
            else if (topic.Contains("thông tin") || topic.Contains("thongtincanhan") || topic.Contains("cá nhân"))
                hdResponse = $"Bạn có thể xem và cập nhật thông tin cá nhân (ảnh đại diện, SĐT, Email, Địa chỉ) tại trang Hồ sơ.\n[BUTTON: Hồ sơ của tôi | {LinkThongTinCaNhan}]";
            else if (topic.Contains("góp ý") || topic.Contains("gopy") || topic.Contains("đánh giá") || topic.Contains("chính sách"))
                hdResponse = $"Cafebook rất vui khi nhận được phản hồi từ bạn. Hãy gửi đánh giá hoặc đóng góp qua trang Liên hệ để quán ngày một tốt hơn nha.\n[BUTTON: Gửi góp ý | {LinkLienHe}]\n[BUTTON: Xem Chính sách | {LinkChinhSach}]";

            return Task.FromResult<object>(new { Message = hdResponse });
        }

        public Task<object> KetNoiNhanVienAsync()
        {
            return Task.FromResult<object>(new { Message = "Đang kết nối nhân viên hỗ trợ... [NEEDS_SUPPORT]" });
        }

        public async Task<object> GetThongTinChungAsync(string chuDe = "")
        {
            string cacheKey = string.IsNullOrEmpty(chuDe) ? "CACHE_THONG_TIN_CHUNG" : $"CACHE_THONG_TIN_CHUNG_{chuDe}";
            return await _cache.GetOrCreateAsync<object>(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                var keys = new List<string> {
            "ThongTin_DiaChi", "ThongTin_SoDienThoai", "ThongTin_GioMoCua", "ThongTin_GioDongCua", "Wifi_MatKhau", "ThongTin_ThuMoCua"
        };

                var settings = await _context.CaiDats.AsNoTracking().Where(c => keys.Contains(c.TenCaiDat)).ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
                string GetVal(string key) => settings.ContainsKey(key) ? (settings[key] ?? "Chưa cập nhật") : "Chưa cập nhật";

                string thuMoCua = GetVal("ThongTin_ThuMoCua");
                if (thuMoCua == "2,3,4,5,6,7,8") thuMoCua = "Thứ 2 đến Chủ Nhật";
                else thuMoCua = "Thứ " + thuMoCua.Replace("8", "Chủ Nhật");

                if (chuDe == "wifi") return new { Message = $"Dạ, mật khẩu Wifi của quán là: **{GetVal("Wifi_MatKhau")}**" };
                if (chuDe == "diachi") return new { Message = $"Dạ, địa chỉ Cafebook tại: **{GetVal("ThongTin_DiaChi")}**\n[BUTTON: Xem Liên Hệ | {LinkLienHe}]" };
                if (chuDe == "giogiac") return new { Message = $"Dạ, quán mở cửa từ **{GetVal("ThongTin_GioMoCua")} đến {GetVal("ThongTin_GioDongCua")}** ({thuMoCua})." };

                string msg = $"Đây là thông tin cơ bản của Cafebook ạ:\n- Địa chỉ: {GetVal("ThongTin_DiaChi")}\n- SĐT: {GetVal("ThongTin_SoDienThoai")}\n- Các ngày hoạt động: {thuMoCua}\n- Giờ mở cửa: {GetVal("ThongTin_GioMoCua")} đến {GetVal("ThongTin_GioDongCua")}\n- Mật khẩu Wifi: {GetVal("Wifi_MatKhau")}\n\n[BUTTON: Xem Liên Hệ | {LinkLienHe}]\n[BUTTON: Chính sách & Quy định | {LinkChinhSach}]";
                return new { Message = msg };
            }) ?? new { };
        }

        public async Task<object> GetKhuyenMaiAsync()
        {
            return await _cache.GetOrCreateAsync<object>("CACHE_KHUYEN_MAI", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                var now = DateTime.Now;
                var listKM = await _context.KhuyenMais.AsNoTracking()
                    .Where(k => k.TrangThai == "Hoạt động" && k.NgayBatDau <= now && k.NgayKetThuc >= now)
                    .OrderByDescending(k => k.GiaTriGiam)
                    .Take(3).ToListAsync();

                if (!listKM.Any()) return new { Message = "Hiện tại quán chưa có chương trình khuyến mãi nào đang diễn ra, bạn theo dõi thêm nhé." };

                string msg = "Cafebook đang có các chương trình khuyến mãi cực hot:\n" + string.Join("\n", listKM.Select(k => $"- {k.TenChuongTrinh}: Giảm {(k.LoaiGiamGia == "Phần trăm" ? k.GiaTriGiam + "%" : k.GiaTriGiam.ToString("N0") + "đ")} (Đến ngày {k.NgayKetThuc:dd/MM})"));
                return new { Message = msg };
            }) ?? new { };
        }

        public async Task<object> KiemTraSanPhamAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new { Message = "Vui lòng cung cấp tên món để mình kiểm tra nhé." };

            string cacheKey = $"KTSP_{keyword}";
            return await _cache.GetOrCreateAsync<object>(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                var keywords = keyword.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var query = _context.SanPhams.AsNoTracking().Include(s => s.DanhMuc).AsQueryable();

                foreach (var kw in keywords) query = query.Where(s => s.TenSanPham.ToLower().Contains(kw));
                var sp = await query.FirstOrDefaultAsync();

                if (sp == null) return new { Message = $"Hiện tại quán không có món '{keyword}' trong thực đơn. Mình mời bạn tham khảo các món ngon khác nhé.\n[BUTTON: Xem Thực đơn | {LinkThucDon}]" };

                bool isOutOfStock = !sp.TrangThaiKinhDoanh;
                if (!isOutOfStock)
                {
                    var dlList = await _context.DinhLuongs.Include(d => d.NguyenLieu).Include(d => d.DonViSuDung).Where(d => d.IdSanPham == sp.IdSanPham).AsNoTracking().ToListAsync();
                    foreach (var dl in dlList)
                    {
                        decimal heSo = (dl.DonViSuDung != null && dl.DonViSuDung.GiaTriQuyDoi > 0) ? dl.DonViSuDung.GiaTriQuyDoi : 1m;
                        decimal luongDung = dl.DonViSuDung != null && dl.DonViSuDung.LaDonViCoBan ? dl.SoLuongSuDung : dl.SoLuongSuDung / heSo;
                        if (dl.NguyenLieu.TonKho < luongDung) { isOutOfStock = true; break; }
                    }
                }

                string token = _protectorSanPham.ProtectToUrlSafe(sp.IdSanPham.ToString());
                string stInfo = isOutOfStock ? "Nhưng tiếc quá, món này tạm ngưng kinh doanh hoặc hết nguyên liệu rồi." : "Sản phẩm hiện đang có sẵn sàng phục vụ.";
                return new { Message = $"Món '{sp.TenSanPham}' có giá {sp.GiaBan:N0}đ. {stInfo}\n[BUTTON: Xem chi tiết món | {LinkDetailSP}?token={token}]" };
            }) ?? new { };
        }

        public async Task<object> TimMonTheoLoaiAsync(string loaiMon)
        {
            return await _cache.GetOrCreateAsync<object>($"CACHE_LOAI_SP_{loaiMon}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                var list = await _context.SanPhams.AsNoTracking().Include(s => s.DanhMuc)
                    .Where(s => s.DanhMuc != null && s.DanhMuc.TenDanhMuc.Contains(loaiMon) && s.TrangThaiKinhDoanh)
                    .OrderBy(s => s.TenSanPham).Take(4).ToListAsync();

                if (!list.Any()) return new { Message = $"Rất tiếc, không tìm thấy món nào thuộc danh mục '{loaiMon}' đang có sẵn lúc này." };

                string msg = $"Mình đã tìm thấy một vài món cực ngon cho bạn:\n";
                foreach (var sp in list)
                {
                    string tk = _protectorSanPham.ProtectToUrlSafe(sp.IdSanPham.ToString());
                    msg += $"- {sp.TenSanPham} ({sp.GiaBan:N0}đ) \n[BUTTON: Chọn món {sp.TenSanPham} | {LinkDetailSP}?token={tk}]\n";
                }
                msg += $"\n[BUTTON: Xem toàn bộ Thực đơn | {LinkThucDon}]";
                return new { Message = msg };
            }) ?? new { };
        }

        public async Task<object> GetGoiYSanPhamAsync()
        {
            return await _cache.GetOrCreateAsync<object>("CACHE_GOI_Y_SP", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                var list = await _context.SanPhams.AsNoTracking().Where(s => s.TrangThaiKinhDoanh)
                    .OrderBy(r => Guid.NewGuid()).Take(3).ToListAsync();

                string msg = "Đây là những món best-seller hôm nay bạn nên thử:\n";
                foreach (var sp in list)
                {
                    string tk = _protectorSanPham.ProtectToUrlSafe(sp.IdSanPham.ToString());
                    msg += $"- {sp.TenSanPham} ({sp.GiaBan:N0}đ) \n[BUTTON: Chọn món {sp.TenSanPham} | {LinkDetailSP}?token={tk}]\n";
                }
                return new { Message = msg };
            }) ?? new { };
        }

        public async Task<object> KiemTraSachAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new { Message = "Vui lòng cung cấp tên sách nhé." };

            string cacheKey = $"KTSACH_{keyword}";
            return await _cache.GetOrCreateAsync<object>(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                var keywords = keyword.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var query = _context.Sachs.AsNoTracking().Include(s => s.SachTacGias).ThenInclude(st => st.TacGia).AsQueryable();

                foreach (var kw in keywords) query = query.Where(s => s.TenSach.ToLower().Contains(kw));

                var sach = await query.FirstOrDefaultAsync();
                if (sach == null) return new { Message = $"Không tìm thấy sách nào khớp với '{keyword}'. Bạn thử cuốn khác nhé!\n[BUTTON: Khám phá Thư viện | {LinkThuVienSach}]" };

                bool isAvailable = sach.SoLuongHienCo > 0;
                string statusMessage = isAvailable ? $"Hiện còn {sach.SoLuongHienCo} cuốn trên kệ." : "Sách này hiện đã có khách mượn hết. Bạn có thể tham khảo các tác phẩm khác.";
                string tacGia = string.Join(", ", sach.SachTacGias.Where(x => x.TacGia != null).Select(x => x.TacGia.TenTacGia));

                string token = _protectorSach.ProtectToUrlSafe(sach.IdSach.ToString());

                return new { Message = $"Cuốn '{sach.TenSach}' của tác giả {tacGia}. {statusMessage}\n[BUTTON: Xem thông tin sách | {LinkDetailSach}?token={token}]" };
            }) ?? new { };
        }

        public async Task<object> TimSachMoRongAsync(string loaiTimKiem, string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa)) return new { Message = "Vui lòng cung cấp từ khóa để tìm sách nhé." };

            return await _cache.GetOrCreateAsync<object>($"CACHE_SACH_MORONG_{loaiTimKiem}_{tuKhoa}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                if (loaiTimKiem.Equals("TacGia", StringComparison.OrdinalIgnoreCase))
                {
                    var tg = await _context.TacGias.AsNoTracking().FirstOrDefaultAsync(t => t.TenTacGia.Contains(tuKhoa));
                    if (tg != null)
                    {
                        string tk = _protectorTacGia.ProtectToUrlSafe(tg.IdTacGia.ToString());
                        return new { Message = $"Mình đã tìm thấy Tác giả {tg.TenTacGia}. Mời bạn nhấp vào nút bên dưới để xem toàn bộ tác phẩm nhé:\n[BUTTON: Xem sách của {tg.TenTacGia} | {LinkThongTinSach}?TokenTacGia={tk}]" };
                    }
                }
                else if (loaiTimKiem.Equals("TheLoai", StringComparison.OrdinalIgnoreCase))
                {
                    var tl = await _context.TheLoais.AsNoTracking().FirstOrDefaultAsync(t => t.TenTheLoai.Contains(tuKhoa));
                    if (tl != null)
                    {
                        string tk = _protectorTheLoai.ProtectToUrlSafe(tl.IdTheLoai.ToString());
                        return new { Message = $"Có ngay sách thể loại {tl.TenTheLoai}. Mời bạn khám phá:\n[BUTTON: Sách thể loại {tl.TenTheLoai} | {LinkThongTinSach}?TokenTheLoai={tk}]" };
                    }
                }
                else if (loaiTimKiem.Equals("NhaXuatBan", StringComparison.OrdinalIgnoreCase))
                {
                    var nxb = await _context.NhaXuatBans.AsNoTracking().FirstOrDefaultAsync(t => t.TenNhaXuatBan.Contains(tuKhoa));
                    if (nxb != null)
                    {
                        string tk = _protectorNXB.ProtectToUrlSafe(nxb.IdNhaXuatBan.ToString());

                        return new { Message = $"Mình đã tìm thấy NXB {nxb.TenNhaXuatBan}. Mời bạn xem sách:\n[BUTTON: Sách của NXB {nxb.TenNhaXuatBan} | {LinkThongTinSach}?TokenNXB={tk}]" };
                    }
                }

                return new { Message = $"Không tìm thấy sách nào khớp với '{tuKhoa}' cho phân loại này. Bạn thử tìm từ khóa khác nhé.\n[BUTTON: Khám phá Thư viện | {LinkThuVienSach}]" };
            }) ?? new { };
        }

        public async Task<object> GetGoiYSachAsync()
        {
            return await _cache.GetOrCreateAsync<object>("CACHE_GOI_Y_SACH", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                var list = await _context.Sachs.AsNoTracking()
                    .Include(s => s.SachTacGias).ThenInclude(st => st.TacGia)
                    .Where(s => s.SoLuongHienCo > 0).OrderBy(r => Guid.NewGuid()).Take(3).ToListAsync();

                string msg = "Những cuốn sách thú vị đang có sẵn trên kệ:\n";
                foreach (var s in list)
                {
                    string tk = _protectorSach.ProtectToUrlSafe(s.IdSach.ToString());
                    msg += $"- {s.TenSach} \n[BUTTON: Xem sách | {LinkDetailSach}?token={tk}]\n";
                }
                return new { Message = msg };
            }) ?? new { };
        }

        private string MaskPhone(string? phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7) return "—";
            return phone.Substring(0, 5) + "xxxx" + phone.Substring(phone.Length - 2);
        }

        private string MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return "—";
            var parts = email.Split('@');
            string name = parts[0];
            if (name.Length <= 3) return "xxx@" + parts[1];
            return name.Substring(0, 3) + new string('x', Math.Max(3, name.Length - 3)) + "@" + parts[1];
        }

        public async Task<object> GetDiemTichLuyAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.FindAsync(idKhachHang);
            return kh == null ? new { Message = "Lỗi xác thực" } : new { Message = $"Tài khoản của bạn hiện có {kh.DiemTichLuy:N0} điểm. \n[BUTTON: Xem Tổng quan | {LinkTaiKhoan}]" };
        }

        public async Task<object> GetThongTinCaNhanAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.FindAsync(idKhachHang);
            return kh == null ? new { Message = "Lỗi xác thực" } : new { Message = $"Thông tin cá nhân của bạn:\n- Họ tên: {kh.HoTen}\n- SĐT: {MaskPhone(kh.SoDienThoai)}\n- Email: {MaskEmail(kh.Email)}\n\n[BUTTON: Cập nhật Hồ sơ | {LinkThongTinCaNhan}]" };
        }

        public async Task<object> GetTongQuanTaiKhoanAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.FindAsync(idKhachHang);
            return kh == null ? new { Message = "Lỗi xác thực" } : new { Message = $"Xin chào {kh.HoTen}, điểm của bạn là {kh.DiemTichLuy:N0}.\n[BUTTON: Quản lý tài khoản | {LinkTaiKhoan}]" };
        }

        public async Task<object> GetLichSuDatBanAsync(int idKhachHang)
        {
            var phieu = await _context.PhieuDatBans.Include(p => p.Ban).AsNoTracking()
                .Where(p => p.IdKhachHang == idKhachHang)
                .OrderByDescending(p => p.ThoiGianDat)
                .FirstOrDefaultAsync();

            if (phieu == null) return new { Message = "Bạn chưa có lịch sử đặt bàn nào trên hệ thống.\n[BUTTON: Đặt bàn ngay | {LinkDatBan}]" };

            string msg = $"Phiếu đặt bàn gần nhất của bạn là: **Bàn {phieu.Ban?.SoBan ?? ""}** (dành cho {phieu.SoLuongKhach} người), thời gian: {phieu.ThoiGianDat:dd/MM/yyyy HH:mm}. Trạng thái hiện tại: **{phieu.TrangThai}**.\n\n[BUTTON: Xem toàn bộ lịch sử đặt bàn | {LinkLichSuDatBan}]";
            return new { Message = msg };
        }

        public Task<object> HuyDatBanAsync(int idPhieuDat, string lyDo, int idKhachHang)
        {
            return Task.FromResult<object>(new { Message = $"Đã tiếp nhận yêu cầu. Bạn có thể thực hiện hủy phiếu an toàn tại trang Lịch sử nhé.\n[BUTTON: Lịch sử đặt bàn | {LinkLichSuDatBan}]" });
        }

        public async Task<object> GetLichSuThueSachAsync(int idKhachHang)
        {
            var phieu = await _context.PhieuThueSachs.Include(p => p.ChiTietPhieuThues).ThenInclude(c => c.Sach).AsNoTracking()
                .Where(p => p.IdKhachHang == idKhachHang && p.TrangThai == "Đang thuê")
                .OrderByDescending(p => p.NgayThue)
                .FirstOrDefaultAsync();

            if (phieu == null) return new { Message = $"Hiện tại bạn không có cuốn sách nào đang mượn.\n[BUTTON: Xem lịch sử thuê sách | {LinkLichSuThue}]" };

            string msg = $"Bạn đang có 1 phiếu mượn sách (Mã #{phieu.IdPhieuThueSach}). Các sách đang mượn:\n";
            foreach (var ct in phieu.ChiTietPhieuThues)
            {
                var daysLeft = (ct.NgayHenTra.Date - DateTime.Now.Date).TotalDays;
                string note = "";
                if (daysLeft < 0) note = " (⚠️ ĐÃ QUÁ HẠN)";
                else if (daysLeft <= 3) note = $" (⏳ Sắp đến hạn: còn {daysLeft} ngày)";

                msg += $"- {ct.Sach?.TenSach ?? "Sách"} (Hẹn trả: {ct.NgayHenTra:dd/MM/yyyy}){note}\n";
            }
            msg += $"\n[BUTTON: Xem chi tiết lịch sử thuê | {LinkLichSuThue}]";
            return new { Message = msg };
        }

        public async Task<object> GetLichSuDonHangAsync(int idKhachHang)
        {
            var listDon = await _context.HoaDons.AsNoTracking().Where(h => h.IdKhachHang == idKhachHang && h.LoaiHoaDon == "Giao hàng").OrderByDescending(h => h.ThoiGianTao).Take(3).ToListAsync();

            if (!listDon.Any()) return new { Message = "Tài khoản của bạn chưa có đơn hàng nào." };

            string msg = "3 đơn hàng mua sắm gần đây nhất của bạn:\n";
            foreach (var hd in listDon)
            {
                string token = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(hd.IdHoaDon.ToString()));
                msg += $"- Đơn #{hd.IdHoaDon} (Trạng thái: {hd.TrangThai ?? hd.TrangThaiGiaoHang})\n[BUTTON: Chi tiết đơn #{hd.IdHoaDon} | {LinkChiTietDonHang}{token}]\n\n";
            }
            msg += $"[BUTTON: Xem tất cả lịch sử | {LinkLichSuDonHang}]";
            return new { Message = msg };
        }

        public async Task<object> GetGoiYComboAsync()
        {
            return await _cache.GetOrCreateAsync<object>("CACHE_GOI_Y_COMBO", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                var sp = await _context.SanPhams.AsNoTracking()
                    .Where(s => s.TrangThaiKinhDoanh)
                    .OrderBy(r => Guid.NewGuid()).FirstOrDefaultAsync();

                var sach = await _context.Sachs.AsNoTracking()
                    .Include(s => s.SachTacGias).ThenInclude(st => st.TacGia)
                    .Where(s => s.SoLuongHienCo > 0).OrderBy(r => Guid.NewGuid()).FirstOrDefaultAsync();

                if (sp == null || sach == null)
                    return new { Message = "Xin lỗi bạn, hiện tại mình chưa thể gom được combo. Mời bạn ghé xem trực tiếp tại menu của quán nhé!" };

                string tkSp = _protectorSanPham.ProtectToUrlSafe(sp.IdSanPham.ToString());
                string tkSach = _protectorSach.ProtectToUrlSafe(sach.IdSach.ToString());

                string msg = "Hôm nay là một ngày tuyệt vời! Mình gợi ý cho bạn một combo cực chill tại Cafebook nhé:\n\n" +
                             $"🍵 **Nước uống:** {sp.TenSanPham} ({sp.GiaBan:N0}đ)\n" +
                             $"[BUTTON: Chi tiết món | {LinkDetailSP}?token={tkSp}]\n\n" +
                             $"📖 **Sách hay:** {sach.TenSach} \n" +
                             $"[BUTTON: Xem sách | {LinkDetailSach}?token={tkSach}]\n\n" +
                             $"Bạn thấy combo này thế nào? Ghé quán tìm một góc nhỏ màu nâu be ấm áp để thưởng thức ngay nhé!";

                return new { Message = msg };
            }) ?? new { };
        }

        public async Task<object> TheoDoiDonHangAsync(int idHoaDon, int idKhachHang)
        {
            var hd = await _context.HoaDons.AsNoTracking().FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon && h.IdKhachHang == idKhachHang);
            if (hd == null) return new { Message = $"Mình không tìm thấy đơn hàng mã #{idHoaDon} trong tài khoản của bạn. Bạn kiểm tra lại mã nha." };

            string token = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(hd.IdHoaDon.ToString()));
            return new { Message = $"Đơn hàng #{hd.IdHoaDon} của bạn đang ở trạng thái: **{hd.TrangThaiGiaoHang ?? hd.TrangThai ?? "Đang xử lý"}**.\n[BUTTON: Xem chi tiết đơn | {LinkChiTietDonHang}{token}]" };
        }
    }
}