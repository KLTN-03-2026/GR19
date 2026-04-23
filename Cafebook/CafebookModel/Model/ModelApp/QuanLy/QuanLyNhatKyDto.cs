using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyNhatKyGridDto
    {
        public int IdNhatKy { get; set; }
        public string NguoiThaoTac { get; set; } = string.Empty; 
        public string VaiTro { get; set; } = string.Empty; 
        public string HanhDong { get; set; } = string.Empty;
        public string BangBiAnhHuong { get; set; } = string.Empty;
        public DateTime ThoiGian { get; set; }
        public string? DiaChiIP { get; set; }
    }

    public class QuanLyNhatKyDetailDto : QuanLyNhatKyGridDto
    {
        public string? KhoaChinh { get; set; }
        public string? DuLieuCu { get; set; }
        public string? DuLieuMoi { get; set; }
    }
}