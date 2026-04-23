using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhatKyHeThong")]
    public class NhatKyHeThong
    {
        [Key]
        public int IdNhatKy { get; set; }

        public int? IdNhanVien { get; set; }

        public int? IdKhachHang { get; set; } 

        [StringLength(50)]
        public string? VaiTro { get; set; } 

        [Required]
        [StringLength(50)]
        public string HanhDong { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BangBiAnhHuong { get; set; } = string.Empty;

        [StringLength(100)]
        public string? KhoaChinh { get; set; }

        public string? DuLieuCu { get; set; }

        public string? DuLieuMoi { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? DiaChiIP { get; set; }

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }

        [ForeignKey("IdKhachHang")]
        public virtual KhachHang? KhachHang { get; set; } 
    }
}