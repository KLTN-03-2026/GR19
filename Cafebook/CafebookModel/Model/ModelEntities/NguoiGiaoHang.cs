using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NguoiGiaoHang")]
    public class NguoiGiaoHang
    {
        [Key]
        public int IdNguoiGiaoHang { get; set; }

        [Required]
        [StringLength(100)]
        public string TenNguoiGiaoHang { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TrangThai { get; set; }
    }
}