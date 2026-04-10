using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChiTietPhuThuHoaDon")]
    public class ChiTietPhuThuHoaDon
    {
        public int IdHoaDon { get; set; }
        public int IdPhuThu { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTien { get; set; }

        [ForeignKey("IdHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = null!;
        [ForeignKey("IdPhuThu")]
        public virtual PhuThu PhuThu { get; set; } = null!;
    }
}
