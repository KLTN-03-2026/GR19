package com.example.cafebook.models;
import com.google.gson.annotations.SerializedName;

public class ChatMessageDto {
    @SerializedName("idChat") public long idChat;
    @SerializedName("noiDung") public String noiDung;
    @SerializedName("thoiGian") public String thoiGian; // Sử dụng String để tránh lỗi parse Date
    @SerializedName("loaiTinNhan") public String loaiTinNhan; // "KhachHang" hoặc "AI" hoặc "NhanVien"
    @SerializedName("idThongBaoHoTro") public Integer idThongBaoHoTro;
    
    public boolean isTypingIndicator = false; 

    public ChatMessageDto() {}

    public ChatMessageDto(boolean isTyping) {
        this.isTypingIndicator = isTyping;
    }
}
