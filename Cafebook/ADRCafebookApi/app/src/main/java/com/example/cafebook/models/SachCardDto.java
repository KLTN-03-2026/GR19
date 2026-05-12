package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class SachCardDto {
    @SerializedName("idSach") private int idSach;
    @SerializedName("tieuDe") private String tieuDe;
    @SerializedName("tacGia") private String tacGia;
    @SerializedName("giaBia") private double giaBia;
    @SerializedName("soLuongCoSan") private int soLuongCoSan;
    @SerializedName("anhBiaUrl") private String anhBiaUrl;

    public int getIdSach() { return idSach; }
    public String getTieuDe() { return tieuDe; }
    public String getTacGia() { return tacGia; }
    public double getGiaBia() { return giaBia; }
    public int getSoLuongCoSan() { return soLuongCoSan; }
    public String getAnhBiaUrl() { return anhBiaUrl; }
}
