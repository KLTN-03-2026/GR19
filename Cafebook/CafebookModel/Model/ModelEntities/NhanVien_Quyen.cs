using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhanVien_Quyen")]
    public class NhanVien_Quyen
    {
        [Key, Column(Order = 0)]
        public int IdNhanVien { get; set; }

        [Key, Column(Order = 1)]
        [StringLength(50)]
        public string IdQuyen { get; set; } = string.Empty;

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }

        [ForeignKey("IdQuyen")]
        public virtual Quyen? Quyen { get; set; }
    }
}