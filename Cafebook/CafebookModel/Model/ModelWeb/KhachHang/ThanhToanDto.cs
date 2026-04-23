using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class ThanhToanLoadDto
    {
        public bool IsStoreOpen { get; set; }
        public string StoreMessage { get; set; } = string.Empty;

        public KhachHangThanhToanDto KhachHang { get; set; } = new();
        public GioHangResponseDto CartSummary { get; set; } = new();
        public decimal TiLeDoiDiemVND { get; set; } = 1000;
        public List<GioHangKhuyenMaiDto> AvailablePromotions { get; set; } = new();
    }

    public class KhachHangThanhToanDto
    {
        public int IdKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public int DiemTichLuy { get; set; }
    }

    public class ThanhToanSubmitDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập SĐT")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = "COD";

        [Range(0, int.MaxValue, ErrorMessage = "Điểm sử dụng không hợp lệ")]
        public int DiemSuDung { get; set; } = 0;

        public string? GhiChu { get; set; }

        public string? ReturnUrl { get; set; }
        public GioHangSyncRequestDto? CartData { get; set; }
    }

    public class ThanhToanResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IdHoaDonMoi { get; set; }
        public string? PaymentUrl { get; set; }
    }

    public class ThanhToanThanhCongDto
    {
        public int IdHoaDonMoi { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public decimal TongTienHang { get; set; }
        public decimal GiamGia { get; set; }
        public decimal PhiGiaoHang { get; set; }
        public decimal ThanhTien { get; set; }
        public List<GioHangItemDto> Items { get; set; } = new();
    }
}