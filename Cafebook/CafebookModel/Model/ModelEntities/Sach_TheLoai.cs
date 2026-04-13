using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach_TheLoai")]
    public class SachTheLoai
    {
        [Column("idSach")]
        public int IdSach { get; set; }

        [ForeignKey("IdSach")]
        public virtual Sach Sach { get; set; } = null!;

        [Column("idTheLoai")]
        public int IdTheLoai { get; set; }

        [ForeignKey("IdTheLoai")]
        public virtual TheLoai TheLoai { get; set; } = null!;
    }
}