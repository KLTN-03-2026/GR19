using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-phanquyen")]
    [ApiController]
    public class QuanLyPhanQuyenController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyPhanQuyenController(CafebookDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách Nhân viên để phân quyền
        [HttpGet("nhanvien")]
        public async Task<IActionResult> GetNhanViens()
        {
            var list = await _context.NhanViens
                .Include(n => n.VaiTro)
                .Where(n => n.TrangThaiLamViec != "Nghỉ việc") // Không phân quyền cho người đã nghỉ
                .Select(n => new PhanQuyen_NhanVienDto
                {
                    IdNhanVien = n.IdNhanVien,
                    HoTen = n.HoTen,
                    TenVaiTro = n.VaiTro.TenVaiTro
                })
                .ToListAsync();
            return Ok(list);
        }

        // 2. Lấy danh sách TẤT CẢ các Quyền có trong hệ thống
        [HttpGet("quyen")]
        public async Task<IActionResult> GetAllQuyen()
        {
            var list = await _context.Quyens
                .Select(q => new PhanQuyen_QuyenDto
                {
                    IdQuyen = q.IdQuyen,
                    TenQuyen = q.TenQuyen,
                    NhomQuyen = q.NhomQuyen
                })
                .OrderBy(q => q.NhomQuyen).ThenBy(q => q.TenQuyen)
                .ToListAsync();
            return Ok(list);
        }

        // 3. Lấy danh sách Quyền ĐANG CÓ của 1 nhân viên cụ thể
        [HttpGet("nhanvien/{idNhanVien}/quyen")]
        public async Task<IActionResult> GetQuyenOfNhanVien(int idNhanVien)
        {
            var quyenIds = await _context.NhanVienQuyens
                .Where(nq => nq.IdNhanVien == idNhanVien)
                .Select(nq => nq.IdQuyen)
                .ToListAsync();
            return Ok(quyenIds);
        }

        // 4. Lưu cấu hình quyền mới cho nhân viên
        [HttpPost("nhanvien/{idNhanVien}/quyen")]
        public async Task<IActionResult> SaveQuyenForNhanVien(int idNhanVien, [FromBody] PhanQuyen_SaveRequestDto request)
        {
            // Xóa toàn bộ quyền cũ của người này
            var oldQuyens = await _context.NhanVienQuyens.Where(nq => nq.IdNhanVien == idNhanVien).ToListAsync();
            _context.NhanVienQuyens.RemoveRange(oldQuyens);

            // Thêm các quyền mới được tick
            foreach (var quyenId in request.SelectedQuyenIds)
            {
                _context.NhanVienQuyens.Add(new NhanVien_Quyen
                {
                    IdNhanVien = idNhanVien,
                    IdQuyen = quyenId
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Lưu phân quyền thành công!" });
        }
    }
}