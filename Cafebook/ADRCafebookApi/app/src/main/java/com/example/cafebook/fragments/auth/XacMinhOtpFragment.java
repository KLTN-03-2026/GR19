package com.example.cafebook.fragments.auth;

import android.os.Bundle;
import android.os.CountDownTimer;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.example.cafebook.R;
import com.example.cafebook.models.DangKyDto;
import com.example.cafebook.models.XacMinhOtpDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.AuthApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class XacMinhOtpFragment extends Fragment {

    private TextInputEditText edtOtpCode;
    private MaterialButton btnVerify, btnResend;
    private TextView tvCountdown;
    private AuthApiService apiService;
    private CountDownTimer countDownTimer;

    // Các biến nhận từ màn hình Đăng ký
    private String email;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_xac_minh_otp, container, false);

        // Nhận dữ liệu truyền sang
        if (getArguments() != null) {
            email = getArguments().getString("TempEmail");
        }

        edtOtpCode = view.findViewById(R.id.edtOtpCode);
        btnVerify = view.findViewById(R.id.btnVerify);
        btnResend = view.findViewById(R.id.btnResend);
        tvCountdown = view.findViewById(R.id.tvCountdown);

        apiService = ApiClient.getClient(requireContext()).create(AuthApiService.class);

        startCountdown();

        btnVerify.setOnClickListener(v -> handleVerify());
        btnResend.setOnClickListener(v -> {
            startCountdown();
            Toast.makeText(getContext(), "Yêu cầu gửi lại mã thành công!", Toast.LENGTH_SHORT).show();
        });

        return view;
    }

    private void startCountdown() {
        btnResend.setEnabled(false);
        countDownTimer = new CountDownTimer(60000, 1000) {
            public void onTick(long millisUntilFinished) {
                tvCountdown.setText("Gửi lại mã sau (" + millisUntilFinished / 1000 + "s)");
            }
            public void onFinish() {
                tvCountdown.setText("Bạn có thể gửi lại mã ngay bây giờ");
                btnResend.setEnabled(true);
            }
        }.start();
    }

    private void handleVerify() {
        String otp = edtOtpCode.getText().toString().trim();
        if (otp.length() < 6) {
            edtOtpCode.setError("Vui lòng nhập đủ 6 số");
            return;
        }

        btnVerify.setEnabled(false);
        btnVerify.setText("ĐANG XÁC THỰC...");

        // Tối ưu API: Server đã lưu dữ liệu tạm trong Cache, chỉ cần gửi Email và OTP
        XacMinhOtpDto.Request request = new XacMinhOtpDto.Request(email, otp);
        apiService.verifyOtp(request).enqueue(new Callback<DangKyDto.Response>() {
            @Override
            public void onResponse(@NonNull Call<DangKyDto.Response> call, @NonNull Response<DangKyDto.Response> response) {
                btnVerify.setEnabled(true);
                btnVerify.setText("XÁC NHẬN KÍCH HOẠT");

                if (response.isSuccessful() && response.body() != null) {
                    if (response.body().success) {
                        Toast.makeText(getContext(), "Kích hoạt thành công! Mời bạn đăng nhập.", Toast.LENGTH_LONG).show();
                        // Chuyển sang màn hình Đăng nhập
                        getParentFragmentManager().beginTransaction()
                                .replace(R.id.fragment_container, new DangNhapFragment())
                                .commit();
                    } else {
                        Toast.makeText(getContext(), response.body().message, Toast.LENGTH_SHORT).show();
                    }
                } else {
                    try {
                        String realError = response.errorBody() != null ? response.errorBody().string() : "Lỗi không xác định";
                        Log.e("API_ERROR", "Mã lỗi: " + response.code() + " | Chi tiết: " + realError);
                        Toast.makeText(getContext(), "Lỗi server " + response.code() + ": Kiểm tra Logcat", Toast.LENGTH_LONG).show();
                    } catch (Exception e) {
                        e.printStackTrace();
                        Toast.makeText(getContext(), "Lỗi server: " + response.code(), Toast.LENGTH_SHORT).show();
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<DangKyDto.Response> call, @NonNull Throwable t) {
                btnVerify.setEnabled(true);
                btnVerify.setText("XÁC NHẬN KÍCH HOẠT");
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (countDownTimer != null) countDownTimer.cancel();
    }
}
