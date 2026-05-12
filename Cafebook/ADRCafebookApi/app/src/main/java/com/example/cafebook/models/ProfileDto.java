package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class ProfileDto {
    // Dữ liệu Tổng quan (Overview)
    public static class Overview {
        @SerializedName("DiemTichLuy") public int diemTichLuy;
        @SerializedName("GiaTriQuyDoiVND") public double giaTriQuyDoiVND;
        @SerializedName("TongChiTieu") public double tongChiTieu;
        @SerializedName("TongHoaDon") public int tongHoaDon;
    }

    // Dữ liệu Thông tin cá nhân
    public static class Info {
        @SerializedName("IdKhachHang") public int idKhachHang;
        @SerializedName("HoTen") public String hoTen;
        @SerializedName("SoDienThoai") public String soDienThoai;
        @SerializedName("Email") public String email;
        @SerializedName("DiaChi") public String diaChi;
        @SerializedName("TenDangNhap") public String tenDangNhap;
        @SerializedName("AnhDaiDien") public String anhDaiDien;
        @SerializedName("AnhDaiDienUrl") public String anhDaiDienUrl;
    }
}
