using System;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class TaoDonXinNghiWebRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn loại đơn.")]
        public string LoaiDon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do nghỉ.")]
        public string LyDo { get; set; } = string.Empty;

        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
    }

    public class LichSuDonXinNghiWebDto
    {
        public int IdDonXinNghi { get; set; }
        public string LoaiDon { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public DateTime? NgayDuyet { get; set; }
        public string? GhiChuPheDuyet { get; set; }
        public string NguoiDuyet { get; set; } = string.Empty;
    }
}