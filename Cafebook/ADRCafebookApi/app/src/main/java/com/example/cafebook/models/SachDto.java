package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class SachDto {
    @SerializedName("idSach") private int idSach;
    @SerializedName("tieuDe") private String tieuDe;
    @SerializedName("tacGia") private String tacGia;
    @SerializedName("anhBiaUrl") private String anhBiaUrl;

    public int getIdSach() { return idSach; }
    public String getTieuDe() { return tieuDe; }
    public String getTacGia() { return tacGia; }
    public String getAnhBiaUrl() { return anhBiaUrl; }
}
