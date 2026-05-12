package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class DanhGiaChiTietDto {
    @SerializedName("tenKhachHang") private String tenKhachHang;
    @SerializedName("soSao") private float soSao;
    @SerializedName("noiDung") private String noiDung;
    @SerializedName("ngayDanhGia") private String ngayDanhGia;
    @SerializedName("avatarUrl") private String avatarUrl;

    public String getTenKhachHang() { return tenKhachHang; }
    public float getSoSao() { return soSao; }
    public String getNoiDung() { return noiDung; }
    public String getNgayDanhGia() { return ngayDanhGia; }
    public String getAvatarUrl() { return avatarUrl; }
}
