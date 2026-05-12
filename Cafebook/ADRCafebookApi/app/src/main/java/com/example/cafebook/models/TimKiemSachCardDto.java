package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class TimKiemSachCardDto {
    @SerializedName("idSach") private int idSach;
    @SerializedName("tieuDe") private String tieuDe;
    @SerializedName("giaBia") private double giaBia;
    @SerializedName("anhBiaUrl") private String anhBiaUrl;

    public int getIdSach() { return idSach; }
    public String getTieuDe() { return tieuDe; }
    public double getGiaBia() { return giaBia; }
    public String getAnhBiaUrl() { return anhBiaUrl; }
}
