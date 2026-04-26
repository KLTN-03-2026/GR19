namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class BanSoDoWebDto
    {
        public int IdBan { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public decimal TongTienHienTai { get; set; }
        public int? IdHoaDonHienTai { get; set; }
        public int? IdKhuVuc { get; set; }
        public string? ThongTinDatBan { get; set; }
    }

    public class BaoCaoSuCoWebRequestDto
    {
        public string GhiChuSuCo { get; set; } = string.Empty;
    }

    public class BanActionWebRequestDto
    {
        public int IdHoaDonNguon { get; set; }
        public int IdBanDich { get; set; }
        public int? IdHoaDonDich { get; set; }
    }

    public class KhuVucWebDto
    {
        public int IdKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
        // Bổ sung list bàn để hứng dữ liệu gộp nếu cần
        public List<BanSoDoWebDto> Bans { get; set; } = new();
    }
}