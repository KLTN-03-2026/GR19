using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhuThu")]
    public class PhuThu
    {
        [Key]
        public int IdPhuThu { get; set; }
        [Required]
        [StringLength(100)]
        public string TenPhuThu { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaTri { get; set; }
        [Required]
        [StringLength(20)]
        public string LoaiGiaTri { get; set; } = string.Empty;

        public virtual ICollection<ChiTietPhuThuHoaDon> ChiTietPhuThuHoaDons { get; set; } = new List<ChiTietPhuThuHoaDon>();
    }
}
