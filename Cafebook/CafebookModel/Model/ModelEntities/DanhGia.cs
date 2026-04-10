using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        public int idDanhGia { get; set; }

        [ForeignKey("KhachHang")]
        public int idKhachHang { get; set; }
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("SanPham")]
        public int? idSanPham { get; set; }
        public virtual SanPham? SanPham { get; set; }

        [ForeignKey("HoaDon")]
        public int idHoaDon { get; set; }
        public virtual HoaDon? HoaDon { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? BinhLuan { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? HinhAnhURL { get; set; }

        [Required]
        // CHỈ CÓ MỘT DÒNG NÀY
        public DateTime NgayTao { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Hiển thị";

        public virtual ICollection<PhanHoiDanhGia> PhanHoiDanhGias { get; set; } = new List<PhanHoiDanhGia>();

        public DanhGia()
        {
            // Constructor gán giá trị cho thuộc tính NgayTao ở trên
            this.NgayTao = DateTime.Now;
        }
    }
}
