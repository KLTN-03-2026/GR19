using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuKiemKho")]
    public class PhieuKiemKho
    {
        [Key]
        public int IdPhieuKiemKho { get; set; }
        public int IdNhanVienKiem { get; set; }
        public DateTime NgayKiem { get; set; }
        [StringLength(500)]
        public string? GhiChu { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [ForeignKey("IdNhanVienKiem")]
        public virtual NhanVien NhanVienKiem { get; set; } = null!;

        public virtual ICollection<ChiTietKiemKho> ChiTietKiemKhos { get; set; } = new List<ChiTietKiemKho>();
    }
}
