package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class SanPhamGoiYDto implements com.example.cafebook.adapters.RecommendationAdapter.RecommendationItem {
    @SerializedName("idSanPham") private int idSanPham;
    @SerializedName("tenSanPham") private String tenSanPham;
    @SerializedName("donGia") private double donGia;
    @SerializedName("hinhAnhUrl") private String hinhAnhUrl;

    @Override public int getId() { return idSanPham; }
    @Override public String getTitle() { return tenSanPham; }
    @Override public String getImageUrl() { return hinhAnhUrl; }
    @Override public boolean isBook() { return false; }

    public double getDonGia() { return donGia; }
}
