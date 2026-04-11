using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyKiemKhoGridDto
    {
        public int IdPhieuKiemKho { get; set; }
        public DateTime NgayKiem { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
    }

    public class QuanLyKiemKhoDetailDto
    {
        public int IdPhieuKiemKho { get; set; }
        public DateTime NgayKiem { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public List<QuanLyChiTietKiemKhoDto> ChiTiet { get; set; } = new();
    }

    public class QuanLyChiTietKiemKhoDto
    {
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public decimal TonKhoHeThong { get; set; }
        public decimal TonKhoThucTe { get; set; }
        public decimal ChenhLech => TonKhoThucTe - TonKhoHeThong;
        public string LyDoChenhLech { get; set; } = string.Empty;
    }

    // DTO Dùng riêng cho lúc tạo Phiếu mới (Tương tác UI Real-time)
    public class QuanLyKiemKhoNguyenLieuDto : INotifyPropertyChanged
    {
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;
        public decimal TonKhoHeThong { get; set; }

        private decimal _tonKhoThucTe;
        public decimal TonKhoThucTe
        {
            get => _tonKhoThucTe;
            set
            {
                if (_tonKhoThucTe != value)
                {
                    _tonKhoThucTe = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ChenhLech)); // Tự động cập nhật cột chênh lệch
                }
            }
        }

        public decimal ChenhLech => TonKhoThucTe - TonKhoHeThong;
        public string LyDoChenhLech { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class QuanLyKiemKhoSaveDto
    {
        public List<QuanLyChiTietKiemKhoSaveDto> ChiTiet { get; set; } = new();
    }

    public class QuanLyChiTietKiemKhoSaveDto
    {
        public int IdNguyenLieu { get; set; }
        public decimal TonKhoHeThong { get; set; }
        public decimal TonKhoThucTe { get; set; }
        public string LyDoChenhLech { get; set; } = string.Empty;
    }
}