package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class DanhGiaChiTietDto {
    @SerializedName("tenKhachHang") private String tenKhachHang;
    @SerializedName("avatarKhachHang") private String avatarKhachHang;
    @SerializedName("hinhAnhDanhGiaUrl") private String hinhAnhDanhGiaUrl;
    @SerializedName("soSao") private float soSao;
    @SerializedName("binhLuan") private String binhLuan;
    @SerializedName("ngayTao") private String ngayTao;
    @SerializedName("phanHoi") private PhanHoiChiTietDto phanHoi;

    public String getTenKhachHang() { return tenKhachHang; }
    public String getAvatarKhachHang() { return avatarKhachHang; }
    public String getHinhAnhDanhGiaUrl() { return hinhAnhDanhGiaUrl; }
    public float getSoSao() { return soSao; }
    public String getBinhLuan() { return binhLuan; }
    public String getNgayTao() { return ngayTao; }
    public PhanHoiChiTietDto getPhanHoi() { return phanHoi; }
}
