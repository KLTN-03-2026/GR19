package com.example.cafebook;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.RadioGroup;
import android.widget.SeekBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.EdgeToEdge;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.example.cafebook.fragments.PromotionSelectionFragment;
import com.example.cafebook.models.GioHangSyncRequestDto;
import com.example.cafebook.models.ThanhToanLoadDto;
import com.example.cafebook.models.ThanhToanResponseDto;
import com.example.cafebook.models.ThanhToanSubmitDto;
import com.example.cafebook.models.GioHangKhuyenMaiDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CheckoutApiService;
import com.example.cafebook.utils.CartManager;

import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CheckoutActivity extends AppCompatActivity {

    private TextView tvStoreStatus, tvAvailablePoints, tvSubtotal, tvPromoDiscount, tvPointDiscount, tvShipping, tvTotal, tvPointUsageInfo, tvSelectedPromo;
    private EditText etName, etEmail, etPhone, etAddress, etNote;
    private SeekBar sbPoints;
    private RadioGroup rgPaymentMethod;
    private View layoutPoints;
    private ProgressBar progressBar;

    private CartManager cartManager;
    private CheckoutApiService apiService;
    private ThanhToanLoadDto checkoutData;
    private String selectedPromoCode = "";

    private final ActivityResultLauncher<Intent> vnpayLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            result -> {
                if (result.getResultCode() == RESULT_OK && result.getData() != null) {
                    Uri uri = result.getData().getData();
                    if (uri != null) {
                        Intent intent = new Intent(this, PaymentResultActivity.class);
                        intent.setData(uri);
                        startActivity(intent);
                        cartManager.clearCart();
                        finish();
                    }
                }
            }
    );

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        EdgeToEdge.enable(this);
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.checkout_main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        cartManager = CartManager.getInstance(this);
        apiService = ApiClient.getClient(this).create(CheckoutApiService.class);

        initViews();
        loadCheckoutData();
    }

    private void initViews() {
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setTitle("Thanh toán");
        }
        toolbar.setNavigationOnClickListener(v -> getOnBackPressedDispatcher().onBackPressed());

        tvStoreStatus = findViewById(R.id.tvStoreStatus);
        tvAvailablePoints = findViewById(R.id.tvAvailablePoints);
        tvPointUsageInfo = findViewById(R.id.tvPointUsageInfo);
        tvSubtotal = findViewById(R.id.tvSubtotal);
        tvPromoDiscount = findViewById(R.id.tvPromoDiscount);
        tvPointDiscount = findViewById(R.id.tvPointDiscount);
        tvShipping = findViewById(R.id.tvShipping);
        tvTotal = findViewById(R.id.tvTotal);
        etName = findViewById(R.id.etName);
        etEmail = findViewById(R.id.etEmail);
        etPhone = findViewById(R.id.etPhone);
        etAddress = findViewById(R.id.etAddress);
        etNote = findViewById(R.id.etNote);
        sbPoints = findViewById(R.id.sbPoints);
        rgPaymentMethod = findViewById(R.id.rgPaymentMethod);
        layoutPoints = findViewById(R.id.layoutPoints);
        progressBar = findViewById(R.id.progressBar);
        tvSelectedPromo = findViewById(R.id.tvSelectedPromo);

        findViewById(R.id.btnSelectPromo).setOnClickListener(v -> openPromoSelection());
        findViewById(R.id.cardPromo).setOnClickListener(v -> openPromoSelection());

        sbPoints.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
            @Override
            public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                updateSummary();
            }

            @Override
            public void onStartTrackingTouch(SeekBar seekBar) {}

            @Override
            public void onStopTrackingTouch(SeekBar seekBar) {}
        });

        findViewById(R.id.btnSubmitOrder).setOnClickListener(v -> submitOrder());
    }

    private void openPromoSelection() {
        if (checkoutData != null && checkoutData.getAvailablePromotions() != null) {
            PromotionSelectionFragment fragment = PromotionSelectionFragment.newInstance(checkoutData.getAvailablePromotions());
            getSupportFragmentManager().beginTransaction()
                    .setCustomAnimations(android.R.anim.fade_in, android.R.anim.fade_out)
                    .add(android.R.id.content, fragment)
                    .addToBackStack(null)
                    .commit();

            getSupportFragmentManager().setFragmentResultListener("PROMO_REQUEST_KEY", this, (requestKey, bundle) -> {
                selectedPromoCode = bundle.getString("SELECTED_PROMO_CODE");
                tvSelectedPromo.setText("Mã áp dụng: " + selectedPromoCode);
                loadCheckoutData(); // Tải lại để áp dụng mã
            });
        }
    }

    private void loadCheckoutData() {
        progressBar.setVisibility(View.VISIBLE);
        GioHangSyncRequestDto request = new GioHangSyncRequestDto();
        request.setItems(cartManager.getCartItems());
        request.setMaKhuyenMaiApDung(selectedPromoCode);

        apiService.loadCheckoutData(request).enqueue(new Callback<ThanhToanLoadDto>() {
            @Override
            public void onResponse(@NonNull Call<ThanhToanLoadDto> call, @NonNull Response<ThanhToanLoadDto> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    checkoutData = response.body();
                    bindData(checkoutData);
                } else {
                    Toast.makeText(CheckoutActivity.this, "Lỗi tải dữ liệu thanh toán", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ThanhToanLoadDto> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(CheckoutActivity.this, "Không thể kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void bindData(ThanhToanLoadDto data) {
        if (!data.isStoreOpen()) {
            tvStoreStatus.setVisibility(View.VISIBLE);
            tvStoreStatus.setText(data.getStoreMessage());
            findViewById(R.id.btnSubmitOrder).setEnabled(false);
            findViewById(R.id.btnSubmitOrder).setAlpha(0.5f);
        } else {
            tvStoreStatus.setVisibility(View.GONE);
        }

        etName.setText(data.getKhachHang().getHoTen());
        etEmail.setText(data.getKhachHang().getEmail());
        etPhone.setText(data.getKhachHang().getSoDienThoai());
        etAddress.setText(data.getKhachHang().getDiaChi());
        tvAvailablePoints.setText(String.format(Locale.getDefault(), "Bạn có: %d điểm", data.getKhachHang().getDiemTichLuy()));

        // Logic giới hạn dùng điểm (không quá 50% đơn hàng)
        double subtotal = data.getCartSummary().getTongTienHang();
        double promoDiscount = data.getCartSummary().getTienGiamGia();
        double maxDiscountVnd = (subtotal - promoDiscount) * 0.5;
        int maxPointsPossible = (int) Math.ceil(maxDiscountVnd / data.getTiLeDoiDiemVND());
        int finalMaxPoints = Math.min(data.getKhachHang().getDiemTichLuy(), maxPointsPossible);

        sbPoints.setMax(finalMaxPoints);
        updateSummary();
    }

    private void updateSummary() {
        if (checkoutData == null) return;

        double subtotal = checkoutData.getCartSummary().getTongTienHang();
        double promoDiscount = checkoutData.getCartSummary().getTienGiamGia();
        double shipping = checkoutData.getCartSummary().getPhiGiaoHang();
        
        int pointsToUse = sbPoints.getProgress();
        double pointDiscount = pointsToUse * checkoutData.getTiLeDoiDiemVND();
        
        if (pointsToUse > 0) {
            layoutPoints.setVisibility(View.VISIBLE);
        } else {
            layoutPoints.setVisibility(View.GONE);
        }

        tvPointUsageInfo.setText(String.format(Locale.getDefault(), "Sử dụng %d điểm (Giảm %,.0f đ)", pointsToUse, pointDiscount));
        tvSubtotal.setText(String.format(Locale.getDefault(), "%,.0f đ", subtotal));
        tvPromoDiscount.setText(String.format(Locale.getDefault(), "-%,.0f đ", promoDiscount));
        tvPointDiscount.setText(String.format(Locale.getDefault(), "-%,.0f đ", pointDiscount));
        tvShipping.setText(String.format(Locale.getDefault(), "%,.0f đ", shipping));
        
        double total = subtotal - promoDiscount - pointDiscount + shipping;
        tvTotal.setText(String.format(Locale.getDefault(), "%,.0f đ", total));
    }

    private void submitOrder() {
        String name = etName.getText().toString().trim();
        String phone = etPhone.getText().toString().trim();
        String address = etAddress.getText().toString().trim();

        if (name.isEmpty() || phone.isEmpty() || address.isEmpty()) {
            Toast.makeText(this, "Vui lòng nhập đầy đủ thông tin", Toast.LENGTH_SHORT).show();
            return;
        }

        progressBar.setVisibility(View.VISIBLE);
        ThanhToanSubmitDto dto = new ThanhToanSubmitDto();
        GioHangSyncRequestDto cartRequest = new GioHangSyncRequestDto();
        cartRequest.setItems(cartManager.getCartItems());
        cartRequest.setMaKhuyenMaiApDung(cartManager.getPromoCode());
        
        dto.setCartData(cartRequest);
        dto.setHoTen(name);
        dto.setEmail(etEmail.getText().toString().trim());
        dto.setSoDienThoai(phone);
        dto.setDiaChiGiaoHang(address);
        dto.setGhiChu(etNote.getText().toString().trim());
        
        String method = rgPaymentMethod.getCheckedRadioButtonId() == R.id.rbVNPAY ? "VNPAY" : "COD";
        dto.setPhuongThucThanhToan(method);
        dto.setDiemSuDung(sbPoints.getProgress());
        dto.setReturnUrl("cafebook://payment_result");

        GioHangSyncRequestDto cartData = new GioHangSyncRequestDto();
        cartData.setItems(cartManager.getCartItems());
        cartData.setMaKhuyenMaiApDung(selectedPromoCode);
        dto.setCartData(cartData);

        apiService.submitOrder(dto).enqueue(new Callback<ThanhToanResponseDto>() {
            @Override
            public void onResponse(@NonNull Call<ThanhToanResponseDto> call, @NonNull Response<ThanhToanResponseDto> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    ThanhToanResponseDto result = response.body();
                    if (result.isSuccess()) {
                        if (result.getPaymentUrl() != null && !result.getPaymentUrl().isEmpty()) {
                            // Handle VNPAY
                            Intent intent = new Intent(CheckoutActivity.this, VnpayWebViewActivity.class);
                            intent.putExtra("PAYMENT_URL", result.getPaymentUrl());
                            intent.putExtra("RETURN_URL", "cafebook://payment_result");
                            vnpayLauncher.launch(intent);
                        } else {
                            Intent intent = new Intent(CheckoutActivity.this, PaymentResultActivity.class);
                            intent.putExtra("ID_HOA_DON", result.getIdHoaDonMoi());
                            startActivity(intent);
                            cartManager.clearCart();
                            finish();
                        }
                    } else {
                        Toast.makeText(CheckoutActivity.this, result.getMessage(), Toast.LENGTH_LONG).show();
                    }
                } else {
                    Toast.makeText(CheckoutActivity.this, "Lỗi đặt hàng", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ThanhToanResponseDto> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(CheckoutActivity.this, "Không thể kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
