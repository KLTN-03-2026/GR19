package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class QuenMatKhauDto {
    public static class GuiMaRequest {
        @SerializedName("Email") public String email;
        @SerializedName("MaXacNhan") public String maXacNhan;

        public GuiMaRequest(String email, String maXacNhan) {
            this.email = email;
            this.maXacNhan = maXacNhan;
        }
    }

    public static class ResetRequest {
        @SerializedName("Email") public String email;
        @SerializedName("NewPassword") public String newPassword;

        public ResetRequest(String email, String newPassword) {
            this.email = email;
            this.newPassword = newPassword;
        }
    }

    public static class Response {
        @SerializedName("success") public boolean success;
        @SerializedName("message") public String message;
    }
}
