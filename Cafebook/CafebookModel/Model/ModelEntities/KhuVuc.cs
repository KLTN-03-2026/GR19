using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("KhuVuc")]
    public class KhuVuc
    {
        [Key]
        public int IdKhuVuc { get; set; }
        [Required]
        [StringLength(100)]
        public string TenKhuVuc { get; set; } = string.Empty;
        [StringLength(500)]
        public string? MoTa { get; set; }

        public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();
    }
}
