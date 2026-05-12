package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class DangNhapDto {

    public static class Request {
        @SerializedName("TenDangNhap") public String tenDangNhap;
        @SerializedName("MatKhau") public String matKhau;

        public Request(String tenDangNhap, String matKhau) {
            this.tenDangNhap = tenDangNhap;
            this.matKhau = matKhau;
        }
    }

    public static class Response {
        @SerializedName("Success") public boolean success;
        @SerializedName("Message") public String message;
        @SerializedName("KhachHangData") public KhachHangData khachHangData;
        @SerializedName("Token") public String token;
    }

    public static class KhachHangData {
        @SerializedName("IdKhachHang") public int idKhachHang;
        @SerializedName("HoTen") public String hoTen;
        @SerializedName("Email") public String email;
        @SerializedName("SoDienThoai") public String soDienThoai;
        @SerializedName("TenDangNhap") public String tenDangNhap;
        @SerializedName("AnhDaiDienUrl") public String anhDaiDienUrl;
    }
}
