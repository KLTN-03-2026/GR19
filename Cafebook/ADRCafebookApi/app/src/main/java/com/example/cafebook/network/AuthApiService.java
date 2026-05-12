package com.example.cafebook.network;
import com.example.cafebook.models.DangKyDto;
import com.example.cafebook.models.DangNhapDto;
import com.example.cafebook.models.XacMinhOtpDto;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface AuthApiService {
    @POST("api/web/khachhang/dangky")
    Call<DangKyDto.Response> register(@Body DangKyDto.Request request);

    @POST("api/web/khachhang/dangky/verify-otp")
    Call<DangKyDto.Response> verifyOtp(@Body XacMinhOtpDto.Request request);

    @POST("api/web/khachhang/dangnhap")
    Call<DangNhapDto.Response> login(@Body DangNhapDto.Request request);
}
