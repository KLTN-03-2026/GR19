package com.example.cafebook;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.cafebook.adapters.RecommendationAdapter;
import com.example.cafebook.models.ChiTietSachDto;
import com.example.cafebook.models.NameItemDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;
import com.google.android.material.appbar.CollapsingToolbarLayout;
import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class BookDetailActivity extends AppCompatActivity {

    private int bookId;
    private ImageView imgLarge;
    private TextView tvTitle, tvPrice, tvStatus, tvDescription, tvBookLocation, tvBookInventory;
    private TextView tvLabelAuthors, tvLabelGenres, tvLabelPublishers;
    private ChipGroup chipGroupAuthors, chipGroupGenres, chipGroupPublishers;
    private CollapsingToolbarLayout collapsingToolbarLayout;
    private RecyclerView rvBookRecommendations;

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
        tvBookLocation = findViewById(R.id.tvBookLocation);
        tvBookInventory = findViewById(R.id.tvBookInventory);

        tvLabelAuthors = findViewById(R.id.tvLabelAuthors);
        tvLabelGenres = findViewById(R.id.tvLabelGenres);
        tvLabelPublishers = findViewById(R.id.tvLabelPublishers);

        chipGroupAuthors = findViewById(R.id.chipGroupAuthors);
        chipGroupGenres = findViewById(R.id.chipGroupGenres);
        chipGroupPublishers = findViewById(R.id.chipGroupPublishers);

        collapsingToolbarLayout = findViewById(R.id.collapsing_toolbar);
        rvBookRecommendations = findViewById(R.id.rvBookRecommendations);

        Toolbar toolbar = findViewById(R.id.toolbarDetail);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        }
        toolbar.setNavigationOnClickListener(v -> onBackPressed());

        // Handle FAB Click
        findViewById(R.id.fabBooking).setOnClickListener(v -> {
            Toast.makeText(this, "Chức năng Đặt mượn đang được phát triển", Toast.LENGTH_SHORT).show();
        });
    }

    private void loadBookDetails() {
        CafebookApi api = ApiClient.getClient(this).create(CafebookApi.class);
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

        // Bind Location & Inventory
        tvBookLocation.setText("Vị trí kệ: " + (data.getViTri() != null ? data.getViTri() : "Chưa cập nhật"));
        tvBookInventory.setText("Tổng kho: " + data.getTongSoLuong() + " cuốn | Sẵn sàng mượn: " + data.getSoLuongCoSan() + " cuốn");

        Glide.with(this)
                .load(data.getAnhBiaUrl())
                .placeholder(R.drawable.default_book_cover)
                .error(R.drawable.default_book_cover)
                .into(imgLarge);

        // Bind Authors
        bindChips(data.getTacGias(), tvLabelAuthors, chipGroupAuthors, "ID_TAC_GIA");

        // Bind Genres
        bindChips(data.getTheLoais(), tvLabelGenres, chipGroupGenres, "ID_THE_LOAI");

        // Bind Publishers
        bindChips(data.getNhaXuatBans(), tvLabelPublishers, chipGroupPublishers, "ID_NXB");

        // Bind Recommendations
        if (data.getGoiY() != null && !data.getGoiY().isEmpty()) {
            findViewById(R.id.tvLabelRecommendations).setVisibility(View.VISIBLE);
            rvBookRecommendations.setVisibility(View.VISIBLE);
            RecommendationAdapter recAdapter = new RecommendationAdapter(new ArrayList<>(data.getGoiY()));
            rvBookRecommendations.setLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.HORIZONTAL, false));
            rvBookRecommendations.setAdapter(recAdapter);
        } else {
            findViewById(R.id.tvLabelRecommendations).setVisibility(View.GONE);
            rvBookRecommendations.setVisibility(View.GONE);
        }
    } // <--- DẤU NGOẶC ĐÓNG HÀM bindData() ĐÃ ĐƯỢC THÊM VÀO ĐÂY

    private void bindChips(List<?> items, TextView label, ChipGroup chipGroup, String intentExtraKey) {
        chipGroup.removeAllViews();
        if (items != null && !items.isEmpty()) {
            label.setVisibility(View.VISIBLE);
            chipGroup.setVisibility(View.VISIBLE);
            for (Object item : items) {
                String name = "";
                int id = 0;

                if (item instanceof ChiTietSachDto.ChiTietSachTacGiaDto) {
                    name = ((ChiTietSachDto.ChiTietSachTacGiaDto) item).getName();
                    id = ((ChiTietSachDto.ChiTietSachTacGiaDto) item).getId();
                } else if (item instanceof ChiTietSachDto.ChiTietSachTheLoaiDto) {
                    name = ((ChiTietSachDto.ChiTietSachTheLoaiDto) item).getName();
                    id = ((ChiTietSachDto.ChiTietSachTheLoaiDto) item).getId();
                } else if (item instanceof ChiTietSachDto.ChiTietSachNxbDto) {
                    name = ((ChiTietSachDto.ChiTietSachNxbDto) item).getName();
                    id = ((ChiTietSachDto.ChiTietSachNxbDto) item).getId();
                }

                Chip chip = new Chip(this);
                chip.setText(name);
                chip.setChipBackgroundColorResource(R.color.cafe_beige);
                chip.setTextColor(getResources().getColor(R.color.cafe_brown));

                int finalId = id;
                chip.setOnClickListener(v -> {
                    Intent intent = new Intent(this, SearchFilterActivity.class); // Thay bằng Activity bạn muốn mở
                    intent.putExtra(intentExtraKey, finalId);
                    startActivity(intent);
                });

                chipGroup.addView(chip);
            }
        } else {
            label.setVisibility(View.GONE);
            chipGroup.setVisibility(View.GONE);
        }
    }
}