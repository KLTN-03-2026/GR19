using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("CaiDat")]
    public class CaiDat
    {
        [Key]
        [StringLength(100)]
        public string TenCaiDat { get; set; } = string.Empty;
        [Required]
        public string GiaTri { get; set; } = string.Empty;
        [StringLength(500)]
        public string? MoTa { get; set; }
    }
}
