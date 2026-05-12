package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class PhanHoiChiTietDto {
    @SerializedName("tenNhanVien") private String tenNhanVien;
    @SerializedName("noiDung") private String noiDung;
    @SerializedName("ngayTao") private String ngayTao;

    public String getTenNhanVien() { return tenNhanVien; }
    public String getNoiDung() { return noiDung; }
    public String getNgayTao() { return ngayTao; }
}
