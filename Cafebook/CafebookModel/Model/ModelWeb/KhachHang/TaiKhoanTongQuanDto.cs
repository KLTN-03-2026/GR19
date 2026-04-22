using System;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class TaiKhoanTongQuanDto
    {
        public int DiemTichLuy { get; set; }
        public decimal GiaTriQuyDoiVND { get; set; } 
        public int TongHoaDon { get; set; }
        public decimal TongChiTieu { get; set; }
        public DateTime NgayTao { get; set; }
    }
}