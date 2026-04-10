using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("HoaDon_KhuyenMai")]
    public class HoaDon_KhuyenMai
    {
        public int IdHoaDon { get; set; }
        public int IdKhuyenMai { get; set; }

        [ForeignKey("IdHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = null!;
        [ForeignKey("IdKhuyenMai")]
        public virtual KhuyenMai KhuyenMai { get; set; } = null!;
    }
}
