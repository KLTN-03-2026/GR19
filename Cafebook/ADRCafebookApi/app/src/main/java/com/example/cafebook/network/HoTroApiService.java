package com.example.cafebook.network;
import com.example.cafebook.models.ChatMessageDto;
import com.example.cafebook.models.HoTroDto;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Query;

public interface HoTroApiService {
    @GET("api/web/guest/hotro/history")
    Call<List<ChatMessageDto>> getHistory(@Query("guestSessionId") String guestSessionId);

    @POST("api/web/guest/hotro/send")
    Call<HoTroDto.SendResponse> sendMessage(@Body HoTroDto.SendRequest request);
}
