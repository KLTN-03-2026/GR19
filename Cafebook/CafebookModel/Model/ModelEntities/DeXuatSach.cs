using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DeXuatSach")]
    public class DeXuatSach
    {
        public int IdSachGoc { get; set; }
        public int IdSachDeXuat { get; set; }
        public double DoLienQuan { get; set; }
        [Required]
        [StringLength(100)]
        public string LoaiDeXuat { get; set; } = string.Empty;

        [ForeignKey("IdSachGoc")]
        public virtual Sach SachGoc { get; set; } = null!;
        [ForeignKey("IdSachDeXuat")]
        public virtual Sach SachDeXuat { get; set; } = null!;
    }
}
