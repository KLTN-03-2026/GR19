using System.ComponentModel.DataAnnotations;
namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyBanGridDto
    {
        public int IdBan { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public int SoGhe { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public int IdKhuVuc { get; set; }
        public string? TenKhuVuc { get; set; }
    }
    public class QuanLyBanSaveDto
    {
        [Required] public string SoBan { get; set; } = string.Empty;
        public int SoGhe { get; set; }
        public int IdKhuVuc { get; set; }
        public string TrangThai { get; set; } = "Trống";
        public string? GhiChu { get; set; }
    }
    public class LookupKhuVucDto { public int IdKhuVuc { get; set; } public string TenKhuVuc { get; set; } = string.Empty; }
    public class QuanLyBanHistoryDto { public int SoLuotPhucVu { get; set; } public decimal TongDoanhThu { get; set; } public int SoLuotDatTruoc { get; set; } }
}