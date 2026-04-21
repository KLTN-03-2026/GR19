using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    // 1. DTO CHUYÊN BIỆT CHO BỘ LỌC DANH MỤC CỦA THỰC ĐƠN
    public class ThucDonFilterDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }

    // 2. DTO CHO SẢN PHẨM TRÊN LƯỚI
    public class SanPhamThucDonDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? TenLoaiSP { get; set; }
        public decimal DonGia { get; set; }
        public string? AnhSanPhamUrl { get; set; }
    }

    // 3. DTO TRẢ VỀ BAO GỒM PHÂN TRANG
    public class ThucDonDto
    {
        public List<SanPhamThucDonDto> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}