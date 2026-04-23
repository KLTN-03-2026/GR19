// Vị trí lưu: CafebookApi/Controllers/Web/KhachHang/TrangChuController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/trangchu")]
    [ApiController]
    [AllowAnonymous]
    public class TrangChuController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public TrangChuController(CafebookDbContext context)
        {
            _context = context;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private string? GetFullImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return $"{baseUrl}{relativePath.Replace(Path.DirectorySeparatorChar, '/')}";
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetTrangChuData()
        {
            var keys = new[] {
                "ThongTin_TenQuan", "ThongTin_GioiThieu", "ThongTin_BannerImage",
                "ThongTin_DiaChi", "ThongTin_SoDienThoai", "ThongTin_GioMoCua",
                "ThongTin_GioDongCua", "LienHe_Email", "LienHe_Facebook",
                "LienHe_Zalo", "LienHe_Website","ThongTin_ThuMoCua"
            };

            var settings = await _context.CaiDats
                .Where(c => keys.Contains(c.TenCaiDat))
                .AsNoTracking()
                .ToListAsync();

            var thongTinChung = new ThongTinChungDto
            {
                TenQuan = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_TenQuan")?.GiaTri ?? "Cafebook",
                GioiThieu = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioiThieu")?.GiaTri,
                BannerImageUrl = GetFullImageUrl(settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_BannerImage")?.GiaTri),
                DiaChi = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_DiaChi")?.GiaTri,
                SoDienThoai = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_SoDienThoai")?.GiaTri,
                EmailLienHe = settings.FirstOrDefault(c => c.TenCaiDat == "LienHe_Email")?.GiaTri,
                GioMoCua = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioMoCua")?.GiaTri,
                GioDongCua = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_GioDongCua")?.GiaTri,
                FacebookUrl = settings.FirstOrDefault(c => c.TenCaiDat == "LienHe_Facebook")?.GiaTri,
                ZaloUrl = settings.FirstOrDefault(c => c.TenCaiDat == "LienHe_Zalo")?.GiaTri,
                WebsiteUrl = settings.FirstOrDefault(c => c.TenCaiDat == "LienHe_Website")?.GiaTri,
                SoBanTrong = await _context.Bans.CountAsync(b => b.TrangThai == "Trống"),
                ThuMoCua = settings.FirstOrDefault(c => c.TenCaiDat == "ThongTin_ThuMoCua")?.GiaTri,
                SoSachSanSang = await _context.Sachs.CountAsync(s => s.SoLuongHienCo > 0)
            };

            var promotions = await _context.KhuyenMais
                .Where(km => km.TrangThai == "Hoạt động" && km.NgayBatDau <= DateTime.Now && km.NgayKetThuc >= DateTime.Now)
                .OrderBy(km => km.NgayBatDau).Take(3)
                .AsNoTracking()
                .Select(km => new KhuyenMaiDto
                {
                    TenKhuyenMai = km.TenChuongTrinh,
                    MoTa = km.MoTa,
                    DieuKienApDung = km.DieuKienApDung
                }).ToListAsync();

            var monNoiBat_Raw = await _context.SanPhams
                .Where(sp => sp.TrangThaiKinhDoanh == true)
                .OrderByDescending(sp => sp.IdSanPham).Take(5)
                .AsNoTracking()
                .Select(sp => new { sp.IdSanPham, sp.TenSanPham, sp.GiaBan, sp.HinhAnh })
                .ToListAsync();

            var monNoiBat = monNoiBat_Raw.Select(sp => new SanPhamDto
            {
                IdSanPham = sp.IdSanPham,
                TenSanPham = sp.TenSanPham,
                DonGia = sp.GiaBan,
                AnhSanPhamUrl = GetFullImageUrl(sp.HinhAnh)
            }).ToList();

            var sachNoiBat_Raw = await _context.Sachs
                .Include(s => s.SachTacGias).ThenInclude(stg => stg.TacGia)
                .OrderByDescending(s => s.SoLuongHienCo).Take(4)
                .AsNoTracking()
                .Select(s => new {
                    s.IdSach,
                    s.TenSach,
                    TacGias = s.SachTacGias.Select(stg => stg.TacGia.TenTacGia),
                    s.AnhBia
                }).ToListAsync();

            var sachNoiBat = sachNoiBat_Raw.Select(s => new SachDto
            {
                IdSach = s.IdSach,
                TieuDe = s.TenSach,
                TacGia = string.Join(", ", s.TacGias),
                AnhBiaUrl = GetFullImageUrl(s.AnhBia)
            }).ToList();

            return Ok(new TrangChuDto
            {
                Info = thongTinChung,
                Promotions = promotions,
                MonNoiBat = monNoiBat,
                SachNoiBat = sachNoiBat
            });
        }
    }
}