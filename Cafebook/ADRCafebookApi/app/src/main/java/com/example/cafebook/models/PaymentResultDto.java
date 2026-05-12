package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class PaymentResultDto {

    public static class VNPayVerifyResult {
        @SerializedName("success") private boolean success;
        @SerializedName("encodedId") private String encodedId;
        @SerializedName("message") private String message;

        public boolean isSuccess() { return success; }
        public String getEncodedId() { return encodedId; }
        public String getMessage() { return message; }
    }

    public static class OrderSummary {
        @SerializedName("idHoaDonMoi") private int idHoaDonMoi;
        @SerializedName("thoiGianTao") private String thoiGianTao;
        @SerializedName("hoTen") private String hoTen;
        @SerializedName("trangThai") private String trangThai;
        @SerializedName("phuongThucThanhToan") private String phuongThucThanhToan;
        @SerializedName("diaChiGiaoHang") private String diaChiGiaoHang;
        @SerializedName("soDienThoai") private String soDienThoai;
        @SerializedName("tongTienHang") private double tongTienHang;
        @SerializedName("giamGia") private double giamGia;
        @SerializedName("phiGiaoHang") private double phiGiaoHang;
        @SerializedName("thanhTien") private double thanhTien;
        @SerializedName("items") private List<GioHangItemDto> items;

        public int getIdHoaDonMoi() { return idHoaDonMoi; }
        public String getThoiGianTao() { return thoiGianTao; }
        public String getHoTen() { return hoTen; }
        public String getTrangThai() { return trangThai; }
        public String getPhuongThucThanhToan() { return phuongThucThanhToan; }
        public String getDiaChiGiaoHang() { return diaChiGiaoHang; }
        public String getSoDienThoai() { return soDienThoai; }
        public double getTongTienHang() { return tongTienHang; }
        public double getGiamGia() { return giamGia; }
        public double getPhiGiaoHang() { return phiGiaoHang; }
        public double getThanhTien() { return thanhTien; }
        public List<GioHangItemDto> getItems() { return items; }
    }
}
