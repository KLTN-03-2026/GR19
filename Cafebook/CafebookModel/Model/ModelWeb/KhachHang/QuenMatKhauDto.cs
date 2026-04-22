using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class GuiMaXacNhanRequestDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MaXacNhan { get; set; } = string.Empty;
    }
    public class ResetPasswordRequestDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}