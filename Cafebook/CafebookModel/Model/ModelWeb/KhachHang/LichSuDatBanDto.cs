using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class LichSuDatBanDto
    {
        public int IdPhieuDatBan { get; set; }
        public string TenBan { get; set; } = string.Empty;
        public DateTime ThoiGianDat { get; set; }
        public int SoLuongKhach { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class PagedLichSuResponseDto
    {
        public List<LichSuDatBanDto> Items { get; set; } = new List<LichSuDatBanDto>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}