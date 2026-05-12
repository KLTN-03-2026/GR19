package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class TableBookingModels {
    public static class OpeningHours {
        @SerializedName("open") public String open;
        @SerializedName("close") public String close;
    }

    public static class TimBanRequest {
        @SerializedName("NgayDat") public String ngayDat;
        @SerializedName("GioDat") public String gioDat;
        @SerializedName("SoNguoi") public int soNguoi;
        
        public TimBanRequest(String ngay, String gio, int ng) {
            this.ngayDat = ngay;
            this.gioDat = gio;
            this.soNguoi = ng;
        }
    }

    public static class BanTrong {
        @SerializedName("idBan") public int idBan;
        @SerializedName("soBan") public String soBan;
        @SerializedName("soGhe") public int soGhe;
        @SerializedName("khuVuc") public String khuVuc;
        @SerializedName("moTa") public String moTa;
    }

    public static class KhuVucBan {
        @SerializedName("idKhuVuc") public int idKhuVuc;
        @SerializedName("tenKhuVuc") public String tenKhuVuc;
        @SerializedName("banList") public List<BanTrong> banList;
    }

    public static class DatBanWebRequest {
        @SerializedName("HoTen") public String hoTen;
        @SerializedName("SoDienThoai") public String soDienThoai;
        @SerializedName("Email") public String email;
        @SerializedName("GhiChu") public String ghiChu;
        @SerializedName("IdBan") public int idBan;
        @SerializedName("SoLuongKhach") public int soLuongKhach;
        @SerializedName("NgayDat") public String ngayDat;
        @SerializedName("GioDat") public String gioDat;
    }

    public static class CustomerInfoResponse {
        @SerializedName("hoTen") public String hoTen;
        @SerializedName("email") public String email;
    }
}
