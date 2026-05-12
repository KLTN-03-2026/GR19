package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class XacMinhOtpDto {
    public static class Request {
        @SerializedName("TempId") public int tempId;
        @SerializedName("Email") public String email;
        @SerializedName("SoDienThoai") public String soDienThoai;
        @SerializedName("Password") public String password;
        @SerializedName("OtpCode") public String otpCode;

        public Request(int tempId, String email, String soDienThoai, String password, String otpCode) {
            this.tempId = tempId;
            this.email = email;
            this.soDienThoai = soDienThoai;
            this.password = password;
            this.otpCode = otpCode;
        }
    }
}
