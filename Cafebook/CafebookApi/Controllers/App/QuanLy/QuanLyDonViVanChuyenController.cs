using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-donvivanchuyen")]
    [ApiController]
    [Authorize]
    public class QuanLyDonViVanChuyenController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyDonViVanChuyenController(CafebookDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.NguoiGiaoHangs.AsNoTracking()
                .Select(n => new QuanLyDonViVanChuyenGridDto
                {
                    IdNguoiGiaoHang = n.IdNguoiGiaoHang,
                    TenNguoiGiaoHang = n.TenNguoiGiaoHang,
                    SoDienThoai = n.SoDienThoai,
                    TrangThai = n.TrangThai ?? "Sẵn sàng"
                }).OrderBy(n => n.TenNguoiGiaoHang).ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyDonViVanChuyenSaveDto dto)
        {
            if (await _context.NguoiGiaoHangs.AnyAsync(n => n.TenNguoiGiaoHang.ToLower() == dto.TenNguoiGiaoHang.ToLower()))
                return Conflict("Tên Đơn vị Vận chuyển / Shipper đã tồn tại.");

            var entity = new NguoiGiaoHang
            {
                TenNguoiGiaoHang = dto.TenNguoiGiaoHang,
                SoDienThoai = dto.SoDienThoai ?? "",
                TrangThai = dto.TrangThai
            };

            _context.NguoiGiaoHangs.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyDonViVanChuyenSaveDto dto)
        {
            if (await _context.NguoiGiaoHangs.AnyAsync(n => n.TenNguoiGiaoHang.ToLower() == dto.TenNguoiGiaoHang.ToLower() && n.IdNguoiGiaoHang != id))
                return Conflict("Tên Đơn vị Vận chuyển / Shipper đã tồn tại.");

            var entity = await _context.NguoiGiaoHangs.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenNguoiGiaoHang = dto.TenNguoiGiaoHang;
            entity.SoDienThoai = dto.SoDienThoai ?? "";
            entity.TrangThai = dto.TrangThai;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Do trong HoaDon.cs của bạn đang Map IdNguoiGiaoHang tới NhanVien, 
            // Nếu bạn muốn rà soát xem Shipper đã giao đơn nào chưa thì cần kiểm tra thủ công cột này.
            bool isInUse = await _context.HoaDons.AnyAsync(h => h.IdNguoiGiaoHang == id);

            if (isInUse)
            {
                return Conflict("Không thể xóa vì đơn vị này đã có lịch sử giao hàng trong hệ thống.");
            }

            var entity = await _context.NguoiGiaoHangs.FindAsync(id);
            if (entity == null) return NotFound();

            _context.NguoiGiaoHangs.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}