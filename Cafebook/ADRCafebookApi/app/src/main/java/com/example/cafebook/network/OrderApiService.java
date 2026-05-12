package com.example.cafebook.network;

import com.example.cafebook.models.OrderDto;

import java.util.List;

import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Multipart;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Part;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface OrderApiService {
    @GET("api/web/khachhang/LichSuDonHangWeb/history")
    Call<List<OrderDto.HistoryItem>> getOrders();

    @GET("api/web/khachhang/LichSuDonHangWeb/detail/{id}")
    Call<OrderDto.Detail> getOrderDetail(@Path("id") int id);

    @PUT("api/web/khachhang/LichSuDonHangWeb/cancel/{id}")
    Call<Void> cancelOrder(@Path("id") int id, @retrofit2.http.Body RequestBody body);

    @POST("api/web/khachhang/LichSuDonHangWeb/repay/{id}")
    Call<OrderDto.RepayResponse> repayOrder(@Path("id") int id, @Query("returnUrl") String returnUrl);

    @Multipart
    @POST("api/web/khachhang/LichSuDonHangWeb/danh-gia")
    Call<Void> submitRating(
            @Part("idHoaDon") RequestBody idHoaDon,
            @Part("idSanPham") RequestBody idSanPham,
            @Part("soSao") RequestBody soSao,
            @Part("binhLuan") RequestBody binhLuan,
            @Part MultipartBody.Part hinhAnh
    );
}
