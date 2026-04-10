using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("DinhLuong")]
    public class DinhLuong
    {
        public int IdSanPham { get; set; }
        public int IdNguyenLieu { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoLuongSuDung { get; set; } // <-- ĐÃ ĐỔI TÊN

        public int IdDonViSuDung { get; set; } // <-- THÊM MỚI

        [ForeignKey("IdSanPham")]
        public virtual SanPham SanPham { get; set; } = null!;
        [ForeignKey("IdNguyenLieu")]
        public virtual NguyenLieu NguyenLieu { get; set; } = null!;

        [ForeignKey("IdDonViSuDung")] // <-- THÊM MỚI
        public virtual DonViChuyenDoi DonViSuDung { get; set; } = null!;
    }
}
