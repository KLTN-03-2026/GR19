using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach_NhaXuatBan")]
    public class SachNhaXuatBan
    {
        [Column("idSach")]
        public int IdSach { get; set; }

        [ForeignKey("IdSach")]
        public virtual Sach Sach { get; set; } = null!;

        [Column("idNhaXuatBan")]
        public int IdNhaXuatBan { get; set; }

        [ForeignKey("IdNhaXuatBan")]
        public virtual NhaXuatBan NhaXuatBan { get; set; } = null!;
    }
}