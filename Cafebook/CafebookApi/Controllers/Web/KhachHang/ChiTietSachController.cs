using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/chitietsach")]
    [ApiController]
    public class ChiTietSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ChiTietSachController(CafebookDbContext context)
        {
            _context = context;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private string? GetFullImageUrl(string? dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) return null;
            var request = HttpContext.Request;
            var cleanBaseUrl = $"{request.Scheme}://{request.Host}";
            var cleanPath = dbPath.Replace(System.IO.Path.DirectorySeparatorChar, '/').TrimStart('/');
            return $"{cleanBaseUrl}/{cleanPath}";
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            // BƯỚC 1: Lấy dữ liệu thô lên RAM để tránh lỗi Client Evaluation của EF Core
            var sach_raw = await _context.Sachs
                .Include(s => s.SachTacGias).ThenInclude(stg => stg.TacGia)
                .Include(s => s.SachTheLoais).ThenInclude(stl => stl.TheLoai)
                .Include(s => s.SachNhaXuatBans).ThenInclude(snxb => snxb.NhaXuatBan)
                .Include(s => s.DeXuatSachGocs).ThenInclude(ds => ds.SachDeXuat)
                .AsNoTracking()
                .Where(s => s.IdSach == id)
                .Select(s => new
                {
                    s.IdSach,
                    s.TenSach,
                    s.AnhBia,
                    s.MoTa,
                    s.GiaBia,
                    s.ViTri,
                    s.SoLuongTong,
                    s.SoLuongHienCo,
                    TacGias = s.SachTacGias.Select(stg => new { stg.IdTacGia, stg.TacGia.TenTacGia }).ToList(),
                    TheLoais = s.SachTheLoais.Select(stl => new { stl.IdTheLoai, stl.TheLoai.TenTheLoai }).ToList(),
                    NhaXuatBans = s.SachNhaXuatBans.Select(snxb => new { snxb.IdNhaXuatBan, snxb.NhaXuatBan.TenNhaXuatBan }).ToList(),
                    GoiY = s.DeXuatSachGocs.Select(ds => new { ds.IdSachDeXuat, ds.SachDeXuat.TenSach, ds.SachDeXuat.AnhBia, ds.SachDeXuat.GiaBia }).ToList()
                })
                .FirstOrDefaultAsync();

            if (sach_raw == null) return NotFound("Không tìm thấy sách.");

            // BƯỚC 2: Map vào DTO chuyên biệt và xử lý Link ảnh an toàn trên RAM
            var dto = new ChiTietSachDto
            {
                IdSach = sach_raw.IdSach,
                TieuDe = sach_raw.TenSach,
                AnhBiaUrl = GetFullImageUrl(sach_raw.AnhBia),
                MoTa = sach_raw.MoTa,
                GiaBia = sach_raw.GiaBia ?? 0,
                ViTri = sach_raw.ViTri,
                TongSoLuong = sach_raw.SoLuongTong,
                SoLuongCoSan = sach_raw.SoLuongHienCo,

                TacGias = sach_raw.TacGias.Select(t => new ChiTietSachTacGiaDto { IdTacGia = t.IdTacGia, TenTacGia = t.TenTacGia }).ToList(),
                TheLoais = sach_raw.TheLoais.Select(tl => new ChiTietSachTheLoaiDto { IdTheLoai = tl.IdTheLoai, TenTheLoai = tl.TenTheLoai }).ToList(),
                NhaXuatBans = sach_raw.NhaXuatBans.Select(nxb => new ChiTietSachNxbDto { IdNhaXuatBan = nxb.IdNhaXuatBan, TenNhaXuatBan = nxb.TenNhaXuatBan }).ToList(),

                GoiY = sach_raw.GoiY.Select(g => new ChiTietSachGoiYDto
                {
                    IdSach = g.IdSachDeXuat,
                    TieuDe = g.TenSach,
                    AnhBiaUrl = GetFullImageUrl(g.AnhBia),
                    GiaBia = g.GiaBia ?? 0
                }).ToList()
            };

            return Ok(dto);
        }
    }
}