using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach")]
    public class Sach
    {
        [Key]
        [Column("idSach")]
        public int IdSach { get; set; }

        [Required]
        [Column("tenSach")]
        [StringLength(500)]
        public string TenSach { get; set; } = string.Empty;

        [Column("namXuatBan")]
        public int? NamXuatBan { get; set; }

        [Column("moTa", TypeName = "ntext")]
        public string? MoTa { get; set; }

        [Column("soLuongTong")]
        public int SoLuongTong { get; set; }

        [Column("soLuongHienCo")]
        public int SoLuongHienCo { get; set; }

        [Column("AnhBia")]
        public string? AnhBia { get; set; }

        [Column("GiaBia", TypeName = "decimal(18, 2)")]
        public decimal? GiaBia { get; set; }

        [Column("ViTri")]
        [StringLength(100)]
        public string? ViTri { get; set; }

        public virtual ICollection<SachTacGia> SachTacGias { get; set; } = new List<SachTacGia>();
        public virtual ICollection<SachTheLoai> SachTheLoais { get; set; } = new List<SachTheLoai>();
        public virtual ICollection<SachNhaXuatBan> SachNhaXuatBans { get; set; } = new List<SachNhaXuatBan>();
        public virtual ICollection<ChiTietPhieuThue> ChiTietPhieuThues { get; set; } = new List<ChiTietPhieuThue>();
        [InverseProperty("SachGoc")]
        public virtual ICollection<DeXuatSach> DeXuatSachGocs { get; set; } = new List<DeXuatSach>();
        [InverseProperty("SachDeXuat")]
        public virtual ICollection<DeXuatSach> DeXuatSachDeXuats { get; set; } = new List<DeXuatSach>();
        public virtual ICollection<ChiTietPhieuTra> ChiTietPhieuTras { get; set; } = new List<ChiTietPhieuTra>();
        [NotMapped]
        public virtual ICollection<DeXuatSach> DeXuatSachs { get; set; } = new List<DeXuatSach>();
    }
}
