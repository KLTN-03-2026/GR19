using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuTraSach")]
    public class PhieuTraSach
    {
        [Key]
        public int IdPhieuTra { get; set; }

        public int IdPhieuThueSach { get; set; }
        public int IdNhanVien { get; set; }
        public DateTime NgayTra { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongPhiThue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongTienPhat { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongTienCocHoan { get; set; }

        public int DiemTichLuy { get; set; }

        // Navigation properties
        [ForeignKey("IdPhieuThueSach")]
        public virtual PhieuThueSach PhieuThueSach { get; set; } = null!;

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;

        public virtual ICollection<ChiTietPhieuTra> ChiTietPhieuTras { get; set; } = new List<ChiTietPhieuTra>();

    }
}
