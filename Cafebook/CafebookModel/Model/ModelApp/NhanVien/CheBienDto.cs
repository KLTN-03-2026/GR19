using System;
using System.Collections.Generic; // Thêm

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class CheBienItemDto
    {
        public int IdTrangThaiCheBien { get; set; }
        public int IdHoaDon { get; set; } 
        public int IdSanPham { get; set; } 
        public string TenMon { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = string.Empty; 
        public DateTime ThoiGianGoi { get; set; }
        public string NhomIn { get; set; } = string.Empty;
        public string LoaiHoaDon { get; set; } = string.Empty; 
        public string ThoiGianGoiDisplay
        {
            get { return ThoiGianGoi.ToString("HH:mm:ss"); }
        }

        public string ThoiGianCho
        {
            get
            {
                var thoiGian = (DateTime.Now - ThoiGianGoi).TotalMinutes;
                return $"({Math.Floor(thoiGian)} phút)";
            }
        }

        public bool IsChoLam
        {
            get { return TrangThai == "Chờ làm"; }
        }

        public bool IsDangLam
        {
            get { return TrangThai == "Đang làm"; }
        }
    }

    // === THÊM MỚI DTO CHO CÔNG THỨC ===
    public class CongThucItemDto
    {
        public string TenNguyenLieu { get; set; } = string.Empty;
        public decimal SoLuongSuDung { get; set; }
        public string TenDonVi { get; set; } = string.Empty;
    }

    public class CheBienGroupDto
    {
        public int IdHoaDon { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public string LoaiHoaDon { get; set; } = string.Empty; // Thêm cái này
        public DateTime ThoiGianGoiNhoNhat { get; set; }

        public string ThoiGianCho => $"({Math.Floor((DateTime.Now - ThoiGianGoiNhoNhat).TotalMinutes)} phút)";

        public string LoaiDonHienThi
        {
            get
            {
                if (LoaiHoaDon == "Giao hàng") return "🚚 GIAO HÀNG";
                if (LoaiHoaDon == "Tại quán" && (string.IsNullOrEmpty(SoBan) || SoBan.Contains("Mang về")))
                    return "🛍️ MANG VỀ";
                return "☕ TẠI QUÁN";
            }
        }

        public List<CheBienItemDto> Items { get; set; } = new List<CheBienItemDto>();
    }
}