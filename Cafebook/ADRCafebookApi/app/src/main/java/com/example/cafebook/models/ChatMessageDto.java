package com.example.cafebook.models;
import com.google.gson.annotations.SerializedName;
import java.util.Date;

public class ChatMessageDto {
    @SerializedName("idChat") public int idChat;
    @SerializedName("noiDung") public String noiDung;
    @SerializedName("thoiGian") public Date thoiGian;
    @SerializedName("loaiTinNhan") public String loaiTinNhan; // "KhachHang" hoặc "AI"
    
    // Thuộc tính dùng cho UI App (Không có trong API)
    public boolean isTypingIndicator = false; 

    public ChatMessageDto(boolean isTyping) {
        this.isTypingIndicator = isTyping;
    }
}
