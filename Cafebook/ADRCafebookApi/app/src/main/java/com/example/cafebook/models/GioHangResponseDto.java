package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class GioHangResponseDto {
    @SerializedName("items") private List<GioHangItemDto> items;
    @SerializedName("canCheckout") private boolean canCheckout;
    @SerializedName("checkoutWarning") private String checkoutWarning;
    @SerializedName("maKhuyenMaiApDung") private String maKhuyenMaiApDung;
    @SerializedName("tienGiamGia") private double tienGiamGia;
    @SerializedName("phiGiaoHang") private double phiGiaoHang;

    public List<GioHangItemDto> getItems() { return items; }
    public void setItems(List<GioHangItemDto> items) { this.items = items; }
    public boolean isCanCheckout() { return canCheckout; }
    public void setCanCheckout(boolean canCheckout) { this.canCheckout = canCheckout; }
    public String getCheckoutWarning() { return checkoutWarning; }
    public void setCheckoutWarning(String checkoutWarning) { this.checkoutWarning = checkoutWarning; }
    public String getMaKhuyenMaiApDung() { return maKhuyenMaiApDung; }
    public void setMaKhuyenMaiApDung(String maKhuyenMaiApDung) { this.maKhuyenMaiApDung = maKhuyenMaiApDung; }
    public double getTienGiamGia() { return tienGiamGia; }
    public void setTienGiamGia(double tienGiamGia) { this.tienGiamGia = tienGiamGia; }
    public double getPhiGiaoHang() { return phiGiaoHang; }
    public void setPhiGiaoHang(double phiGiaoHang) { this.phiGiaoHang = phiGiaoHang; }

    public double getTongTienHang() {
        double total = 0;
        if (items != null) {
            for (GioHangItemDto item : items) {
                total += item.getDonGia() * item.getSoLuong();
            }
        }
        return total;
    }

    public double getTongThanhToan() {
        return getTongTienHang() - tienGiamGia + phiGiaoHang;
    }
}
