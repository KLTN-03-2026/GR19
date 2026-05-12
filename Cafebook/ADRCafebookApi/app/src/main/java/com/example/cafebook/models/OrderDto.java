package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class OrderDto {
    // DTO cho danh sách đơn hàng
    public static class HistoryItem {
        @SerializedName("idHoaDon") public int idHoaDon;
        @SerializedName("maDonHang") public String maDonHang;
        @SerializedName("thoiGianTao") public String thoiGianTao;
        @SerializedName("tenSanPham") public String tenSanPham;
        @SerializedName("soLuongSanPhamKhac") public int soLuongKhach;
        @SerializedName("hinhAnhUrl") public String hinhAnhUrl;
        @SerializedName("thanhTien") public double thanhTien;
        @SerializedName("trangThaiGiaoHang") public String trangThaiGiaoHang;
        @SerializedName("trangThaiThanhToan") public String trangThaiThanhToan;
        @SerializedName("phuongThucThanhToan") public String phuongThucThanhToan;
    }

    // DTO cho chi tiết đơn hàng
    public static class Detail {
        @SerializedName("idHoaDon") public int idHoaDon;
        @SerializedName("maDonHang") public String maDonHang;
        @SerializedName("trangThaiGiaoHang") public String trangThaiGiaoHang;
        @SerializedName("trangThaiThanhToan") public String trangThaiThanhToan;
        @SerializedName("thoiGianTao") public String thoiGianTao;
        @SerializedName("hoTen") public String hoTen;
        @SerializedName("soDienThoai") public String soDienThoai;
        @SerializedName("diaChiGiaoHang") public String diaChiGiaoHang;
        @SerializedName("tongTienHang") public double tongTienHang;
        @SerializedName("phiGiaoHang") public double phiGiaoHang;
        @SerializedName("giamGia") public double giamGia;
        @SerializedName("thanhTien") public double thanhTien;
        @SerializedName("trackingEvents") public List<TrackingEvent> trackingEvents;
        @SerializedName("items") public List<OrderItem> items;
        @SerializedName("anhXacNhanGiaoHangUrl") public String anhXacNhanGiaoHangUrl;
        @SerializedName("isStoreOpen") public boolean isStoreOpen;
    }

    public static class TrackingEvent {
        @SerializedName("timestamp") public String timestamp;
        @SerializedName("status") public String status;
        @SerializedName("description") public String description;
        @SerializedName("isCurrent") public boolean isCurrent;
    }

    public static class OrderItem {
        @SerializedName("idSanPham") public int idSanPham;
        @SerializedName("tenSanPham") public String tenSanPham;
        @SerializedName("hinhAnhUrl") public String hinhAnhUrl;
        @SerializedName("soLuong") public int soLuong;
        @SerializedName("donGia") public double donGia;
        @SerializedName("thanhTien") public double thanhTien;
        @SerializedName("daDanhGia") public boolean daDanhGia;
    }

    public static class RepayResponse {
        @SerializedName("paymentUrl") public String paymentUrl;
    }
}
