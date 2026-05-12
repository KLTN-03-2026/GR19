package com.example.cafebook.fragments.auth;

import android.os.Bundle;
import android.transition.TransitionManager;
import android.util.Patterns;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.example.cafebook.R;
import com.example.cafebook.models.QuenMatKhauDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.QuenMatKhauApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.material.textfield.TextInputEditText;
import java.util.Random;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class QuenMatKhauFragment extends Fragment {

    private LinearLayout llStep1Email, llStep2Otp, llStep3Reset;
    private TextInputEditText edtEmail, edtOtp, edtNewPassword, edtConfirmPassword;
    private MaterialButton btnSendCode, btnVerifyOtp, btnResetPassword;
    private TextView tvOtpDesc;

    private QuenMatKhauApiService apiService;
    private String generatedOtp = ""; // Lưu mã OTP để đối chiếu nội bộ
    private String currentEmail = "";

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_quen_mat_khau, container, false);

        apiService = ApiClient.getClient().create(QuenMatKhauApiService.class);

        // Ánh xạ
        llStep1Email = view.findViewById(R.id.llStep1Email);
        llStep2Otp = view.findViewById(R.id.llStep2Otp);
        llStep3Reset = view.findViewById(R.id.llStep3Reset);
        
        edtEmail = view.findViewById(R.id.edtEmail);
        edtOtp = view.findViewById(R.id.edtOtp);
        edtNewPassword = view.findViewById(R.id.edtNewPassword);
        edtConfirmPassword = view.findViewById(R.id.edtConfirmPassword);
        
        btnSendCode = view.findViewById(R.id.btnSendCode);
        btnVerifyOtp = view.findViewById(R.id.btnVerifyOtp);
        btnResetPassword = view.findViewById(R.id.btnResetPassword);
        tvOtpDesc = view.findViewById(R.id.tvOtpDesc);

        view.findViewById(R.id.btnBackToLogin).setOnClickListener(v -> {
            if (getActivity() != null) {
                getActivity().onBackPressed();
            }
        });

        btnSendCode.setOnClickListener(v -> handleSendCode(view));
        btnVerifyOtp.setOnClickListener(v -> handleVerifyOtp(view));
        btnResetPassword.setOnClickListener(v -> handleResetPassword(view));

        return view;
    }

    // BƯỚC 1: GỬI MAIL OTP
    private void handleSendCode(View view) {
        currentEmail = edtEmail.getText().toString().trim();
        if (currentEmail.isEmpty()) {
            edtEmail.setError("Vui lòng nhập Email");
            return;
        }
        if (!Patterns.EMAIL_ADDRESS.matcher(currentEmail).matches()) {
            edtEmail.setError("Email không hợp lệ");
            return;
        }

        btnSendCode.setEnabled(false);
        btnSendCode.setText("ĐANG GỬI...");

        // App tự sinh mã OTP 6 số ngẫu nhiên
        generatedOtp = String.format("%06d", new Random().nextInt(999999));
        
        QuenMatKhauDto.GuiMaRequest request = new QuenMatKhauDto.GuiMaRequest(currentEmail, generatedOtp);
        apiService.guiMaXacNhan(request).enqueue(new Callback<QuenMatKhauDto.Response>() {
            @Override
            public void onResponse(@NonNull Call<QuenMatKhauDto.Response> call, @NonNull Response<QuenMatKhauDto.Response> response) {
                btnSendCode.setEnabled(true);
                btnSendCode.setText("GỬI MÃ XÁC NHẬN");

                if (response.isSuccessful() && response.body() != null) {
                    if (response.body().success) {
                        // Chuyển sang Bước 2 mượt mà
                        TransitionManager.beginDelayedTransition((ViewGroup) view);
                        llStep1Email.setVisibility(View.GONE);
                        llStep2Otp.setVisibility(View.VISIBLE);
                        tvOtpDesc.setText("Nhập mã 6 số đã được gửi tới " + currentEmail);
                    } else {
                        showSnackbar(view, response.body().message, R.color.cf_orange);
                    }
                } else {
                    showSnackbar(view, "Lỗi server: " + response.code(), R.color.cf_orange);
                }
            }

            @Override
            public void onFailure(@NonNull Call<QuenMatKhauDto.Response> call, @NonNull Throwable t) {
                btnSendCode.setEnabled(true);
                btnSendCode.setText("GỬI MÃ XÁC NHẬN");
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }

    // BƯỚC 2: KIỂM TRA OTP TẠI APP
    private void handleVerifyOtp(View view) {
        String inputOtp = edtOtp.getText().toString().trim();
        if (inputOtp.isEmpty() || inputOtp.length() < 6) {
            edtOtp.setError("Vui lòng nhập đủ 6 số");
            return;
        }

        if (inputOtp.equals(generatedOtp)) {
            // Chuyển sang Bước 3
            TransitionManager.beginDelayedTransition((ViewGroup) view);
            llStep2Otp.setVisibility(View.GONE);
            llStep3Reset.setVisibility(View.VISIBLE);
        } else {
            showSnackbar(view, "Mã xác nhận không chính xác", R.color.cf_orange);
        }
    }

    // BƯỚC 3: ĐẶT LẠI MẬT KHẨU
    private void handleResetPassword(View view) {
        String newPass = edtNewPassword.getText().toString();
        String confirmPass = edtConfirmPassword.getText().toString();

        if (newPass.length() < 6) {
            edtNewPassword.setError("Mật khẩu phải từ 6 ký tự");
            return;
        }
        if (!newPass.equals(confirmPass)) {
            edtConfirmPassword.setError("Mật khẩu xác nhận không khớp");
            return;
        }

        btnResetPassword.setEnabled(false);
        btnResetPassword.setText("ĐANG XỬ LÝ...");

        QuenMatKhauDto.ResetRequest request = new QuenMatKhauDto.ResetRequest(currentEmail, newPass);
        apiService.resetPassword(request).enqueue(new Callback<QuenMatKhauDto.Response>() {
            @Override
            public void onResponse(@NonNull Call<QuenMatKhauDto.Response> call, @NonNull Response<QuenMatKhauDto.Response> response) {
                btnResetPassword.setEnabled(true);
                btnResetPassword.setText("LƯU MẬT KHẨU MỚI");

                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    Toast.makeText(getContext(), "Cập nhật mật khẩu thành công!", Toast.LENGTH_LONG).show();
                    // Chuyển về màn hình đăng nhập
                    if (getActivity() != null) {
                        getActivity().onBackPressed();
                    }
                } else {
                    showSnackbar(view, "Đã có lỗi xảy ra. Vui lòng thử lại", R.color.cf_orange);
                }
            }
            @Override
            public void onFailure(@NonNull Call<QuenMatKhauDto.Response> call, @NonNull Throwable t) {
                btnResetPassword.setEnabled(true);
                btnResetPassword.setText("LƯU MẬT KHẨU MỚI");
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showSnackbar(View view, String msg, int colorResId) {
        if (msg == null) msg = "Đã có lỗi xảy ra";
        Snackbar.make(view, msg, Snackbar.LENGTH_LONG)
                .setBackgroundTint(getResources().getColor(colorResId))
                .show();
    }
}
