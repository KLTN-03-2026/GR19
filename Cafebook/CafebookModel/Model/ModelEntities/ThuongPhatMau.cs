using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ThuongPhatMau")]
    public class ThuongPhatMau
    {
        [Key]
        public int IdMau { get; set; }

        [Required]
        [StringLength(50)]
        public string Loai { get; set; } = "Thưởng";

        [Required]
        [StringLength(500)]
        public string TenMau { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }
    }
}