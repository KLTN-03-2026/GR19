package com.example.cafebook.network;

import com.example.cafebook.models.LichSuThueSachDto;
import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Query;

public interface RentalHistoryApiService {
    @GET("api/web/khach-hang/lich-su-thue-sach")
    Call<LichSuThueSachDto.Response> getRentalHistory(
            @Query("page") int page,
            @Query("search") String search,
            @Query("status") String status
    );
}
