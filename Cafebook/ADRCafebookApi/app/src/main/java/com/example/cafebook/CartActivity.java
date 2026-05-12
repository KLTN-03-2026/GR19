package com.example.cafebook;

import android.content.Intent;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.EdgeToEdge;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.adapters.CartAdapter;
import com.example.cafebook.fragments.PromotionSelectionFragment;
import com.example.cafebook.models.GioHangItemDto;
import com.example.cafebook.models.GioHangKhuyenMaiDto;
import com.example.cafebook.models.GioHangResponseDto;
import com.example.cafebook.models.GioHangSyncRequestDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CartApiService;
import com.example.cafebook.utils.CartManager;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CartActivity extends AppCompatActivity implements CartAdapter.OnCartItemChangeListener {

    private RecyclerView rvCartItems;
    private CartAdapter adapter;
    private TextView tvSubtotal, tvDiscount, tvShipping, tvTotal;
    private View layoutDiscount;
    private EditText etPromoCode;
    private ProgressBar progressBar;
    private CartManager cartManager;
    private CartApiService apiService;
    private List<GioHangKhuyenMaiDto> availablePromotions;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        EdgeToEdge.enable(this);
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_cart);

        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.cart_main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        cartManager = CartManager.getInstance(this);
        apiService = ApiClient.getClient(this).create(CartApiService.class);

        initViews();
        syncCart();
    }

    private void initViews() {
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setTitle("Giỏ hàng");
        }
        toolbar.setNavigationOnClickListener(v -> onBackPressed());

        rvCartItems = findViewById(R.id.rvCartItems);
        tvSubtotal = findViewById(R.id.tvSubtotal);
        tvDiscount = findViewById(R.id.tvDiscount);
        tvShipping = findViewById(R.id.tvShipping);
        tvTotal = findViewById(R.id.tvTotal);
        layoutDiscount = findViewById(R.id.layoutDiscount);
        etPromoCode = findViewById(R.id.etPromoCode);
        progressBar = findViewById(R.id.progressBar);

        etPromoCode.setText(cartManager.getPromoCode());

        adapter = new CartAdapter(new ArrayList<>(), this);
        rvCartItems.setLayoutManager(new LinearLayoutManager(this));
        rvCartItems.setAdapter(adapter);

        findViewById(R.id.btnApplyPromo).setOnClickListener(v -> {
            if (availablePromotions != null && !availablePromotions.isEmpty()) {
                PromotionSelectionFragment fragment = PromotionSelectionFragment.newInstance(availablePromotions);
                getSupportFragmentManager().beginTransaction()
                        .setCustomAnimations(android.R.anim.fade_in, android.R.anim.fade_out)
                        .add(android.R.id.content, fragment)
                        .addToBackStack(null)
                        .commit();

                getSupportFragmentManager().setFragmentResultListener("PROMO_REQUEST_KEY", this, (requestKey, bundle) -> {
                    String code = bundle.getString("SELECTED_PROMO_CODE");
                    etPromoCode.setText(code);
                    cartManager.setPromoCode(code);
                    syncCart();
                });
            } else {
                String promo = etPromoCode.getText().toString().trim();
                cartManager.setPromoCode(promo);
                syncCart();
            }
        });

        findViewById(R.id.btnCheckout).setOnClickListener(v -> {
            startActivity(new Intent(this, CheckoutActivity.class));
        });
    }

    private void syncCart() {
        List<GioHangItemDto> localItems = cartManager.getCartItems();
        if (localItems.isEmpty()) {
            adapter.updateItems(new ArrayList<>());
            updateSummary(new GioHangResponseDto());
            return;
        }

        progressBar.setVisibility(View.VISIBLE);
        GioHangSyncRequestDto request = new GioHangSyncRequestDto();
        request.setItems(localItems);
        request.setMaKhuyenMaiApDung(cartManager.getPromoCode());

        // Fetch promotions first
        apiService.getAvailablePromotions(localItems).enqueue(new Callback<List<GioHangKhuyenMaiDto>>() {
            @Override
            public void onResponse(@NonNull Call<List<GioHangKhuyenMaiDto>> call, @NonNull Response<List<GioHangKhuyenMaiDto>> response) {
                if (response.isSuccessful()) {
                    availablePromotions = response.body();
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<GioHangKhuyenMaiDto>> call, @NonNull Throwable t) {}
        });

        apiService.syncCart(request).enqueue(new Callback<GioHangResponseDto>() {
            @Override
            public void onResponse(@NonNull Call<GioHangResponseDto> call, @NonNull Response<GioHangResponseDto> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    GioHangResponseDto data = response.body();
                    adapter.updateItems(data.getItems());
                    updateSummary(data);
                } else {
                    Toast.makeText(CartActivity.this, "Lỗi đồng bộ giỏ hàng", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<GioHangResponseDto> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(CartActivity.this, "Không thể kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void updateSummary(GioHangResponseDto data) {
        tvSubtotal.setText(String.format(Locale.getDefault(), "%,.0f đ", data.getTongTienHang()));
        if (data.getTienGiamGia() > 0) {
            layoutDiscount.setVisibility(View.VISIBLE);
            tvDiscount.setText(String.format(Locale.getDefault(), "-%,.0f đ", data.getTienGiamGia()));
        } else {
            layoutDiscount.setVisibility(View.GONE);
        }
        tvShipping.setText(String.format(Locale.getDefault(), "%,.0f đ", data.getPhiGiaoHang()));
        tvTotal.setText(String.format(Locale.getDefault(), "%,.0f đ", data.getTongThanhToan()));

        if (!data.isCanCheckout() && data.getCheckoutWarning() != null) {
            Toast.makeText(this, data.getCheckoutWarning(), Toast.LENGTH_LONG).show();
            findViewById(R.id.btnCheckout).setEnabled(false);
            findViewById(R.id.btnCheckout).setAlpha(0.5f);
        } else {
            findViewById(R.id.btnCheckout).setEnabled(true);
            findViewById(R.id.btnCheckout).setAlpha(1.0f);
        }
    }

    @Override
    public void onQuantityChange(int idSanPham, int newQuantity) {
        cartManager.updateQuantity(idSanPham, newQuantity);
        syncCart();
    }

    @Override
    public void onRemoveItem(int idSanPham) {
        cartManager.removeFromCart(idSanPham);
        syncCart();
    }
}
