using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    // DTO hiển thị trên DataGrid
    public class QuanLyNguyenLieuGridDto
    {
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal TonKho { get; set; }
        public string DonViTinh { get; set; } = string.Empty;
        public decimal TonKhoToiThieu { get; set; }
    }

    // DTO dùng khi Thêm/Sửa
    public class QuanLyNguyenLieuSaveDto
    {
        [Required]
        public string TenNguyenLieu { get; set; } = string.Empty;

        [Required]
        public string DonViTinh { get; set; } = string.Empty;

        public decimal TonKhoToiThieu { get; set; }
        public decimal TonKho { get; set; } // Có thể gán khi khởi tạo nguyên liệu mới
    }
}