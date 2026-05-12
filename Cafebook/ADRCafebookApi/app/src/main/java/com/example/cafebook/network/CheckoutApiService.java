package com.example.cafebook.network;

import com.example.cafebook.models.GioHangSyncRequestDto;
import com.example.cafebook.models.ThanhToanLoadDto;
import com.example.cafebook.models.ThanhToanResponseDto;
import com.example.cafebook.models.ThanhToanSubmitDto;

import com.example.cafebook.models.PaymentResultDto;

import java.util.Map;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.QueryMap;

public interface CheckoutApiService {
    @POST("api/web/khach-hang/thanh-toan/load")
    Call<ThanhToanLoadDto> loadCheckoutData(@Body GioHangSyncRequestDto cartRequest);

    @POST("api/web/khach-hang/thanh-toan/submit")
    Call<ThanhToanResponseDto> submitOrder(@Body ThanhToanSubmitDto dto);

    @GET("api/web/khach-hang/thanh-toan/vnpay-return")
    Call<PaymentResultDto.VNPayVerifyResult> verifyVNPay(@QueryMap Map<String, String> vnpayParams);

    @GET("api/web/khach-hang/thanh-toan/order-summary/{id}")
    Call<PaymentResultDto.OrderSummary> getOrderSummary(@Path("id") int idHoaDon);
}
