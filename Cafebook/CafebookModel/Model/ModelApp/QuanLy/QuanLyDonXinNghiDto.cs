using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDonXinNghiGridDto
    {
        public int IdDonXinNghi { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string LoaiDon { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? TenNguoiDuyet { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public string? GhiChuPheDuyet { get; set; }
    }

    public class QuanLyDonXinNghiActionDto
    {
        public int IdNguoiDuyet { get; set; }
        public string? GhiChuPheDuyet { get; set; }
    }

    // THÊM MỚI: DTO hiển thị danh sách ca làm việc sẽ bị hủy
    public class AffectedShiftDto
    {
        public int IdLichLamViec { get; set; }
        public DateTime NgayLam { get; set; }
        public string TenCa { get; set; } = string.Empty;
    }
}