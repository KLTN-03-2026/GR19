using CafebookModel.Model.Shared;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class TongQuanDto
    {
        public ThongTinNhanVienDto ThongTin { get; set; } = new();
        public List<SharedThongBaoItemDto> DanhSachThongBao { get; set; } = new();
        public int SoThongBaoChuaDoc { get; set; }
    }

    public class ThongTinNhanVienDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string TrangThaiLamViec { get; set; } = string.Empty;
    }
}