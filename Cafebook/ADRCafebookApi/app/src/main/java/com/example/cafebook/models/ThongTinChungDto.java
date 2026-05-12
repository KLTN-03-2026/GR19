package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class ThongTinChungDto {
    @SerializedName("tenQuan") private String tenQuan;
    @SerializedName("gioiThieu") private String gioiThieu;
    @SerializedName("bannerImageUrl") private String bannerImageUrl;
    @SerializedName("diaChi") private String diaChi;
    @SerializedName("soDienThoai") private String soDienThoai;
    @SerializedName("emailLienHe") private String emailLienHe;
    @SerializedName("gioMoCua") private String gioMoCua;
    @SerializedName("gioDongCua") private String gioDongCua;
    @SerializedName("thuMoCua") private String thuMoCua;
    @SerializedName("soBanTrong") private int soBanTrong;
    @SerializedName("soSachSanSang") private int soSachSanSang;

    public String getTenQuan() { return tenQuan; }
    public String getGioiThieu() { return gioiThieu; }
    public String getBannerImageUrl() { return bannerImageUrl; }
    public String getDiaChi() { return diaChi; }
    public String getSoDienThoai() { return soDienThoai; }
    public String getEmailLienHe() { return emailLienHe; }
    public String getGioMoCua() { return gioMoCua; }
    public String getGioDongCua() { return gioDongCua; }
    public String getThuMoCua() { return thuMoCua; }
    public int getSoBanTrong() { return soBanTrong; }
    public int getSoSachSanSang() { return soSachSanSang; }
}
