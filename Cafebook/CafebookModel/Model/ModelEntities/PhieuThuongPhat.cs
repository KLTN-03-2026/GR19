using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuThuongPhat")]
    public class PhieuThuongPhat
    {
        [Key]
        public int IdPhieuThuongPhat { get; set; }

        public int IdNhanVien { get; set; }

        public int IdNguoiTao { get; set; }

        public DateTime NgayTao { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }

        [Required]
        [StringLength(500)]
        public string LyDo { get; set; } = string.Empty;

        public int? IdPhieuLuong { get; set; }

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;

        [ForeignKey("IdNguoiTao")]
        public virtual NhanVien NguoiTao { get; set; } = null!;

        [ForeignKey("IdPhieuLuong")]
        public virtual PhieuLuong? PhieuLuong { get; set; }
    }
}
