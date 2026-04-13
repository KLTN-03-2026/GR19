using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils; // Kéo thư viện Utils vào
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-sach")]
    [ApiController]
    public class QuanLySachController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuanLySachController(CafebookDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            if (string.IsNullOrEmpty(_env.WebRootPath))
                _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var sachs = await _context.Set<Sach>()
                .AsNoTracking()
                .OrderByDescending(s => s.IdSach)
                .ToListAsync();

            var sachIds = sachs.Select(s => s.IdSach).ToList();
            var sachTacGias = await _context.Set<SachTacGia>().Where(st => sachIds.Contains(st.IdSach)).AsNoTracking().ToListAsync();
            var tacGias = await _context.Set<TacGia>().AsNoTracking().ToListAsync();
            var sachTheLoais = await _context.Set<SachTheLoai>().Where(st => sachIds.Contains(st.IdSach)).AsNoTracking().ToListAsync();
            var theLoais = await _context.Set<TheLoai>().AsNoTracking().ToListAsync();

            var data = sachs.Select(s => new QuanLySachGridDto
            {
                IdSach = s.IdSach,
                TenSach = s.TenSach,
                TenTacGia = string.Join(", ", sachTacGias.Where(st => st.IdSach == s.IdSach).Select(st => tacGias.FirstOrDefault(t => t.IdTacGia == st.IdTacGia)?.TenTacGia).Where(name => !string.IsNullOrEmpty(name))),
                TenTheLoai = string.Join(", ", sachTheLoais.Where(st => st.IdSach == s.IdSach).Select(st => theLoais.FirstOrDefault(t => t.IdTheLoai == st.IdTheLoai)?.TenTheLoai).Where(name => !string.IsNullOrEmpty(name))),
                ViTri = s.ViTri,
                SoLuongTong = s.SoLuongTong,
                SoLuongHienCo = s.SoLuongHienCo,
                SoLuongDangMuon = s.SoLuongTong - s.SoLuongHienCo
            }).ToList();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var s = await _context.Set<Sach>().AsNoTracking().FirstOrDefaultAsync(x => x.IdSach == id);
            if (s == null) return NotFound();

            var stgs = await _context.Set<SachTacGia>().Where(x => x.IdSach == id).ToListAsync();
            var stls = await _context.Set<SachTheLoai>().Where(x => x.IdSach == id).ToListAsync();
            var snxbs = await _context.Set<SachNhaXuatBan>().Where(x => x.IdSach == id).ToListAsync();

            var tgIds = stgs.Select(x => x.IdTacGia).ToList();
            var tlIds = stls.Select(x => x.IdTheLoai).ToList();
            var nxbIds = snxbs.Select(x => x.IdNhaXuatBan).ToList();

            var tacGias = await _context.Set<TacGia>().Where(t => tgIds.Contains(t.IdTacGia)).Select(t => t.TenTacGia).ToListAsync();
            var theLoais = await _context.Set<TheLoai>().Where(t => tlIds.Contains(t.IdTheLoai)).Select(t => t.TenTheLoai).ToListAsync();
            var nxbs = await _context.Set<NhaXuatBan>().Where(t => nxbIds.Contains(t.IdNhaXuatBan)).Select(t => t.TenNhaXuatBan).ToListAsync();

            var dto = new QuanLySachDetailDto
            {
                IdSach = s.IdSach,
                TenSach = s.TenSach,
                DanhSachTacGia = string.Join(", ", tacGias),
                DanhSachTheLoai = string.Join(", ", theLoais),
                DanhSachNhaXuatBan = string.Join(", ", nxbs),
                ViTri = s.ViTri,
                NamXuatBan = s.NamXuatBan,
                GiaBia = s.GiaBia,
                SoLuongTong = s.SoLuongTong,
                SoLuongHienCo = s.SoLuongHienCo,
                MoTa = s.MoTa,
                AnhBia = s.AnhBia
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromForm] IFormCollection form)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newBook = new Sach
                {
                    TenSach = form["TenSach"].ToString().Trim(),
                    NamXuatBan = int.TryParse(form["NamXuatBan"], out int nxb) ? nxb : null,
                    GiaBia = decimal.TryParse(form["GiaBia"], out decimal gb) ? gb : 0,
                    SoLuongTong = int.TryParse(form["SoLuongTong"], out int sl) ? sl : 0,
                    SoLuongHienCo = int.TryParse(form["SoLuongTong"], out int st) ? st : 0,
                    ViTri = form["ViTri"].ToString(),
                    MoTa = form["MoTa"].ToString()
                };

                // Lưu thực thể trước để lấy IdSach phát sinh
                _context.Set<Sach>().Add(newBook);
                await _context.SaveChangesAsync();

                // TÍCH HỢP SLUGIFY VÀ ĐỔI TÊN ẢNH
                if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    string extension = Path.GetExtension(file.FileName);
                    string slug = newBook.TenSach.GenerateSlug();
                    string fileName = $"{newBook.IdSach}_{slug}{extension}";

                    // Sử dụng thư mục từ HinhAnhPaths thay vì hardcode
                    string folderPath = Path.Combine(_env.WebRootPath, HinhAnhPaths.UrlBooks.TrimStart('/'));
                    Directory.CreateDirectory(folderPath);
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }

                    newBook.AnhBia = $"{HinhAnhPaths.UrlBooks}/{fileName}";
                    await _context.SaveChangesAsync(); // Cập nhật lại đường dẫn ảnh
                }

                await UpdateSachRelations(newBook.IdSach, form["DanhSachTacGia"], form["DanhSachTheLoai"], form["DanhSachNhaXuatBan"]);

                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] IFormCollection form)
        {
            var book = await _context.Set<Sach>().FindAsync(id);
            if (book == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                book.TenSach = form["TenSach"].ToString().Trim();
                book.NamXuatBan = int.TryParse(form["NamXuatBan"], out int nxb) ? nxb : null;
                book.GiaBia = decimal.TryParse(form["GiaBia"], out decimal gb) ? gb : 0;
                book.ViTri = form["ViTri"].ToString();

                int soLuongTongMoi = int.TryParse(form["SoLuongTong"], out int sl) ? sl : 0;
                int chenhLech = soLuongTongMoi - book.SoLuongTong;
                book.SoLuongTong = soLuongTongMoi;
                book.SoLuongHienCo += chenhLech;
                if (book.SoLuongHienCo < 0) book.SoLuongHienCo = 0;

                book.MoTa = form["MoTa"].ToString();

                // TÍCH HỢP SLUGIFY VÀ ĐỔI TÊN ẢNH
                if (bool.TryParse(form["XoaAnhBia"], out bool xoaAnh) && xoaAnh)
                {
                    book.AnhBia = null;
                }
                else if (form.Files.Count > 0)
                {
                    var file = form.Files[0];
                    string extension = Path.GetExtension(file.FileName);
                    string slug = book.TenSach.GenerateSlug();
                    string fileName = $"{book.IdSach}_{slug}{extension}";

                    string folderPath = Path.Combine(_env.WebRootPath, HinhAnhPaths.UrlBooks.TrimStart('/'));
                    Directory.CreateDirectory(folderPath);
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
                    book.AnhBia = $"{HinhAnhPaths.UrlBooks}/{fileName}";
                }

                await UpdateSachRelations(id, form["DanhSachTacGia"], form["DanhSachTheLoai"], form["DanhSachNhaXuatBan"]);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Set<Sach>().FindAsync(id);
            if (book == null) return NotFound();

            try
            {
                _context.Set<SachTacGia>().RemoveRange(_context.Set<SachTacGia>().Where(x => x.IdSach == id));
                _context.Set<SachTheLoai>().RemoveRange(_context.Set<SachTheLoai>().Where(x => x.IdSach == id));
                _context.Set<SachNhaXuatBan>().RemoveRange(_context.Set<SachNhaXuatBan>().Where(x => x.IdSach == id));

                _context.Set<Sach>().Remove(book);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException)
            {
                return Conflict("Không thể xóa sách này do đã từng được liên kết trong lịch sử cho thuê.");
            }
        }

        // TỰ ĐỘNG THÊM DANH MỤC NẾU CHƯA TỒN TẠI
        private async Task UpdateSachRelations(int idSach, string? tacGiasStr, string? theLoaisStr, string? nxbsStr)
        {
            // Xử lý Tác Giả
            _context.Set<SachTacGia>().RemoveRange(_context.Set<SachTacGia>().Where(x => x.IdSach == idSach));
            if (!string.IsNullOrWhiteSpace(tacGiasStr))
            {
                var names = tacGiasStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Distinct();
                foreach (var name in names)
                {
                    var tg = await _context.Set<TacGia>().FirstOrDefaultAsync(t => t.TenTacGia == name) ?? new TacGia { TenTacGia = name };
                    if (tg.IdTacGia == 0) _context.Set<TacGia>().Add(tg); // Tự động Add nếu chưa có
                    await _context.SaveChangesAsync();
                    _context.Set<SachTacGia>().Add(new SachTacGia { IdSach = idSach, IdTacGia = tg.IdTacGia });
                }
            }

            // Xử lý Thể Loại
            _context.Set<SachTheLoai>().RemoveRange(_context.Set<SachTheLoai>().Where(x => x.IdSach == idSach));
            if (!string.IsNullOrWhiteSpace(theLoaisStr))
            {
                var names = theLoaisStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Distinct();
                foreach (var name in names)
                {
                    var tl = await _context.Set<TheLoai>().FirstOrDefaultAsync(t => t.TenTheLoai == name) ?? new TheLoai { TenTheLoai = name };
                    if (tl.IdTheLoai == 0) _context.Set<TheLoai>().Add(tl); // Tự động Add nếu chưa có
                    await _context.SaveChangesAsync();
                    _context.Set<SachTheLoai>().Add(new SachTheLoai { IdSach = idSach, IdTheLoai = tl.IdTheLoai });
                }
            }

            // Xử lý NXB
            _context.Set<SachNhaXuatBan>().RemoveRange(_context.Set<SachNhaXuatBan>().Where(x => x.IdSach == idSach));
            if (!string.IsNullOrWhiteSpace(nxbsStr))
            {
                var names = nxbsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Distinct();
                foreach (var name in names)
                {
                    var nxb = await _context.Set<NhaXuatBan>().FirstOrDefaultAsync(t => t.TenNhaXuatBan == name) ?? new NhaXuatBan { TenNhaXuatBan = name };
                    if (nxb.IdNhaXuatBan == 0) _context.Set<NhaXuatBan>().Add(nxb); // Tự động Add nếu chưa có
                    await _context.SaveChangesAsync();
                    _context.Set<SachNhaXuatBan>().Add(new SachNhaXuatBan { IdSach = idSach, IdNhaXuatBan = nxb.IdNhaXuatBan });
                }
            }
            await _context.SaveChangesAsync();
        }

        [HttpGet("lookup/theloai")]
        public async Task<IActionResult> GetTheLoais() => Ok(await _context.Set<TheLoai>().Select(t => new QuanLySachFilterLookupDto { Id = t.IdTheLoai, Ten = t.TenTheLoai }).ToListAsync());

        [HttpGet("lookup/tacgia")]
        public async Task<IActionResult> GetTacGias() => Ok(await _context.Set<TacGia>().Select(t => new QuanLySachFilterLookupDto { Id = t.IdTacGia, Ten = t.TenTacGia }).ToListAsync());

        [HttpGet("lookup/nxb")]
        public async Task<IActionResult> GetNXBs() => Ok(await _context.Set<NhaXuatBan>().Select(t => new QuanLySachFilterLookupDto { Id = t.IdNhaXuatBan, Ten = t.TenNhaXuatBan }).ToListAsync());
    }
}