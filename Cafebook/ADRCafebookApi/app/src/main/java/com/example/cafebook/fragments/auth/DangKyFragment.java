package com.example.cafebook.fragments.auth;

import android.os.Bundle;
import android.util.Patterns;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.example.cafebook.R;
import com.example.cafebook.models.DangKyDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.AuthApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DangKyFragment extends Fragment {

    private TextInputLayout tilPhone, tilEmail, tilPassword, tilConfirmPassword;
    private TextInputEditText edtPhone, edtEmail, edtPassword, edtConfirmPassword;
    private MaterialButton btnRegister;
    private AuthApiService apiService;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_dang_ky, container, false);

        // Ánh xạ View
        tilPhone = view.findViewById(R.id.tilPhone);
        tilEmail = view.findViewById(R.id.tilEmail);
        tilPassword = view.findViewById(R.id.tilPassword);
        tilConfirmPassword = view.findViewById(R.id.tilConfirmPassword);
        edtPhone = view.findViewById(R.id.edtPhone);
        edtEmail = view.findViewById(R.id.edtEmail);
        edtPassword = view.findViewById(R.id.edtPassword);
        edtConfirmPassword = view.findViewById(R.id.edtConfirmPassword);
        btnRegister = view.findViewById(R.id.btnRegister);
        
        view.findViewById(R.id.tvGoToLogin).setOnClickListener(v -> {
            // TODO: Navigate sang màn hình Đăng Nhập
        });

        apiService = ApiClient.getClient().create(AuthApiService.class);

        btnRegister.setOnClickListener(v -> performRegistration(view));

        return view;
    }

    private void performRegistration(View view) {
        // Reset lỗi UI
        tilPhone.setError(null); tilEmail.setError(null); 
        tilPassword.setError(null); tilConfirmPassword.setError(null);

        String phone = edtPhone.getText().toString().trim();
        String email = edtEmail.getText().toString().trim();
        String password = edtPassword.getText().toString();
        String confirmPassword = edtConfirmPassword.getText().toString();

        // 1. CLIENT-SIDE VALIDATION (Giảm tải Server)
        boolean isValid = true;

        if (!phone.matches("^(0[3|5|7|8|9])+([0-9]{8})$")) {
            tilPhone.setError("SĐT không hợp lệ (Bắt đầu 03,05,07,08,09 - Đủ 10 số)");
            isValid = false;
        }
        if (!Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
            tilEmail.setError("Định dạng Email không hợp lệ");
            isValid = false;
        }
        if (password.length() < 6) {
            tilPassword.setError("Mật khẩu phải có ít nhất 6 ký tự");
            isValid = false;
        }
        if (!password.equals(confirmPassword)) {
            tilConfirmPassword.setError("Mật khẩu xác nhận không khớp");
            isValid = false;
        }

        if (!isValid) return;

        // 2. GỌI API (Vô hiệu hóa nút bấm tránh spam)
        btnRegister.setEnabled(false);
        btnRegister.setText("ĐANG XỬ LÝ...");

        DangKyDto.Request request = new DangKyDto.Request(email, phone, password);
        apiService.register(request).enqueue(new Callback<DangKyDto.Response>() {
            @Override
            public void onResponse(@NonNull Call<DangKyDto.Response> call, @NonNull Response<DangKyDto.Response> response) {
                btnRegister.setEnabled(true);
                btnRegister.setText("ĐĂNG KÝ");

                if (response.isSuccessful() && response.body() != null) {
                    DangKyDto.Response data = response.body();

                    if (data.success && data.requireOtp) {
                        // CHUYỂN SANG MÀN HÌNH XÁC MINH OTP
                        Snackbar.make(view, data.message != null ? data.message : "Vui lòng kiểm tra Email để lấy OTP", Snackbar.LENGTH_LONG)
                                .setBackgroundTint(getResources().getColor(R.color.cf_orange)).show();
                                
                    } else if (data.isOfficialAccount) {
                        // TÀI KHOẢN ĐÃ TỒN TẠI -> Chuyển về Đăng Nhập
                        Snackbar.make(view, data.message, Snackbar.LENGTH_LONG)
                                .setBackgroundTint(getResources().getColor(R.color.cf_brown)).show();
                    } else {
                        // Lỗi logic từ Server
                        Snackbar.make(view, data.message != null ? data.message : "Lỗi đăng ký", Snackbar.LENGTH_LONG).show();
                    }
                } else {
                    Snackbar.make(view, "Lỗi server: " + response.code(), Snackbar.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<DangKyDto.Response> call, @NonNull Throwable t) {
                btnRegister.setEnabled(true);
                btnRegister.setText("ĐĂNG KÝ");
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
