using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DeXuatSanPham")]
    public class DeXuatSanPham
    {
        public int IdSanPhamGoc { get; set; }
        public int IdSanPhamDeXuat { get; set; }
        public double DoLienQuan { get; set; }
        [Required]
        [StringLength(100)]
        public string LoaiDeXuat { get; set; } = string.Empty;

        [ForeignKey("IdSanPhamGoc")]
        public virtual SanPham SanPhamGoc { get; set; } = null!;
        [ForeignKey("IdSanPhamDeXuat")]
        public virtual SanPham SanPhamDeXuat { get; set; } = null!;
    }
}
