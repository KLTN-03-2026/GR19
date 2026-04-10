using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("KhuyenMai")]
    public class KhuyenMai
    {
        [Key]
        public int IdKhuyenMai { get; set; }
        [Required]
        [StringLength(50)]
        public string MaKhuyenMai { get; set; } = string.Empty;
        [Required]
        [StringLength(255)]
        public string TenChuongTrinh { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        [Required]
        [StringLength(20)]
        public string LoaiGiamGia { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaTriGiam { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        [StringLength(500)]
        public string? DieuKienApDung { get; set; } // Mô tả cho DataGrid
        public int? SoLuongConLai { get; set; }

        // --- CÁC CỘT MỚI ---
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty; // Hoạt động, Tạm dừng, Hết hạn
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiamToiDa { get; set; }
        public int? IdSanPhamApDung { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? HoaDonToiThieu { get; set; }
        public TimeSpan? GioBatDau { get; set; }
        public TimeSpan? GioKetThuc { get; set; }
        [StringLength(50)]
        public string? NgayTrongTuan { get; set; }

        [ForeignKey("IdSanPhamApDung")]
        public virtual SanPham? SanPhamApDung { get; set; }

        public virtual ICollection<HoaDon_KhuyenMai> HoaDonKhuyenMais { get; set; } = new List<HoaDon_KhuyenMai>();
    }
}
