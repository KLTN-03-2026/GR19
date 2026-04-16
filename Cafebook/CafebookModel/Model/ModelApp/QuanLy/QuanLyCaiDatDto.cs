using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyCaiDatDto
    {
        [Required]
        public string TenCaiDat { get; set; } = string.Empty;

        [Required]
        public string GiaTri { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        // Trường bổ trợ để hiển thị trên UI theo nhóm (e.g., HR, VNPay, Smtp)
        public string Nhom { get; set; } = "Hệ thống";
    }
}