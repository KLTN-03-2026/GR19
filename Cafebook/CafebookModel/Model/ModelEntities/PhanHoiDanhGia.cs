using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhanHoiDanhGia")]
    public class PhanHoiDanhGia
    {
        [Key]
        public int idPhanHoi { get; set; }

        [ForeignKey("DanhGia")]
        public int idDanhGia { get; set; }
        public virtual DanhGia? DanhGia { get; set; }

        [ForeignKey("NhanVien")]
        public int idNhanVien { get; set; }
        public virtual NhanVien? NhanVien { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string NoiDung { get; set; } = null!;

        [Required]
        // CHỈ CÓ MỘT DÒNG NÀY
        public DateTime NgayTao { get; set; }

        public PhanHoiDanhGia()
        {
            // Constructor gán giá trị cho thuộc tính NgayTao ở trên
            this.NgayTao = DateTime.Now;
        }
    }
}
