package com.example.cafebook;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.example.cafebook.models.ChiTietSanPhamDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProductDetailActivity extends AppCompatActivity {

    private int productId;
    private ImageView imgProduct;
    private TextView tvName, tvPrice, tvDesc;

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

        findViewById(R.id.btnAddToCart).setOnClickListener(v -> 
            Toast.makeText(this, "Chức năng thêm vào giỏ hàng đang phát triển", Toast.LENGTH_SHORT).show()
        );
    }

    private void loadProductDetails() {
        CafebookApi api = ApiClient.getClient().create(CafebookApi.class);
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
    }

    private void bindData(ChiTietSanPhamDto data) {
        tvName.setText(data.getTenSanPham());
        tvPrice.setText(String.format(Locale.getDefault(), "%,.0f đ", data.getDonGia()));
        tvDesc.setText(data.getMoTa() != null ? data.getMoTa() : "Không có mô tả cho sản phẩm này.");

        Glide.with(this)
             .load(data.getHinhAnhUrl())
             .diskCacheStrategy(DiskCacheStrategy.ALL)
             .placeholder(R.drawable.ic_launcher_background)
             .into(imgProduct);
    }
}
