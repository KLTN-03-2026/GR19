namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyTonKhoDto
    {
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal TonKho { get; set; }
        public string DonViTinh { get; set; } = string.Empty;
        public decimal TonKhoToiThieu { get; set; }
        public string TinhTrang { get; set; } = string.Empty; // Sắp hết, Hết hàng, Đủ dùng
    }
}