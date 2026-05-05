using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyThuongPhatGridDto
    {
        public int IdMau { get; set; }
        public string Loai { get; set; } = string.Empty;
        public string TenMau { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }

    public class QuanLyThuongPhatSaveDto
    {
        [Required]
        public string Loai { get; set; } = "Thưởng";

        [Required]
        public string TenMau { get; set; } = string.Empty;

        public decimal SoTien { get; set; }
    }
}