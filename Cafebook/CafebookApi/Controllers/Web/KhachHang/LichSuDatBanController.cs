using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khach-hang/lich-su-dat-ban")]
    [ApiController]
    [Authorize(Roles = "KhachHang")] // Lớp bảo mật 2: Bắt buộc có quyền Khách hàng
    public class LichSuDatBanController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public LichSuDatBanController(CafebookDbContext context)
        {
            _context = context;
        }

        // Lấy UserID từ Token (Chống IDOR tuyệt đối, không nhận ID từ tham số URL)
        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            int.TryParse(idClaim?.Value, out int id);
            return id;
        }

        [HttpGet]
        public async Task<IActionResult> GetLichSuDatBan(
            [FromQuery] int page = 1,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { Message = "Phiên đăng nhập không hợp lệ." });

            int pageSize = 5; // Cố định 5 phiếu / 1 trang theo yêu cầu

            // 1. Khởi tạo Query ban đầu (Chưa truy vấn DB)
            var query = _context.PhieuDatBans
                .Include(p => p.Ban)
                .Where(p => p.IdKhachHang == userId)
                .AsQueryable();

            // 2. Áp dụng các bộ lọc (Filtering)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.TrangThai == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.ThoiGianDat.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.ThoiGianDat.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Ban != null && p.Ban.SoBan.ToLower().Contains(searchLower)) ||
                    (p.GhiChu != null && p.GhiChu.ToLower().Contains(searchLower)));
            }

            // 3. Đếm tổng số bản ghi thỏa mãn điều kiện
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 4. Lấy dữ liệu của trang hiện tại (Paging)
            var items = await query
                    .OrderByDescending(p => p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đã xác nhận")
                    .ThenByDescending(p => p.ThoiGianDat)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new LichSuDatBanDto
                    {
                        IdPhieuDatBan = p.IdPhieuDatBan,
                        TenBan = p.Ban != null ? p.Ban.SoBan : "Bàn không xác định",
                        ThoiGianDat = p.ThoiGianDat,
                        SoLuongKhach = p.SoLuongKhach,
                        TrangThai = p.TrangThai ?? "Chờ xử lý",
                        GhiChu = p.GhiChu
                    })
                    .AsNoTracking()
                    .ToListAsync();

            // 5. Trả về cấu trúc Paged Response
            var response = new PagedLichSuResponseDto
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return Ok(response);
        }


        [HttpPut("huy/{id}")]
        public async Task<IActionResult> HuyDatBan(int id, [FromBody] string? lyDoHuy)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { Message = "Phiên đăng nhập không hợp lệ." });

            // Lấy phiếu đặt bàn kèm theo thông tin Bàn và Khách Hàng để tạo thông báo
            var phieuDatBan = await _context.PhieuDatBans
                .Include(p => p.Ban)
                .Include(p => p.KhachHang)
                .FirstOrDefaultAsync(p => p.IdPhieuDatBan == id && p.IdKhachHang == userId);

            if (phieuDatBan == null) return NotFound(new { Message = "Không tìm thấy phiếu." });

            if (phieuDatBan.TrangThai != "Chờ xác nhận" && phieuDatBan.TrangThai != "Đã xác nhận")
            {
                return BadRequest(new { Message = "Trạng thái hiện tại không cho phép hủy." });
            }

            // 1. Cập nhật ghi chú và trạng thái
            string cleanReason = string.IsNullOrWhiteSpace(lyDoHuy) ? "Khách yêu cầu hủy" : lyDoHuy.Trim();
            phieuDatBan.GhiChu = string.IsNullOrWhiteSpace(phieuDatBan.GhiChu)
                ? cleanReason
                : $"{phieuDatBan.GhiChu} | {cleanReason}";

            phieuDatBan.TrangThai = "Đã hủy";

            // 2. TẠO THÔNG BÁO CHO NHÂN VIÊN/QUẢN LÝ
            string tenKhach = phieuDatBan.KhachHang?.HoTen ?? "Khách hàng";
            string soBan = phieuDatBan.Ban?.SoBan ?? "chưa xếp bàn";

            var thongBao = new ThongBao
            {
                NoiDung = $"Khách hàng {tenKhach} vừa hủy phiếu đặt {soBan}. Lý do: {cleanReason}",
                ThoiGianTao = DateTime.Now,
                LoaiThongBao = "HuyDatBan",
                IdLienQuan = phieuDatBan.IdBan, 
                DaXem = false
            };

            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Hủy đặt bàn thành công." });
        }
    }
}