using System.Collections.Generic;
using System.Linq;

namespace CafebookModel.Model.ModelWeb.KhachHang
{
    public class CartSessionItemDto
    {
        public int IdSanPham { get; set; }
        public int SoLuong { get; set; }
    }

    public class GioHangSyncRequestDto
    {
        public List<CartSessionItemDto> Items { get; set; } = new();
        public string? MaKhuyenMaiApDung { get; set; }
    }

    public class GioHangItemDto
    {
        public int IdSanPham { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? HinhAnhUrl { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;

        public bool IsOutOfStock { get; set; } = false;
        public string? OutOfStockMessage { get; set; }
    }

    public class GioHangKhuyenMaiDto
    {
        public string MaKhuyenMai { get; set; } = string.Empty;
        public string TenChuongTrinh { get; set; } = string.Empty;
        public string? DieuKienApDung { get; set; }
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public decimal CalculatedDiscount { get; set; }
    }

    public class GioHangResponseDto
    {
        public List<GioHangItemDto> Items { get; set; } = new();
        public decimal TongTienHang => Items.Sum(x => x.ThanhTien);
        public int TongSoLuong => Items.Sum(x => x.SoLuong);

        public string? MaKhuyenMaiApDung { get; set; }
        public decimal TienGiamGia { get; set; }

        public decimal PhiGiaoHang { get; set; }

        public decimal TongTienThanhToan
        {
            get
            {
                decimal tienSauGiam = TongTienHang - TienGiamGia;
                if (tienSauGiam < 0) tienSauGiam = 0;
                return tienSauGiam + PhiGiaoHang;
            }
        }

        public bool CanCheckout { get; set; } = true;
        public string? CheckoutWarning { get; set; }
    }
}