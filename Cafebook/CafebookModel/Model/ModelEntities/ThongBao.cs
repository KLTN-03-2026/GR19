using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int IdThongBao { get; set; }

        public int? IdNhanVienTao { get; set; }

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime ThoiGianTao { get; set; }

        [StringLength(50)]
        public string? LoaiThongBao { get; set; }

        public int? IdLienQuan { get; set; } // Sẽ là idBan

        public bool DaXem { get; set; }

        [ForeignKey("IdNhanVienTao")]
        public virtual NhanVien? NhanVienTao { get; set; }
    }
}
