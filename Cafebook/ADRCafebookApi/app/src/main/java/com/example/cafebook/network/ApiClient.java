package com.example.cafebook.network;

import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {
    // THAY ĐỔI TẠI ĐÂY: Chỉ cần đổi 1 lần ở dòng này
    private static final String BASE_URL = "https://your-cafebook-api.runasp.net/"; 
    
    private static Retrofit retrofit = null;

    public static Retrofit getClient() {
        if (retrofit == null) {
            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }
}
