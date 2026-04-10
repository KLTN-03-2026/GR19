using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhatKyHeThong")]
    public class NhatKyHeThong
    {
        [Key]
        public int IdNhatKy { get; set; }

        public int? IdNhanVien { get; set; } // Người thực hiện thao tác

        [Required]
        [StringLength(50)]
        public string HanhDong { get; set; } = string.Empty; // THÊM MỚI, CẬP NHẬT, XÓA, ĐĂNG NHẬP

        [Required]
        [StringLength(100)]
        public string BangBiAnhHuong { get; set; } = string.Empty; // Tên bảng (VD: SanPham, HoaDon)

        [StringLength(100)]
        public string? KhoaChinh { get; set; } // ID của dòng dữ liệu bị tác động

        public string? DuLieuCu { get; set; } // Giá trị JSON trước khi thay đổi

        public string? DuLieuMoi { get; set; } // Giá trị JSON sau khi thay đổi

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? DiaChiIP { get; set; }

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}