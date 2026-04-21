using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    // Bao bọc toàn bộ kết quả trả về
    public class TimKiemSachResultDto
    {
        public string TieuDeTrang { get; set; } = string.Empty;
        public string? MoTaTrang { get; set; }
        public List<TimKiemSachCardDto> SachList { get; set; } = new();
    }

    // Thẻ sách thu gọn dành riêng cho màn hình tìm kiếm
    public class TimKiemSachCardDto
    {
        public int IdSach { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public decimal GiaBia { get; set; }
        public string? AnhBiaUrl { get; set; }
    }
}