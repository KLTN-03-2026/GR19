using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class DoiMatKhauDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string MatKhauCu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, ErrorMessage = "Mật khẩu mới phải dài ít nhất 6 ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string MatKhauMoi { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }
}