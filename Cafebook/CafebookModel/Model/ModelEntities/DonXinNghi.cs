using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DonXinNghi")]
    public class DonXinNghi
    {
        [Key]
        public int IdDonXinNghi { get; set; }
        public int IdNhanVien { get; set; }
        [Required]
        [StringLength(100)]
        public string LoaiDon { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        public string LyDo { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;
        public int? IdNguoiDuyet { get; set; }
        public DateTime? NgayDuyet { get; set; }
        [StringLength(255)]
        public string? GhiChuPheDuyet { get; set; }

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;
        [ForeignKey("IdNguoiDuyet")]
        public virtual NhanVien? NguoiDuyet { get; set; }
    }
}
