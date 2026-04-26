using CafebookApi.Data;
using CafebookModel.Model.ModelWeb.NhanVien;
using CafebookModel.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafebookApi.Controllers.Web.NhanVien
{
    [Route("api/web/nhanvien/[controller]")]
    [ApiController]
    [Authorize]
    public class GiaoHangWebController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public GiaoHangWebController(CafebookDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("available-orders")]
        public async Task<IActionResult> GetAvailableOrders()
        {
            var orders = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Where(h => h.LoaiHoaDon == "Giao hàng" && h.TrangThaiGiaoHang == "Chờ lấy hàng")
                .OrderByDescending(h => h.ThoiGianTao)
                .Select(h => new DonGiaoHangWebDto
                {
                    IdHoaDon = h.IdHoaDon,
                    ThoiGianTao = h.ThoiGianTao,
                    TenKhachHang = h.KhachHang != null ? h.KhachHang.HoTen : h.DiaChiGiaoHang ?? "Khách vãng lai",
                    SoDienThoai = h.SoDienThoaiGiaoHang ?? "",
                    DiaChi = h.DiaChiGiaoHang ?? "",
                    ThanhTien = h.ThanhTien,
                    TrangThaiThanhToan = h.TrangThai ?? "Chưa thanh toán",
                    PhuongThucThanhToan = h.PhuongThucThanhToan ?? "COD",
                    TrangThaiGiaoHang = h.TrangThaiGiaoHang ?? "Chờ lấy hàng",
                    GhiChu = h.GhiChu
                }).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            int idShipper = GetCurrentUserId();
            var orders = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Where(h => h.IdNguoiGiaoHang == idShipper && h.TrangThaiGiaoHang == "Đang giao")
                .Select(h => new DonGiaoHangWebDto
                {
                    IdHoaDon = h.IdHoaDon,
                    ThoiGianTao = h.ThoiGianTao,
                    ThanhTien = h.ThanhTien,
                    TenKhachHang = h.KhachHang != null ? h.KhachHang.HoTen : h.DiaChiGiaoHang ?? "",
                    DiaChi = h.DiaChiGiaoHang ?? "",
                    SoDienThoai = h.SoDienThoaiGiaoHang ?? "",
                    TrangThaiThanhToan = h.TrangThai ?? "Chưa thanh toán",
                    PhuongThucThanhToan = h.PhuongThucThanhToan ?? "COD",
                    TrangThaiGiaoHang = "Đang giao"
                }).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("order-details/{idHoaDon}")]
        public async Task<IActionResult> GetOrderDetails(int idHoaDon)
        {
            var chiTiet = await _context.ChiTietHoaDons
                .Include(c => c.SanPham)
                .Where(c => c.IdHoaDon == idHoaDon)
                .Select(c => new ChiTietDonGiaoWebDto
                {
                    TenSanPham = c.SanPham.TenSanPham,
                    SoLuong = c.SoLuong,
                    DonGia = c.DonGia,
                    ThanhTien = c.ThanhTien,
                    GhiChuMon = c.GhiChu
                }).ToListAsync();

            if (!chiTiet.Any()) return NotFound();
            return Ok(chiTiet);
        }

        [HttpGet("history-summary")]
        public async Task<IActionResult> GetHistorySummary()
        {
            int idShipper = GetCurrentUserId();
            var today = DateTime.Today;

            var query = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Where(h => h.IdNguoiGiaoHang == idShipper &&
                            h.ThoiGianThanhToan != null &&
                            h.ThoiGianThanhToan.Value.Date == today &&
                            (h.TrangThaiGiaoHang == "Hoàn thành" || h.TrangThaiGiaoHang == "Đã hủy"))
                .OrderByDescending(h => h.ThoiGianThanhToan)
                .ToListAsync();

            var summary = new ShipperHistorySummaryWebDto
            {
                TongTienMatCam = query.Where(h => h.TrangThaiGiaoHang == "Hoàn thành" && (h.PhuongThucThanhToan == "COD" || h.PhuongThucThanhToan == "Tiền mặt")).Sum(h => h.ThanhTien),
                TongDonHoanThanh = query.Count(h => h.TrangThaiGiaoHang == "Hoàn thành"),
                TongDonHuy = query.Count(h => h.TrangThaiGiaoHang == "Đã hủy"),

                LichSuDonHang = query.Select(h => new DonGiaoHangWebDto
                {
                    IdHoaDon = h.IdHoaDon,
                    TenKhachHang = h.KhachHang?.HoTen ?? h.DiaChiGiaoHang ?? "Khách vãng lai",
                    DiaChi = h.DiaChiGiaoHang ?? "",
                    ThanhTien = h.ThanhTien,
                    PhuongThucThanhToan = h.PhuongThucThanhToan ?? "COD",
                    TrangThaiGiaoHang = h.TrangThaiGiaoHang ?? "",
                    ThoiGianTao = h.ThoiGianThanhToan ?? h.ThoiGianTao
                }).ToList()
            };
            return Ok(summary);
        }

        [HttpPost("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromForm] string tacVu, [FromForm] string? lyDoHuy, IFormFile? proofImage)
        {
            var hd = await _context.HoaDons.FindAsync(id);
            if (hd == null) return NotFound();
            int idShipper = GetCurrentUserId();

            switch (tacVu)
            {
                case "NhanDon":
                    hd.IdNguoiGiaoHang = idShipper;
                    hd.TrangThaiGiaoHang = "Đang giao";
                    break;
                case "HoanThanh":
                    if (hd.IdNguoiGiaoHang != idShipper) return Forbid();

                    hd.TrangThaiGiaoHang = "Hoàn thành";
                    hd.TrangThai = "Đã thanh toán";
                    hd.ThoiGianThanhToan = DateTime.Now;

                    if (proofImage != null && proofImage.Length > 0)
                    {
                        string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "anhgiaohang");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                        string ext = Path.GetExtension(proofImage.FileName);
                        string fileName = $"GH_{id}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) { await proofImage.CopyToAsync(stream); }
                        hd.AnhGiaoHang = $"{HinhAnhPaths.Urldelivery}/{fileName}";
                    }
                    break;
                case "Huy":
                    hd.TrangThaiGiaoHang = "Đã hủy";
                    hd.GhiChu = $"[Shipper hủy]: {lyDoHuy}";
                    break;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }
    }
}