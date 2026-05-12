package com.example.cafebook.fragments;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.example.cafebook.R;
import com.example.cafebook.models.ProfileDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.ProfileApiService;
import com.bumptech.glide.Glide;
import com.google.android.material.imageview.ShapeableImageView;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProfileFragment extends Fragment {

    private ProfileApiService apiService;
    private String token;
    private int userId;
    
    private ShapeableImageView imgAvatar;
    private TextView tvFullName, tvPoints, tvTotalSpend, tvTotalBill;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_profile, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        
        // 1. Lấy thông tin phiên đăng nhập
        SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
        token = "Bearer " + prefs.getString("JWT_TOKEN", "");
        userId = prefs.getInt("USER_ID", 0);

        // 2. Khởi tạo UI
        imgAvatar = view.findViewById(R.id.imgAvatar);
        tvFullName = view.findViewById(R.id.tvFullName);
        tvPoints = view.findViewById(R.id.tvPointsBadge);
        tvTotalSpend = view.findViewById(R.id.tvTotalSpend);
        tvTotalBill = view.findViewById(R.id.tvTotalBill);

        view.findViewById(R.id.btnLogout).setOnClickListener(v -> handleLogout());

        apiService = ApiClient.getClient().create(ProfileApiService.class);

        loadDashboardData();
    }

    private void loadDashboardData() {
        if (userId == 0) return;

        // Tối ưu Server: Gọi song song thông tin tổng quan
        apiService.getOverview(token, userId).enqueue(new Callback<ProfileDto.Overview>() {
            @Override
            public void onResponse(@NonNull Call<ProfileDto.Overview> call, @NonNull Response<ProfileDto.Overview> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ProfileDto.Overview ov = response.body();
                    tvPoints.setText(ov.diemTichLuy + " Điểm");
                    tvTotalSpend.setText(String.format("%,.0fđ", ov.tongChiTieu));
                    tvTotalBill.setText(ov.tongHoaDon + " đơn");
                }
            }
            @Override public void onFailure(@NonNull Call<ProfileDto.Overview> call, @NonNull Throwable t) {}
        });

        apiService.getPersonalInfo(token).enqueue(new Callback<ProfileDto.Info>() {
            @Override
            public void onResponse(@NonNull Call<ProfileDto.Info> call, @NonNull Response<ProfileDto.Info> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ProfileDto.Info info = response.body();
                    tvFullName.setText(info.hoTen);
                    
                    // Dùng Glide để load avatar bypass cache
                    if (getContext() != null) {
                        String avatarUrl = info.anhDaiDienUrl;
                        if (avatarUrl != null && !avatarUrl.isEmpty()) {
                            Glide.with(getContext())
                                 .load(avatarUrl + "?v=" + System.currentTimeMillis())
                                 .placeholder(R.drawable.ic_person)
                                 .into(imgAvatar);
                        }
                    }
                }
            }
            @Override public void onFailure(@NonNull Call<ProfileDto.Info> call, @NonNull Throwable t) {}
        });
    }

    private void handleLogout() {
        SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
        prefs.edit().clear().apply();
        
        Toast.makeText(getContext(), "Đã đăng xuất", Toast.LENGTH_SHORT).show();
        
        // Reload main activity or navigate to login
        if (getActivity() instanceof com.example.cafebook.MainActivity) {
            ((com.example.cafebook.MainActivity) getActivity()).loadFragment(new com.example.cafebook.fragments.auth.DangNhapFragment());
        }
    }
}
