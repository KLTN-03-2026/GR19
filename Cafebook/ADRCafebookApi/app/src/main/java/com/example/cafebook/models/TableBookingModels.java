package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class TableBookingModels {
    public static class OpeningHours {
        @SerializedName("open") public String open;
        @SerializedName("close") public String close;
    }

    public static class TimBanRequest {
        public String ngayDat;
        public String gioDat;
        public int soNguoi;
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
    }

    public static class KhuVucBan {
        @SerializedName("idKhuVuc") public int idKhuVuc;
        @SerializedName("tenKhuVuc") public String tenKhuVuc;
        @SerializedName("banList") public List<BanTrong> banList;
    }

    public static class DatBanWebRequest {
        public String hoTen;
        public String soDienThoai;
        public String email;
        public String ghiChu;
        public int idBan;
        public int soLuongKhach;
        public String ngayDat;
        public String gioDat;
    }

    public static class CustomerInfoResponse {
        @SerializedName("hoTen") public String hoTen;
        @SerializedName("email") public String email;
    }
}
