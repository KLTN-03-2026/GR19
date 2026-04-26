using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class LichSuThueSachDto
    {
        public int IdPhieuThueSach { get; set; }
        public DateTime NgayThue { get; set; }
        public DateTime? NgayHenTra { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuongSach { get; set; }
        public decimal TongTienCoc { get; set; }

        public DateTime? NgayTra { get; set; }
        public decimal? TongPhiThue { get; set; }
        public decimal? TongTienPhat { get; set; }
        public decimal? TongTienCocHoan { get; set; }

        public bool LaSoTienTamTinh { get; set; }

        public List<ChiTietLichSuThueDto> ChiTietSachs { get; set; } = new();
    }

    public class ChiTietLichSuThueDto
    {
        public string TenSach { get; set; } = string.Empty;
        public int DoMoiKhiThue { get; set; }
        public string? GhiChuKhiThue { get; set; }
        public int? DoMoiKhiTra { get; set; }
        public string? GhiChuKhiTra { get; set; }
        public decimal TienPhatTre { get; set; }
        public decimal TienPhatHuHong { get; set; }
    }

    public class PagedLichSuThueSachResponseDto
    {
        public List<LichSuThueSachDto> Items { get; set; } = new List<LichSuThueSachDto>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public decimal PhatGiamDoMoi1Percent { get; set; }
    }
}