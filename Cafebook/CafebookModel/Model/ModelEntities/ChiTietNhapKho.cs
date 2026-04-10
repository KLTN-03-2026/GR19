using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietNhapKho")]
    public class ChiTietNhapKho
    {
        public int IdPhieuNhapKho { get; set; }
        public int IdNguyenLieu { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoLuongNhap { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DonGiaNhap { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ThanhTien { get; set; }

        [ForeignKey("IdPhieuNhapKho")]
        public virtual PhieuNhapKho PhieuNhapKho { get; set; } = null!;
        [ForeignKey("IdNguyenLieu")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;
    }
}
