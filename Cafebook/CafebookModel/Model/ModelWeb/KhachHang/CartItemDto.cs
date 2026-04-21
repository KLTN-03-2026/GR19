// Vị trí lưu: E:\Tai Lieu Hoc Tap\N19 KLTN 032026\Cafebook\CafebookModel\Model\ModelWeb\KhachHang\CartItemDto.cs
namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class CartItemDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public string? AnhSanPhamUrl { get; set; }

        // Thuộc tính tính toán tự động
        public decimal ThanhTien => DonGia * SoLuong;
    }
}