using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/thuvien")]
    [ApiController]
    public class ThuVienSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        // CONSTRUCTOR SIÊU GỌN, KHÔNG CẦN ĐỌC CONFIG
        public ThuVienSachController(CafebookDbContext context)
        {
            _context = context;
        }

        // HÀM LẤY ẢNH TỰ ĐỘNG NHẬN DIỆN MÁY CHỦ
        [ApiExplorerSettings(IgnoreApi = true)]
        private string? GetFullImageUrl(string? dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) return null;
            var request = HttpContext.Request;
            var cleanBaseUrl = $"{request.Scheme}://{request.Host}";
            var cleanPath = dbPath.Replace(System.IO.Path.DirectorySeparatorChar, '/').TrimStart('/');
            return $"{cleanBaseUrl}/{cleanPath}";
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var filters = new SachFiltersDto
            {
                TheLoais = await _context.TheLoais
                    .OrderBy(d => d.TenTheLoai)
                    // SỬ DỤNG DTO CÔ LẬP
                    .Select(d => new ThuVienSachFilterItemDto { Id = d.IdTheLoai, Ten = d.TenTheLoai })
                    .ToListAsync()
            };
            return Ok(filters);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? search,
            [FromQuery] int? theLoaiId,
            [FromQuery] string? trangThai,
            [FromQuery] string sortBy = "ten_asc",
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 12)
        {
            var query = _context.Sachs.AsQueryable();

            if (theLoaiId.HasValue && theLoaiId > 0)
                query = query.Where(s => s.SachTheLoais.Any(stl => stl.IdTheLoai == theLoaiId));

            if (trangThai == "con_sach") query = query.Where(s => s.SoLuongHienCo > 0);
            else if (trangThai == "het_sach") query = query.Where(s => s.SoLuongHienCo <= 0);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s =>
                    s.TenSach.ToLower().Contains(searchLower) ||
                    s.SachTacGias.Any(stg => stg.TacGia.TenTacGia.ToLower().Contains(searchLower))
                );
            }

            query = sortBy switch
            {
                "ten_desc" => query.OrderByDescending(s => s.TenSach),
                "gia_asc" => query.OrderBy(s => s.GiaBia),
                "gia_desc" => query.OrderByDescending(s => s.GiaBia),
                _ => query.OrderBy(s => s.TenSach),
            };

            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;

            // Xử lý chống lỗi EF Core Constant bằng cách gọi ToListAsync trước khi GetFullImageUrl
            var items_raw = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new {
                    s.IdSach,
                    s.TenSach,
                    TacGia = string.Join(", ", s.SachTacGias.Select(stg => stg.TacGia.TenTacGia)),
                    s.GiaBia,
                    s.SoLuongHienCo,
                    s.AnhBia
                })
                .ToListAsync();

            var items_dto = items_raw.Select(s => new SachCardDto
            {
                IdSach = s.IdSach,
                TieuDe = s.TenSach,
                TacGia = string.IsNullOrEmpty(s.TacGia) ? "Không rõ" : s.TacGia,
                GiaBia = s.GiaBia ?? 0,
                SoLuongCoSan = s.SoLuongHienCo,
                AnhBiaUrl = GetFullImageUrl(s.AnhBia)
            }).ToList();

            return Ok(new SachPhanTrangDto
            {
                Items = items_dto,
                TotalPages = totalPages,
                CurrentPage = pageNum
            });
        }
    }
}