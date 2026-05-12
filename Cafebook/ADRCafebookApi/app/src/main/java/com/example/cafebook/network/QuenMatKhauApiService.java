package com.example.cafebook.network;

import com.example.cafebook.models.QuenMatKhauDto;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface QuenMatKhauApiService {
    @POST("api/web/quenmatkhau/gui-ma")
    Call<QuenMatKhauDto.Response> guiMaXacNhan(@Body QuenMatKhauDto.GuiMaRequest request);

    @POST("api/web/quenmatkhau/reset")
    Call<QuenMatKhauDto.Response> resetPassword(@Body QuenMatKhauDto.ResetRequest request);
}
