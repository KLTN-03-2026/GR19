using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("TrangThaiCheBien")]
    public class TrangThaiCheBien
    {
        [Key]
        public int IdTrangThaiCheBien { get; set; }

        public int IdChiTietHoaDon { get; set; }
        public int IdHoaDon { get; set; }
        public int IdSanPham { get; set; }

        [Required]
        [StringLength(255)]
        public string TenMon { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SoBan { get; set; } = string.Empty;

        public int SoLuong { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [StringLength(50)]
        public string? NhomIn { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty; // Chờ làm, Đang làm, Hoàn thành

        public DateTime ThoiGianGoi { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianHoanThanh { get; set; }

        // Khóa ngoại
        [ForeignKey("IdChiTietHoaDon")]
        public virtual ChiTietHoaDon ChiTietHoaDon { get; set; } = null!;

        [ForeignKey("IdHoaDon")]
        public virtual HoaDon HoaDon { get; set; } = null!;

        [ForeignKey("IdSanPham")]
        public virtual SanPham SanPham { get; set; } = null!;
    }
}
