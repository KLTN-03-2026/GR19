using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        public int IdHoaDon { get; set; }
        public int? IdBan { get; set; }
        public int? IdNhanVien { get; set; } // Nhân viên order/tạo đơn
        public int? IdKhachHang { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public DateTime? ThoiGianThanhToan { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongTienGoc { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiamGia { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongPhuThu { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal ThanhTien { get; set; }
        [StringLength(50)]
        public string? PhuongThucThanhToan { get; set; }
        public string? GhiChu { get; set; }
        [Required]
        [StringLength(50)]
        public string LoaiHoaDon { get; set; } = string.Empty;
        [StringLength(100)]
        public string? TrangThaiGiaoHang { get; set; }
        [StringLength(500)]
        public string? DiaChiGiaoHang { get; set; }
        [StringLength(20)]
        public string? SoDienThoaiGiaoHang { get; set; }
        public string? AnhGiaoHang { get; set; }
        public int? IdNguoiGiaoHang { get; set; } // Sẽ lưu IdNhanVien của Shipper

        [ForeignKey("IdBan")]
        public virtual Ban? Ban { get; set; }

        [ForeignKey("IdNhanVien")]
        public virtual NhanVien? NhanVienTao { get; set; } // Map với InverseProperty NhanVienTao

        [ForeignKey("IdKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("IdNguoiGiaoHang")]
        public virtual NhanVien? NhanVienGiaoHang { get; set; } // Map với InverseProperty NhanVienGiaoHang

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
        public virtual ICollection<NhatKyHuyMon> NhatKyHuyMons { get; set; } = new List<NhatKyHuyMon>();
        public virtual ICollection<GiaoDichThanhToan> GiaoDichThanhToans { get; set; } = new List<GiaoDichThanhToan>();
        public virtual ICollection<ChiTietPhuThuHoaDon> ChiTietPhuThuHoaDons { get; set; } = new List<ChiTietPhuThuHoaDon>();
        public virtual ICollection<HoaDon_KhuyenMai> HoaDonKhuyenMais { get; set; } = new List<HoaDon_KhuyenMai>();
    }
}