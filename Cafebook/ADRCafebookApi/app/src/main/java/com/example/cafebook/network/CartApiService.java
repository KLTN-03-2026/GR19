package com.example.cafebook.network;

import com.example.cafebook.models.GioHangItemDto;
import com.example.cafebook.models.GioHangKhuyenMaiDto;
import com.example.cafebook.models.GioHangResponseDto;
import com.example.cafebook.models.GioHangSyncRequestDto;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface CartApiService {
    @POST("api/web/khach-hang/gio-hang/sync")
    Call<GioHangResponseDto> syncCart(@Body GioHangSyncRequestDto request);

    @POST("api/web/khach-hang/gio-hang/khuyen-mai")
    Call<List<GioHangKhuyenMaiDto>> getAvailablePromotions(@Body List<GioHangItemDto> currentItems);
}
