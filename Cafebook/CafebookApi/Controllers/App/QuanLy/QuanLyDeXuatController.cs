// File: CafebookApi/Controllers/App/QuanLy/QuanLyDeXuatController.cs
using CafebookApi.Data; // Chỉnh sửa theo namespace Context thực tế của bạn
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Model.ModelEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-dexuat")]
    [ApiController]
    [Authorize]

    public class QuanLyDeXuatController : ControllerBase 
    {
        private readonly CafebookDbContext _context;

        public QuanLyDeXuatController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string loaiDoiTuong)
        {
            try
            {
                if (loaiDoiTuong == "SACH")
                {
                    // Sử dụng Set<DeXuatSach>() thay vì gọi tên Property chưa xác định
                    var data = await _context.Set<DeXuatSach>().AsNoTracking()
                        .Include(x => x.SachGoc)
                        .Include(x => x.SachDeXuat)
                        .Select(x => new QuanLyDeXuatGridDto
                        {
                            LoaiDoiTuong = "SACH",
                            IdGoc = x.IdSachGoc,
                            TenGoc = x.SachGoc.TenSach,
                            IdDeXuat = x.IdSachDeXuat,
                            TenDeXuat = x.SachDeXuat.TenSach,
                            DoLienQuan = x.DoLienQuan,
                            LoaiDeXuat = x.LoaiDeXuat
                        }).ToListAsync();
                    return Ok(data);
                }
                else
                {
                    var data = await _context.Set<DeXuatSanPham>().AsNoTracking()
                        .Include(x => x.SanPhamGoc)
                        .Include(x => x.SanPhamDeXuat)
                        .Select(x => new QuanLyDeXuatGridDto
                        {
                            LoaiDoiTuong = "SANPHAM",
                            IdGoc = x.IdSanPhamGoc,
                            TenGoc = x.SanPhamGoc.TenSanPham,
                            IdDeXuat = x.IdSanPhamDeXuat,
                            TenDeXuat = x.SanPhamDeXuat.TenSanPham,
                            DoLienQuan = x.DoLienQuan,
                            LoaiDeXuat = x.LoaiDeXuat
                        }).ToListAsync();
                    return Ok(data);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup([FromQuery] string loaiDoiTuong)
        {
            if (loaiDoiTuong == "SACH")
            {
                return Ok(await _context.Set<Sach>().AsNoTracking()
                    .Select(x => new DeXuatLookupDto { Id = x.IdSach, Ten = x.TenSach }).ToListAsync());
            }
            return Ok(await _context.Set<SanPham>().AsNoTracking()
                .Select(x => new DeXuatLookupDto { Id = x.IdSanPham, Ten = x.TenSanPham }).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuanLyDeXuatSaveDto dto)
        {
            try
            {
                if (dto.LoaiDoiTuong == "SACH")
                {
                    var entity = new DeXuatSach
                    {
                        IdSachGoc = dto.IdGoc,
                        IdSachDeXuat = dto.IdDeXuat,
                        DoLienQuan = dto.DoLienQuan,
                        LoaiDeXuat = dto.LoaiDeXuat
                    };
                    _context.Set<DeXuatSach>().Add(entity);
                }
                else
                {
                    var entity = new DeXuatSanPham
                    {
                        IdSanPhamGoc = dto.IdGoc,
                        IdSanPhamDeXuat = dto.IdDeXuat,
                        DoLienQuan = dto.DoLienQuan,
                        LoaiDeXuat = dto.LoaiDeXuat
                    };
                    _context.Set<DeXuatSanPham>().Add(entity);
                }
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Thêm đề xuất thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string loaiDoiTuong, [FromQuery] int idGoc, [FromQuery] int idDeXuat, [FromQuery] string loaiDeXuat)
        {
            try
            {
                if (loaiDoiTuong == "SACH")
                {
                    var entity = await _context.Set<DeXuatSach>()
                        .FirstOrDefaultAsync(x => x.IdSachGoc == idGoc && x.IdSachDeXuat == idDeXuat && x.LoaiDeXuat == loaiDeXuat);
                    if (entity == null) return NotFound("Không tìm thấy đề xuất sách.");
                    _context.Set<DeXuatSach>().Remove(entity);
                }
                else
                {
                    var entity = await _context.Set<DeXuatSanPham>()
                        .FirstOrDefaultAsync(x => x.IdSanPhamGoc == idGoc && x.IdSanPhamDeXuat == idDeXuat && x.LoaiDeXuat == loaiDeXuat);
                    if (entity == null) return NotFound("Không tìm thấy đề xuất sản phẩm.");
                    _context.Set<DeXuatSanPham>().Remove(entity);
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Xóa thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}