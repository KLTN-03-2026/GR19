using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuThueSach")]
    public class PhieuThueSach
    {
        [Key]
        public int IdPhieuThueSach { get; set; }
        public int IdKhachHang { get; set; }
        public int? IdNhanVien { get; set; }
        public DateTime NgayThue { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongTienCoc { get; set; }
        [ForeignKey("IdKhachHang")]
        public virtual KhachHang KhachHang { get; set; } = null!;
        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }
        public virtual ICollection<ChiTietPhieuThue> ChiTietPhieuThues { get; set; } = new List<ChiTietPhieuThue>();
        public virtual ICollection<PhieuTraSach> PhieuTraSachs { get; set; } = new List<PhieuTraSach>();
    }
}
