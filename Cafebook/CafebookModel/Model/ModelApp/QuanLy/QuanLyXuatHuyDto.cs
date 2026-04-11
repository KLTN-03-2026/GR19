using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    // DTO cho Grid hiển thị danh sách Phiếu Hủy
    public class QuanLyXuatHuyGridDto
    {
        public int IdPhieuXuatHuy { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string LyDoHuy { get; set; } = string.Empty;
    }

    // DTO Chi tiết 1 Phiếu Hủy (khi bấm xem)
    public class QuanLyXuatHuyDetailDto
    {
        public int IdPhieuXuatHuy { get; set; }
        public string LyDoHuy { get; set; } = string.Empty;
        public List<QuanLyChiTietXuatHuyDto> ChiTiet { get; set; } = new();
    }

    public class QuanLyChiTietXuatHuyDto
    {
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal SoLuong { get; set; }
        public string LyDoChiTiet { get; set; } = string.Empty;
    }

    // DTO Dùng để Gửi dữ liệu tạo phiếu mới từ Client lên Server
    public class QuanLyXuatHuySaveDto
    {
        public string LyDoHuy { get; set; } = string.Empty;
        public List<QuanLyChiTietXuatHuySaveDto> ChiTiet { get; set; } = new();
    }

    public class QuanLyChiTietXuatHuySaveDto
    {
        public int IdNguyenLieu { get; set; }
        public decimal SoLuong { get; set; }
        public string LyDoChiTiet { get; set; } = string.Empty;
    }

    // DTO cho ComboBox chọn Nguyên liệu
    public class LookupXuatHuyDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }
}