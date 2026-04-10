using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietKiemKho")]
    public class ChiTietKiemKho
    {
        public int IdPhieuKiemKho { get; set; }
        public int IdNguyenLieu { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TonKhoHeThong { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TonKhoThucTe { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ChenhLech { get; set; }
        [StringLength(255)]
        public string? LyDoChenhLech { get; set; }

        [ForeignKey("IdPhieuKiemKho")]
        public virtual PhieuKiemKho PhieuKiemKho { get; set; } = null!;
        [ForeignKey("IdNguyenLieu")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;
    }
}
