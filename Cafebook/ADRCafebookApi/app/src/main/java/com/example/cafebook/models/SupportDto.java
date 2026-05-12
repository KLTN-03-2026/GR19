package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class SupportDto {

    public static class ChatSession {
        @SerializedName("sessionId") public String sessionId;
        @SerializedName("title") public String title;
        @SerializedName("lastActive") public String lastActive;
        @SerializedName("idThongBao") public Integer idThongBao;
    }

    public static class SendRequest {
        @SerializedName("NoiDung") public String noiDung;
        @SerializedName("SessionId") public String sessionId;
        
        public SendRequest(String noiDung, String sessionId) {
            this.noiDung = noiDung;
            this.sessionId = sessionId;
        }
    }

    public static class SendResponse {
        @SerializedName("tinNhanCuaKhach") public ChatMessageDto tinNhanCuaKhach;
        @SerializedName("tinNhanPhanHoi") public ChatMessageDto tinNhanPhanHoi;
        @SerializedName("idThongBaoHoTro") public Integer idThongBaoHoTro;
        @SerializedName("daChuyenNhanVien") public boolean daChuyenNhanVien;
    }
}
