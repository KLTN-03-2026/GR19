using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietPhieuTra")]
    public class ChiTietPhieuTra
    {
        public int IdPhieuTra { get; set; }
        public int IdSach { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TienPhat { get; set; }

        // Navigation properties
        [ForeignKey("IdPhieuTra")]
        public virtual PhieuTraSach PhieuTraSach { get; set; } = null!;

        [ForeignKey("IdSach")]
        public virtual Sach Sach { get; set; } = null!;
    }
}
