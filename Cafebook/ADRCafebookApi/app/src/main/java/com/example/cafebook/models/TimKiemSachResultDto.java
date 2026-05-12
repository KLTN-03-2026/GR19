package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class TimKiemSachResultDto {
    @SerializedName("tieuDeTrang") private String tieuDeTrang;
    @SerializedName("moTaTrang") private String moTaTrang;
    @SerializedName("sachList") private List<TimKiemSachCardDto> sachList;
    @SerializedName("totalPages") private int totalPages;
    @SerializedName("currentPage") private int currentPage;

    public String getTieuDeTrang() { return tieuDeTrang; }
    public String getMoTaTrang() { return moTaTrang; }
    public List<TimKiemSachCardDto> getSachList() { return sachList; }
    public int getTotalPages() { return totalPages; }
    public int getCurrentPage() { return currentPage; }
}
