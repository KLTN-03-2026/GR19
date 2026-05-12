package com.example.cafebook;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Base64;
import android.view.View;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.EdgeToEdge;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.example.cafebook.models.PaymentResultDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CheckoutApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.card.MaterialCardView;

import java.util.HashMap;
import java.util.Map;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class PaymentResultActivity extends AppCompatActivity {

    private ProgressBar pbLoading;
    private ImageView imgStatusIcon;
    private TextView tvStatusTitle, tvStatusMessage, tvOrderId, tvPaymentMethod, tvTotalAmount;
    private MaterialCardView cardOrderSummary;
    private MaterialButton btnTrackOrder, btnContinueShopping;

    private CheckoutApiService apiService;
    private int idHoaDonToLoad = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        EdgeToEdge.enable(this);
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_payment_result);

        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.payment_result_root), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        apiService = ApiClient.getClient(this).create(CheckoutApiService.class);
        initViews();
        handleIncomingData();
    }

    private void initViews() {
        pbLoading = findViewById(R.id.pbLoading);
        imgStatusIcon = findViewById(R.id.imgStatusIcon);
        tvStatusTitle = findViewById(R.id.tvStatusTitle);
        tvStatusMessage = findViewById(R.id.tvStatusMessage);
        cardOrderSummary = findViewById(R.id.cardOrderSummary);
        tvOrderId = findViewById(R.id.tvOrderId);
        tvPaymentMethod = findViewById(R.id.tvPaymentMethod);
        tvTotalAmount = findViewById(R.id.tvTotalAmount);
        btnTrackOrder = findViewById(R.id.btnTrackOrder);
        btnContinueShopping = findViewById(R.id.btnContinueShopping);

        btnContinueShopping.setOnClickListener(v -> {
            Intent intent = new Intent(this, MainActivity.class);
            intent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
            startActivity(intent);
            finish();
        });

        btnTrackOrder.setOnClickListener(v -> {
            // TODO: Open order details
            Toast.makeText(this, "Chức năng theo dõi đơn hàng đang phát triển", Toast.LENGTH_SHORT).show();
        });
    }

    private void handleIncomingData() {
        // 1. COD or Direct Order ID
        idHoaDonToLoad = getIntent().getIntExtra("ID_HOA_DON", 0);
        if (idHoaDonToLoad > 0) {
            fetchOrderSummary();
            return;
        }

        // 2. VNPAY Deep Link
        Uri data = getIntent().getData();
        if (data != null && data.getQueryParameterNames().contains("vnp_ResponseCode")) {
            verifyVNPaySignature(data);
            return;
        }

        showError("Không tìm thấy thông tin giao dịch.");
    }

    private void verifyVNPaySignature(Uri data) {
        Map<String, String> params = new HashMap<>();
        for (String key : data.getQueryParameterNames()) {
            params.put(key, data.getQueryParameter(key));
        }

        apiService.verifyVNPay(params).enqueue(new Callback<PaymentResultDto.VNPayVerifyResult>() {
            @Override
            public void onResponse(@NonNull Call<PaymentResultDto.VNPayVerifyResult> call, @NonNull Response<PaymentResultDto.VNPayVerifyResult> response) {
                if (response.isSuccessful() && response.body() != null) {
                    PaymentResultDto.VNPayVerifyResult res = response.body();
                    if (res.isSuccess() && res.getEncodedId() != null) {
                        decodeIdAndLoadSummary(res.getEncodedId());
                    } else {
                        showError(res.getMessage() != null ? res.getMessage() : "Thanh toán thất bại hoặc bị hủy.");
                    }
                } else {
                    showError("Lỗi kết nối khi xác thực thanh toán.");
                }
            }

            @Override
            public void onFailure(@NonNull Call<PaymentResultDto.VNPayVerifyResult> call, @NonNull Throwable t) {
                showError("Không thể kết nối đến máy chủ.");
            }
        });
    }

    private void decodeIdAndLoadSummary(String encoded) {
        try {
            String incoming = encoded.replace('-', '+').replace('_', '/');
            switch (incoming.length() % 4) {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            byte[] bytes = Base64.decode(incoming, Base64.DEFAULT);
            idHoaDonToLoad = Integer.parseInt(new String(bytes));
            fetchOrderSummary();
        } catch (Exception e) {
            showError("Mã đơn hàng bị sai lệch.");
        }
    }

    private void fetchOrderSummary() {
        apiService.getOrderSummary(idHoaDonToLoad).enqueue(new Callback<PaymentResultDto.OrderSummary>() {
            @Override
            public void onResponse(@NonNull Call<PaymentResultDto.OrderSummary> call, @NonNull Response<PaymentResultDto.OrderSummary> response) {
                if (response.isSuccessful() && response.body() != null) {
                    showSuccess(response.body());
                } else {
                    showError("Đơn hàng không tồn tại hoặc bạn không có quyền xem.");
                }
            }

            @Override
            public void onFailure(@NonNull Call<PaymentResultDto.OrderSummary> call, @NonNull Throwable t) {
                showError("Lỗi kết nối máy chủ.");
            }
        });
    }

    private void showSuccess(PaymentResultDto.OrderSummary order) {
        pbLoading.setVisibility(View.GONE);
        imgStatusIcon.setVisibility(View.VISIBLE);
        imgStatusIcon.setColorFilter(0xFF4CAF50);

        tvStatusTitle.setText("ĐẶT HÀNG THÀNH CÔNG");
        tvStatusMessage.setText("Cảm ơn bạn đã lựa chọn Cafebook!");

        cardOrderSummary.setVisibility(View.VISIBLE);
        btnTrackOrder.setVisibility(View.VISIBLE);

        tvOrderId.setText("#" + order.getIdHoaDonMoi());
        tvPaymentMethod.setText(order.getPhuongThucThanhToan());
        tvTotalAmount.setText(String.format("%,.0f đ", order.getThanhTien()));
    }

    private void showError(String errorMsg) {
        pbLoading.setVisibility(View.GONE);
        imgStatusIcon.setVisibility(View.VISIBLE);
        imgStatusIcon.setImageResource(R.drawable.ic_policy); // Reuse policy icon as fallback
        imgStatusIcon.setColorFilter(0xFFD32F2F);

        tvStatusTitle.setText("ĐÃ CÓ LỖI XẢY RA");
        tvStatusTitle.setTextColor(0xFFD32F2F);
        tvStatusMessage.setText(errorMsg);
        
        btnTrackOrder.setVisibility(View.GONE);
        cardOrderSummary.setVisibility(View.GONE);
    }
}
