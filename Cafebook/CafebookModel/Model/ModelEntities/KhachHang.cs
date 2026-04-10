using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafebookModel.Model.ModelEntities
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public int IdKhachHang { get; set; }
        [Required]
        [StringLength(255)]
        public string HoTen { get; set; } = string.Empty;
        [StringLength(20)]
        public string? SoDienThoai { get; set; }
        [StringLength(100)]
        public string? Email { get; set; }
        [StringLength(500)]
        public string? DiaChi { get; set; }
        public int DiemTichLuy { get; set; }
        [StringLength(100)]
        public string? TenDangNhap { get; set; }
        [StringLength(255)]
        public string? MatKhau { get; set; }
        public DateTime NgayTao { get; set; }
        public bool BiKhoa { get; set; }
        public string? AnhDaiDien { get; set; }
        public bool TaiKhoanTam { get; set; }

        // --- ĐÃ FIX LỖI TẠI ĐÂY ---
        // Xóa 2 dòng HoaDonsTao và HoaDonsGiao bị sai.
        // Thay bằng một danh sách HoaDons chuẩn của Khách Hàng.
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

        public virtual ICollection<PhieuDatBan> PhieuDatBans { get; set; } = new List<PhieuDatBan>();
        public virtual ICollection<PhieuThueSach> PhieuThueSachs { get; set; } = new List<PhieuThueSach>();
        public virtual ICollection<ChatLichSu> ChatLichSus { get; set; } = new List<ChatLichSu>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}