using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach_TacGia")]
    public class SachTacGia
    {
        public int IdSach { get; set; }
        public virtual Sach Sach { get; set; } = null!;
        public int IdTacGia { get; set; }
        public virtual TacGia TacGia { get; set; } = null!;
    }
}
