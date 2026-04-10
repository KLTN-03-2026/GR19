using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhatKyHuyMon")]
    public class NhatKyHuyMon
    {
        [Key]
        public int IdNhatKy { get; set; }
        public int IdHoaDon { get; set; }
        public int IdSanPham { get; set; }
        public int SoLuongHuy { get; set; }
        [Required]
        [StringLength(255)]
        public string LyDo { get; set; } = string.Empty;
        public int IdNhanVienHuy { get; set; }
        public DateTime ThoiGianHuy { get; set; }

        [ForeignKey("IdHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = null!;
        [ForeignKey("IdSanPham")]
        public virtual SanPham SanPham { get; set; } = null!;
        [ForeignKey("IdNhanVienHuy")]
        public virtual NhanVien NhanVienHuy { get; set; } = null!;
    }
}
