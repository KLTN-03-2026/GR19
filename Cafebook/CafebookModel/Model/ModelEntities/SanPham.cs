using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int IdSanPham { get; set; }
        [Required]
        [StringLength(255)]
        public string TenSanPham { get; set; } = string.Empty;
        public int IdDanhMuc { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaBan { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThaiKinhDoanh { get; set; }
        public string? HinhAnh { get; set; } 
        [StringLength(50)]
        public string? NhomIn { get; set; }

        [ForeignKey("IdDanhMuc")]
        public virtual DanhMuc DanhMuc { get; set; } = null!;

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
        public virtual ICollection<NhatKyHuyMon> NhatKyHuyMons { get; set; } = new List<NhatKyHuyMon>();
        public virtual ICollection<DinhLuong> DinhLuongs { get; set; } = new List<DinhLuong>();
        public virtual ICollection<DeXuatSanPham> DeXuatSanPhamGocs { get; set; } = new List<DeXuatSanPham>();
        public virtual ICollection<DeXuatSanPham> DeXuatSanPhamDeXuats { get; set; } = new List<DeXuatSanPham>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        [NotMapped]
        public virtual ICollection<DeXuatSanPham> DeXuatSanPhams { get; set; } = new List<DeXuatSanPham>();
    }
}
