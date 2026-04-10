using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuXuatHuy")]
    public class PhieuXuatHuy
    {
        [Key]
        public int IdPhieuXuatHuy { get; set; }
        public int IdNhanVienXuat { get; set; }
        public DateTime NgayXuatHuy { get; set; }
        [Required]
        [StringLength(500)]
        public string LyDoXuatHuy { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongGiaTriHuy { get; set; }

        [ForeignKey("IdNhanVienXuat")]
        public virtual NhanVien NhanVienXuat { get; set; } = null!;

        public virtual ICollection<ChiTietXuatHuy> ChiTietXuatHuys { get; set; } = new List<ChiTietXuatHuy>();
    }
}
