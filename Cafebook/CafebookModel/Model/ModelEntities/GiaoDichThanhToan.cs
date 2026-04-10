using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("GiaoDichThanhToan")]
    public class GiaoDichThanhToan
    {
        [Key]
        public int IdGiaoDich { get; set; }
        public int IdHoaDon { get; set; }
        [Required]
        [StringLength(100)]
        public string MaGiaoDichNgoai { get; set; } = string.Empty; // Sẽ lưu mã VNPay ở đây
        [Required]
        [StringLength(50)]
        public string CongThanhToan { get; set; } = string.Empty; // Lưu chữ "VNPay"
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }
        public DateTime ThoiGianGiaoDich { get; set; }
        [Required]
        [StringLength(100)]
        public string TrangThai { get; set; } = string.Empty;
        [StringLength(50)]
        public string? MaLoi { get; set; }
        [StringLength(500)]
        public string? MoTaLoi { get; set; }

        [ForeignKey("IdHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = null!;
    }
}