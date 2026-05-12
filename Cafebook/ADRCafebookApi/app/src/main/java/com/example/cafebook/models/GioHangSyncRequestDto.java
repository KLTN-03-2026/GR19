package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class GioHangSyncRequestDto {
    @SerializedName("items") private List<GioHangItemDto> items;
    @SerializedName("maKhuyenMaiApDung") private String maKhuyenMaiApDung;

    public List<GioHangItemDto> getItems() { return items; }
    public void setItems(List<GioHangItemDto> items) { this.items = items; }
    public String getMaKhuyenMaiApDung() { return maKhuyenMaiApDung; }
    public void setMaKhuyenMaiApDung(String maKhuyenMaiApDung) { this.maKhuyenMaiApDung = maKhuyenMaiApDung; }
}
