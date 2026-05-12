package com.example.cafebook.network;

import com.example.cafebook.models.DoiMatKhauDto;
import com.example.cafebook.models.ProfileDto;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.http.*;

public interface ProfileApiService {
    @GET("api/web/taikhoantongquan/{id}")
    Call<ProfileDto.Overview> getOverview(@Path("id") int id);

    @GET("api/web/khachhang/ThongTinCaNhan/{id}")
    Call<ProfileDto.Info> getPersonalInfo(@Path("id") int id);

    @Multipart
    @PUT("api/web/khachhang/ThongTinCaNhan/update/{id}")
    Call<Void> updateProfile(
        @Path("id") int id,
        @Part("HoTen") RequestBody hoTen,
        @Part("SoDienThoai") RequestBody sdt,
        @Part("Email") RequestBody email,
        @Part("DiaChi") RequestBody diaChi,
        @Part("TenDangNhap") RequestBody tenDN,
        @Part MultipartBody.Part avatarFile
    );

    @PUT("api/web/khachhang/doi-mat-khau/{id}")
    Call<Void> changePassword(@Path("id") int id, @Body DoiMatKhauDto model);
}
