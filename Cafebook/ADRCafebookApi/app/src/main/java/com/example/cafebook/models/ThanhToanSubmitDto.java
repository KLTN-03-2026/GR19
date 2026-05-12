package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class ThanhToanSubmitDto {
    @SerializedName("cartData") private GioHangSyncRequestDto cartData;
    @SerializedName("hoTen") private String hoTen;
    @SerializedName("email") private String email;
    @SerializedName("diaChiGiaoHang") private String diaChiGiaoHang;
    @SerializedName("soDienThoai") private String soDienThoai;
    @SerializedName("ghiChu") private String ghiChu;
    @SerializedName("phuongThucThanhToan") private String phuongThucThanhToan;
    @SerializedName("diemSuDung") private int diemSuDung;
    @SerializedName("returnUrl") private String returnUrl;

    public void setCartData(GioHangSyncRequestDto cartData) { this.cartData = cartData; }
    public void setHoTen(String hoTen) { this.hoTen = hoTen; }
    public void setEmail(String email) { this.email = email; }
    public void setDiaChiGiaoHang(String diaChiGiaoHang) { this.diaChiGiaoHang = diaChiGiaoHang; }
    public void setSoDienThoai(String soDienThoai) { this.soDienThoai = soDienThoai; }
    public void setGhiChu(String ghiChu) { this.ghiChu = ghiChu; }
    public void setPhuongThucThanhToan(String phuongThucThanhToan) { this.phuongThucThanhToan = phuongThucThanhToan; }
    public void setDiemSuDung(int diemSuDung) { this.diemSuDung = diemSuDung; }
    public void setReturnUrl(String returnUrl) { this.returnUrl = returnUrl; }
}
