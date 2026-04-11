using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyNhaCungCapGridDto
    {
        public int IdNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? Email { get; set; }
    }

    public class QuanLyNhaCungCapSaveDto
    {
        [Required]
        public string TenNhaCungCap { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? Email { get; set; }
    }
}