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
    @SerializedName("tongSoLuong") private int tongSoLuong;

    @SerializedName("tacGias") private List<ChiTietSachTacGiaDto> tacGias;
    @SerializedName("theLoais") private List<ChiTietSachTheLoaiDto> theLoais;
    @SerializedName("nhaXuatBans") private List<ChiTietSachNxbDto> nhaXuatBans;
    @SerializedName("goiY") private List<ChiTietSachGoiYDto> goiY;

    public int getIdSach() { return idSach; }
    public String getTieuDe() { return tieuDe; }
    public double getGiaBia() { return giaBia; }
    public String getAnhBiaUrl() { return anhBiaUrl; }
    public String getMoTa() { return moTa; }
    public String getViTri() { return viTri; }
    public int getSoLuongCoSan() { return soLuongCoSan; }
    public int getTongSoLuong() { return tongSoLuong; }
    public List<ChiTietSachTacGiaDto> getTacGias() { return tacGias; }
    public List<ChiTietSachTheLoaiDto> getTheLoais() { return theLoais; }
    public List<ChiTietSachNxbDto> getNhaXuatBans() { return nhaXuatBans; }
    public List<ChiTietSachGoiYDto> getGoiY() { return goiY; }

    public static class ChiTietSachTacGiaDto {
        @SerializedName("idTacGia") private int idTacGia;
        @SerializedName("tenTacGia") private String tenTacGia;
        public String getName() { return tenTacGia; }
        public int getId() { return idTacGia; }
    }

    public static class ChiTietSachTheLoaiDto {
        @SerializedName("idTheLoai") private int idTheLoai;
        @SerializedName("tenTheLoai") private String tenTheLoai;
        public String getName() { return tenTheLoai; }
        public int getId() { return idTheLoai; }
    }

    public static class ChiTietSachNxbDto {
        @SerializedName("idNhaXuatBan") private int idNhaXuatBan;
        @SerializedName("tenNhaXuatBan") private String tenNhaXuatBan;
        public String getName() { return tenNhaXuatBan; }
        public int getId() { return idNhaXuatBan; }
    }

    public static class ChiTietSachGoiYDto implements com.example.cafebook.adapters.RecommendationAdapter.RecommendationItem {
        @SerializedName("idSach") private int idSach;
        @SerializedName("tieuDe") private String tieuDe;
        @SerializedName("anhBiaUrl") private String anhBiaUrl;

        @Override public int getId() { return idSach; }
        @Override public String getTitle() { return tieuDe; }
        @Override public String getImageUrl() { return anhBiaUrl; }
        @Override public boolean isBook() { return true; }
    }
}
