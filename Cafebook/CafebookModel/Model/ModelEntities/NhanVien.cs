using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int IdNhanVien { get; set; }
        [Required]
        [StringLength(255)]
        public string HoTen { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;
        [StringLength(100)]
        public string? Email { get; set; }
        [StringLength(500)]
        public string? DiaChi { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public int IdVaiTro { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LuongCoBan { get; set; }
        [Required]
        [StringLength(50)]
        public string TrangThaiLamViec { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string TenDangNhap { get; set; } = string.Empty;
        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }

        [ForeignKey("IdVaiTro")]
        public virtual VaiTro VaiTro { get; set; } = null!;

        // --- ĐÃ FIX LỖI KHÓA NGOẠI ---
        [InverseProperty("NhanVienTao")]
        public virtual ICollection<HoaDon> HoaDonsTao { get; set; } = new List<HoaDon>();

        [InverseProperty("NhanVienGiaoHang")]
        public virtual ICollection<HoaDon> HoaDonsGiao { get; set; } = new List<HoaDon>();

        public virtual ICollection<NhatKyHuyMon> NhatKyHuyMons { get; set; } = new List<NhatKyHuyMon>();
        public virtual ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; } = new List<PhieuNhapKho>();
        public virtual ICollection<PhieuKiemKho> PhieuKiemKhos { get; set; } = new List<PhieuKiemKho>();
        public virtual ICollection<PhieuXuatHuy> PhieuXuatHuys { get; set; } = new List<PhieuXuatHuy>();
        public virtual ICollection<PhieuThueSach> PhieuThueSachs { get; set; } = new List<PhieuThueSach>();
        public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();

        [InverseProperty("NhanVien")]
        public virtual ICollection<PhieuLuong> PhieuLuongs { get; set; } = new List<PhieuLuong>();

        [InverseProperty("NguoiPhat")]
        public virtual ICollection<PhieuLuong> PhieuLuongsDaPhat { get; set; } = new List<PhieuLuong>();

        [InverseProperty("NhanVien")]
        public virtual ICollection<DonXinNghi> DonXinNghis { get; set; } = new List<DonXinNghi>();

        [InverseProperty("NguoiDuyet")]
        public virtual ICollection<DonXinNghi> DonXinNghiNguoiDuyets { get; set; } = new List<DonXinNghi>();

        public virtual ICollection<ChatLichSu> ChatLichSus { get; set; } = new List<ChatLichSu>();
        public virtual ICollection<PhieuTraSach> PhieuTraSachs { get; set; } = new List<PhieuTraSach>();
        [NotMapped]
        public virtual ICollection<PhieuThuongPhat> PhieuThuongPhatNguoiTaos { get; set; } = new List<PhieuThuongPhat>();
        // Thêm property này vào class NhanVien
        public virtual ICollection<NhanVien_Quyen> NhanVienQuyens { get; set; } = new List<NhanVien_Quyen>();
    }
}