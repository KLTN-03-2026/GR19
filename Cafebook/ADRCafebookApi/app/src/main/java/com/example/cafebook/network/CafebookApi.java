package com.example.cafebook.network;

import com.example.cafebook.models.TrangChuDto;
import retrofit2.Call;
import retrofit2.http.GET;

public interface CafebookApi {
    @GET("api/web/trangchu/data")
    Call<TrangChuDto> getTrangChuData();
}
