using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach_TheLoai")]
    public class SachTheLoai
    {
        public int IdSach { get; set; }
        public virtual Sach Sach { get; set; } = null!;
        public int IdTheLoai { get; set; }
        public virtual TheLoai TheLoai { get; set; } = null!;
    }
}
