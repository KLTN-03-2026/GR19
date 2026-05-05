using CafebookModel.Model.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.NhanVien
{
    public class TongQuanDto
    {
        public ThongTinNhanVienDto ThongTin { get; set; } = new();
        public List<SharedThongBaoItemDto> DanhSachThongBao { get; set; } = new();
        public int SoThongBaoChuaDoc { get; set; }
    }

    public class ThongTinNhanVienDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; } // Thêm địa chỉ
        public string TrangThaiLamViec { get; set; } = string.Empty;
    }

    // --- CÁC DTO THÊM MỚI CHO CHỨC NĂNG CẬP NHẬT ---
    public class CapNhatThongTinWebDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string SoDienThoai { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? DiaChi { get; set; }
    }

    public class DoiMatKhauWebDto
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string MatKhauCu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}