using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("ChatLichSu")]
    public class ChatLichSu
    {
        [Key]
        public long IdChat { get; set; }
        public int? IdKhachHang { get; set; }
        public int? IdNhanVien { get; set; }
        [Required]
        public string NoiDungHoi { get; set; } = string.Empty;
        [Required]
        public string NoiDungTraLoi { get; set; } = string.Empty;
        public DateTime ThoiGian { get; set; }
        [StringLength(50)]
        public string? LoaiChat { get; set; }

        // === CẬP NHẬT (THEO FSD MỚI) ===
        [StringLength(100)]
        public string? GuestSessionId { get; set; } // Hỗ trợ khách vãng lai

        [StringLength(20)]
        public string? LoaiTinNhan { get; set; } // "KhachHang", "AI", "NhanVien"

        public int? IdThongBaoHoTro { get; set; }
        // === KẾT THÚC CẬP NHẬT ===

        [ForeignKey("IdKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }
        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }

        [ForeignKey("IdThongBaoHoTro")]
        public virtual ThongBaoHoTro? ThongBaoHoTro { get; set; }
    }
}
