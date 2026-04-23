using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using CafebookModel.Model.ModelApp;
using CafebookModel.Utils; // BẮT BUỘC THÊM ĐỂ GỌI HinhAnhPaths
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/thucdon")]
    [ApiController]
    [AllowAnonymous]
    public class ThucDonController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public ThucDonController(CafebookDbContext context)
        {
            _context = context;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private string? GetFullImageUrl(string? dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) return null;

            var request = HttpContext.Request;
            var cleanBaseUrl = $"{request.Scheme}://{request.Host}";
            var cleanPath = dbPath.Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');

            return $"{cleanBaseUrl}/{cleanPath}";
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var danhMucs = await _context.DanhMucs
                .OrderBy(d => d.TenDanhMuc)
                .Select(d => new ThucDonFilterDto { Id = d.IdDanhMuc, Ten = d.TenDanhMuc })
                .ToListAsync();
            return Ok(danhMucs);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] int? loaiId,
            [FromQuery] string? search,
            [FromQuery] decimal? giaMin,
            [FromQuery] decimal? giaMax,
            [FromQuery] string sortBy = "ten_asc",
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 9)
        {
            var query = _context.SanPhams
                .Include(s => s.DanhMuc)
                .Where(s => s.TrangThaiKinhDoanh == true);

            if (loaiId.HasValue && loaiId > 0) query = query.Where(s => s.IdDanhMuc == loaiId);
            if (giaMin.HasValue) query = query.Where(s => s.GiaBan >= giaMin);
            if (giaMax.HasValue) query = query.Where(s => s.GiaBan <= giaMax);
            if (!string.IsNullOrEmpty(search)) query = query.Where(s => s.TenSanPham.Contains(search));

            query = sortBy switch
            {
                "ten_desc" => query.OrderByDescending(s => s.TenSanPham),
                "gia_asc" => query.OrderBy(s => s.GiaBan),
                "gia_desc" => query.OrderByDescending(s => s.GiaBan),
                _ => query.OrderBy(s => s.TenSanPham),
            };

            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;

            var items_raw = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new {
                    s.IdSanPham,
                    s.TenSanPham,
                    TenLoaiSP = s.DanhMuc.TenDanhMuc,
                    s.GiaBan,
                    s.HinhAnh
                }).ToListAsync();

            var result = new ThucDonDto
            {
                Items = items_raw.Select(s => new SanPhamThucDonDto
                {
                    IdSanPham = s.IdSanPham,
                    TenSanPham = s.TenSanPham,
                    TenLoaiSP = s.TenLoaiSP,
                    DonGia = s.GiaBan,

                    AnhSanPhamUrl = GetFullImageUrl(s.HinhAnh)
                }).ToList(),
                TotalPages = totalPages,
                CurrentPage = pageNum
            };
            return Ok(result);
        }
    }
}