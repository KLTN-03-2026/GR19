package com.example.cafebook.fragments.auth;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.example.cafebook.MainActivity;
import com.example.cafebook.R;
import com.example.cafebook.models.DangNhapDto;
import com.example.cafebook.fragments.auth.QuenMatKhauFragment;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.AuthApiService;
import com.example.cafebook.utils.SessionManager;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DangNhapFragment extends Fragment {

    private TextInputLayout tilUsername, tilPassword;
    private TextInputEditText edtUsername, edtPassword;
    private MaterialButton btnLogin;
    private AuthApiService apiService;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_dang_nhap, container, false);

        // Ánh xạ View
        tilUsername = view.findViewById(R.id.tilUsername);
        tilPassword = view.findViewById(R.id.tilPassword);
        edtUsername = view.findViewById(R.id.edtUsername);
        edtPassword = view.findViewById(R.id.edtPassword);
        btnLogin = view.findViewById(R.id.btnLogin);

        apiService = ApiClient.getClient(requireContext()).create(AuthApiService.class);

        // Nút Về trang chủ: Chuyển tab BottomNav về Home
        View btnBack = view.findViewById(R.id.btnBack);
        if (btnBack != null) {
            btnBack.setOnClickListener(v -> {
                if (getActivity() != null) {
                    BottomNavigationView bottomNav = getActivity().findViewById(R.id.bottomNavigation);
                    if (bottomNav != null) {
                        bottomNav.setSelectedItemId(R.id.nav_home);
                    }
                }
            });
        }

        view.findViewById(R.id.tvGoToRegister).setOnClickListener(v -> {
            getParentFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new DangKyFragment())
                    .addToBackStack(null)
                    .commit();
        });

        view.findViewById(R.id.tvForgotPassword).setOnClickListener(v -> {
            getParentFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new QuenMatKhauFragment())
                    .addToBackStack(null)
                    .commit();
        });

        // Xử lý nút Đăng Nhập
        btnLogin.setOnClickListener(v -> performLogin(view));

        return view;
    }

    private void performLogin(View view) {
        tilUsername.setError(null);
        tilPassword.setError(null);

        String username = edtUsername.getText().toString().trim();
        String password = edtPassword.getText().toString().trim();

        // 1. Client-Side Validation
        if (username.isEmpty()) {
            tilUsername.setError("Vui lòng nhập tài khoản");
            return;
        }
        if (password.isEmpty()) {
            tilPassword.setError("Vui lòng nhập mật khẩu");
            return;
        }

        // 2. Chặn tương tác khi đang xử lý
        btnLogin.setEnabled(false);
        btnLogin.setText("ĐANG ĐĂNG NHẬP...");

        DangNhapDto.Request request = new DangNhapDto.Request(username, password);
        
        apiService.login(request).enqueue(new Callback<DangNhapDto.Response>() {
            @Override
            public void onResponse(Call<DangNhapDto.Response> call, Response<DangNhapDto.Response> response) {
                btnLogin.setEnabled(true);
                btnLogin.setText("ĐĂNG NHẬP");

                if (response.isSuccessful() && response.body() != null) {
                    DangNhapDto.Response data = response.body();
                    
                    // LOG RAW JSON FOR DEBUGGING
                    Log.d("API_DEBUG", "Raw JSON: " + new com.google.gson.Gson().toJson(data));

                    if (data.success && data.khachHangData != null && data.token != null) {
                        // 3. Đăng nhập thành công -> Lưu Token & User Info
                        saveUserSession(data.token, data.khachHangData);

                        Toast.makeText(getContext(), "Đăng nhập thành công!", Toast.LENGTH_SHORT).show();
                        
                        // Reset ApiClient để Interceptor lấy được Token mới
                        com.example.cafebook.network.ApiClient.reset();

                        // Chuyển sang ProfileFragment
                        getParentFragmentManager().beginTransaction()
                                .replace(R.id.fragment_container, new com.example.cafebook.fragments.ProfileFragment())
                                .commit();
                        
                    } else {
                        // Hiển thị lỗi từ Server
                        Snackbar.make(view, data.message != null ? data.message : "Đăng nhập thất bại", Snackbar.LENGTH_LONG)
                                .setBackgroundTint(getResources().getColor(R.color.cf_orange))
                                .show();
                    }
                } else {
                    try {
                        String realError = response.errorBody() != null ? response.errorBody().string() : "Lỗi không xác định";
                        Log.e("API_ERROR", "Mã lỗi: " + response.code() + " | Chi tiết: " + realError);
                        Snackbar.make(view, "Lỗi server " + response.code() + ": Kiểm tra Logcat", Snackbar.LENGTH_LONG)
                                .setBackgroundTint(getResources().getColor(R.color.cf_orange))
                                .show();
                    } catch (Exception e) {
                        e.printStackTrace();
                        Snackbar.make(view, "Lỗi phản hồi từ server", Snackbar.LENGTH_LONG)
                                .setBackgroundTint(getResources().getColor(R.color.cf_orange))
                                .show();
                    }
                }
            }

            @Override
            public void onFailure(Call<DangNhapDto.Response> call, Throwable t) {
                btnLogin.setEnabled(true);
                btnLogin.setText("ĐĂNG NHẬP");
                Snackbar.make(view, "Lỗi kết nối đến máy chủ. Vui lòng thử lại.", Snackbar.LENGTH_SHORT)
                        .setBackgroundTint(getResources().getColor(R.color.cf_dark_brown))
                        .show();
            }
        });
    }

    private void saveUserSession(String token, DangNhapDto.KhachHangData user) {
        // Clear session cũ trước khi lưu session mới
        SessionManager.clearSession();

        SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = prefs.edit();
        
        editor.putString("JWT_TOKEN", token);
        editor.putInt("USER_ID", user.idKhachHang);
        editor.putString("USER_NAME", user.hoTen);
        editor.putString("USER_EMAIL", user.email);
        editor.putString("USER_PHONE", user.soDienThoai);
        editor.putString("USER_AVATAR", user.anhDaiDienUrl);
        
        editor.apply();
    }
}
