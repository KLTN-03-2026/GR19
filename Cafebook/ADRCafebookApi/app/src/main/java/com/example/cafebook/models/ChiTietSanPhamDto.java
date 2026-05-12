package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class ChiTietSanPhamDto {
    @SerializedName("idSanPham") private int idSanPham;
    @SerializedName("tenSanPham") private String tenSanPham;
    @SerializedName("tenLoaiSP") private String tenLoaiSP;
    @SerializedName("donGia") private double donGia;
    @SerializedName("hinhAnhUrl") private String hinhAnhUrl;
    @SerializedName("moTa") private String moTa;
    @SerializedName("goiY") private List<SanPhamGoiYDto> goiY;

    public int getIdSanPham() { return idSanPham; }
    public String getTenSanPham() { return tenSanPham; }
    public String getTenLoaiSP() { return tenLoaiSP; }
    public double getDonGia() { return donGia; }
    public String getHinhAnhUrl() { return hinhAnhUrl; }
    public String getMoTa() { return moTa; }
    public List<SanPhamGoiYDto> getGoiY() { return goiY; }
}
