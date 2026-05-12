package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class SanPhamThucDonDto {
    @SerializedName("idSanPham") private int idSanPham;
    @SerializedName("tenSanPham") private String tenSanPham;
    @SerializedName("tenLoaiSP") private String tenLoaiSP;
    @SerializedName("donGia") private double donGia;
    @SerializedName("anhSanPhamUrl") private String anhSanPhamUrl;

    public int getIdSanPham() { return idSanPham; }
    public String getTenSanPham() { return tenSanPham; }
    public String getTenLoaiSP() { return tenLoaiSP; }
    public double getDonGia() { return donGia; }
    public String getAnhSanPhamUrl() { return anhSanPhamUrl; }
}
