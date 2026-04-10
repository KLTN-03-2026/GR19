using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("Sach")]
    public class Sach
    {
        [Key]
        public int IdSach { get; set; }
        public string TenSach { get; set; } = null!;
        public int? NamXuatBan { get; set; }
        public string? MoTa { get; set; }
        public int SoLuongTong { get; set; }
        public int SoLuongHienCo { get; set; }
        public string? AnhBia { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiaBia { get; set; }
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
