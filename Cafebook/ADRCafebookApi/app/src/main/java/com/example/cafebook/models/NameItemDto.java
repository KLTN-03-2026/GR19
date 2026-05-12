package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class NameItemDto {
    @SerializedName("tenTacGia") private String tenTacGia;
    @SerializedName("tenTheLoai") private String tenTheLoai;
    @SerializedName("tenNhaXuatBan") private String tenNhaXuatBan;

    public String getName() {
        if (tenTacGia != null) return tenTacGia;
        if (tenTheLoai != null) return tenTheLoai;
        return tenNhaXuatBan;
    }
}
