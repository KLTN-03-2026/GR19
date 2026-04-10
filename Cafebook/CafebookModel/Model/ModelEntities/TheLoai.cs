using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("TheLoai")]
    public class TheLoai
    {
        [Key]
        public int IdTheLoai { get; set; }
        public string TenTheLoai { get; set; } = null!;
        public string? MoTa { get; set; }
        public virtual ICollection<SachTheLoai> SachTheLoais { get; set; } = new List<SachTheLoai>();
    }
}
