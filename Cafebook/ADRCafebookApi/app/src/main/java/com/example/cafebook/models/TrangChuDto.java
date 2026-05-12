package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class TrangChuDto {
    @SerializedName("info") private ThongTinChungDto info;
    @SerializedName("promotions") private List<KhuyenMaiDto> promotions;
    @SerializedName("monNoiBat") private List<SanPhamDto> monNoiBat;
    @SerializedName("sachNoiBat") private List<SachDto> sachNoiBat;

    public ThongTinChungDto getInfo() { return info; }
    public List<KhuyenMaiDto> getPromotions() { return promotions; }
    public List<SanPhamDto> getMonNoiBat() { return monNoiBat; }
    public List<SachDto> getSachNoiBat() { return sachNoiBat; }
}
