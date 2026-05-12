package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class SachPhanTrangDto {
    @SerializedName("items") private List<SachCardDto> items;
    @SerializedName("totalPages") private int totalPages;
    @SerializedName("currentPage") private int currentPage;

    public List<SachCardDto> getItems() { return items; }
    public int getTotalPages() { return totalPages; }
    public int getCurrentPage() { return currentPage; }
}
