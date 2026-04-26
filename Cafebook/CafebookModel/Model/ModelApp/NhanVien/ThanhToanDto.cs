using CafebookModel.Model.ModelEntities;
using System.Collections.Generic;
using System; // Thêm

namespace CafebookModel.Model.ModelApp.NhanVien
{
    public class PhuThuDto
    {
        public int IdPhuThu { get; set; }
        public string TenPhuThu { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public string LoaiGiaTri { get; set; } = "VND";
        public decimal GiaTri { get; set; }
    }

    public class KhachHangTimKiemDto
    {
        public int IdKhachHang { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public KhachHang KhachHangData { get; set; } = null!;
        public bool IsNew { get; set; } // <-- THÊM DÒNG NÀY
    }

    public class HoaDonPreviewDto
    {
        public string TenQuan { get; set; } = "CafeBook";
        public string DiaChi { get; set; } = "N/A";
        public string SoDienThoai { get; set; } = "N/A";
        public string WifiMatKhau { get; set; } = "N/A";
        public int IdHoaDon { get; set; }
        public string SoBan { get; set; } = "Mang về";
        public DateTime ThoiGianTao { get; set; }
        public string TenNhanVien { get; set; } = "Nhân viên";
        public string TenKhachHang { get; set; } = "Khách vãng lai";
        public bool IsProvisional { get; set; } = true; // Là hóa đơn Tạm tính?
        public List<ChiTietDto> Items { get; set; } = new List<ChiTietDto>();
        public List<PhuThuDto> Surcharges { get; set; } = new List<PhuThuDto>();
        public decimal TongTienGoc { get; set; }
        public decimal GiamGiaKM { get; set; }
        public decimal GiamGiaDiem { get; set; }
        public decimal TongPhuThu { get; set; }
        public decimal ThanhTien { get; set; }
        public string PhuongThucThanhToan { get; set; } = "Tiền mặt";
        public decimal KhachDua { get; set; }
        public decimal TienThoi { get; set; }
        public int DiemCong { get; set; }
        public int TongDiemTichLuy { get; set; }
    }

    public class ThanhToanViewDto
    {
        public HoaDonInfoDto HoaDonInfo { get; set; } = null!;
        public List<ChiTietDto> ChiTietItems { get; set; } = new List<ChiTietDto>();
        public int? IdKhuyenMaiDaApDung { get; set; }
        public List<PhuThuDto> PhuThusDaApDung { get; set; } = new List<PhuThuDto>();
        public List<PhuThu> PhuThusKhaDung { get; set; } = new List<PhuThu>();
        public KhachHang? KhachHang { get; set; }
        public List<KhachHangTimKiemDto> KhachHangsList { get; set; } = new List<KhachHangTimKiemDto>();
        public decimal DiemTichLuy_DoiVND { get; set; }
        public decimal DiemTichLuy_NhanVND { get; set; }
        public string TenQuan { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string WifiMatKhau { get; set; } = string.Empty;

        public string NganHang_SoTaiKhoan { get; set; } = string.Empty;
        public string NganHang_ChuTaiKhoan { get; set; } = string.Empty;
        public string NganHang_MaDinhDanhNganHang { get; set; } = string.Empty;

    }

    public class ThanhToanRequestDto
    {
        public int IdHoaDonGoc { get; set; }
        public List<int> IdChiTietTach { get; set; } = new List<int>();
        public List<int> IdPhuThuTach { get; set; } = new List<int>();
        public int? IdKhuyenMai { get; set; }
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        public decimal KhachDua { get; set; }
        public int DiemSuDung { get; set; }
        public int? IdKhachHang { get; set; }
    }

    public class KhuyenMaiHienThiThanhToanDto
    {
        public int IdKhuyenMai { get; set; }
        public string MaKhuyenMai { get; set; } = string.Empty;
        public string TenChuongTrinh { get; set; } = string.Empty;
        public string? DieuKienApDung { get; set; }
        public string LoaiGiamGia { get; set; } = string.Empty;
        public decimal GiaTriGiam { get; set; }
        public decimal? GiamToiDa { get; set; }
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public decimal CalculatedDiscount { get; set; }
    }

    public class VNPayUrlRequestDto
    {
        public decimal Amount { get; set; }
        public int IdHoaDonGoc { get; set; }
    }

    public class VNPayUrlResponseDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
    }
}