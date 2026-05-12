package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class ChiTietSachDto {
    @SerializedName("idSach") private int idSach;
    @SerializedName("tieuDe") private String tieuDe;
    @SerializedName("giaBia") private double giaBia;
    @SerializedName("anhBiaUrl") private String anhBiaUrl;
    @SerializedName("moTa") private String moTa;
    @SerializedName("viTri") private String viTri;
    @SerializedName("soLuongCoSan") private int soLuongCoSan;

    @SerializedName("tacGias") private List<NameItemDto> tacGias;
    @SerializedName("theLoais") private List<NameItemDto> theLoais;
    @SerializedName("nhaXuatBans") private List<NameItemDto> nhaXuatBans;

    public int getIdSach() { return idSach; }
    public String getTieuDe() { return tieuDe; }
    public double getGiaBia() { return giaBia; }
    public String getAnhBiaUrl() { return anhBiaUrl; }
    public String getMoTa() { return moTa; }
    public String getViTri() { return viTri; }
    public int getSoLuongCoSan() { return soLuongCoSan; }
    public List<NameItemDto> getTacGias() { return tacGias; }
    public List<NameItemDto> getTheLoais() { return theLoais; }
    public List<NameItemDto> getNhaXuatBans() { return nhaXuatBans; }
}
