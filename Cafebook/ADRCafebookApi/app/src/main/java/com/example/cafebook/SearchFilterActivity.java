package com.example.cafebook;

import android.os.Bundle;
import android.transition.TransitionManager;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.adapters.BookSearchAdapter;
import com.example.cafebook.models.TimKiemSachCardDto;
import com.example.cafebook.models.TimKiemSachResultDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class SearchFilterActivity extends AppCompatActivity {

    private Integer idTacGia, idTheLoai, idNXB;
    private int currentPage = 1;
    private int totalPages = 1;
    private boolean isLoading = false;

    private TextView tvPageTitle, tvPageDescription, btnToggleDescription;
    private RecyclerView rvBooks;
    private ProgressBar pbLoading;
    private BookSearchAdapter adapter;
    private List<TimKiemSachCardDto> bookList = new ArrayList<>();
    private CafebookApi api;
    private boolean isDescExpanded = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_search_filter);

        idTacGia = getIntent().hasExtra("ID_TAC_GIA") ? getIntent().getIntExtra("ID_TAC_GIA", 0) : null;
        idTheLoai = getIntent().hasExtra("ID_THE_LOAI") ? getIntent().getIntExtra("ID_THE_LOAI", 0) : null;
        idNXB = getIntent().hasExtra("ID_NXB") ? getIntent().getIntExtra("ID_NXB", 0) : null;

        // Ensure 0 is treated as null for the API
        if (idTacGia != null && idTacGia == 0) idTacGia = null;
        if (idTheLoai != null && idTheLoai == 0) idTheLoai = null;
        if (idNXB != null && idNXB == 0) idNXB = null;

        api = ApiClient.getClient(this).create(CafebookApi.class);
        initViews();
        setupRecyclerView();
        setupDescriptionToggle();
        
        loadBooks(1);
    }

    private void initViews() {
        tvPageTitle = findViewById(R.id.tvPageTitle);
        tvPageDescription = findViewById(R.id.tvPageDescription);
        btnToggleDescription = findViewById(R.id.btnToggleDescription);
        rvBooks = findViewById(R.id.rvBooks);
        pbLoading = findViewById(R.id.pbLoading);

        Toolbar toolbar = findViewById(R.id.toolbarFilter);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setTitle("");
        }
        toolbar.setNavigationOnClickListener(v -> onBackPressed());
    }

    private void setupRecyclerView() {
        int spanCount = getResources().getConfiguration().screenWidthDp >= 600 ? 4 : 2;
        GridLayoutManager layoutManager = new GridLayoutManager(this, spanCount);
        rvBooks.setLayoutManager(layoutManager);
        
        adapter = new BookSearchAdapter(bookList, this);
        rvBooks.setAdapter(adapter);

        rvBooks.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                if (dy > 0) {
                    int visibleItemCount = layoutManager.getChildCount();
                    int totalItemCount = layoutManager.getItemCount();
                    int pastVisibleItems = layoutManager.findFirstVisibleItemPosition();

                    if (!isLoading && currentPage < totalPages) {
                        if ((visibleItemCount + pastVisibleItems) >= totalItemCount) {
                            loadBooks(currentPage + 1);
                        }
                    }
                }
            }
        });
    }

    private void setupDescriptionToggle() {
        btnToggleDescription.setOnClickListener(v -> {
            TransitionManager.beginDelayedTransition((ViewGroup) tvPageDescription.getParent());
            if (isDescExpanded) {
                tvPageDescription.setMaxLines(3);
                btnToggleDescription.setText("Đọc tiếp ▼");
            } else {
                tvPageDescription.setMaxLines(Integer.MAX_VALUE);
                btnToggleDescription.setText("Thu gọn ▲");
            }
            isDescExpanded = !isDescExpanded;
        });
    }

    private void loadBooks(int page) {
        isLoading = true;
        if (page > 1) pbLoading.setVisibility(View.VISIBLE);

        api.searchBooksStandalone(null, idTacGia, idTheLoai, idNXB, page).enqueue(new Callback<TimKiemSachResultDto>() {
            @Override
            public void onResponse(@NonNull Call<TimKiemSachResultDto> call, @NonNull Response<TimKiemSachResultDto> response) {
                isLoading = false;
                pbLoading.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    TimKiemSachResultDto data = response.body();
                    
                    if (page == 1) {
                        tvPageTitle.setText(data.getTieuDeTrang());
                        if (data.getMoTaTrang() != null && !data.getMoTaTrang().isEmpty()) {
                            tvPageDescription.setText(data.getMoTaTrang());
                            tvPageDescription.post(() -> {
                                if (tvPageDescription.getLineCount() > 3) {
                                    btnToggleDescription.setVisibility(View.VISIBLE);
                                }
                            });
                        }
                    }

                    totalPages = data.getTotalPages();
                    currentPage = data.getCurrentPage();
                    
                    int startPos = bookList.size();
                    if (data.getSachList() != null) {
                        bookList.addAll(data.getSachList());
                        adapter.notifyItemRangeInserted(startPos, data.getSachList().size());
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<TimKiemSachResultDto> call, @NonNull Throwable t) {
                isLoading = false;
                pbLoading.setVisibility(View.GONE);
                Log.e("API_ERROR", "Search filter failed", t);
                Toast.makeText(SearchFilterActivity.this, "Lỗi kết nối!", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
