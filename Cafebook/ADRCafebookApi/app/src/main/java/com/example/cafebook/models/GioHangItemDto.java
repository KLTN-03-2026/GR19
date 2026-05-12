package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class GioHangItemDto {
    @SerializedName("idSanPham") private int idSanPham;
    @SerializedName("tenSanPham") private String tenSanPham;
    @SerializedName("donGia") private double donGia;
    @SerializedName("soLuong") private int soLuong;
    @SerializedName("hinhAnhUrl") private String hinhAnhUrl;
    @SerializedName("isOutOfStock") private boolean isOutOfStock;
    @SerializedName("outOfStockMessage") private String outOfStockMessage;

    public GioHangItemDto() {}

    public GioHangItemDto(int idSanPham, int soLuong) {
        this.idSanPham = idSanPham;
        this.soLuong = soLuong;
    }

    public int getIdSanPham() { return idSanPham; }
    public void setIdSanPham(int idSanPham) { this.idSanPham = idSanPham; }
    public String getTenSanPham() { return tenSanPham; }
    public void setTenSanPham(String tenSanPham) { this.tenSanPham = tenSanPham; }
    public double getDonGia() { return donGia; }
    public void setDonGia(double donGia) { this.donGia = donGia; }
    public int getSoLuong() { return soLuong; }
    public void setSoLuong(int soLuong) { this.soLuong = soLuong; }
    public String getHinhAnhUrl() { return hinhAnhUrl; }
    public void setHinhAnhUrl(String hinhAnhUrl) { this.hinhAnhUrl = hinhAnhUrl; }
    public boolean isOutOfStock() { return isOutOfStock; }
    public void setOutOfStock(boolean outOfStock) { isOutOfStock = outOfStock; }
    public String getOutOfStockMessage() { return outOfStockMessage; }
    public void setOutOfStockMessage(String outOfStockMessage) { this.outOfStockMessage = outOfStockMessage; }

    public double getThanhTien() { return donGia * soLuong; }
}
