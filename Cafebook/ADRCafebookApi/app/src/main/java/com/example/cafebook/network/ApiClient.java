package com.example.cafebook.network;

import android.content.Context;
import android.content.SharedPreferences;
import java.io.IOException;
import java.util.concurrent.TimeUnit;

import kotlin.text.UStringsKt;
import okhttp3.Interceptor;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {
    //public static final String BASE_URL = "http://10.0.2.2:5202/";
    //public static final String BASE_URL = "https://cafebookapi.shushushu.id.vn/";
    //public static final String BASE_URL = "https://apicafebook.shushushu.id.vn/";
    public static final String BASE_URL = "https://api.shushushu.id.vn/";
    //public static final String BASE_URL = "https://api.cafebook.id.vn/";

    private static Retrofit retrofit = null;

    public static Retrofit getClient(Context context) {
        if (retrofit == null) {
            // Interceptor tự động đính kèm Token
            Interceptor authInterceptor = chain -> {
                Request originalRequest = chain.request();
                
                SharedPreferences prefs = context.getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
                String token = prefs.getString("JWT_TOKEN", "");

                if (!token.isEmpty()) {
                    Request newRequest = originalRequest.newBuilder()
                            .header("Authorization", "Bearer " + token)
                            .build();
                    return chain.proceed(newRequest);
                }
                return chain.proceed(originalRequest);
            };

            okhttp3.Cache cache = new okhttp3.Cache(context.getCacheDir(), 10 * 1024 * 1024); // 10MB Cache

            OkHttpClient client = new OkHttpClient.Builder()
                    .cache(cache)
                    .connectTimeout(30, TimeUnit.SECONDS)
                    .readTimeout(120, TimeUnit.SECONDS)
                    .addInterceptor(authInterceptor)
                    .build();

            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(client)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }

    public static void reset() {
        retrofit = null;
    }
}
