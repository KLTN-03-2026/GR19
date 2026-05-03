namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class BanSoDoDto
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

    public class BaoCaoSuCoRequestDto
    {
        public string GhiChuSuCo { get; set; } = string.Empty;
    }

    public class BanActionRequestDto
    {
        public int IdHoaDonNguon { get; set; }
        public int IdBanDich { get; set; }
        public int? IdHoaDonDich { get; set; }
    }

    public class KhuVucDto
    {
        public int IdKhuVuc { get; set; }
        public string TenKhuVuc { get; set; } = string.Empty;
    }
}