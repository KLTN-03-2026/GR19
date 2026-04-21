using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    // 1. DTO CHUYÊN BIỆT CHO BỘ LỌC CỦA THƯ VIỆN SÁCH (Thay thế FilterLookupDto)
    public class ThuVienSachFilterItemDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }

    public class SachFiltersDto
    {
        public List<ThuVienSachFilterItemDto> TheLoais { get; set; } = new();
    }

    // 2. DTO CHO THẺ SÁCH TRÊN LƯỚI
    public class SachCardDto
    {
        public int IdSach { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? TacGia { get; set; }
        public decimal GiaBia { get; set; }
        public int SoLuongCoSan { get; set; }
        public string? AnhBiaUrl { get; set; }
    }

    // 3. DTO PHÂN TRANG
    public class SachPhanTrangDto
    {
        public List<SachCardDto> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}