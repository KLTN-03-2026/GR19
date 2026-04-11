using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-nguyenlieu")]
    [ApiController]
    public class QuanLyNguyenLieuController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyNguyenLieuController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.NguyenLieus.AsNoTracking()
                .Select(nl => new QuanLyNguyenLieuGridDto
                {
                    IdNguyenLieu = nl.IdNguyenLieu,
                    TenNguyenLieu = nl.TenNguyenLieu,
                    TonKho = nl.TonKho,
                    DonViTinh = nl.DonViTinh,
                    TonKhoToiThieu = nl.TonKhoToiThieu
                })
                .OrderBy(nl => nl.TenNguyenLieu)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyNguyenLieuSaveDto dto)
        {
            if (await _context.NguyenLieus.AnyAsync(n => n.TenNguyenLieu.ToLower() == dto.TenNguyenLieu.ToLower()))
            {
                return Conflict("Tên nguyên liệu đã tồn tại trong hệ thống.");
            }

            var entity = new NguyenLieu
            {
                TenNguyenLieu = dto.TenNguyenLieu,
                DonViTinh = dto.DonViTinh,
                TonKhoToiThieu = dto.TonKhoToiThieu,
                TonKho = dto.TonKho // Mặc định thường là 0
            };

            _context.NguyenLieus.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyNguyenLieuSaveDto dto)
        {
            if (await _context.NguyenLieus.AnyAsync(n => n.TenNguyenLieu.ToLower() == dto.TenNguyenLieu.ToLower() && n.IdNguyenLieu != id))
            {
                return Conflict("Tên nguyên liệu đã tồn tại trong hệ thống.");
            }

            var entity = await _context.NguyenLieus.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenNguyenLieu = dto.TenNguyenLieu;
            entity.DonViTinh = dto.DonViTinh;
            entity.TonKhoToiThieu = dto.TonKhoToiThieu;
            // Không cập nhật TonKho ở đây, TonKho chỉ được cập nhật qua Nhập/Xuất/Kiểm kho

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Ràng buộc 1: Không cho xóa nếu đang có Tồn kho > 0
            var entity = await _context.NguyenLieus.FindAsync(id);
            if (entity == null) return NotFound();

            if (entity.TonKho > 0)
            {
                return Conflict("Không thể xóa nguyên liệu đang có số lượng tồn kho lớn hơn 0.");
            }

            // Ràng buộc 2: Không cho xóa nếu đã được dùng trong Định lượng (Công thức)
            if (await _context.DinhLuongs.AnyAsync(d => d.IdNguyenLieu == id))
            {
                return Conflict("Không thể xóa nguyên liệu đang được sử dụng trong định lượng sản phẩm.");
            }

            _context.NguyenLieus.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}