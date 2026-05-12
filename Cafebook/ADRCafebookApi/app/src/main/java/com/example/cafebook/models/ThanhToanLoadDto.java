package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class ThanhToanLoadDto {
    @SerializedName("isStoreOpen") private boolean isStoreOpen;
    @SerializedName("storeMessage") private String storeMessage;
    @SerializedName("khachHang") private KhachHangThanhToanDto khachHang;
    @SerializedName("cartSummary") private GioHangResponseDto cartSummary;
    @SerializedName("tiLeDoiDiemVND") private double tiLeDoiDiemVND;
    @SerializedName("availablePromotions") private List<GioHangKhuyenMaiDto> availablePromotions;

    public boolean isStoreOpen() { return isStoreOpen; }
    public String getStoreMessage() { return storeMessage; }
    public KhachHangThanhToanDto getKhachHang() { return khachHang; }
    public GioHangResponseDto getCartSummary() { return cartSummary; }
    public double getTiLeDoiDiemVND() { return tiLeDoiDiemVND; }
    public List<GioHangKhuyenMaiDto> getAvailablePromotions() { return availablePromotions; }
}
