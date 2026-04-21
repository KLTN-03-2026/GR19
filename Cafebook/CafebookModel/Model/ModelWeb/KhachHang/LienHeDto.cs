namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class LienHeDto
    {
        public string? TenQuan { get; set; }
        public string? GioiThieu { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? EmailLienHe { get; set; }
        public string? GioHoatDong { get; set; }
        public string? LinkFacebook { get; set; }
        public string? LinkInstagram { get; set; }
        public string? LinkGoogleMapsEmbed { get; set; }
        public string? LinkZalo { get; set; }
        public string? LinkYoutube { get; set; }
        public string? LinkWebsite { get; set; }
        public string? LinkX { get; set; }
    }

    public class PhanHoiInputModel
    {
        public string Ten { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
    }
}