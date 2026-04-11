using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLySanPhamGridDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public bool TrangThaiKinhDoanh { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
    }
    public class QuanLySanPhamDetailDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public int IdDanhMuc { get; set; }
        public string NhomIn { get; set; } = "Khác";
        public bool TrangThaiKinhDoanh { get; set; } = true;
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
    }
    public class LookupDanhMucDto { public int Id { get; set; } public string Ten { get; set; } = string.Empty; }
}