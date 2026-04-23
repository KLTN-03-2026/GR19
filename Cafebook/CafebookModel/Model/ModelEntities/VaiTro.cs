using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("VaiTro")]
    public class VaiTro
    {
        [Key]
        public int IdVaiTro { get; set; }

        [Required]
        [StringLength(100)]
        public string TenVaiTro { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MoTa { get; set; }

        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();

        public virtual ICollection<NhuCauCaLam> NhuCauCaLams { get; set; } = new List<NhuCauCaLam>();
    }
}