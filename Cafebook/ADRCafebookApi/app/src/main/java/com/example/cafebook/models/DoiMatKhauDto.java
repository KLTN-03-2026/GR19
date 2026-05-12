package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class DoiMatKhauDto {
    @SerializedName("matKhauCu") private String matKhauCu;
    @SerializedName("matKhauMoi") private String matKhauMoi;
    @SerializedName("xacNhanMatKhauMoi") private String xacNhanMatKhauMoi;

    public void setMatKhauCu(String matKhauCu) { this.matKhauCu = matKhauCu; }
    public void setMatKhauMoi(String matKhauMoi) { this.matKhauMoi = matKhauMoi; }
    public void setXacNhanMatKhauMoi(String xacNhanMatKhauMoi) { this.xacNhanMatKhauMoi = xacNhanMatKhauMoi; }
}
