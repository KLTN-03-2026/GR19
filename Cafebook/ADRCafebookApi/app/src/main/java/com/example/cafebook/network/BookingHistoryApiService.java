package com.example.cafebook.network;

import com.example.cafebook.models.LichSuDatBanDto;
import retrofit2.Call;
import retrofit2.http.*;

public interface BookingHistoryApiService {
    @GET("api/web/khach-hang/lich-su-dat-ban")
    Call<LichSuDatBanDto.Response> getHistory(
            @Query("page") int page,
            @Query("search") String search,
            @Query("status") String status,
            @Query("fromDate") String fromDate,
            @Query("toDate") String toDate
    );

    @PUT("api/web/khach-hang/lich-su-dat-ban/huy/{id}")
    Call<Void> cancelBooking(@Path("id") int id, @Body String lyDoHuy);
}
