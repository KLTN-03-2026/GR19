package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class GioHangKhuyenMaiDto {
    @SerializedName("maKhuyenMai") private String maKhuyenMai;
    @SerializedName("tenChuongTrinh") private String tenChuongTrinh;
    @SerializedName("dieukienApDung") private String dieukienApDung;
    @SerializedName("isEligible") private boolean isEligible;
    @SerializedName("ineligibilityReason") private String ineligibilityReason;
    @SerializedName("calculatedDiscount") private double calculatedDiscount;

    public String getMaKhuyenMai() { return maKhuyenMai; }
    public void setMaKhuyenMai(String maKhuyenMai) { this.maKhuyenMai = maKhuyenMai; }
    public String getTenChuongTrinh() { return tenChuongTrinh; }
    public void setTenChuongTrinh(String tenChuongTrinh) { this.tenChuongTrinh = tenChuongTrinh; }
    public String getDieukienApDung() { return dieukienApDung; }
    public void setDieukienApDung(String dieukienApDung) { this.dieukienApDung = dieukienApDung; }
    public boolean isEligible() { return isEligible; }
    public void setEligible(boolean eligible) { isEligible = eligible; }
    public String getIneligibilityReason() { return ineligibilityReason; }
    public void setIneligibilityReason(String ineligibilityReason) { this.ineligibilityReason = ineligibilityReason; }
    public double getCalculatedDiscount() { return calculatedDiscount; }
    public void setCalculatedDiscount(double calculatedDiscount) { this.calculatedDiscount = calculatedDiscount; }
}
