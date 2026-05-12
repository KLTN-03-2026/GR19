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
        @SerializedName("success") public boolean success;
        @SerializedName("message") public String message;
        @SerializedName("khachHangData") public KhachHangData khachHangData;
        @SerializedName("token") public String token;
    }

    public static class KhachHangData {
        @SerializedName("idKhachHang") public int idKhachHang;
        @SerializedName("hoTen") public String hoTen;
        @SerializedName("email") public String email;
        @SerializedName("soDienThoai") public String soDienThoai;
        @SerializedName("tenDangNhap") public String tenDangNhap;
        @SerializedName("anhDaiDienUrl") public String anhDaiDienUrl;
    }
}
