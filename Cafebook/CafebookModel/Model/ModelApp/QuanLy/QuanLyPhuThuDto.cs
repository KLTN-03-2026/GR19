using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyPhuThuGridDto
    {
        public int IdPhuThu { get; set; }
        public string TenPhuThu { get; set; } = string.Empty;
        public decimal GiaTri { get; set; }
        public string LoaiGiaTri { get; set; } = string.Empty; // Hiển thị "%" hoặc "VNĐ"
    }

    public class QuanLyPhuThuSaveDto
    {
        [Required]
        public string TenPhuThu { get; set; } = string.Empty;
        public decimal GiaTri { get; set; }
        [Required]
        public string LoaiGiaTri { get; set; } = string.Empty;
    }
}