using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelWeb.NhanVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class DonXinNghiWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public DonXinNghiWebController(CafebookDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(claim, out int id);
            return id;
        }

        // Lấy lịch sử đơn xin nghỉ của chính nhân viên đang đăng nhập
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            int idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            var history = await _context.DonXinNghis
                .Include(d => d.NguoiDuyet)
                .Where(d => d.IdNhanVien == idNhanVien)
                .OrderByDescending(d => d.IdDonXinNghi) // Đơn mới nhất lên đầu
                .Select(d => new LichSuDonXinNghiWebDto
                {
                    IdDonXinNghi = d.IdDonXinNghi,
                    LoaiDon = d.LoaiDon ?? "Khác",
                    LyDo = d.LyDo ?? "",
                    NgayBatDau = d.NgayBatDau,
                    NgayKetThuc = d.NgayKetThuc,
                    TrangThai = d.TrangThai ?? "Chờ duyệt",
                    NgayDuyet = d.NgayDuyet,
                    GhiChuPheDuyet = d.GhiChuPheDuyet,
                    NguoiDuyet = d.NguoiDuyet != null ? d.NguoiDuyet.HoTen : ""
                })
                .ToListAsync();

            return Ok(history);
        }

        // Tạo đơn xin nghỉ mới
        [HttpPost("create")]
        public async Task<IActionResult> CreateRequest([FromBody] TaoDonXinNghiWebRequest req)
        {
            int idNhanVien = GetCurrentUserId();
            if (idNhanVien == 0) return Unauthorized();

            // Validate ngày
            if (req.NgayBatDau.Date < DateTime.Now.Date)
                return BadRequest(new { message = "Ngày bắt đầu không được trong quá khứ." });

            if (req.NgayKetThuc.Date < req.NgayBatDau.Date)
                return BadRequest(new { message = "Ngày kết thúc phải lớn hơn hoặc bằng Ngày bắt đầu." });

            var donMoi = new DonXinNghi
            {
                IdNhanVien = idNhanVien,
                LoaiDon = req.LoaiDon,
                LyDo = req.LyDo,
                NgayBatDau = req.NgayBatDau,
                NgayKetThuc = req.NgayKetThuc,
                TrangThai = "Chờ duyệt"
            };

            _context.DonXinNghis.Add(donMoi);
            await _context.SaveChangesAsync();

            // QUY TẮC 9: Tạo thông báo gửi đến Quản lý hệ thống
            var nv = await _context.NhanViens.FindAsync(idNhanVien);
            var thongBao = new ThongBao
            {
                IdNhanVienTao = idNhanVien,
                NoiDung = $"Nhân viên {nv?.HoTen} vừa nộp đơn {req.LoaiDon}.",
                ThoiGianTao = DateTime.Now,
                LoaiThongBao = "DonXinNghi",
                IdLienQuan = donMoi.IdDonXinNghi,
                DaXem = false
            };
            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi đơn xin nghỉ thành công! Vui lòng chờ Quản lý duyệt." });
        }
    }
}