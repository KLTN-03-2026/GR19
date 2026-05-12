package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class KhachHangThanhToanDto {
    @SerializedName("idKhachHang") private int idKhachHang;
    @SerializedName("hoTen") private String hoTen;
    @SerializedName("soDienThoai") private String soDienThoai;
    @SerializedName("email") private String email;
    @SerializedName("diaChi") private String diaChi;
    @SerializedName("diemTichLuy") private int diemTichLuy;

    public int getIdKhachHang() { return idKhachHang; }
    public String getHoTen() { return hoTen; }
    public String getSoDienThoai() { return soDienThoai; }
    public String getEmail() { return email; }
    public String getDiaChi() { return diaChi; }
    public int getDiemTichLuy() { return diemTichLuy; }
}
