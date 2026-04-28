using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CafebookApi.Services
{
    public class AiToolService
    {
        private readonly CafebookDbContext _context;
        private readonly IDataProtector _protectorSanPham;
        private readonly IDataProtector _protectorSach;
        private readonly IDataProtector _protectorHoaDon;

        private const int SlotDurationHours = 2;

        // ==========================================
        // KHAI BÁO CÁC ĐƯỜNG DẪN CHUẨN
        // ==========================================
        private const string LinkTaiKhoan = "/tai-khoan/tong-quan";
        private const string LinkThongTinCaNhan = "/tai-khoan/thong-tin-ca-nhan";
        private const string LinkLichSuThue = "/tai-khoan/lich-su-thue-sach";
        private const string LinkLichSuDatBan = "/tai-khoan/lich-su-dat-ban";
        private const string LinkLichSuDonHang = "/tai-khoan/lich-su-don-hang";
        private const string LinkDoiMatKhau = "/tai-khoan/doi-mat-khau";
        private const string LinkThongTinSach = "/thu-vien-sach/tim-kiem";
        private const string LinkLienHe = "/lien-he";
        private const string LinkDangNhap = "/dang-nhap";
        private const string LinkDatBan = "/dat-ban";
        private const string LinkGioHang = "/gio-hang";
        private const string LinkChinhSach = "/chinh-sach";

        // Đường dẫn cần ghép Token động
        private const string LinkDetailSach = "/chi-tiet-sach/";
        private const string LinkDetailSP = "/chi-tiet-san-pham/";
        private const string LinkChiTietDonHang = "/tai-khoan/lich-su-don-hang/";

        public AiToolService(CafebookDbContext context, IDataProtectionProvider provider)
        {
            _context = context;
            _protectorSanPham = provider.CreateProtector("Cafebook.SanPham.Id");
            _protectorSach = provider.CreateProtector("Cafebook.Sach.Id");
            _protectorHoaDon = provider.CreateProtector("Cafebook.HoaDon.Id");
        }

        // ==================================================================================
        // NHÓM 1: THÔNG TIN CHUNG
        // ==================================================================================

        public async Task<object> GetThongTinChungAsync()
        {
            var keys = new List<string> {
                "ThongTin_TenQuan", "ThongTin_DiaChi", "ThongTin_SoDienThoai", "ThongTin_GioiThieu",
                "ThongTin_GioMoCua", "ThongTin_GioDongCua", "ThongTin_ThuMoCua",
                "Wifi_MatKhau",
                "LienHe_Facebook", "LienHe_Zalo", "LienHe_Email", "LienHe_Website",
                "DiemTichLuy_DoiVND", "DiemTichLuy_NhanVND",
                "Sach_SoNgayMuonToiDa", "Sach_PhiThue", "Sach_PhiTraTreMoiNgay", "Sach_DiemPhieuThue", "Sach_PhatGiamDoMoi1Percent"
            };

            var settings = await _context.CaiDats.AsNoTracking()
                .Where(c => keys.Contains(c.TenCaiDat))
                .ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            string GetVal(string key) => settings.ContainsKey(key) ? settings[key] : "Chưa cập nhật";
            string FormatMoney(string val) => double.TryParse(val, out double d) ? $"{d:N0}đ" : val;

            return new
            {
                ThongTinCoBan = new
                {
                    TenQuan = GetVal("ThongTin_TenQuan"),
                    DiaChi = GetVal("ThongTin_DiaChi"),
                    Hotline = GetVal("ThongTin_SoDienThoai"),
                    ThoiGianHoatDong = $"{GetVal("ThongTin_GioMoCua")} - {GetVal("ThongTin_GioDongCua")} (Mở cửa thứ: {GetVal("ThongTin_ThuMoCua")})",
                    GioiThieu = GetVal("ThongTin_GioiThieu")
                },
                Wifi = new { MatKhau = GetVal("Wifi_MatKhau") },
                QuyDinhSach = new
                {
                    HanMuon = $"{GetVal("Sach_SoNgayMuonToiDa")} ngày",
                    PhiThue = $"{FormatMoney(GetVal("Sach_PhiThue"))} (trừ khi trả sách)",
                    PhatQuaHan = $"{FormatMoney(GetVal("Sach_PhiTraTreMoiNgay"))}/ngày"
                },
                Actions = new List<object>
                {
                    new { Label = "Thông tin liên hệ", Link = LinkLienHe },
                    new { Label = "Chính sách & Quy định", Link = LinkChinhSach }
                }
            };
        }

        public async Task<object> GetKhuyenMaiAsync()
        {
            var now = DateTime.Now;
            var listKM = await _context.KhuyenMais.AsNoTracking()
                .Where(k => k.TrangThai == "Hoạt động" && k.NgayBatDau <= now && k.NgayKetThuc >= now)
                .OrderByDescending(k => k.GiaTriGiam)
                .Take(3)
                .Select(k => new
                {
                    ChuongTrinh = k.TenChuongTrinh,
                    Giam = k.LoaiGiamGia == "Phần trăm" ? $"{k.GiaTriGiam}%" : $"{k.GiaTriGiam:N0}đ",
                    MoTa = k.MoTa,
                    Han = k.NgayKetThuc.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            if (!listKM.Any()) return "Hiện tại quán chưa có chương trình khuyến mãi nào đang diễn ra.";
            return new { DanhSachKhuyenMai = listKM };
        }

        // ==================================================================================
        // NHÓM 2: SẢN PHẨM (TỐI ƯU SQL TÌM KIẾM CẮT TỪ KHÓA)
        // ==================================================================================

        public async Task<object> KiemTraSanPhamAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new { Status = "NotFound", Message = "Vui lòng cung cấp tên món." };

            var keywords = keyword.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var query = _context.SanPhams.AsNoTracking().Include(s => s.DanhMuc).AsQueryable();

            foreach (var kw in keywords)
            {
                query = query.Where(s => s.TenSanPham.ToLower().Contains(kw));
            }

            var sp = await query.FirstOrDefaultAsync();

            if (sp == null) return new { Status = "NotFound", Message = $"Không tìm thấy món nào khớp với '{keyword}' trong menu." };

            string token = _protectorSanPham.Protect(sp.IdSanPham.ToString());

            return new
            {
                Name = sp.TenSanPham,
                Category = sp.DanhMuc?.TenDanhMuc ?? "Khác",
                Price = sp.GiaBan,
                Desc = sp.MoTa,
                Status = sp.TrangThaiKinhDoanh ? "Đang kinh doanh" : "Ngừng kinh doanh",
                Actions = new[] { new { Label = "Xem chi tiết món", Link = $"{LinkDetailSP}{token}" } }
            };
        }

        public async Task<object> TimMonTheoLoaiAsync(string loaiMon)
        {
            var list = await _context.SanPhams.AsNoTracking()
                .Include(s => s.DanhMuc)
                .Where(s => s.DanhMuc != null && s.DanhMuc.TenDanhMuc.Contains(loaiMon) && s.TrangThaiKinhDoanh)
                .OrderBy(s => s.TenSanPham)
                .Take(5)
                .ToListAsync();

            if (!list.Any()) return new { Message = $"Không tìm thấy loại món nào tên là '{loaiMon}'." };

            var returnList = list.Select(s => new
            {
                Ten = s.TenSanPham,
                Gia = s.GiaBan,
                Link = $"{LinkDetailSP}{_protectorSanPham.Protect(s.IdSanPham.ToString())}"
            }).ToList();

            var actions = returnList.Select(s => new { Label = s.Ten, Link = s.Link }).ToList();

            return new { Message = $"Tìm thấy {returnList.Count} món thuộc loại '{loaiMon}':", DanhSach = returnList, Actions = actions };
        }

        public async Task<object> GetGoiYSanPhamAsync()
        {
            var list = await _context.SanPhams.AsNoTracking()
                .Where(s => s.TrangThaiKinhDoanh)
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .ToListAsync();

            var returnList = list.Select(s => new
            {
                Ten = s.TenSanPham,
                Gia = s.GiaBan,
                Link = $"{LinkDetailSP}{_protectorSanPham.Protect(s.IdSanPham.ToString())}"
            }).ToList();

            var actions = returnList.Select(s => new { Label = s.Ten, Link = s.Link }).ToList();
            return new { Message = "Một vài món ngon hôm nay:", DanhSach = returnList, Actions = actions };
        }

        // ==================================================================================
        // NHÓM 3: SÁCH THƯ VIỆN (TỐI ƯU SQL TÌM KIẾM CẮT TỪ KHÓA)
        // ==================================================================================

        public async Task<object> KiemTraSachAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new { Status = "NotFound", Message = "Vui lòng cung cấp tên sách." };

            var keywords = keyword.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var query = _context.Sachs.AsNoTracking().Include(s => s.SachTacGias).ThenInclude(st => st.TacGia).AsQueryable();

            foreach (var kw in keywords)
            {
                query = query.Where(s => s.TenSach.ToLower().Contains(kw));
            }

            var sach = await query.FirstOrDefaultAsync();

            if (sach == null) return new { Status = "NotFound", Message = $"Không tìm thấy sách nào khớp với '{keyword}'." };

            string token = _protectorSach.Protect(sach.IdSach.ToString());

            return new
            {
                Name = sach.TenSach,
                Author = string.Join(", ", sach.SachTacGias.Where(x => x.TacGia != null).Select(x => x.TacGia.TenTacGia)),
                Location = sach.ViTri,
                Stock = sach.SoLuongHienCo,
                Status = sach.SoLuongHienCo > 0 ? "Có sẵn" : "Đã hết",
                Actions = new[] { new { Label = "Xem thông tin sách", Link = $"{LinkDetailSach}{token}" } }
            };
        }

        public async Task<object> TimSachTheoTacGiaAsync(string tenTacGia)
        {
            var list = await _context.Sachs.AsNoTracking()
                .Include(s => s.SachTacGias).ThenInclude(st => st.TacGia)
                .Where(s => s.SachTacGias.Any(t => t.TacGia != null && t.TacGia.TenTacGia.Contains(tenTacGia)))
                .Take(5)
                .ToListAsync();

            if (!list.Any()) return new { Message = $"Không tìm thấy sách nào của tác giả '{tenTacGia}'." };

            var returnList = list.Select(s => new
            {
                Ten = s.TenSach,
                ViTri = s.ViTri,
                Link = $"{LinkDetailSach}{_protectorSach.Protect(s.IdSach.ToString())}"
            }).ToList();

            var actions = returnList.Select(s => new { Label = s.Ten, Link = s.Link }).ToList();
            return new { Message = $"Sách của tác giả {tenTacGia}:", DanhSach = returnList, Actions = actions };
        }

        public async Task<object> GetGoiYSachAsync()
        {
            var list = await _context.Sachs.AsNoTracking()
                .Include(s => s.SachTacGias).ThenInclude(st => st.TacGia)
                .Where(s => s.SoLuongHienCo > 0)
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .ToListAsync();

            var returnList = list.Select(s => new
            {
                Ten = s.TenSach,
                TacGia = string.Join(", ", s.SachTacGias.Where(st => st.TacGia != null).Select(st => st.TacGia.TenTacGia)),
                Link = $"{LinkDetailSach}{_protectorSach.Protect(s.IdSach.ToString())}"
            }).ToList();

            var actions = returnList.Select(s => new { Label = s.Ten, Link = s.Link }).ToList();
            return new { Message = "Những cuốn sách thú vị:", DanhSach = returnList, Actions = actions };
        }

        // ==================================================================================
        // NHÓM 4: ĐẶT BÀN
        // ==================================================================================

        public async Task<object> KiemTraBanTrongAsync(int soNguoi)
        {
            var bans = await _context.Bans.AsNoTracking()
                .Include(b => b.KhuVuc)
                .Where(b => b.SoGhe >= soNguoi && b.TrangThai != "Hỏng" && b.TrangThai != "Bảo trì" && b.TrangThai != "Đã Đặt")
                .OrderBy(b => b.SoGhe)
                .Take(6)
                .Select(b => new
                {
                    TenBan = b.SoBan,
                    SoGhe = b.SoGhe,
                    KhuVuc = b.KhuVuc != null ? b.KhuVuc.TenKhuVuc : "Chung",
                    Mota = $"Bàn {b.SoBan} ({b.SoGhe} ghế) - {(b.KhuVuc != null ? b.KhuVuc.TenKhuVuc : "Chung")}"
                })
                .ToListAsync();

            if (!bans.Any()) return new { Message = "Rất tiếc, không tìm thấy bàn nào phù hợp với số lượng người này." };
            return new { DanhSachBanTrong = bans, Note = "Vui lòng chọn một bàn từ danh sách trên." };
        }

        public async Task<object> DatBanThucSuAsync(string tenBan, int soNguoi, DateTime thoiGianDat, string hoTen, string sdt, string email, string ghiChu, int? idKhachHang)
        {
            if (thoiGianDat < DateTime.Now.AddMinutes(10))
                return new { Error = "Vui lòng đặt trước ít nhất 15 phút so với hiện tại." };

            var openingHours = await GetAndParseOpeningHours();
            if (!IsTimeValid(thoiGianDat, openingHours))
                return new { Error = $"Quán đóng cửa vào giờ đó. Giờ mở cửa: {openingHours.Open:hh\\:mm} - {openingHours.Close:hh\\:mm}" };

            var ban = await _context.Bans.FirstOrDefaultAsync(b => b.SoBan == tenBan || b.SoBan.Contains(tenBan));
            if (ban == null) return new { Error = $"Không tìm thấy bàn tên '{tenBan}'." };

            DateTime thoiGianKetThuc = thoiGianDat.AddHours(SlotDurationHours);
            bool isConflict = await _context.PhieuDatBans.AnyAsync(p =>
                p.IdBan == ban.IdBan &&
                p.TrangThai != "Đã Hủy" && p.TrangThai != "Hoàn thành" &&
                (
                    (thoiGianDat >= p.ThoiGianDat && thoiGianDat < p.ThoiGianDat.AddHours(SlotDurationHours)) ||
                    (thoiGianKetThuc > p.ThoiGianDat && thoiGianKetThuc <= p.ThoiGianDat.AddHours(SlotDurationHours)) ||
                    (thoiGianDat <= p.ThoiGianDat && thoiGianKetThuc >= p.ThoiGianDat.AddHours(SlotDurationHours))
                )
            );

            if (isConflict) return new { Error = $"Rất tiếc, bàn {ban.SoBan} đã có người đặt trong khung giờ này." };

            try
            {
                _context.IsAiOperation = true;
                int finalIdKhach;
                if (idKhachHang.HasValue && idKhachHang > 0)
                {
                    finalIdKhach = idKhachHang.Value;
                    _context.AiCustomerId = finalIdKhach;
                }
                else
                {
                    var guest = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt);
                    if (guest == null)
                    {
                        guest = new KhachHang { HoTen = hoTen, SoDienThoai = sdt, Email = email, TaiKhoanTam = true, TenDangNhap = sdt, MatKhau = Guid.NewGuid().ToString("N").Substring(0, 10), NgayTao = DateTime.Now, BiKhoa = false };
                        _context.KhachHangs.Add(guest);
                        await _context.SaveChangesAsync();
                    }
                    finalIdKhach = guest.IdKhachHang;
                    _context.AiCustomerId = finalIdKhach;
                }

                var phieu = new PhieuDatBan { IdBan = ban.IdBan, IdKhachHang = finalIdKhach, SoLuongKhach = soNguoi, ThoiGianDat = thoiGianDat, GhiChu = ghiChu, TrangThai = "Chờ xác nhận", HoTenKhach = hoTen, SdtKhach = sdt };
                _context.PhieuDatBans.Add(phieu);
                var tb = new ThongBao { NoiDung = $"Khách {hoTen} ({sdt}) đặt {ban.SoBan} lúc {thoiGianDat:HH:mm dd/MM}", LoaiThongBao = "DatBan", ThoiGianTao = DateTime.Now, DaXem = false, IdLienQuan = phieu.IdPhieuDatBan };
                _context.ThongBaos.Add(tb);
                await _context.SaveChangesAsync();

                return new { Status = "Success", Message = $"Đặt bàn {ban.SoBan} thành công! Mã phiếu: {phieu.IdPhieuDatBan}.", CanhBao = "Bàn sẽ tự động hủy nếu quý khách đến trễ 15 phút.", Actions = new[] { new { Label = "Quản lý đặt bàn", Link = LinkLichSuDatBan } } };
            }
            finally
            {
                _context.IsAiOperation = false;
                _context.AiCustomerId = null;
            }
        }

        private class OpeningHours { public TimeSpan Open { get; set; } = new TimeSpan(6, 0, 0); public TimeSpan Close { get; set; } = new TimeSpan(23, 0, 0); }
        private async Task<OpeningHours> GetAndParseOpeningHours() { var setting = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(cd => cd.TenCaiDat == "LienHe_GioMoCua"); string settingValue = (setting != null && !string.IsNullOrEmpty(setting.GiaTri)) ? setting.GiaTri : "06:00 - 23:00"; var hours = new OpeningHours(); try { var match = Regex.Match(settingValue, @"(\d{1,2}:\d{2})\s*-\s*(\d{1,2}:\d{2})"); if (match.Success) { if (TimeSpan.TryParse(match.Groups[1].Value, out TimeSpan open)) hours.Open = open; if (TimeSpan.TryParse(match.Groups[2].Value, out TimeSpan close)) hours.Close = close; } } catch { } return hours; }
        private bool IsTimeValid(DateTime thoiGianDat, OpeningHours hours) { var timeOfDay = thoiGianDat.TimeOfDay; return timeOfDay >= hours.Open && timeOfDay <= hours.Close; }
        private string MaskInfo(string input) { if (string.IsNullOrEmpty(input) || input.Length < 4) return "***"; return input.Substring(0, 3) + "***" + input.Substring(input.Length - 2); }

        // ==================================================================================
        // NHÓM 5: TÀI KHOẢN & LỊCH SỬ CÁ NHÂN
        // ==================================================================================

        public async Task<object> GetDiemTichLuyAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.FindAsync(idKhachHang);
            return kh == null ? "Lỗi." : new { Message = $"Tài khoản của bạn hiện có {kh.DiemTichLuy:N0} điểm.", DiemTichLuy = kh.DiemTichLuy, Actions = new[] { new { Label = "Quản lý điểm", Link = LinkTaiKhoan } } };
        }

        public async Task<object> GetThongTinCaNhanAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.FindAsync(idKhachHang);
            return kh == null ? "Lỗi." : new { HoTen = kh.HoTen, SDT = MaskInfo(kh.SoDienThoai ?? ""), Email = MaskInfo(kh.Email ?? ""), Actions = new List<object> { new { Label = "Cập nhật hồ sơ", Link = LinkThongTinCaNhan } } };
        }

        public async Task<object> GetTongQuanTaiKhoanAsync(int idKhachHang)
        {
            var kh = await _context.KhachHangs.FindAsync(idKhachHang);
            return kh == null ? "Lỗi." : new { HoTen = kh.HoTen, Diem = kh.DiemTichLuy, Actions = new List<object> { new { Label = "Vào trang quản lý", Link = LinkTaiKhoan } } };
        }

        public async Task<object> GetLichSuDatBanAsync(int idKhachHang)
        {
            return new { Message = "Để xem chi tiết các lịch đặt bàn của bạn, vui lòng truy cập đường dẫn bên dưới:", Actions = new[] { new { Label = "Lịch sử đặt bàn", Link = LinkLichSuDatBan } } };
        }

        public async Task<object> HuyDatBanAsync(int idPhieuDat, string lyDo, int idKhachHang)
        {
            return "Đã gửi yêu cầu hủy. Để xem trạng thái mới nhất, bạn vui lòng truy cập Lịch sử đặt bàn nhé.";
        }

        public async Task<object> GetLichSuThueSachAsync(int idKhachHang)
        {
            return new { Message = "Danh sách các quyển sách bạn đang mượn được lưu trong lịch sử thuê:", Actions = new[] { new { Label = "Sách đang mượn", Link = LinkLichSuThue } } };
        }

        public async Task<object> GetLichSuDonHangAsync(int idKhachHang)
        {
            return new { Message = "Tất cả các đơn hàng mua nước/bánh của bạn đều nằm ở đây nhé:", Actions = new[] { new { Label = "Lịch sử đơn hàng", Link = LinkLichSuDonHang } } };
        }

        public async Task<object> TheoDoiDonHangAsync(int idHoaDon, int idKhachHang)
        {
            var hd = await _context.HoaDons.AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHoaDon == idHoaDon && h.IdKhachHang == idKhachHang);

            if (hd == null)
            {
                return new { Message = $"Mình không tìm thấy đơn hàng mã #{idHoaDon} trong tài khoản của bạn. Bạn kiểm tra lại mã đơn giúp mình nhé!" };
            }

            string token = _protectorHoaDon.Protect(hd.IdHoaDon.ToString());

            return new
            {
                Message = $"Đơn hàng **#{hd.IdHoaDon}** của bạn hiện đang ở trạng thái: **{hd.TrangThai ?? "Đang xử lý"}**.",
                Actions = new[] { new { Label = "Xem chi tiết đơn", Link = $"{LinkChiTietDonHang}{token}" } }
            };
        }
    }
}