using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("BangChamCong")]
    public class BangChamCong
    {
        [Key]
        public int IdChamCong { get; set; }
        public int IdLichLamViec { get; set; }
        public DateTime? GioVao { get; set; }
        public DateTime? GioRa { get; set; }

        // --- SỬA LỖI TỪ double? SANG decimal? VÀ THÊM TYPE ---
        [Column(TypeName = "decimal(18, 2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? SoGioLam { get; set; }

        [ForeignKey("IdLichLamViec")]
        public virtual LichLamViec LichLamViec { get; set; } = null!;
    }
}
