using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class CaiDatThueSachDto
    {
        public decimal PhiThue { get; set; }
        public decimal PhiTraTreMoiNgay { get; set; }
        public int SoNgayMuonToiDa { get; set; }
        public int DiemPhieuThue { get; set; }
        public decimal PointToVND { get; set; }
        public string BankId { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;

        public decimal PhatGiamDoMoi1Percent { get; set; }
    }

    public class KhachHangSearchDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public int DiemTichLuy { get; set; }
        public string? Email { get; set; }
    }

    public class KhachHangInfoDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
    }

    public class PhieuThueGridDto
    {
        public int IdPhieuThueSach { get; set; }
        public string HoTenKH { get; set; } = string.Empty;
        public string? SoDienThoaiKH { get; set; }
        public DateTime NgayThue { get; set; }
        public DateTime NgayHenTra { get; set; }
        public int SoLuongSach { get; set; }
        public decimal TongTienCoc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string TinhTrang { get; set; } = string.Empty;
    }

    public class ChiTietSachThueDto
    {
        public int IdPhieuThueSach { get; set; }
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public DateTime NgayHenTra { get; set; }
        public decimal TienCoc { get; set; }
        public string TinhTrang { get; set; } = string.Empty;
        public decimal TienPhat { get; set; }

        public int DoMoiKhiThue { get; set; }
        public string? GhiChuKhiThue { get; set; }
    }

    public class PhieuThueChiTietDto
    {
        public int IdPhieuThueSach { get; set; }
        public string HoTenKH { get; set; } = string.Empty;
        public string? SoDienThoaiKH { get; set; }
        public string? EmailKH { get; set; }
        public int DiemTichLuyKH { get; set; }
        public DateTime NgayThue { get; set; }
        public string TrangThaiPhieu { get; set; } = string.Empty;
        public List<ChiTietSachThueDto> SachDaThue { get; set; } = new();
        public List<int> DsIdPhieuTra { get; set; } = new();
    }

    public class SachTimKiemDto
    {
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public int SoLuongHienCo { get; set; }
        public decimal GiaBia { get; set; }
    }

    public class PhieuThueRequestDto
    {
        public KhachHangInfoDto KhachHangInfo { get; set; } = new();
        public List<SachThueRequestDto> SachCanThue { get; set; } = new();
        public DateTime NgayHenTra { get; set; }
        public int IdNhanVien { get; set; }
    }

    public class SachThueRequestDto
    {
        public int IdSach { get; set; }
        public decimal TienCoc { get; set; }

        public int DoMoiKhiThue { get; set; } = 100;
        public string? GhiChuKhiThue { get; set; }
    }

    public class TraSachItemRequestDto
    {
        public int IdSach { get; set; }
        public int DoMoiKhiTra { get; set; } = 100;
        public string? GhiChuKhiTra { get; set; }
    }

    public class TraSachRequestDto
    {
        public int IdPhieuThueSach { get; set; }
        public int IdNhanVien { get; set; }

        public List<TraSachItemRequestDto> DanhSachTra { get; set; } = new();
    }

    public class GiaHanRequestDto
    {
        public int IdPhieuThueSach { get; set; }
        public DateTime NgayHenTraMoi { get; set; }
    }

    public class TraSachResponseDto
    {
        public int IdPhieuTra { get; set; }
        public int SoSachDaTra { get; set; }
        public decimal TongPhiThue { get; set; }
        public decimal TongTienPhat { get; set; }
        public decimal TongTienCoc { get; set; }
        public decimal TongHoanTra { get; set; }
        public int DiemTichLuy { get; set; }
    }

    public class PhieuThuePrintDto
    {
        public string IdPhieu { get; set; } = string.Empty;
        public string TenQuan { get; set; } = string.Empty;
        public string DiaChiQuan { get; set; } = string.Empty;
        public string SdtQuan { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string SdtKhachHang { get; set; } = string.Empty;
        public DateTime NgayHenTra { get; set; }
        public List<ChiTietPrintDto> ChiTiet { get; set; } = new();
        public decimal TongPhiThue { get; set; }
        public decimal TongTienCoc { get; set; }
    }

    public class ChiTietPrintDto
    {
        public string TenSach { get; set; } = string.Empty;
        public int DoMoi { get; set; }          
        public string? GhiChu { get; set; }  
        public decimal TienCoc { get; set; }
    }

    public class PhieuTraGridDto
    {
        public int IdPhieuTra { get; set; }
        public int IdPhieuThueSach { get; set; }
        public DateTime NgayTra { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public decimal TongHoanTra { get; set; }
    }

    public class PhieuTraPrintDto
    {
        public string IdPhieuTra { get; set; } = string.Empty;
        public string IdPhieuThue { get; set; } = string.Empty;
        public string TenQuan { get; set; } = string.Empty;
        public string DiaChiQuan { get; set; } = string.Empty;
        public string SdtQuan { get; set; } = string.Empty;
        public DateTime NgayTra { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string SdtKhachHang { get; set; } = string.Empty;
        public int DiemTichLuy { get; set; }
        public List<ChiTietTraPrintDto> ChiTiet { get; set; } = new();
        public decimal TongTienCoc { get; set; }
        public decimal TongPhiThue { get; set; }
        public decimal TongTienPhat { get; set; }
        public decimal TongHoanTra { get; set; }
    }

    public class ChiTietTraPrintDto
    {
        public string TenSach { get; set; } = string.Empty;
        public int DoMoi { get; set; }          
        public string? GhiChu { get; set; }      
        public decimal TienCoc { get; set; }
        public decimal TienPhat { get; set; }
    }

    public class ChiTietSachTraUI_Dto : INotifyPropertyChanged
    {
        private bool _isSelected;
        public int IdPhieuThueSach { get; set; }
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public decimal TienCoc { get; set; }
        public decimal TienPhat { get; set; }
        public string TinhTrang { get; set; } = string.Empty;

        public int DoMoiKhiThue { get; set; }
        public string? GhiChuKhiThue { get; set; }

        private int _doMoiKhiTra = 100;
        public int DoMoiKhiTra
        {
            get => _doMoiKhiTra;
            set
            {
                if (_doMoiKhiTra != value)
                {
                    _doMoiKhiTra = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _ghiChuKhiTra;
        public string? GhiChuKhiTra
        {
            get => _ghiChuKhiTra;
            set
            {
                if (_ghiChuKhiTra != value)
                {
                    _ghiChuKhiTra = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _tienPhatHuHong;
        public decimal TienPhatHuHong
        {
            get => _tienPhatHuHong;
            set
            {
                if (_tienPhatHuHong != value)
                {
                    _tienPhatHuHong = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}