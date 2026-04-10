using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NguyenLieu")]
    public class NguyenLieu
    {
        [Key]
        public int IdNguyenLieu { get; set; }
        [Required]
        [StringLength(255)]
        public string TenNguyenLieu { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string DonViTinh { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TonKho { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TonKhoToiThieu { get; set; }

        public virtual ICollection<DinhLuong> DinhLuongs { get; set; } = new List<DinhLuong>();
        public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();
        public virtual ICollection<ChiTietKiemKho> ChiTietKiemKhos { get; set; } = new List<ChiTietKiemKho>();
        public virtual ICollection<ChiTietXuatHuy> ChiTietXuatHuys { get; set; } = new List<ChiTietXuatHuy>();
    }
}
