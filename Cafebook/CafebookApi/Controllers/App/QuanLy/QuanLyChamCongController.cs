// File: CafebookApi/Controllers/App/QuanLy/QuanLyChamCongController.cs
using CafebookApi.Data;
using CafebookModel.Model.ModelEntities;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using NhanVienEntity = CafebookModel.Model.ModelEntities.NhanVien;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-chamcong")]
    [ApiController]
    [Authorize]
    public class QuanLyChamCongController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyChamCongController(CafebookDbContext context) { _context = context; }

        [HttpGet("nhanvien-lookup")]
        public async Task<IActionResult> GetNhanVienLookup()
        {
            var list = await _context.Set<NhanVienEntity>().AsNoTracking()
                .OrderBy(nv => nv.HoTen)
                .Select(nv => new ChamCongNhanVienLookupDto { IdNhanVien = nv.IdNhanVien, HoTen = nv.HoTen })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] int? idNhanVien, [FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay)
        {
            // 1. LẤY CẤU HÌNH TỪ BẢNG CÀI ĐẶT
            var settingTre = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatDiTre_Phut");
            var settingSom = await _context.CaiDats.AsNoTracking().FirstOrDefaultAsync(c => c.TenCaiDat == "HR_PhatRaSom_Phut");

            int phutTreChoPhep = (settingTre != null && int.TryParse(settingTre.GiaTri, out int t)) ? t : 10;
            int phutSomChoPhep = (settingSom != null && int.TryParse(settingSom.GiaTri, out int s)) ? s : 10;

            var query = _context.Set<BangChamCong>()
                .Include(b => b.LichLamViec).ThenInclude(l => l.CaLamViec)
                .Include(b => b.LichLamViec).ThenInclude(l => l.NhanVien)
                .AsNoTracking()
                .Where(b => b.LichLamViec != null && b.LichLamViec.TrangThai == "Đã duyệt");

            if (idNhanVien.HasValue && idNhanVien.Value > 0)
                query = query.Where(b => b.LichLamViec.IdNhanVien == idNhanVien.Value);

            if (tuNgay.HasValue)
                query = query.Where(b => b.LichLamViec.NgayLam >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(b => b.LichLamViec.NgayLam <= denNgay.Value.Date);

            var list = await query.Select(b => new { b, l = b.LichLamViec, nv = b.LichLamViec.NhanVien, ca = b.LichLamViec.CaLamViec }).ToListAsync();

            var data = list.Select(x => {
                string trangThai = "Vắng mặt";

                // 2. ĐÁNH GIÁ TRẠNG THÁI DỰA TRÊN CẤU HÌNH ĐỘNG
                if (x.b.GioVao.HasValue)
                {
                    if (x.b.GioRa.HasValue)
                    {
                        // Số phút đi trễ = Giờ vào thực tế - Giờ bắt đầu ca
                        bool diTre = (x.b.GioVao.Value.TimeOfDay - x.ca.GioBatDau).TotalMinutes > phutTreChoPhep;

                        // Số phút về sớm = Giờ kết thúc ca - Giờ ra thực tế
                        bool veSom = (x.ca.GioKetThuc - x.b.GioRa.Value.TimeOfDay).TotalMinutes > phutSomChoPhep;

                        if (diTre && veSom) trangThai = "Đi trễ, Về sớm";
                        else if (diTre) trangThai = "Đi trễ";
                        else if (veSom) trangThai = "Về sớm";
                        else trangThai = "Đúng giờ";
                    }
                    else
                    {
                        trangThai = "Đang làm";
                    }
                }

                var gioVaoTime = x.b.GioVao?.TimeOfDay;
                var gioRaTime = x.b.GioRa?.TimeOfDay;

                return new QuanLyChamCongGridDto
                {
                    IdChamCong = x.b.IdChamCong,
                    IdNhanVien = x.nv.IdNhanVien,
                    TenNhanVien = x.nv.HoTen,
                    NgayLam = x.l.NgayLam,
                    TenCa = x.ca.TenCa,
                    CaGioBatDau = x.ca.GioBatDau,
                    CaGioKetThuc = x.ca.GioKetThuc,
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

            if (vao.HasValue && ra.HasValue && ra.Value < vao.Value)
                return BadRequest("Giờ ra không được nhỏ hơn giờ vào!");

            chamCong.GioVao = vao;
            chamCong.GioRa = ra;
            chamCong.GhiChuSua = dto.GhiChuSua;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}