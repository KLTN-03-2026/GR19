using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhaCungCap")]
    public class NhaCungCap
    {
        [Key]
        public int IdNhaCungCap { get; set; }
        [Required]
        [StringLength(255)]
        public string TenNhaCungCap { get; set; } = string.Empty;
        [StringLength(20)]
        public string? SoDienThoai { get; set; }
        [StringLength(500)]
        public string? DiaChi { get; set; }
        [StringLength(100)]
        public string? Email { get; set; }

        public virtual ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; } = new List<PhieuNhapKho>();
    }
}
