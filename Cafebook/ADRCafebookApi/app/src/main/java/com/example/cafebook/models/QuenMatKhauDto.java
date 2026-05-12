package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class QuenMatKhauDto {
    public static class GuiMaRequest {
        @SerializedName("email") public String email;
        @SerializedName("maXacNhan") public String maXacNhan;

        public GuiMaRequest(String email, String maXacNhan) {
            this.email = email;
            this.maXacNhan = maXacNhan;
        }
    }

    public static class ResetRequest {
        @SerializedName("email") public String email;
        @SerializedName("newPassword") public String newPassword;

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
