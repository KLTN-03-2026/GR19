using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class LichSuThueSachGridDto
    {
        public int IdPhieuThue { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public DateTime NgayThue { get; set; }
        public DateTime NgayHenTra { get; set; }
        public DateTime? NgayTraThucTe { get; set; }

        public int DoMoiKhiThue { get; set; }
        public string? GhiChuKhiThue { get; set; }
        public int? DoMoiKhiTra { get; set; }
        public string? GhiChuKhiTra { get; set; }

        public decimal TienPhat { get; set; } // Phạt trễ
        public decimal TienPhatHuHong { get; set; } // Phạt khấu hao

        public string TrangThai { get; set; } = string.Empty;
    }

    public class SachQuaHanGridDto
    {
        public int IdPhieuThue { get; set; }
        public int IdSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public DateTime NgayThue { get; set; }
        public DateTime NgayHenTra { get; set; }

        public int DoMoiKhiThue { get; set; }
        public string? GhiChuKhiThue { get; set; }

        public string TinhTrang { get; set; } = string.Empty;
    }

    public class BaoCaoLichSuThueDto
    {
        public System.Collections.Generic.List<SachQuaHanGridDto> SachQuaHan { get; set; } = new();
        public System.Collections.Generic.List<LichSuThueSachGridDto> LichSuThue { get; set; } = new();
    }
}