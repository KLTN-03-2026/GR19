package com.example.cafebook;

import android.os.Bundle;
import android.view.View;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import com.example.cafebook.models.GioHangSyncRequestDto;
import com.example.cafebook.models.ThanhToanLoadDto;
import com.example.cafebook.models.ThanhToanResponseDto;
import com.example.cafebook.models.ThanhToanSubmitDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CheckoutApiService;
import com.example.cafebook.utils.CartManager;

import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CheckoutActivity extends AppCompatActivity {

    private TextView tvStoreStatus, tvCustomerName, tvAvailablePoints, tvSubtotal, tvPromoDiscount, tvPointDiscount, tvShipping, tvTotal;
    private EditText etPhone, etAddress, etNote;
    private CheckBox cbUsePoints;
    private RadioGroup rgPaymentMethod;
    private View layoutPoints;
    private ProgressBar progressBar;

    private CartManager cartManager;
    private CheckoutApiService apiService;
    private ThanhToanLoadDto checkoutData;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

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
        toolbar.setNavigationOnClickListener(v -> onBackPressed());

        tvStoreStatus = findViewById(R.id.tvStoreStatus);
        tvCustomerName = findViewById(R.id.tvCustomerName);
        tvAvailablePoints = findViewById(R.id.tvAvailablePoints);
        tvSubtotal = findViewById(R.id.tvSubtotal);
        tvPromoDiscount = findViewById(R.id.tvPromoDiscount);
        tvPointDiscount = findViewById(R.id.tvPointDiscount);
        tvShipping = findViewById(R.id.tvShipping);
        tvTotal = findViewById(R.id.tvTotal);
        etPhone = findViewById(R.id.etPhone);
        etAddress = findViewById(R.id.etAddress);
        etNote = findViewById(R.id.etNote);
        cbUsePoints = findViewById(R.id.cbUsePoints);
        rgPaymentMethod = findViewById(R.id.rgPaymentMethod);
        layoutPoints = findViewById(R.id.layoutPoints);
        progressBar = findViewById(R.id.progressBar);

        cbUsePoints.setOnCheckedChangeListener((buttonView, isChecked) -> {
            layoutPoints.setVisibility(isChecked ? View.VISIBLE : View.GONE);
            updateSummary();
        });

        findViewById(R.id.btnSubmitOrder).setOnClickListener(v -> submitOrder());
    }

    private void loadCheckoutData() {
        progressBar.setVisibility(View.VISIBLE);
        GioHangSyncRequestDto request = new GioHangSyncRequestDto();
        request.setItems(cartManager.getCartItems());
        request.setMaKhuyenMaiApDung(cartManager.getPromoCode());

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

        tvCustomerName.setText(data.getKhachHang().getHoTen());
        etPhone.setText(data.getKhachHang().getSoDienThoai());
        etAddress.setText(data.getKhachHang().getDiaChi());
        tvAvailablePoints.setText(String.format(Locale.getDefault(), "(Đang có: %d điểm)", data.getKhachHang().getDiemTichLuy()));

        updateSummary();
    }

    private void updateSummary() {
        if (checkoutData == null) return;

        double subtotal = checkoutData.getCartSummary().getTongTienHang();
        double promoDiscount = checkoutData.getCartSummary().getTienGiamGia();
        double shipping = checkoutData.getCartSummary().getPhiGiaoHang();
        
        double pointDiscount = 0;
        if (cbUsePoints.isChecked()) {
            pointDiscount = checkoutData.getKhachHang().getDiemTichLuy() * checkoutData.getTiLeDoiDiemVND();
            double maxAllowed = (subtotal - promoDiscount) * 0.5;
            if (pointDiscount > maxAllowed) pointDiscount = maxAllowed;
        }

        tvSubtotal.setText(String.format(Locale.getDefault(), "%,.0f đ", subtotal));
        tvPromoDiscount.setText(String.format(Locale.getDefault(), "-%,.0f đ", promoDiscount));
        tvPointDiscount.setText(String.format(Locale.getDefault(), "-%,.0f đ", pointDiscount));
        tvShipping.setText(String.format(Locale.getDefault(), "%,.0f đ", shipping));
        
        double total = subtotal - promoDiscount - pointDiscount + shipping;
        tvTotal.setText(String.format(Locale.getDefault(), "%,.0f đ", total));
    }

    private void submitOrder() {
        String phone = etPhone.getText().toString().trim();
        String address = etAddress.getText().toString().trim();

        if (phone.isEmpty() || address.isEmpty()) {
            Toast.makeText(this, "Vui lòng nhập đầy đủ thông tin", Toast.LENGTH_SHORT).show();
            return;
        }

        progressBar.setVisibility(View.VISIBLE);
        ThanhToanSubmitDto dto = new ThanhToanSubmitDto();
        GioHangSyncRequestDto cartRequest = new GioHangSyncRequestDto();
        cartRequest.setItems(cartManager.getCartItems());
        cartRequest.setMaKhuyenMaiApDung(cartManager.getPromoCode());
        
        dto.setCartData(cartRequest);
        dto.setSoDienThoai(phone);
        dto.setDiaChiGiaoHang(address);
        dto.setGhiChu(etNote.getText().toString().trim());
        
        String method = rgPaymentMethod.getCheckedRadioButtonId() == R.id.rbVNPAY ? "VNPAY" : "COD";
        dto.setPhuongThucThanhToan(method);
        
        if (cbUsePoints.isChecked()) {
            dto.setDiemSuDung(checkoutData.getKhachHang().getDiemTichLuy());
        } else {
            dto.setDiemSuDung(0);
        }
        
        dto.setReturnUrl("cafebook://vnpay-return");

        apiService.submitOrder(dto).enqueue(new Callback<ThanhToanResponseDto>() {
            @Override
            public void onResponse(@NonNull Call<ThanhToanResponseDto> call, @NonNull Response<ThanhToanResponseDto> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    ThanhToanResponseDto result = response.body();
                    if (result.isSuccess()) {
                        if (result.getPaymentUrl() != null && !result.getPaymentUrl().isEmpty()) {
                            // Handle VNPAY
                            Toast.makeText(CheckoutActivity.this, "Mở trình duyệt thanh toán...", Toast.LENGTH_SHORT).show();
                            // In real app, open WebView or Browser
                        } else {
                            Toast.makeText(CheckoutActivity.this, "Đặt hàng thành công!", Toast.LENGTH_LONG).show();
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
