using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("BangChamCong")]
    public class BangChamCong
    {
        [Key]
        [Column("idChamCong")]
        public int IdChamCong { get; set; }

        [Column("idLichLamViec")]
        public int IdLichLamViec { get; set; }

        [Column("gioVao")]
        public DateTime? GioVao { get; set; }

        [Column("gioRa")]
        public DateTime? GioRa { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("soGioLam", TypeName = "decimal(18, 2)")]
        public decimal? SoGioLam { get; set; }

        [Column("ghiChuSua")]
        [StringLength(500)]
        public string? GhiChuSua { get; set; }

        [ForeignKey("IdLichLamViec")]
        public virtual LichLamViec LichLamViec { get; set; } = null!;
    }
}