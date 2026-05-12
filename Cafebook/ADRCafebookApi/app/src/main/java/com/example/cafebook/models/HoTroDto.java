package com.example.cafebook.models;
import com.google.gson.annotations.SerializedName;

public class HoTroDto {
    public static class SendRequest {
        public String noiDung;
        public String guestSessionId;
        public SendRequest(String noiDung, String guestSessionId) {
            this.noiDung = noiDung; this.guestSessionId = guestSessionId;
        }
    }

    public static class SendResponse {
        @SerializedName("tinNhanCuaKhach") public ChatMessageDto tinNhanCuaKhach;
        @SerializedName("tinNhanPhanHoi") public ChatMessageDto tinNhanPhanHoi;
    }
}
