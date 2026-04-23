using CafebookApi.Data;
using CafebookModel.Model.ModelEntities; // ĐÃ SỬA TẠI ĐÂY
using CafebookModel.Model.ModelApp.QuanLy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CafebookApi.Controllers.App.QuanLy
{
    [Route("api/app/quanly-caidat")]
    [ApiController]
    [Authorize]
    public class QuanLyCaiDatController : ControllerBase
    {
        private readonly CafebookDbContext _context;

        public QuanLyCaiDatController(CafebookDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSettings()
        {
            var settings = await _context.Set<CaiDat>().AsNoTracking().ToListAsync();

            var dtos = settings.Select(c => new QuanLyCaiDatDto
            {
                TenCaiDat = c.TenCaiDat,
                GiaTri = c.GiaTri,
                MoTa = c.MoTa,
                Nhom = ExtractNhom(c.TenCaiDat)
            }).OrderBy(x => x.Nhom).ThenBy(x => x.TenCaiDat).ToList();

            return Ok(dtos);
        }

        [HttpPut("update-single")]
        public async Task<IActionResult> UpdateSetting([FromBody] QuanLyCaiDatDto dto)
        {
            var setting = await _context.Set<CaiDat>().FirstOrDefaultAsync(c => c.TenCaiDat == dto.TenCaiDat);
            if (setting == null) return NotFound("Không tìm thấy cài đặt này.");

            setting.GiaTri = dto.GiaTri;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        private string ExtractNhom(string key)
        {
            if (!key.Contains('_')) return "Hệ thống chung";
            var prefix = key.Split('_')[0];
            return prefix switch
            {
                "AI" => "🤖 Trí tuệ nhân tạo (AI)",
                "DiemTichLuy" => "⭐ Hệ thống Điểm thưởng",
                "HR" => "👥 Quản trị Nhân sự (HR)",
                "LienHe" => "📞 Thông tin Liên hệ",
                "Sach" => "📚 Quản lý Mượn/Trả sách",
                "Smtp" => "📧 Cấu hình Email (SMTP)",
                "ThongTin" => "🏠 Thông tin Cửa hàng",
                "VNPay" => "💳 Thanh toán VNPay",
                "Wifi" => "📶 Cấu hình hạ tầng",
                _ => prefix
            };
        }
    }
}