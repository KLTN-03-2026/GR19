package com.example.cafebook.fragments;

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
import com.example.cafebook.models.ChinhSachDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.text.DecimalFormat;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ChinhSachFragment extends Fragment {

    private TextView tvHeaderTitle, tvGioiThieu, tvChinhSachDiem;
    private CafebookApi api;
    
    private static ChinhSachDto cachedData = null; 

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_chinh_sach, container, false);

        tvHeaderTitle = view.findViewById(R.id.tvHeaderTitle);
        tvGioiThieu = view.findViewById(R.id.tvGioiThieu);
        tvChinhSachDiem = view.findViewById(R.id.tvChinhSachDiem);

        api = ApiClient.getClient().create(CafebookApi.class);

        if (cachedData != null) {
            bindDataToUI(cachedData);
        } else {
            fetchDataFromServer();
        }

        return view;
    }

    private void fetchDataFromServer() {
        api.getChinhSachData().enqueue(new Callback<ChinhSachDto>() {
            @Override
            public void onResponse(@NonNull Call<ChinhSachDto> call, @NonNull Response<ChinhSachDto> response) {
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    cachedData = response.body();
                    bindDataToUI(cachedData);
                }
            }

            @Override
            public void onFailure(@NonNull Call<ChinhSachDto> call, @NonNull Throwable t) {
                if (isAdded()) {
                    Toast.makeText(getContext(), "Không thể tải chính sách", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    private void bindDataToUI(ChinhSachDto data) {
        DecimalFormat formatter = new DecimalFormat("#,###");

        tvHeaderTitle.setText("Về " + (data.tenQuan != null ? data.tenQuan : "Chúng Tôi"));
        
        String gioiThieuStr = data.gioiThieu != null ? data.gioiThieu : "Đang cập nhật giới thiệu...";
        tvGioiThieu.setText(gioiThieuStr);

        String chinhSachDiemStr = "• Với mỗi " + formatter.format(data.diemNhanVND) + "đ, bạn tích được 1 điểm.\n" +
                                  "• 1 điểm quy đổi được " + formatter.format(data.diemDoiVND) + "đ để giảm giá.\n" +
                                  "• Mượn sách thành công nhận ngay " + data.diemPhieuThue + " điểm.";
        tvChinhSachDiem.setText(chinhSachDiemStr);
    }
}
