package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class SanPhamGoiYDto {
    @SerializedName("idSanPham") private int idSanPham;
    @SerializedName("tenSanPham") private String tenSanPham;
    @SerializedName("donGia") private double donGia;
    @SerializedName("hinhAnhUrl") private String hinhAnhUrl;

    public int getIdSanPham() { return idSanPham; }
    public String getTenSanPham() { return tenSanPham; }
    public double getDonGia() { return donGia; }
    public String getHinhAnhUrl() { return hinhAnhUrl; }
}
