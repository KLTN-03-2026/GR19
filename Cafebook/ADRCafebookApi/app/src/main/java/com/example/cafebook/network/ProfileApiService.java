package com.example.cafebook.network;

import com.example.cafebook.models.ProfileDto;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.http.*;

public interface ProfileApiService {
    @GET("api/web/taikhoantongquan/{id}")
    Call<ProfileDto.Overview> getOverview(@Header("Authorization") String token, @Path("id") int id);

    @GET("api/web/khachhang/ThongTinCaNhan")
    Call<ProfileDto.Info> getPersonalInfo(@Header("Authorization") String token);

    @Multipart
    @POST("api/web/khachhang/ThongTinCaNhan/Update")
    Call<Void> updateProfile(
        @Header("Authorization") String token,
        @Part("HoTen") RequestBody hoTen,
        @Part("SoDienThoai") RequestBody sdt,
        @Part("Email") RequestBody email,
        @Part("DiaChi") RequestBody diaChi,
        @Part("TenDangNhap") RequestBody tenDN,
        @Part MultipartBody.Part avatarFile
    );
}
