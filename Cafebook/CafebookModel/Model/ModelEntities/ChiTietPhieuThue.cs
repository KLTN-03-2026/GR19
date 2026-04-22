using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietPhieuThue")]
    public class ChiTietPhieuThue
    {
        public int IdPhieuThueSach { get; set; }
        public int IdSach { get; set; }
        public DateTime NgayHenTra { get; set; }
        public DateTime? NgayTraThucTe { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TienCoc { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TienPhatTraTre { get; set; }

        public int? DoMoiKhiThue { get; set; } = 100;
        public string? GhiChuKhiThue { get; set; }

        [ForeignKey("IdPhieuThueSach")]
        public virtual PhieuThueSach PhieuThueSach { get; set; } = null!;
        [ForeignKey("IdSach")]
        public virtual Sach Sach { get; set; } = null!;
    }
}