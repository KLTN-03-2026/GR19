using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Quyen")]
    public class Quyen
    {
        [Key]
        [StringLength(100)]
        public string IdQuyen { get; set; } = string.Empty;
        [Required]
        [StringLength(255)]
        public string TenQuyen { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string NhomQuyen { get; set; } = string.Empty;

        // XÓA dòng này (nếu có): 
        // public virtual ICollection<VaiTro_Quyen> VaiTroQuyens { get; set; } = new List<VaiTro_Quyen>();

        // THÊM dòng này:
        public virtual ICollection<NhanVien_Quyen> NhanVienQuyens { get; set; } = new List<NhanVien_Quyen>();
    }
}
