using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDonViVanChuyenGridDto
    {
        public int IdNguoiGiaoHang { get; set; }
        public string TenNguoiGiaoHang { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class QuanLyDonViVanChuyenSaveDto
    {
        [Required]
        public string TenNguoiGiaoHang { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        [Required]
        public string TrangThai { get; set; } = "Sẵn sàng";
    }
}