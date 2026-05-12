package com.example.cafebook.models;

import androidx.annotation.NonNull;
import com.google.gson.annotations.SerializedName;

public class ThuVienSachFilterItemDto {
    @SerializedName("id") private int id;
    @SerializedName("ten") private String ten;

    public ThuVienSachFilterItemDto() {
        this.id = 0;
        this.ten = "Tất cả thể loại";
    }

    public int getId() { return id; }
    public String getTen() { return ten; }
    
    @NonNull
    @Override public String toString() { return ten; } 
}
