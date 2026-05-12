package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.io.Serializable;
import java.util.List;

public class LichSuThueSachDto {

    public static class Response {
        @SerializedName("items") public List<Item> items;
        @SerializedName("totalPages") public int totalPages;
        @SerializedName("currentPage") public int currentPage;
        @SerializedName("phatGiamDoMoi1Percent") public double phatGiamDoMoi1Percent;
    }

    public static class Item implements Serializable {
        @SerializedName("idPhieuThueSach") public int idPhieuThueSach;
        @SerializedName("ngayThue") public String ngayThue;
        @SerializedName("ngayHenTra") public String ngayHenTra;
        @SerializedName("trangThai") public String trangThai;
        @SerializedName("soLuongSach") public int soLuongSach;
        @SerializedName("tongTienCoc") public double tongTienCoc;
        @SerializedName("ngayTra") public String ngayTra;
        @SerializedName("tongPhiThue") public Double tongPhiThue;
        @SerializedName("tongTienPhat") public Double tongTienPhat;
        @SerializedName("tongTienCocHoan") public Double tongTienCocHoan;
        @SerializedName("laSoTienTamTinh") public boolean laSoTienTamTinh;
        @SerializedName("chiTietSachs") public List<Detail> chiTietSachs;
    }

    public static class Detail implements Serializable {
        @SerializedName("tenSach") public String tenSach;
        @SerializedName("doMoiKhiThue") public int doMoiKhiThue;
        @SerializedName("ghiChuKhiThue") public String ghiChuKhiThue;
        @SerializedName("doMoiKhiTra") public Integer doMoiKhiTra;
        @SerializedName("ghiChuKhiTra") public String ghiChuKhiTra;
        @SerializedName("tienPhatTre") public double tienPhatTre;
        @SerializedName("tienPhatHuHong") public double tienPhatHuHong;
    }
}
