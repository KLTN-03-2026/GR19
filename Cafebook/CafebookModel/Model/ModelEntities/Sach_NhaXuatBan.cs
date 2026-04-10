using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach_NhaXuatBan")]
    public class SachNhaXuatBan
    {
        public int IdSach { get; set; }
        public virtual Sach Sach { get; set; } = null!;
        public int IdNhaXuatBan { get; set; }
        public virtual NhaXuatBan NhaXuatBan { get; set; } = null!;
    }
}
