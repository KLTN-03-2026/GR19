package com.example.cafebook.models;
import com.google.gson.annotations.SerializedName;

public class DangKyDto {
    public static class Request {
        public String email;
        public String soDienThoai;
        public String password;

        public Request(String email, String soDienThoai, String password) {
            this.email = email;
            this.soDienThoai = soDienThoai;
            this.password = password;
        }
    }

    public static class Response {
        @SerializedName("success") public boolean success;
        @SerializedName("message") public String message;
        @SerializedName("requireOtp") public boolean requireOtp;
        @SerializedName("isOfficialAccount") public boolean isOfficialAccount;
        @SerializedName("tempId") public int tempId;
        @SerializedName("tempEmail") public String tempEmail;
        @SerializedName("tempPhone") public String tempPhone;
    }
}
