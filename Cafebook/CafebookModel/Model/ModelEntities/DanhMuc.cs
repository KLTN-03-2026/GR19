using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int IdDanhMuc { get; set; }
        [Required]
        [StringLength(255)]
        public string TenDanhMuc { get; set; } = string.Empty;
        public int? IdDanhMucCha { get; set; }

        [ForeignKey("IdDanhMucCha")]
        public virtual DanhMuc? DanhMucCha { get; set; }
        public virtual ICollection<DanhMuc> DanhMucCons { get; set; } = new List<DanhMuc>();
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
