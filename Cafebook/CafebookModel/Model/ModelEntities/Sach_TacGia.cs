using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach_TacGia")]
    public class SachTacGia
    {
        [Column("idSach")]
        public int IdSach { get; set; }

        [ForeignKey("IdSach")]
        public virtual Sach Sach { get; set; } = null!;

        [Column("idTacGia")]
        public int IdTacGia { get; set; }

        [ForeignKey("IdTacGia")]
        public virtual TacGia TacGia { get; set; } = null!;
    }
}