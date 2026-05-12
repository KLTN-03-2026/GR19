package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class LienHeDto {
    @SerializedName("tenQuan") public String tenQuan;
    @SerializedName("gioiThieu") public String gioiThieu;
    @SerializedName("diaChi") public String diaChi;
    @SerializedName("soDienThoai") public String soDienThoai;
    @SerializedName("emailLienHe") public String emailLienHe;
    @SerializedName("gioHoatDong") public String gioHoatDong;
    @SerializedName("linkFacebook") public String linkFacebook;
    @SerializedName("linkZalo") public String linkZalo;
}
