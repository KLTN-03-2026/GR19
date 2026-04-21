using System.Collections.Generic;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class ChiTietSachDto
    {
        public int IdSach { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public decimal GiaBia { get; set; }
        public string? AnhBiaUrl { get; set; }
        public string? MoTa { get; set; }
        public string? ViTri { get; set; }
        public int TongSoLuong { get; set; }
        public int SoLuongCoSan { get; set; }

        public List<ChiTietSachTacGiaDto> TacGias { get; set; } = new();
        public List<ChiTietSachTheLoaiDto> TheLoais { get; set; } = new();
        public List<ChiTietSachNxbDto> NhaXuatBans { get; set; } = new();
        public List<ChiTietSachGoiYDto> GoiY { get; set; } = new();
    }

    public class ChiTietSachTacGiaDto
    {
        public int IdTacGia { get; set; }
        public string TenTacGia { get; set; } = string.Empty;
    }

    public class ChiTietSachTheLoaiDto
    {
        public int IdTheLoai { get; set; }
        public string TenTheLoai { get; set; } = string.Empty;
    }

    public class ChiTietSachNxbDto
    {
        public int IdNhaXuatBan { get; set; }
        public string TenNhaXuatBan { get; set; } = string.Empty;
    }

    public class ChiTietSachGoiYDto
    {
        public int IdSach { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public decimal GiaBia { get; set; }
        public string? AnhBiaUrl { get; set; }
    }
}