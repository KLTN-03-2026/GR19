package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class ProfileDto {
    // Dữ liệu Tổng quan (Overview)
    public static class Overview {
        @SerializedName("diemTichLuy") public int diemTichLuy;
        @SerializedName("giaTriQuyDoiVND") public double giaTriQuyDoiVND;
        @SerializedName("tongChiTieu") public double tongChiTieu;
        @SerializedName("tongHoaDon") public int tongHoaDon;
    }

    // Dữ liệu Thông tin cá nhân
    public static class Info {
        @SerializedName("idKhachHang") public int idKhachHang;
        @SerializedName("hoTen") public String hoTen;
        @SerializedName("soDienThoai") public String soDienThoai;
        @SerializedName("email") public String email;
        @SerializedName("diaChi") public String diaChi;
        @SerializedName("tenDangNhap") public String tenDangNhap;
        @SerializedName("anhDaiDienUrl") public String anhDaiDienUrl;
    }
}
