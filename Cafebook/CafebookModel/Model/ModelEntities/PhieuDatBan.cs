using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuDatBan")]
    public class PhieuDatBan
    {
        [Key]
        public int IdPhieuDatBan { get; set; }
        public int? IdKhachHang { get; set; }
        public int IdBan { get; set; }
        [StringLength(100)]
        public string? HoTenKhach { get; set; }
        [StringLength(20)]
        public string? SdtKhach { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public int SoLuongKhach { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [ForeignKey("IdKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }
        [ForeignKey("IdBan")]
        public virtual Ban Ban { get; set; } = null!;
    }
}
