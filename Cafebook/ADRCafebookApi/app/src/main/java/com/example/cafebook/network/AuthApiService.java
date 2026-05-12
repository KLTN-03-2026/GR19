package com.example.cafebook.network;
import com.example.cafebook.models.DangKyDto;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface AuthApiService {
    @POST("api/web/khachhang/dangky")
    Call<DangKyDto.Response> register(@Body DangKyDto.Request request);
}
