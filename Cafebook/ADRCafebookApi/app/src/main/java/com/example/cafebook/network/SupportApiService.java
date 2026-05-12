package com.example.cafebook.network;

import com.example.cafebook.models.ChatMessageDto;
import com.example.cafebook.models.SupportDto;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Query;

public interface SupportApiService {
    // THÊM Query "guestSessionId" để Backend tự động chuyển chủ sở hữu
    @GET("api/web/khachhang/hotro/sessions")
    Call<List<SupportDto.ChatSession>> getSessions(@Query("guestSessionId") String guestSessionId);

    @GET("api/web/khachhang/hotro/history")
    Call<List<ChatMessageDto>> getHistory(@Query("sessionId") String sessionId);

    @POST("api/web/khachhang/hotro/send")
    Call<SupportDto.SendResponse> sendMessage(@Body SupportDto.SendRequest request);
}
