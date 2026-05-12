package com.example.cafebook.network;

import com.example.cafebook.models.GioHangSyncRequestDto;
import com.example.cafebook.models.ThanhToanLoadDto;
import com.example.cafebook.models.ThanhToanResponseDto;
import com.example.cafebook.models.ThanhToanSubmitDto;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface CheckoutApiService {
    @POST("api/web/khach-hang/thanh-toan/load")
    Call<ThanhToanLoadDto> loadCheckoutData(@Body GioHangSyncRequestDto cartRequest);

    @POST("api/web/khach-hang/thanh-toan/submit")
    Call<ThanhToanResponseDto> submitOrder(@Body ThanhToanSubmitDto dto);
}
