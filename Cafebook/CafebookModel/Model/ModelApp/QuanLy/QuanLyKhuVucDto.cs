using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyKhuVucDto
    {
        public int IdKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int SoLuongBan { get; set; } // Tối ưu: Chỉ đếm số bàn, không lấy list
    }

    public class QuanLyKhuVucSaveDto
    {
        [Required] public string TenKhuVuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }
}