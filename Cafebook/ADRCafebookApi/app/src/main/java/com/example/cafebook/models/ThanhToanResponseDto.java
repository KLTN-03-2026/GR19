package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;

public class ThanhToanResponseDto {
    @SerializedName("success") private boolean success;
    @SerializedName("message") private String message;
    @SerializedName("paymentUrl") private String paymentUrl;
    @SerializedName("idHoaDonMoi") private int idHoaDonMoi;

    public boolean isSuccess() { return success; }
    public String getMessage() { return message; }
    public String getPaymentUrl() { return paymentUrl; }
    public int getIdHoaDonMoi() { return idHoaDonMoi; }
}
