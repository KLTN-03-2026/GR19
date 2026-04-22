namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class ChinhSachDto
    {
        // Thông tin chung
        public string TenQuan { get; set; } = string.Empty;
        public string GioiThieu { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string GioMoCua { get; set; } = string.Empty;
        public string GioDongCua { get; set; } = string.Empty;
        public string ThuMoCua { get; set; } = string.Empty;

        // Liên hệ
        public string Email { get; set; } = string.Empty;

        // Dịch vụ (Giá trị mặc định để phòng hờ)
        public decimal PhiThue { get; set; } = 15000;
        public decimal PhiTraTreMoiNgay { get; set; } = 5000;
        public string SoNgayMuonToiDa { get; set; } = "7";

        // Tích điểm
        public decimal DiemNhanVND { get; set; } = 10000;
        public decimal DiemDoiVND { get; set; } = 1000;
    }
}