package com.example.cafebook;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.widget.ImageView;
import android.widget.RatingBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.example.cafebook.adapters.RecommendationAdapter;
import com.example.cafebook.adapters.ReviewAdapter;
import com.example.cafebook.models.ChiTietSanPhamDto;
import com.example.cafebook.models.DanhGiaChiTietDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import com.example.cafebook.utils.CartManager;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProductDetailActivity extends AppCompatActivity {

    private int productId;
    private ImageView imgProduct;
    private TextView tvName, tvPrice, tvDesc, tvRatingScore, tvTotalReviews;
    private RatingBar ratingBarProduct;
    private RecyclerView rvReviews, rvProductRecommendations;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_product_detail);

        productId = getIntent().getIntExtra("PRODUCT_ID", 0);
        if (productId == 0) {
            Toast.makeText(this, "Không tìm thấy sản phẩm", Toast.LENGTH_SHORT).show();
            finish();
            return;
        }
        
        initViews();
        loadProductDetails();
    }

    @SuppressLint("ClickableViewAccessibility")
    private void initViews() {
        imgProduct = findViewById(R.id.imgProductDetail);
        tvName = findViewById(R.id.tvDetailName);
        tvPrice = findViewById(R.id.tvDetailPrice);
        tvDesc = findViewById(R.id.tvDetailDesc);
        tvRatingScore = findViewById(R.id.tvRatingScore);
        tvTotalReviews = findViewById(R.id.tvTotalReviews);
        ratingBarProduct = findViewById(R.id.ratingBarProduct);
        rvReviews = findViewById(R.id.rvReviews);
        rvProductRecommendations = findViewById(R.id.rvProductRecommendations);

        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setTitle("");
        }
        toolbar.setNavigationOnClickListener(v -> onBackPressed());

        // Button animation
        findViewById(R.id.btnAddToCart).setOnTouchListener((v, event) -> {
            if (event.getAction() == MotionEvent.ACTION_DOWN) {
                v.animate().scaleX(0.95f).scaleY(0.95f).setDuration(100).start();
            } else if (event.getAction() == MotionEvent.ACTION_UP || event.getAction() == MotionEvent.ACTION_CANCEL) {
                v.animate().scaleX(1f).scaleY(1f).setDuration(100).start();
            }
            return false;
        });

        findViewById(R.id.btnAddToCart).setOnClickListener(v -> {
            android.content.SharedPreferences prefs = getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
            String token = prefs.getString("JWT_TOKEN", "");
            if (!token.isEmpty()) {
                CartManager.getInstance(this).addToCart(productId, 1);
                Toast.makeText(this, "Đã thêm vào giỏ hàng", Toast.LENGTH_SHORT).show();
            } else {
                Toast.makeText(this, "Vui lòng đăng nhập để sử dụng tính năng này", Toast.LENGTH_SHORT).show();
                // Optional: Mở màn hình đăng nhập
                // loadFragment logic in Activity is different, usually startActivity
            }
        });
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.top_toolbar_menu, menu);
        // Hide other items if not needed, or just show cart
        menu.findItem(R.id.action_contact).setVisible(false);
        menu.findItem(R.id.action_support).setVisible(false);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (item.getItemId() == R.id.action_cart) {
            android.content.SharedPreferences prefs = getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
            String token = prefs.getString("JWT_TOKEN", "");
            if (!token.isEmpty()) {
                startActivity(new Intent(this, CartActivity.class));
            } else {
                Toast.makeText(this, "Vui lòng đăng nhập để sử dụng tính năng giỏ hàng", Toast.LENGTH_SHORT).show();
            }
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

    private void loadProductDetails() {
        CafebookApi api = ApiClient.getClient(this).create(CafebookApi.class);
        
        // Fetch Details
        api.getProductDetails(productId).enqueue(new Callback<ChiTietSanPhamDto>() {
            @Override
            public void onResponse(@NonNull Call<ChiTietSanPhamDto> call, @NonNull Response<ChiTietSanPhamDto> response) {
                if (response.isSuccessful() && response.body() != null) {
                    bindData(response.body());
                } else {
                    Toast.makeText(ProductDetailActivity.this, "Lỗi server: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ChiTietSanPhamDto> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Failed to load product details", t);
                Toast.makeText(ProductDetailActivity.this, "Không thể kết nối đến máy chủ", Toast.LENGTH_SHORT).show();
            }
        });

        // Fetch Reviews Separately
        api.getProductReviews(productId).enqueue(new Callback<List<DanhGiaChiTietDto>>() {
            @Override
            public void onResponse(@NonNull Call<List<DanhGiaChiTietDto>> call, @NonNull Response<List<DanhGiaChiTietDto>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    bindReviews(response.body());
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<DanhGiaChiTietDto>> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Failed to load reviews", t);
            }
        });
    }

    private void bindData(ChiTietSanPhamDto data) {
        tvName.setText(data.getTenSanPham());
        tvPrice.setText(String.format(Locale.getDefault(), "%,.0f đ", data.getDonGia()));
        tvDesc.setText(data.getMoTa() != null ? data.getMoTa() : "Không có mô tả cho sản phẩm này.");

        // Bind Recommendations
        if (data.getGoiY() != null && !data.getGoiY().isEmpty()) {
            findViewById(R.id.tvLabelProductRecommendations).setVisibility(View.VISIBLE);
            rvProductRecommendations.setVisibility(View.VISIBLE);
            RecommendationAdapter recAdapter = new RecommendationAdapter(new ArrayList<>(data.getGoiY()));
            rvProductRecommendations.setLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.HORIZONTAL, false));
            rvProductRecommendations.setAdapter(recAdapter);
        } else {
            findViewById(R.id.tvLabelProductRecommendations).setVisibility(View.GONE);
            rvProductRecommendations.setVisibility(View.GONE);
        }

        Glide.with(this)
             .load(data.getHinhAnhUrl())
             .diskCacheStrategy(DiskCacheStrategy.ALL)
             .placeholder(R.drawable.default_food_icon)
             .error(R.drawable.default_food_icon)
             .into(imgProduct);
    }

    private void bindReviews(List<DanhGiaChiTietDto> reviews) {
        if (reviews != null && !reviews.isEmpty()) {
            rvReviews.setVisibility(View.VISIBLE);
            findViewById(R.id.layoutRatingBox).setVisibility(View.VISIBLE);

            float sum = 0;
            for (DanhGiaChiTietDto r : reviews) sum += r.getSoSao();
            float avg = sum / reviews.size();

            tvRatingScore.setText(String.format(Locale.getDefault(), "%.1f", avg));
            ratingBarProduct.setRating(avg);
            tvTotalReviews.setText("(" + reviews.size() + " đánh giá)");

            ReviewAdapter reviewAdapter = new ReviewAdapter(reviews);
            rvReviews.setLayoutManager(new LinearLayoutManager(this));
            rvReviews.setAdapter(reviewAdapter);
        } else {
            rvReviews.setVisibility(View.GONE);
            findViewById(R.id.layoutRatingBox).setVisibility(View.GONE);
        }
    }
}
