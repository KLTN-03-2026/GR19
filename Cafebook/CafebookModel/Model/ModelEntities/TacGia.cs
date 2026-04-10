using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("TacGia")]
    public class TacGia
    {
        [Key]
        public int IdTacGia { get; set; }
        [Required]
        [StringLength(255)]
        public string TenTacGia { get; set; } = string.Empty;
        public string? GioiThieu { get; set; }
        public virtual ICollection<SachTacGia> SachTacGias { get; set; } = new List<SachTacGia>();
    }
}
