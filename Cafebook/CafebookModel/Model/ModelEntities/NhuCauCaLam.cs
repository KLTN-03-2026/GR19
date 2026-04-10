using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhuCauCaLam")]
    public class NhuCauCaLam
    {
        [Key]
        public int IdNhuCau { get; set; }

        public DateTime NgayLam { get; set; }

        public int IdCa { get; set; }

        public int IdVaiTro { get; set; }

        public int SoLuongCan { get; set; } = 1;

        [StringLength(50)]
        public string LoaiYeuCau { get; set; } = "Tất cả"; // Full-time, Part-time, Tất cả

        [StringLength(255)]
        public string? GhiChu { get; set; }

        [ForeignKey("IdCa")]
        public virtual CaLamViec CaLamViec { get; set; } = null!;

        [ForeignKey("IdVaiTro")]
        public virtual VaiTro VaiTro { get; set; } = null!;
    }
}