using System;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyThuongPhatGridDto
    {
        public int IdPhieuThuongPhat { get; set; }
        public DateTime NgayTao { get; set; }
        public decimal SoTien { get; set; }
        public string Loai { get; set; } = string.Empty; // "Thưởng" hoặc "Phạt"
        public string LyDo { get; set; } = string.Empty;
        public string TenNguoiTao { get; set; } = string.Empty;
        public bool DaChot { get; set; } // true nếu idPhieuLuong != null
    }

    public class QuanLyThuongPhatSaveDto
    {
        public int IdNhanVien { get; set; }
        public int IdNguoiTao { get; set; }
        public DateTime NgayTao { get; set; }
        public decimal SoTien { get; set; } // Luôn gửi lên số dương, BE tự tính âm/dương dựa vào Loai
        public string Loai { get; set; } = "Thưởng";
        [Required] public string LyDo { get; set; } = string.Empty;
    }
}