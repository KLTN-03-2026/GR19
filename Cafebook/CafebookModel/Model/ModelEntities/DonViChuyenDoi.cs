using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DonViChuyenDoi")]
    public class DonViChuyenDoi
    {
        [Key]
        public int IdChuyenDoi { get; set; }

        public int IdNguyenLieu { get; set; }

        [Required]
        [StringLength(50)]
        public string TenDonVi { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 6)")]
        public decimal GiaTriQuyDoi { get; set; }

        public bool LaDonViCoBan { get; set; }

        [ForeignKey("IdNguyenLieu")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;
        public virtual ICollection<DinhLuong> DinhLuongs { get; set; } = new List<DinhLuong>();
    }
}
