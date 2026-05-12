package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class LichSuDatBanDto {

    public static class Response {
        @SerializedName("items") public List<Item> items;
        @SerializedName("totalCount") public int totalCount;
        @SerializedName("totalPages") public int totalPages;
        @SerializedName("currentPage") public int currentPage;
    }

    public static class Item {
        @SerializedName("idPhieuDatBan") public int idPhieuDatBan;
        @SerializedName("tenBan") public String tenBan;
        @SerializedName("thoiGianDat") public String thoiGianDat; // ISO 8601
        @SerializedName("soLuongKhach") public int soLuongKhach;
        @SerializedName("trangThai") public String trangThai;
        @SerializedName("ghiChu") public String ghiChu;
    }
}
