using CafebookApi.Data;
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-kho")]
    [ApiController]
    public class QuanLyKhoController : ControllerBase
    {
        private readonly CafebookDbContext _context;
        public QuanLyKhoController(CafebookDbContext context) { _context = context; }

        [HttpGet("tonkho")]
        public async Task<IActionResult> GetTonKho()
        {
            var data = await _context.NguyenLieus.AsNoTracking()
                .Select(nl => new QuanLyTonKhoDto
                {
                    IdNguyenLieu = nl.IdNguyenLieu,
                    TenNguyenLieu = nl.TenNguyenLieu,
                    TonKho = nl.TonKho,
                    DonViTinh = nl.DonViTinh,
                    TonKhoToiThieu = nl.TonKhoToiThieu,
                    TinhTrang = (nl.TonKho <= 0) ? "Hết hàng" : (nl.TonKho <= nl.TonKhoToiThieu ? "Sắp hết" : "Đủ dùng")
                })
                .OrderBy(nl => nl.TinhTrang)
                .ThenBy(nl => nl.TenNguyenLieu)
                .ToListAsync();

            return Ok(data);
        }
    }
}