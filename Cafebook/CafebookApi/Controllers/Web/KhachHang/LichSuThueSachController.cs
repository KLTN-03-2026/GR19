using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.KhachHang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.Web.KhachHang
{
    [Route("api/web/khach-hang/lich-su-thue-sach")]
    [ApiController]
    [Authorize]
    [Authorize(Roles = "KhachHang")]
    public class LichSuThueSachController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public LichSuThueSachController(CafebookDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            int.TryParse(idClaim?.Value, out int id);
            return id;
        }

        [HttpGet]
        public async Task<IActionResult> GetLichSuThueSach([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new { Message = "Phiên đăng nhập không hợp lệ." });

            int pageSize = 5;

            var query = _context.PhieuThueSachs
                .Include(p => p.ChiTietPhieuThues).ThenInclude(ct => ct.Sach)          // Lấy tên Sách
                .Include(p => p.PhieuTraSachs).ThenInclude(pt => pt.ChiTietPhieuTras)  // Lấy biên bản Trả
                .Where(p => p.IdKhachHang == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.TrangThai == status);

            if (!string.IsNullOrEmpty(search) && int.TryParse(search.Replace("#", "").Trim(), out int idSearch))
            {
                query = query.Where(p => p.IdPhieuThueSach == idSearch);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedEntities = await query
                .OrderByDescending(p => p.TrangThai == "Đang thuê")
                .ThenByDescending(p => p.NgayThue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            decimal mucPhiThue = 15000m;
            decimal mucPhatTre = 5000m;

            var items = pagedEntities.Select(p =>
            {
                var dto = new LichSuThueSachDto
                {
                    IdPhieuThueSach = p.IdPhieuThueSach,
                    NgayThue = p.NgayThue,
                    TrangThai = p.TrangThai ?? "Không rõ",
                    SoLuongSach = p.ChiTietPhieuThues.Count,
                    TongTienCoc = p.TongTienCoc,
                    NgayHenTra = p.ChiTietPhieuThues.FirstOrDefault()?.NgayHenTra
                };

                string statusNorm = dto.TrangThai.Trim().ToLower();
                var chiTietList = new List<ChiTietLichSuThueDto>();

                if (statusNorm == "đang thuê")
                {
                    dto.LaSoTienTamTinh = true;
                    dto.TongPhiThue = dto.SoLuongSach * mucPhiThue;

                    decimal totalPhat = 0;
                    foreach (var ct in p.ChiTietPhieuThues)
                    {
                        decimal tienPhatTre = 0;
                        if (DateTime.Now.Date > ct.NgayHenTra.Date)
                        {
                            int daysLate = (DateTime.Now.Date - ct.NgayHenTra.Date).Days;
                            tienPhatTre = daysLate * mucPhatTre;
                        }
                        totalPhat += tienPhatTre;

                        chiTietList.Add(new ChiTietLichSuThueDto
                        {
                            TenSach = ct.Sach?.TenSach ?? "Sách",
                            DoMoiKhiThue = ct.DoMoiKhiThue ?? 100,
                            GhiChuKhiThue = string.IsNullOrWhiteSpace(ct.GhiChuKhiThue) ? "-" : ct.GhiChuKhiThue,
                            TienPhatTre = tienPhatTre,
                            TienPhatHuHong = 0
                        });
                    }
                    dto.TongTienPhat = totalPhat;
                    dto.TongTienCocHoan = dto.TongTienCoc - dto.TongPhiThue.Value - dto.TongTienPhat.Value;
                }
                else if (statusNorm == "đã trả")
                {
                    dto.LaSoTienTamTinh = false;
                    var phieuTra = p.PhieuTraSachs.OrderByDescending(pt => pt.NgayTra).FirstOrDefault();
                    if (phieuTra != null)
                    {
                        dto.NgayTra = phieuTra.NgayTra;
                        dto.TongPhiThue = phieuTra.TongPhiThue;
                        dto.TongTienPhat = phieuTra.TongTienPhat;
                        dto.TongTienCocHoan = phieuTra.TongTienCocHoan;

                        foreach (var ct in p.ChiTietPhieuThues)
                        {
                            var ctTra = phieuTra.ChiTietPhieuTras?.FirstOrDefault(ctr => ctr.IdSach == ct.IdSach);
                            chiTietList.Add(new ChiTietLichSuThueDto
                            {
                                TenSach = ct.Sach?.TenSach ?? "Sách",
                                DoMoiKhiThue = ct.DoMoiKhiThue ?? 100,
                                GhiChuKhiThue = string.IsNullOrWhiteSpace(ct.GhiChuKhiThue) ? "-" : ct.GhiChuKhiThue,
                                DoMoiKhiTra = ctTra?.DoMoiKhiTra,
                                GhiChuKhiTra = string.IsNullOrWhiteSpace(ctTra?.GhiChuKhiTra) ? "-" : ctTra?.GhiChuKhiTra,
                                TienPhatTre = ctTra?.TienPhat ?? 0,
                                TienPhatHuHong = ctTra?.TienPhatHuHong ?? 0
                            });
                        }
                    }
                }
                else
                {
                    dto.LaSoTienTamTinh = false;
                    dto.TongPhiThue = 0; dto.TongTienPhat = 0; dto.TongTienCocHoan = dto.TongTienCoc;

                    foreach (var ct in p.ChiTietPhieuThues)
                    {
                        chiTietList.Add(new ChiTietLichSuThueDto
                        {
                            TenSach = ct.Sach?.TenSach ?? "Sách",
                            DoMoiKhiThue = ct.DoMoiKhiThue ?? 100,
                            GhiChuKhiThue = string.IsNullOrWhiteSpace(ct.GhiChuKhiThue) ? "-" : ct.GhiChuKhiThue,
                            TienPhatTre = 0,
                            TienPhatHuHong = 0
                        });
                    }
                }

                dto.ChiTietSachs = chiTietList;
                return dto;
            }).ToList();

            return Ok(new PagedLichSuThueSachResponseDto
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page
            });
        }
    }
}