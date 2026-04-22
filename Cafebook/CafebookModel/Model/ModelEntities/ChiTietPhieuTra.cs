using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietPhieuTra")]
    public class ChiTietPhieuTra
    {
        public int IdPhieuTra { get; set; }
        public int IdSach { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TienPhat { get; set; } 

        public int? DoMoiKhiTra { get; set; }
        public string? GhiChuKhiTra { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TienPhatHuHong { get; set; } 

        [ForeignKey("IdPhieuTra")]
        public virtual PhieuTraSach PhieuTraSach { get; set; } = null!;

        [ForeignKey("IdSach")]
        public virtual Sach Sach { get; set; } = null!;
    }
}