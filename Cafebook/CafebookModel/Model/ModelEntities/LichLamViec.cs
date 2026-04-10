using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("LichLamViec")]
    public class LichLamViec
    {
        [Key] public int IdLichLamViec { get; set; }
        public int IdNhanVien { get; set; }
        public int IdCa { get; set; }
        public DateTime NgayLam { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; } = "Đã duyệt";

        // BỔ SUNG CỘT GHI CHÚ NHIỆM VỤ CÁ NHÂN
        [StringLength(255)]
        public string? GhiChu { get; set; }

        [ForeignKey("IdNhanVien")] public virtual NhanVien NhanVien { get; set; } = null!;
        [ForeignKey("IdCa")] public virtual CaLamViec CaLamViec { get; set; } = null!;
        public virtual ICollection<BangChamCong> BangChamCongs { get; set; } = new List<BangChamCong>();
    }
}