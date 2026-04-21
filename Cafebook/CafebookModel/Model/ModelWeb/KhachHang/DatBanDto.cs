using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class TimBanRequestDto
    {
        [Required] public DateTime NgayDat { get; set; }
        [Required] public TimeSpan GioDat { get; set; }
        [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1 đến 50")] public int SoNguoi { get; set; }
    }

    public class BanTrongDto
    {
        public int IdBan { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public int SoGhe { get; set; }
        public string KhuVuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }

    public class DatBanWebRequestDto
    {
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        [Required] public int IdBan { get; set; }
        [Required] public DateTime NgayDat { get; set; }
        [Required] public TimeSpan GioDat { get; set; }
        [Required] public int SoLuongKhach { get; set; }
        public string? GhiChu { get; set; }
    }

    public class KhuVucBanDto
    {
        public int IdKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
        public List<BanTrongDto> BanList { get; set; } = new();
    }

    public class OpeningHoursDto
    {
        public TimeSpan Open { get; set; }
        public TimeSpan Close { get; set; }
    }
}