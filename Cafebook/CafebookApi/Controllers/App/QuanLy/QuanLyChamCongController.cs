using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

// --- FIX LỖI: Khai báo bí danh (Alias) chỉ định rõ ràng class Entity ---
using NhanVienEntity = CafebookModel.Model.ModelEntities.NhanVien;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-chamcong")]
    [ApiController]
    public class QuanLyChamCongController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyChamCongController(CafebookDbContext context) { _context = context; }

        [HttpGet("nhanvien-lookup")]
        public async Task<IActionResult> GetNhanVienLookup()
        {
            // SỬ DỤNG BÍ DANH TẠI ĐÂY
            var list = await _context.Set<NhanVienEntity>().AsNoTracking()
                .OrderBy(nv => nv.HoTen)
                .Select(nv => new ChamCongNhanVienLookupDto { IdNhanVien = nv.IdNhanVien, HoTen = nv.HoTen })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay)
        {
            var query = from b in _context.Set<BangChamCong>()
                        join l in _context.Set<LichLamViec>() on b.IdLichLamViec equals l.IdLichLamViec
                        // SỬ DỤNG BÍ DANH TẠI ĐÂY ĐỂ FIX LỖI JOIN CS1941
                        join nv in _context.Set<NhanVienEntity>() on l.IdNhanVien equals nv.IdNhanVien
                        join c in _context.Set<CaLamViec>() on l.IdCa equals c.IdCa
                        select new { b, l, nv, c };

            if (tuNgay.HasValue) query = query.Where(x => x.l.NgayLam >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(x => x.l.NgayLam <= denNgay.Value.Date);

            var rawData = await query.OrderByDescending(x => x.l.NgayLam).ThenByDescending(x => x.c.GioBatDau).ToListAsync();

            var data = rawData.Select(x => {
                TimeSpan? gioVaoTime = x.b.GioVao?.TimeOfDay;
                TimeSpan? gioRaTime = x.b.GioRa?.TimeOfDay;

                // TÍNH TOÁN TRẠNG THÁI ĐỘNG DỰA TRÊN CA LÀM VIỆC
                string trangThai = "Chưa chấm công";
                if (x.b.GioVao.HasValue && x.b.GioRa.HasValue)
                {
                    List<string> statusList = new();
                    if (gioVaoTime > x.c.GioBatDau) statusList.Add("Đi trễ");
                    if (gioRaTime < x.c.GioKetThuc) statusList.Add("Về sớm");
                    trangThai = statusList.Any() ? string.Join(", ", statusList) : "Hợp lệ";
                }
                else if (x.b.GioVao.HasValue) trangThai = "Thiếu giờ ra";
                else if (x.b.GioRa.HasValue) trangThai = "Thiếu giờ vào";
                if (x.l.NgayLam.Date < DateTime.Today && !x.b.GioVao.HasValue && !x.b.GioRa.HasValue) trangThai = "Vắng mặt";

                return new QuanLyChamCongGridDto
                {
                    IdChamCong = x.b.IdChamCong,
                    IdNhanVien = x.l.IdNhanVien,
                    TenNhanVien = x.nv.HoTen,
                    NgayLam = x.l.NgayLam,
                    TenCa = x.c.TenCa,
                    CaGioBatDau = x.c.GioBatDau,
                    CaGioKetThuc = x.c.GioKetThuc,
                    GioVao = gioVaoTime,
                    GioRa = gioRaTime,
                    TrangThai = trangThai,
                    GhiChuSua = x.b.GhiChuSua,
                    TongGioLam = (x.b.GioVao.HasValue && x.b.GioRa.HasValue && x.b.GioRa.Value > x.b.GioVao.Value)
                                 ? (x.b.GioRa.Value - x.b.GioVao.Value).TotalHours : 0
                };
            }).ToList();

            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuanLyChamCongUpdateDto dto)
        {
            var chamCong = await _context.Set<BangChamCong>()
                .Include(b => b.LichLamViec)
                .FirstOrDefaultAsync(b => b.IdChamCong == id);
            if (chamCong == null) return NotFound();

            var ngayLam = chamCong.LichLamViec.NgayLam.Date;

            DateTime? vao = !string.IsNullOrEmpty(dto.GioVao) && TimeSpan.TryParse(dto.GioVao, out var tsVao) ? ngayLam.Add(tsVao) : null;
            DateTime? ra = !string.IsNullOrEmpty(dto.GioRa) && TimeSpan.TryParse(dto.GioRa, out var tsRa) ? ngayLam.Add(tsRa) : null;

            if (vao.HasValue && ra.HasValue && ra <= vao) ra = ra.Value.AddDays(1); // Ca qua đêm

            chamCong.GioVao = vao;
            chamCong.GioRa = ra;
            chamCong.GhiChuSua = dto.GhiChuSua;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}