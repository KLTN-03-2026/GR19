using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/phatluong")]
    [ApiController]
    [Authorize]
    public class PhatLuongController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public PhatLuongController(CafebookDbContext context) { _context = context; }

        [HttpGet("danhsach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int nam, [FromQuery] int thang)
        {
            var query = _context.Set<PhieuLuong>().Include(p => p.NhanVien).AsNoTracking().AsQueryable();

            if (nam > 0) query = query.Where(p => p.Nam == nam);
            if (thang > 0) query = query.Where(p => p.Thang == thang);

            var data = await query.OrderByDescending(p => p.IdPhieuLuong)
                .Select(p => new PhatLuongGridDto
                {
                    IdPhieuLuong = p.IdPhieuLuong,
                    TenNhanVien = p.NhanVien.HoTen,
                    KyLuong = $"Tháng {p.Thang}/{p.Nam} (Chốt: {p.NgayTao:dd/MM})",
                    TongGioLam = p.TongGioLam,
                    ThucLanh = p.ThucLanh,
                    TrangThai = p.TrangThai
                }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("chitiet/{id}")]
        public async Task<IActionResult> GetChiTiet(int id)
        {
            var p = await _context.Set<PhieuLuong>().Include(x => x.NhanVien).FirstOrDefaultAsync(x => x.IdPhieuLuong == id);
            if (p == null) return NotFound("Không tìm thấy phiếu lương.");

            var thuongPhats = await _context.Set<PhieuThuongPhat>().Where(t => t.IdPhieuLuong == id).AsNoTracking().ToListAsync();

            // SỬA: Truy vấn cấu hình quán từ bảng CaiDat
            var configs = await _context.Set<CaiDat>()
                .Where(c => c.TenCaiDat.StartsWith("ThongTin_"))
                .AsNoTracking()
                .ToListAsync();

            string tenQuan = configs.FirstOrDefault(c => c.TenCaiDat == "ThongTin_TenQuan")?.GiaTri ?? "CAFEBOOK SYSTEM";
            string diaChi = configs.FirstOrDefault(c => c.TenCaiDat == "ThongTin_DiaChi")?.GiaTri ?? "";
            string sdt = configs.FirstOrDefault(c => c.TenCaiDat == "ThongTin_SoDienThoai")?.GiaTri ?? "";

            var chiTiet = new PhatLuongDetailDto
            {
                IdPhieuLuong = p.IdPhieuLuong,
                TenNhanVien = p.NhanVien.HoTen,
                KyLuong = $"Lương Tuần (Của Tháng {p.Thang}/{p.Nam})",
                NgayChot = p.NgayTao,
                LuongCoBan = p.LuongCoBan,
                TongGioLam = p.TongGioLam,
                TienThuong = p.TienThuong ?? 0m,
                KhauTru = p.KhauTru ?? 0m,
                ThucLanh = p.ThucLanh,
                TrangThai = p.TrangThai,
                TenQuan = tenQuan,
                DiaChiQuan = diaChi,
                SoDienThoaiQuan = sdt,
                DanhSachThuongPhat = thuongPhats.Select(t => new ChiTietThuongPhatPhatLuongDto
                {
                    Loai = t.SoTien >= 0 ? "Thưởng" : "Phạt",
                    LyDo = t.LyDo,
                    SoTien = Math.Abs(t.SoTien)
                }).ToList()
            };

            return Ok(chiTiet);
        }

        [HttpPut("xacnhan/{id}")]
        public async Task<IActionResult> XacNhanPhat(int id)
        {
            var p = await _context.Set<PhieuLuong>().FindAsync(id);
            if (p == null) return NotFound("Không tìm thấy phiếu lương.");

            if (p.TrangThai == "Đã phát") return BadRequest("Phiếu lương này đã được phát trước đó.");

            p.TrangThai = "Đã phát";
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}