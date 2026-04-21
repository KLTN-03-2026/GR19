using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/chitietsanpham")]
    [ApiController]
    public class ChiTietSanPhamController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ChiTietSanPhamController(CafebookDbContext context)
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
            var sp = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSanPham == id);

            if (sp == null) return NotFound("Không tìm thấy sản phẩm.");

            // Lấy 4 món gợi ý
            var suggestions_raw = await _context.DeXuatSanPhams
                .Where(d => d.IdSanPhamGoc == id)
                .Include(d => d.SanPhamDeXuat)
                .OrderByDescending(d => d.DoLienQuan)
                .Take(4)
                .Select(d => d.SanPhamDeXuat)
                .ToListAsync();

            var dto = new ChiTietSanPhamDto
            {
                IdSanPham = sp.IdSanPham,
                TenSanPham = sp.TenSanPham,
                TenLoaiSP = sp.DanhMuc?.TenDanhMuc,
                DonGia = sp.GiaBan,
                HinhAnhUrl = GetFullImageUrl(sp.HinhAnh),
                MoTa = sp.MoTa,
                GoiY = suggestions_raw.Select(g => new SanPhamGoiYDto
                {
                    IdSanPham = g.IdSanPham,
                    TenSanPham = g.TenSanPham,
                    DonGia = g.GiaBan,
                    AnhSanPhamUrl = GetFullImageUrl(g.HinhAnh)
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("{id}/danhgia")]
        public async Task<IActionResult> GetReviews(int id)
        {
            // BƯỚC 1: LẤY DỮ LIỆU THÔ TỪ DATABASE (Không gọi hàm GetFullImageUrl ở đây)
            var reviews_raw = await _context.DanhGias
                .Where(d => d.idSanPham == id && d.TrangThai == "Hiển thị")
                .Include(d => d.KhachHang)
                .Include(d => d.PhanHoiDanhGias).ThenInclude(p => p.NhanVien)
                .AsNoTracking()
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new
                {
                    TenKhachHang = d.KhachHang != null ? d.KhachHang.HoTen : "Khách hàng",
                    AnhDaiDienTho = d.KhachHang != null ? d.KhachHang.AnhDaiDien : null, // Chỉ lấy tên file ảnh thô
                    SoSao = d.SoSao,
                    NgayTao = d.NgayTao,
                    BinhLuan = d.BinhLuan,
                    PhanHoi = d.PhanHoiDanhGias.OrderByDescending(p => p.NgayTao).Select(p => new PhanHoiChiTietDto
                    {
                        TenNhanVien = p.NhanVien != null ? p.NhanVien.HoTen : "Nhân viên",
                        NoiDung = p.NoiDung,
                        NgayTao = p.NgayTao
                    }).FirstOrDefault()
                })
                .ToListAsync(); // <--- Chạy lệnh SQL và đưa dữ liệu lên RAM tại đây

            // BƯỚC 2: CHUYỂN ĐỔI DỮ LIỆU TRÊN RAM (Lúc này gọi hàm GetFullImageUrl thoải mái)
            var reviews = reviews_raw.Select(r => new DanhGiaChiTietDto
            {
                TenKhachHang = r.TenKhachHang,
                AvatarKhachHang = GetFullImageUrl(r.AnhDaiDienTho), // Xử lý link ảnh an toàn trên RAM
                SoSao = r.SoSao,
                NgayTao = r.NgayTao,
                BinhLuan = r.BinhLuan,
                PhanHoi = r.PhanHoi
            }).ToList();

            return Ok(reviews);
        }
    }
}