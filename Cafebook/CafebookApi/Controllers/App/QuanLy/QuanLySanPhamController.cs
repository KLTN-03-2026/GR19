using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    // CLASS WRAPPER NÀY SẼ GIẢI QUYẾT TRIỆT ĐỂ LỖI SWAGGER CRASH
    public class QuanLySanPhamSaveRequest
    {
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public int IdDanhMuc { get; set; }
        public string NhomIn { get; set; } = "Khác";
        public bool TrangThaiKinhDoanh { get; set; } = true;
        public string? MoTa { get; set; }

        public IFormFile? AnhBia { get; set; } // File ảnh đính kèm
        public bool DeleteImage { get; set; }  // Cờ hiệu xóa ảnh
    }

    [Route("api/app/quanly-sanpham")]
    [ApiController]
    public class QuanLySanPhamController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuanLySanPhamController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("lookup-danhmuc")]
        public async Task<IActionResult> GetDanhMuc() => Ok(await _context.DanhMucs.Select(d => new LookupDanhMucDto { Id = d.IdDanhMuc, Ten = d.TenDanhMuc }).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.SanPhams.Include(s => s.DanhMuc).AsNoTracking().Select(s => new QuanLySanPhamGridDto
            {
                IdSanPham = s.IdSanPham,
                TenSanPham = s.TenSanPham,
                GiaBan = s.GiaBan,
                TrangThaiKinhDoanh = s.TrangThaiKinhDoanh,
                TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : "Khác",
                HinhAnh = s.HinhAnh
            }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _context.SanPhams.FindAsync(id);
            if (s == null) return NotFound();
            return Ok(new QuanLySanPhamDetailDto { IdSanPham = s.IdSanPham, TenSanPham = s.TenSanPham, GiaBan = s.GiaBan, IdDanhMuc = s.IdDanhMuc, NhomIn = s.NhomIn ?? "Khác", TrangThaiKinhDoanh = s.TrangThaiKinhDoanh, MoTa = s.MoTa, HinhAnh = s.HinhAnh });
        }

        // ĐÃ SỬA: Gom tham số vào QuanLySanPhamSaveRequest
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] QuanLySanPhamSaveRequest request)
        {
            var entity = new SanPham
            {
                TenSanPham = request.TenSanPham,
                GiaBan = request.GiaBan,
                IdDanhMuc = request.IdDanhMuc,
                NhomIn = request.NhomIn,
                TrangThaiKinhDoanh = request.TrangThaiKinhDoanh,
                MoTa = request.MoTa
            };

            if (request.AnhBia != null)
                entity.HinhAnh = await ProcessImage(request.AnhBia, request.TenSanPham);

            _context.SanPhams.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ĐÃ SỬA: Gom tham số vào QuanLySanPhamSaveRequest
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] QuanLySanPhamSaveRequest request)
        {
            var entity = await _context.SanPhams.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenSanPham = request.TenSanPham;
            entity.GiaBan = request.GiaBan;
            entity.IdDanhMuc = request.IdDanhMuc;
            entity.NhomIn = request.NhomIn;
            entity.TrangThaiKinhDoanh = request.TrangThaiKinhDoanh;
            entity.MoTa = request.MoTa;

            if (request.DeleteImage)
                entity.HinhAnh = null;
            else if (request.AnhBia != null)
                entity.HinhAnh = await ProcessImage(request.AnhBia, request.TenSanPham);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.SanPhams.FindAsync(id);
            if (entity == null) return NotFound();
            _context.SanPhams.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<string> ProcessImage(IFormFile file, string tenSanPham)
        {
            string folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "foods");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = SlugifyUtil.GenerateSlug(tenSanPham) + "-" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);

            using (var stream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return HinhAnhPaths.UrlFoods + "/" + fileName;
        }
    }
}