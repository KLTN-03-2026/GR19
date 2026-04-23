using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/timkiemsach")]
    [ApiController]
    [AllowAnonymous]
    public class TimKiemSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public TimKiemSachController(CafebookDbContext context)
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

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] int? idTacGia, [FromQuery] int? idTheLoai, [FromQuery] int? idNXB)
        {
            var query = _context.Sachs.AsQueryable();
            var resultDto = new TimKiemSachResultDto();

            // Xác định tiêu đề và mô tả trang dựa trên tham số
            if (idTacGia.HasValue)
            {
                var tacGia = await _context.TacGias.FindAsync(idTacGia.Value);
                if (tacGia == null) return NotFound("Không tìm thấy tác giả.");

                resultDto.TieuDeTrang = $"Sách của tác giả: {tacGia.TenTacGia}";
                resultDto.MoTaTrang = tacGia.GioiThieu;

                query = query.Where(s => s.SachTacGias.Any(stg => stg.IdTacGia == idTacGia.Value));
            }
            else if (idTheLoai.HasValue)
            {
                var theLoai = await _context.TheLoais.FindAsync(idTheLoai.Value);
                if (theLoai == null) return NotFound("Không tìm thấy thể loại.");

                resultDto.TieuDeTrang = $"Sách thuộc thể loại: {theLoai.TenTheLoai}";
                resultDto.MoTaTrang = theLoai.MoTa;

                query = query.Where(s => s.SachTheLoais.Any(stl => stl.IdTheLoai == idTheLoai.Value));
            }
            else if (idNXB.HasValue)
            {
                var nxb = await _context.NhaXuatBans.FindAsync(idNXB.Value);
                if (nxb == null) return NotFound("Không tìm thấy NXB.");

                resultDto.TieuDeTrang = $"Sách của NXB: {nxb.TenNhaXuatBan}";
                resultDto.MoTaTrang = nxb.MoTa;

                query = query.Where(s => s.SachNhaXuatBans.Any(snxb => snxb.IdNhaXuatBan == idNXB.Value));
            }
            else
            {
                return BadRequest("Cần cung cấp một tiêu chí tìm kiếm.");
            }

            // Tách làm 2 bước để chống lỗi Constant của Entity Framework
            var rawList = await query
                .Select(s => new {
                    s.IdSach,
                    s.TenSach,
                    s.AnhBia,
                    s.GiaBia
                })
                .ToListAsync();

            resultDto.SachList = rawList.Select(s => new TimKiemSachCardDto
            {
                IdSach = s.IdSach,
                TieuDe = s.TenSach,
                AnhBiaUrl = GetFullImageUrl(s.AnhBia),
                GiaBia = s.GiaBia ?? 0
            }).ToList();

            return Ok(resultDto);
        }
    }
}