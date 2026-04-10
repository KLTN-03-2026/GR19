using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Ban")]
    public class Ban
    {
        [Key]
        public int IdBan { get; set; }
        [Required]
        [StringLength(50)]
        public string SoBan { get; set; } = string.Empty;
        public int SoGhe { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;
        [StringLength(500)]
        public string? GhiChu { get; set; }

        public int? IdKhuVuc { get; set; }
        [ForeignKey("IdKhuVuc")]
        public virtual KhuVuc? KhuVuc { get; set; }

        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        public virtual ICollection<PhieuDatBan> PhieuDatBans { get; set; } = new List<PhieuDatBan>();
    }
}
