using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("PhieuLuong")]
    public class PhieuLuong
    {
        [Key]
        public int IdPhieuLuong { get; set; }
        public int IdNhanVien { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LuongCoBan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongGioLam { get; set; }

        // --- SỬA LỖI KHỚP VỚI CSDL (cho phép NULL) ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TienThuong { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? KhauTru { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ThucLanh { get; set; }
        public DateTime NgayTao { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;

        // --- SỬA LỖI KHỚP VỚI CSDL (thêm cột mới) ---
        public DateTime? NgayPhatLuong { get; set; }
        public int? IdNguoiPhat { get; set; }
        [ForeignKey("IdNguoiPhat")]
        public virtual NhanVien? NguoiPhat { get; set; }
    }
}
