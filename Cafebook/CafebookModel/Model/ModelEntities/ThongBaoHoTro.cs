using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ThongBaoHoTro")]
    public class ThongBaoHoTro
    {
        [Key]
        public int IdThongBao { get; set; }

        // ======================================
        // === SỬA LỖI CS0266 TẠI ĐÂY ===
        // ======================================
        // 1. Xóa [Required]
        // 2. Thêm dấu '?' để cho phép NULL
        public int? IdKhachHang { get; set; }
        // ======================================

        [StringLength(100)]
        public string? GuestSessionId { get; set; } // Hỗ trợ khách vãng lai

        public string? NoiDungYeuCau { get; set; }

        [Required]
        public DateTime ThoiGianTao { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        public int? IdNhanVien { get; set; }

        public DateTime? ThoiGianPhanHoi { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Navigation Properties
        [ForeignKey("IdKhachHang")]
        public virtual KhachHang? KhachHang { get; set; } // Thêm '?'

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }

        public virtual ICollection<ChatLichSu> ChatLichSus { get; set; } = new List<ChatLichSu>();
    }
}
