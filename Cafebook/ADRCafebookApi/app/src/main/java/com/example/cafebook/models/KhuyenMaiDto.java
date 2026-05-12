package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class KhuyenMaiDto {
    @SerializedName("tenKhuyenMai") private String tenKhuyenMai;
    @SerializedName("moTa") private String moTa;
    @SerializedName("dieuKienApDung") private String dieuKienApDung;

    public String getTenKhuyenMai() { return tenKhuyenMai; }
    public String getMoTa() { return moTa; }
    public String getDieuKienApDung() { return dieuKienApDung; }
}
