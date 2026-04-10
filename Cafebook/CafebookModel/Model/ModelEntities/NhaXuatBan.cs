using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhaXuatBan")]
    public class NhaXuatBan
    {
        [Key]
        public int IdNhaXuatBan { get; set; }
        public string TenNhaXuatBan { get; set; } = null!;
        public string? MoTa { get; set; }
        public virtual ICollection<SachNhaXuatBan> SachNhaXuatBans { get; set; } = new List<SachNhaXuatBan>();
    }
}
