using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class ChamCongDashboardDto
    {
        public string TrangThai { get; set; } = "KhongCoCa";
        public bool DangTrongCa { get; set; }

        public string? TenCa { get; set; }
        public TimeSpan? GioBatDauCa { get; set; }
        public TimeSpan? GioKetThucCa { get; set; }

        public DateTime? LanVaoGanNhat { get; set; }
        public DateTime? LanRaGanNhat { get; set; }
        public decimal TongGioLamHienTai { get; set; }

        public string TenNhanVien { get; set; } = string.Empty;
        public int SoLanDiTreThangNay { get; set; } = 0;
        public int SoLanVeSomThangNay { get; set; } = 0; // Thêm biến đếm về sớm
    }

    public class LichSuItemDto
    {
        public string Ngay { get; set; } = string.Empty;
        public string CaLamViec { get; set; } = string.Empty;
        public string GioVaoNhanhNhat { get; set; } = string.Empty;
        public string GioRaMuonNhat { get; set; } = string.Empty;

        public string DiTre { get; set; } = string.Empty;
        public string VeSom { get; set; } = string.Empty; // Thêm cột Về Sớm (ví dụ: "15 phút")

        public decimal TongGioLam { get; set; }
        public int SoLanRaVao { get; set; }
    }

    public class ThongKeChamCongDto
    {
        public decimal TongGioLam { get; set; }
        public int SoLanDiTre { get; set; }
        public int SoLanVeSom { get; set; } // Thống kê thêm về sớm
    }

    public class LichSuChamCongPageDto
    {
        public List<LichSuItemDto> LichSuChamCong { get; set; } = new List<LichSuItemDto>();
        public ThongKeChamCongDto ThongKe { get; set; } = new ThongKeChamCongDto();
    }
}