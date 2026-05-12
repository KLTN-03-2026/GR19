package com.example.cafebook.fragments;

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
import com.example.cafebook.R;
import com.example.cafebook.models.ProfileDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.ProfileApiService;
import com.example.cafebook.utils.SessionManager;
import com.bumptech.glide.Glide;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import com.google.android.material.imageview.ShapeableImageView;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProfileFragment extends Fragment {

    private ProfileApiService apiService;
    private int userId;
    
    private ShapeableImageView imgAvatar;
    private TextView tvFullName, tvPoints, tvTotalSpend, tvTotalBill;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        // Trả về layout fragment_profile mới
        return inflater.inflate(R.layout.fragment_profile, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        
        SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
        userId = prefs.getInt("USER_ID", 0);

        imgAvatar = view.findViewById(R.id.imgAvatar);
        tvFullName = view.findViewById(R.id.tvFullName);
        tvPoints = view.findViewById(R.id.tvPointsBadge);
        tvTotalSpend = view.findViewById(R.id.tvTotalSpend);
        tvTotalBill = view.findViewById(R.id.tvTotalBill);

        apiService = ApiClient.getClient(requireContext()).create(ProfileApiService.class);

        // Nút Settings được quản lý bởi MainActivity Toolbar

        view.findViewById(R.id.btnHistory).setOnClickListener(v -> {
            getParentFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new OrderHistoryFragment())
                    .addToBackStack(null)
                    .commit();
        });

        view.findViewById(R.id.btnBookingHistory).setOnClickListener(v -> {
            getParentFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new BookingHistoryFragment())
                    .addToBackStack(null)
                    .commit();
        });

        view.findViewById(R.id.btnRentalHistory).setOnClickListener(v -> {
            getParentFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new LichSuThueSachFragment())
                    .addToBackStack(null)
                    .commit();
        });

        loadDashboardData();
    }

    private void loadDashboardData() {
        if (userId == 0) {
            Log.e("PROFILE_DEBUG", "User ID is 0, cannot load data");
            return;
        }

        // Kiểm tra Cache trước khi tải
        ProfileDto.Overview cachedOverview = SessionManager.getCachedOverview();
        ProfileDto.Info cachedInfo = SessionManager.getCachedInfo();

        if (cachedOverview != null) {
            bindOverview(cachedOverview);
        } else {
            fetchOverview();
        }

        if (cachedInfo != null) {
            bindInfo(cachedInfo);
        } else {
            fetchInfo();
        }
    }

    private void fetchOverview() {
        apiService.getOverview(userId).enqueue(new Callback<ProfileDto.Overview>() {
            @Override
            public void onResponse(@NonNull Call<ProfileDto.Overview> call, @NonNull Response<ProfileDto.Overview> response) {
                if (response.isSuccessful() && response.body() != null) {
                    SessionManager.setCachedOverview(response.body());
                    bindOverview(response.body());
                }
            }
            @Override public void onFailure(@NonNull Call<ProfileDto.Overview> call, @NonNull Throwable t) {}
        });
    }

    private void fetchInfo() {
        apiService.getPersonalInfo(userId).enqueue(new Callback<ProfileDto.Info>() {
            @Override
            public void onResponse(@NonNull Call<ProfileDto.Info> call, @NonNull Response<ProfileDto.Info> response) {
                if (response.isSuccessful() && response.body() != null) {
                    SessionManager.setCachedInfo(response.body());
                    bindInfo(response.body());
                }
            }
            @Override public void onFailure(@NonNull Call<ProfileDto.Info> call, @NonNull Throwable t) {}
        });
    }

    private void bindOverview(ProfileDto.Overview ov) {
        tvPoints.setText(ov.diemTichLuy + " Điểm");
        tvTotalSpend.setText(String.format("%,.0fđ", ov.tongChiTieu));
        tvTotalBill.setText(ov.tongHoaDon + " đơn");
    }

    private void bindInfo(ProfileDto.Info info) {
        tvFullName.setText(info.hoTen);
        if (getContext() != null) {
            Glide.with(getContext())
                 .load((info.anhDaiDienUrl != null && !info.anhDaiDienUrl.isEmpty()) ? info.anhDaiDienUrl : null)
                 .placeholder(R.drawable.default_avatar)
                 .error(R.drawable.default_avatar)
                 .circleCrop() // Tối ưu avatar tròn
                 .into(imgAvatar);
        }
    }
}
