package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class ChinhSachDto {
    @SerializedName("tenQuan") public String tenQuan;
    @SerializedName("gioiThieu") public String gioiThieu;
    @SerializedName("diaChi") public String diaChi;
    @SerializedName("soDienThoai") public String soDienThoai;
    @SerializedName("gioMoCua") public String gioMoCua;
    @SerializedName("gioDongCua") public String gioDongCua;
    @SerializedName("thuMoCua") public String thuMoCua;
    @SerializedName("email") public String email;
    
    @SerializedName("phiThue") public double phiThue;
    @SerializedName("phiTraTreMoiNgay") public double phiTraTreMoiNgay;
    @SerializedName("soNgayMuonToiDa") public String soNgayMuonToiDa;
    @SerializedName("diemPhieuThue") public String diemPhieuThue;
    @SerializedName("diemNhanVND") public double diemNhanVND;
    @SerializedName("diemDoiVND") public double diemDoiVND;
}
