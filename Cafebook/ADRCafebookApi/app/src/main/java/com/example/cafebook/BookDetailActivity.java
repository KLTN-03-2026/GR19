package com.example.cafebook;

import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import com.bumptech.glide.Glide;
import com.example.cafebook.models.ChiTietSachDto;
import com.example.cafebook.models.NameItemDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;
import com.google.android.material.appbar.CollapsingToolbarLayout;
import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class BookDetailActivity extends AppCompatActivity {

    private int bookId;
    private ImageView imgLarge;
    private TextView tvTitle, tvPrice, tvStatus, tvDescription;
    private ChipGroup chipGroupAuthors;
    private CollapsingToolbarLayout collapsingToolbarLayout;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_book_detail);

        bookId = getIntent().getIntExtra("BOOK_ID", 0);
        if (bookId == 0) {
            Toast.makeText(this, "Không tìm thấy thông tin sách", Toast.LENGTH_SHORT).show();
            finish();
            return;
        }

        initViews();
        loadBookDetails();
    }

    private void initViews() {
        imgLarge = findViewById(R.id.imgBookLarge);
        tvTitle = findViewById(R.id.tvDetailTitle);
        tvPrice = findViewById(R.id.tvDetailPrice);
        tvStatus = findViewById(R.id.tvDetailStatus);
        tvDescription = findViewById(R.id.tvDetailDescription);
        chipGroupAuthors = findViewById(R.id.chipGroupAuthors);
        collapsingToolbarLayout = findViewById(R.id.collapsing_toolbar);
        
        Toolbar toolbar = findViewById(R.id.toolbarDetail);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        }
        toolbar.setNavigationOnClickListener(v -> onBackPressed());
    }

    private void loadBookDetails() {
        CafebookApi api = ApiClient.getClient().create(CafebookApi.class);
        api.getBookDetails(bookId).enqueue(new Callback<ChiTietSachDto>() {
            @Override
            public void onResponse(@NonNull Call<ChiTietSachDto> call, @NonNull Response<ChiTietSachDto> response) {
                if (response.isSuccessful() && response.body() != null) {
                    bindData(response.body());
                } else {
                    Toast.makeText(BookDetailActivity.this, "Lỗi server: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ChiTietSachDto> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Failed to load book details", t);
                Toast.makeText(BookDetailActivity.this, "Không thể kết nối đến máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void bindData(ChiTietSachDto data) {
        collapsingToolbarLayout.setTitle(data.getTieuDe());
        tvTitle.setText(data.getTieuDe());
        tvPrice.setText(String.format("Tiền cọc: %,.0f đ", data.getGiaBia()));
        
        if (data.getSoLuongCoSan() > 0) {
            tvStatus.setText("✅ Còn " + data.getSoLuongCoSan() + " cuốn");
            tvStatus.setTextColor(getResources().getColor(android.R.color.holo_green_dark));
        } else {
            tvStatus.setText("❌ Đã hết sách");
            tvStatus.setTextColor(getResources().getColor(android.R.color.holo_red_dark));
        }
        
        tvDescription.setText(data.getMoTa() != null ? data.getMoTa() : "Không có mô tả cho cuốn sách này.");

        if (data.getAnhBiaUrl() != null && !data.getAnhBiaUrl().isEmpty()) {
            Glide.with(this)
                    .load(data.getAnhBiaUrl())
                    .placeholder(R.drawable.ic_launcher_background)
                    .into(imgLarge);
        }

        chipGroupAuthors.removeAllViews();
        if (data.getTacGias() != null) {
            for (NameItemDto author : data.getTacGias()) {
                Chip chip = new Chip(this);
                chip.setText(author.getName());
                chip.setChipBackgroundColorResource(R.color.cafe_beige);
                chip.setTextColor(getResources().getColor(R.color.cafe_brown));
                chipGroupAuthors.addView(chip);
            }
        }
    }
}
