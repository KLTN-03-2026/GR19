package com.example.cafebook.fragments;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.fragment.app.Fragment;

import com.example.cafebook.R;
import com.example.cafebook.models.ChinhSachDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ChinhSachFragment extends Fragment {

    private TextView tvAboutContent, tvPolicyAddress, tvPolicyPhone, tvPolicyEmail, tvPolicyHours;
    private TextView tvPolicyRent, tvPolicyPoints;
    private Toolbar policyToolbar;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_chinh_sach, container, false);
        initViews(view);
        loadData();
        return view;
    }

    private void initViews(View view) {
        tvAboutContent = view.findViewById(R.id.tvAboutContent);
        tvPolicyAddress = view.findViewById(R.id.tvPolicyAddress);
        tvPolicyPhone = view.findViewById(R.id.tvPolicyPhone);
        tvPolicyEmail = view.findViewById(R.id.tvPolicyEmail);
        tvPolicyHours = view.findViewById(R.id.tvPolicyHours);
        tvPolicyRent = view.findViewById(R.id.tvPolicyRent);
        tvPolicyPoints = view.findViewById(R.id.tvPolicyPoints);
        policyToolbar = view.findViewById(R.id.policyToolbar);

        policyToolbar.setNavigationOnClickListener(v -> {
            if (getActivity() != null) getActivity().onBackPressed();
        });
    }

    private void loadData() {
        CafebookApi api = ApiClient.getClient(requireContext()).create(CafebookApi.class);
        api.getChinhSachData().enqueue(new Callback<ChinhSachDto>() {
            @Override
            public void onResponse(@NonNull Call<ChinhSachDto> call, @NonNull Response<ChinhSachDto> response) {
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    ChinhSachDto p = response.body();
                    
                    if (p.gioiThieu != null) tvAboutContent.setText(p.gioiThieu);
                    tvPolicyAddress.setText("• Địa chỉ: " + (p.diaChi != null ? p.diaChi : "--"));
                    tvPolicyPhone.setText("• Hotline: " + (p.soDienThoai != null ? p.soDienThoai : "--"));
                    tvPolicyEmail.setText("• Email: " + (p.email != null ? p.email : "--"));
                    tvPolicyHours.setText(String.format("• Giờ mở cửa: %s - %s (%s)", 
                        p.gioMoCua, p.gioDongCua, p.thuMoCua));

                    String rentText = String.format(Locale.getDefault(),
                        "• Phí dịch vụ: %.0fđ/lượt.\n• Thời gian thuê tối đa: %s ngày.\n• Phí phạt trễ hạn: %.0fđ/ngày.\n• Tích lũy %s điểm thưởng mỗi lượt thuê.\n• Phụ thu hư hại dựa trên %% độ mới hao hụt.",
                        p.phiThue, p.soNgayMuonToiDa, p.phiTraTreMoiNgay, p.diemPhieuThue);
                    tvPolicyRent.setText(rentText);

                    String pointText = String.format(Locale.getDefault(),
                        "• Tích 1 điểm cho mỗi 50.000đ hóa đơn.\n• 1 điểm = %.0fđ khi thanh toán (tối đa 50%% hóa đơn).\n• Không áp dụng tích điểm khi dùng điểm thanh toán.",
                        p.diemDoiVND);
                    tvPolicyPoints.setText(pointText);
                }
            }

            @Override
            public void onFailure(@NonNull Call<ChinhSachDto> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Policy failed", t);
            }
        });
    }
}
