package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class XacMinhOtpDto {
    public static class Request {
        @SerializedName("email") public String email;
        @SerializedName("otpCode") public String otpCode;

        public Request(String email, String otpCode) {
            this.email = email;
            this.otpCode = otpCode;
        }
    }
}
