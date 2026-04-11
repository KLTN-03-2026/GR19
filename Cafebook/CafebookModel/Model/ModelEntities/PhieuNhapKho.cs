using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuNhapKho")]
    public class PhieuNhapKho
    {
        [Key]
        public int IdPhieuNhapKho { get; set; }
        public int? IdNhaCungCap { get; set; }
        public int IdNhanVien { get; set; }
        public DateTime NgayNhap { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongTien { get; set; }
        [StringLength(500)]
        public string? GhiChu { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [ForeignKey("IdNhaCungCap")]
        public virtual NhaCungCap? NhaCungCap { get; set; }
        [ForeignKey("IdNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;
        public string? HoaDonDinhKem { get; set; }
        public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();
    }
}
