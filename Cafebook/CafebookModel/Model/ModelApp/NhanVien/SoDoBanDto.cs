using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class BanSoDoDto : INotifyPropertyChanged
    {
        private string _trangThai = string.Empty;
        private decimal _tongTienHienTai;
        private int? _idHoaDonHienTai;
        private string? _thongTinDatBan;
        private string? _ghiChu;
        private int? _idKhuVuc;

        public int IdBan { get; set; }
        public string SoBan { get; set; } = string.Empty;

        public string TrangThai
        {
            get => _trangThai;
            set { if (_trangThai != value) { _trangThai = value; OnPropertyChanged(); } }
        }

        public decimal TongTienHienTai
        {
            get => _tongTienHienTai;
            set { if (_tongTienHienTai != value) { _tongTienHienTai = value; OnPropertyChanged(); } }
        }

        public int? IdHoaDonHienTai
        {
            get => _idHoaDonHienTai;
            set { if (_idHoaDonHienTai != value) { _idHoaDonHienTai = value; OnPropertyChanged(); } }
        }

        public string? ThongTinDatBan
        {
            get => _thongTinDatBan;
            set { if (_thongTinDatBan != value) { _thongTinDatBan = value; OnPropertyChanged(); } }
        }

        public string? GhiChu
        {
            get => _ghiChu;
            set { if (_ghiChu != value) { _ghiChu = value; OnPropertyChanged(); } }
        }

        public int? IdKhuVuc
        {
            get => _idKhuVuc;
            set { if (_idKhuVuc != value) { _idKhuVuc = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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