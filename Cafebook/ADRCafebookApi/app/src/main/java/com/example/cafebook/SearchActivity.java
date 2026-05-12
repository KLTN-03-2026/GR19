package com.example.cafebook;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.widget.EditText;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.adapters.SearchBookAdapter;
import com.example.cafebook.models.TimKiemSachCardDto;
import com.example.cafebook.models.TimKiemSachResultDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class SearchActivity extends AppCompatActivity {

    private RecyclerView rvSearchResult;
    private EditText edtSearchInput;
    private SearchBookAdapter adapter;
    private List<TimKiemSachCardDto> bookList = new ArrayList<>();
    
    private Handler searchHandler = new Handler(Looper.getMainLooper());
    private Runnable searchRunnable;
    private CafebookApi api;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_search);

        api = ApiClient.getClient().create(CafebookApi.class);
        initViews();
    }

    private void initViews() {
        rvSearchResult = findViewById(R.id.rvSearchResult);
        edtSearchInput = findViewById(R.id.edtSearchInput);
        findViewById(R.id.btnBack).setOnClickListener(v -> finish());

        int spanCount = getResources().getConfiguration().screenWidthDp >= 600 ? 3 : 2;
        rvSearchResult.setLayoutManager(new GridLayoutManager(this, spanCount));
        adapter = new SearchBookAdapter(bookList, this);
        rvSearchResult.setAdapter(adapter);

        edtSearchInput.addTextChangedListener(new TextWatcher() {
            @Override public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            @Override public void onTextChanged(CharSequence s, int start, int before, int count) {}
            
            @Override
            public void afterTextChanged(Editable s) {
                searchHandler.removeCallbacks(searchRunnable);
                searchRunnable = () -> {
                    String query = s.toString().trim();
                    if (!query.isEmpty()) {
                        performSearch(query);
                    } else {
                        bookList.clear();
                        adapter.notifyDataSetChanged();
                    }
                };
                searchHandler.postDelayed(searchRunnable, 500);
            }
        });
    }

    private void performSearch(String keyword) {
        api.searchBooksStandalone(keyword, 1).enqueue(new Callback<TimKiemSachResultDto>() {
            @Override
            public void onResponse(@NonNull Call<TimKiemSachResultDto> call, @NonNull Response<TimKiemSachResultDto> response) {
                if (response.isSuccessful() && response.body() != null) {
                    bookList.clear();
                    if (response.body().getSachList() != null) {
                        bookList.addAll(response.body().getSachList());
                    }
                    adapter.notifyDataSetChanged();
                }
            }

            @Override
            public void onFailure(@NonNull Call<TimKiemSachResultDto> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Search failed", t);
                Toast.makeText(SearchActivity.this, "Lỗi tìm kiếm", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
