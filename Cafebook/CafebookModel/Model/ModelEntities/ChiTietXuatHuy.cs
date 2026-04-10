using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietXuatHuy")]
    public class ChiTietXuatHuy
    {
        public int IdPhieuXuatHuy { get; set; }
        public int IdNguyenLieu { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoLuong { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DonGiaVon { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ThanhTien { get; set; }

        [ForeignKey("IdPhieuXuatHuy")]
        public virtual PhieuXuatHuy PhieuXuatHuy { get; set; } = null!;
        [ForeignKey("IdNguyenLieu")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;
    }
}
