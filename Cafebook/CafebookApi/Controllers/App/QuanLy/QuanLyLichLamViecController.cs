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

namespace CafebookApi.Controllers.App.QuanLy
{
    public class CopyTuanDto { public DateTime SourceDate { get; set; } }

    [Route("api/app/quanly-lichlamviec")]
    [ApiController]
    [Authorize]
    public class QuanLyLichLamViecController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyLichLamViecController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("cai-dat")]
        public async Task<IActionResult> GetCaiDat()
        {
            var moCua = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "ThongTin_GioMoCua");
            var dongCua = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "ThongTin_GioDongCua");
            var thuMoCua = await _context.CaiDats.FirstOrDefaultAsync(c => c.TenCaiDat == "ThongTin_ThuMoCua");

            var dto = new QuanLyLichLamViec_CaiDatDto
            {
                GioMoCua = moCua != null ? TimeSpan.Parse(moCua.GiaTri) : new TimeSpan(7, 0, 0),
                GioDongCua = dongCua != null ? TimeSpan.Parse(dongCua.GiaTri) : new TimeSpan(22, 0, 0),
            };
            dto.ThuMoCua = (thuMoCua != null && !string.IsNullOrEmpty(thuMoCua.GiaTri))
                ? thuMoCua.GiaTri.Split(',').Select(int.Parse).ToList()
                : new List<int> { 2, 3, 4, 5, 6, 7, 8 };

            return Ok(dto);
        }

        [HttpGet("vaitro")]
        public async Task<IActionResult> GetVaiTros()
        {
            var data = await _context.VaiTros.Select(v => new QuanLyVaiTroLookupDto { IdVaiTro = v.IdVaiTro, TenVaiTro = v.TenVaiTro }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("nhanvien")]
        public async Task<IActionResult> GetNhanViens()
        {
            var data = await _context.NhanViens.Include(n => n.VaiTro)
                .Where(n => n.TrangThaiLamViec != "Nghỉ việc")
                .Select(n => new QuanLyNhanVienLookupDto { IdNhanVien = n.IdNhanVien, HoTen = n.HoTen, TenVaiTro = n.VaiTro!.TenVaiTro })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetLichData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var nhuCaus = await _context.NhuCauCaLams
                .Include(nc => nc.CaLamViec).Include(nc => nc.VaiTro)
                .Where(nc => nc.NgayLam >= fromDate.Date && nc.NgayLam <= toDate.Date).ToListAsync();

            var lichs = await _context.LichLamViecs
                .Include(l => l.NhanVien).ThenInclude(n => n.VaiTro)
                .Include(l => l.CaLamViec)
                .Where(l => l.NgayLam >= fromDate.Date && l.NgayLam <= toDate.Date).ToListAsync();

            var result = new List<QuanLyLichLamViec_ItemDto>();

            foreach (var nc in nhuCaus)
            {
                var item = new QuanLyLichLamViec_ItemDto
                {
                    IdNhuCau = nc.IdNhuCau,
                    NgayLam = nc.NgayLam,
                    IdCa = nc.IdCa,
                    TenCa = nc.CaLamViec!.TenCa,
                    GioBatDau = nc.CaLamViec.GioBatDau,
                    GioKetThuc = nc.CaLamViec.GioKetThuc,
                    TenVaiTroYeuCau = nc.VaiTro!.TenVaiTro,
                    SoLuongCan = nc.SoLuongCan,
                    LoaiYeuCau = nc.LoaiYeuCau,
                    GhiChu = nc.GhiChu
                };

                var registered = lichs.Where(l => l.NgayLam == nc.NgayLam && l.IdCa == nc.IdCa && l.NhanVien.IdVaiTro == nc.IdVaiTro).ToList();

                item.NhanViens = registered.Select(r => new QuanLyLichLamViec_NhanVienDangKyDto
                {
                    IdLichLamViec = r.IdLichLamViec,
                    IdNhanVien = r.IdNhanVien,
                    TenNhanVien = r.NhanVien!.HoTen,
                    TenVaiTro = r.NhanVien.VaiTro!.TenVaiTro,
                    TrangThai = r.TrangThai,
                    GhiChu = r.GhiChu 
                }).ToList();

                result.Add(item);
            }
            return Ok(result);
        }

        [HttpPost("nhucau")]
        public async Task<IActionResult> CreateNhuCau([FromBody] QuanLyLichLamViec_NhuCauSaveDto dto)
        {
            var exists = await _context.NhuCauCaLams.AnyAsync(nc => nc.NgayLam == dto.NgayLam.Date && nc.IdCa == dto.IdCa && nc.IdVaiTro == dto.IdVaiTro);
            if (exists) return Conflict("Nhu cầu cho vị trí này trong ca này đã tồn tại. Vui lòng chọn Sửa (Click đúp) để tăng số lượng.");

            var entity = new NhuCauCaLam { NgayLam = dto.NgayLam.Date, IdCa = dto.IdCa, IdVaiTro = dto.IdVaiTro, SoLuongCan = dto.SoLuongCan, LoaiYeuCau = dto.LoaiYeuCau, GhiChu = dto.GhiChu };
            _context.NhuCauCaLams.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo Nhu cầu thành công." });
        }

        [HttpPut("nhucau/{id}")]
        public async Task<IActionResult> UpdateNhuCau(int id, [FromBody] QuanLyLichLamViec_NhuCauSaveDto dto)
        {
            var entity = await _context.NhuCauCaLams.FindAsync(id);
            if (entity == null) return NotFound();
            entity.SoLuongCan = dto.SoLuongCan; entity.LoaiYeuCau = dto.LoaiYeuCau; entity.GhiChu = dto.GhiChu;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công." });
        }

        [HttpDelete("nhucau/{id}")]
        public async Task<IActionResult> DeleteNhuCau(int id)
        {
            var entity = await _context.NhuCauCaLams.FindAsync(id);
            if (entity == null) return NotFound();

            var lichs = await _context.LichLamViecs
                .Include(l => l.NhanVien)
                .Where(l => l.NgayLam == entity.NgayLam && l.IdCa == entity.IdCa && l.NhanVien.IdVaiTro == entity.IdVaiTro)
                .ToListAsync();

            if (lichs.Any()) _context.LichLamViecs.RemoveRange(lichs);

            _context.NhuCauCaLams.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("copy-tuan")]
        public async Task<IActionResult> CopyTuan([FromBody] CopyTuanDto req)
        {
            DateTime sourceDate = req.SourceDate;
            int diff = (7 + (sourceDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = sourceDate.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6);

            var sourceNhuCaus = await _context.NhuCauCaLams.Where(nc => nc.NgayLam.Date == sourceDate.Date).ToListAsync();

            var existingNhuCaus = await _context.NhuCauCaLams.Where(nc => nc.NgayLam >= startOfWeek && nc.NgayLam <= endOfWeek && nc.NgayLam.Date != sourceDate.Date).ToListAsync();
            var existingLichs = await _context.LichLamViecs.Where(l => l.NgayLam >= startOfWeek && l.NgayLam <= endOfWeek && l.NgayLam.Date != sourceDate.Date).ToListAsync();

            _context.LichLamViecs.RemoveRange(existingLichs);
            _context.NhuCauCaLams.RemoveRange(existingNhuCaus);

            for (int i = 0; i < 7; i++)
            {
                var targetDate = startOfWeek.AddDays(i);
                if (targetDate.Date == sourceDate.Date) continue;

                foreach (var snc in sourceNhuCaus)
                {
                    _context.NhuCauCaLams.Add(new NhuCauCaLam
                    {
                        NgayLam = targetDate,
                        IdCa = snc.IdCa,
                        IdVaiTro = snc.IdVaiTro,
                        SoLuongCan = snc.SoLuongCan,
                        LoaiYeuCau = snc.LoaiYeuCau,
                        GhiChu = snc.GhiChu
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Chép lịch thành công" });
        }


        [HttpPost("assign")]
        public async Task<IActionResult> AssignLich([FromBody] QuanLyLichLamViec_AssignDto dto)
        {
            var nv = await _context.NhanViens.Include(n => n.VaiTro).FirstOrDefaultAsync(n => n.IdNhanVien == dto.IdNhanVien);
            if (nv == null) return NotFound("Không tìm thấy nhân viên.");

            var caMoi = await _context.CaLamViecs.FindAsync(dto.IdCa);
            if (caMoi == null) return NotFound("Ca làm không tồn tại.");

            var caHienTais = await _context.LichLamViecs
                .Include(l => l.CaLamViec)
                .Where(l => l.IdNhanVien == dto.IdNhanVien && l.NgayLam == dto.NgayLam.Date)
                .ToListAsync();

            foreach (var l in caHienTais)
            {
                if (l.IdCa == dto.IdCa) return Conflict("Nhân viên này đã được phân vào ca này rồi!");

                if (caMoi.GioBatDau < l.CaLamViec.GioKetThuc && caMoi.GioKetThuc > l.CaLamViec.GioBatDau)
                {
                    return Conflict($"Nhân viên này đã có lịch làm ({l.CaLamViec.TenCa}) trùng giờ với ca bạn đang chọn.");
                }
            }

            var nhuCau = await _context.NhuCauCaLams.FirstOrDefaultAsync(nc => nc.NgayLam == dto.NgayLam.Date && nc.IdCa == dto.IdCa && nc.IdVaiTro == nv.IdVaiTro);
            if (nhuCau != null)
            {
                int currentCount = await _context.LichLamViecs.CountAsync(l => l.NgayLam == dto.NgayLam.Date && l.IdCa == dto.IdCa && l.NhanVien.IdVaiTro == nv.IdVaiTro);
                if (currentCount >= nhuCau.SoLuongCan)
                {
                    return Conflict($"Ca này chỉ yêu cầu {nhuCau.SoLuongCan} {nv.VaiTro.TenVaiTro}. Đã đủ số lượng nhân sự!");
                }
            }

            var entity = new LichLamViec
            {
                IdNhanVien = dto.IdNhanVien,
                IdCa = dto.IdCa,
                NgayLam = dto.NgayLam.Date,
                TrangThai = dto.TrangThai,
                GhiChu = dto.GhiChu 
            };
            _context.LichLamViecs.Add(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPut("duyet-ca/{idLich}")]
        public async Task<IActionResult> DuyetCa(int idLich)
        {
            var entity = await _context.LichLamViecs.FindAsync(idLich);
            if (entity == null) return NotFound();
            entity.TrangThai = "Đã duyệt";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("xoa-ca/{idLich}")]
        public async Task<IActionResult> DeleteCaNV(int idLich)
        {
            var entity = await _context.LichLamViecs.FindAsync(idLich);
            if (entity == null) return NotFound();
            _context.LichLamViecs.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("ca-lam-viec")]
        public async Task<IActionResult> GetCaLamViec()
        {
            var data = await _context.CaLamViecs.Select(c => new QuanLyLichLamViec_CaDto { IdCa = c.IdCa, TenCa = c.TenCa, GioBatDau = c.GioBatDau, GioKetThuc = c.GioKetThuc }).ToListAsync();
            return Ok(data);
        }

        [HttpPost("ca-lam-viec")]
        public async Task<IActionResult> CreateCa([FromBody] QuanLyLichLamViec_CaDto dto)
        {
            var moCuaStr = await _context.CaiDats.Where(c => c.TenCaiDat == "ThongTin_GioMoCua").Select(c => c.GiaTri).FirstOrDefaultAsync();
            var dongCuaStr = await _context.CaiDats.Where(c => c.TenCaiDat == "ThongTin_GioDongCua").Select(c => c.GiaTri).FirstOrDefaultAsync();
            TimeSpan moCua = moCuaStr != null ? TimeSpan.Parse(moCuaStr) : new TimeSpan(7, 0, 0);
            TimeSpan dongCua = dongCuaStr != null ? TimeSpan.Parse(dongCuaStr) : new TimeSpan(22, 0, 0);

            if (dto.GioBatDau < moCua || dto.GioKetThuc > dongCua)
                return Conflict($"Khung giờ ca làm không hợp lệ! Quán chỉ hoạt động từ {moCua:hh\\:mm} đến {dongCua:hh\\:mm}.");

            var entity = new CaLamViec { TenCa = dto.TenCa, GioBatDau = dto.GioBatDau, GioKetThuc = dto.GioKetThuc };
            _context.CaLamViecs.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("ca-lam-viec/{idCa}")]
        public async Task<IActionResult> DeleteCa(int idCa)
        {
            if (await _context.NhuCauCaLams.AnyAsync(l => l.IdCa == idCa) || await _context.LichLamViecs.AnyAsync(l => l.IdCa == idCa))
                return Conflict("Không thể xóa ca làm này vì đang được sử dụng.");

            var entity = await _context.CaLamViecs.FindAsync(idCa);
            if (entity == null) return NotFound();
            _context.CaLamViecs.Remove(entity);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}