package com.example.cafebook.fragments;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.resource.drawable.DrawableTransitionOptions;
import com.example.cafebook.R;
import com.example.cafebook.adapters.SachAdapter;
import com.example.cafebook.adapters.SanPhamAdapter;
import com.example.cafebook.models.ThongTinChungDto;
import com.example.cafebook.models.TrangChuDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class HomeFragment extends Fragment {

    private ImageView imgBanner;
    private TextView tvTenQuan, tvGioiThieu, tvDiaChi, tvDienThoai, tvGioMoCua, tvSoBanTrong, tvSoSach;
    private View infoCard;
    private RecyclerView rvMonNoiBat, rvSachNoiBat;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);
        initViews(view);
        fetchTrangChuData();
        return view;
    }

    private void initViews(View view) {
        imgBanner = view.findViewById(R.id.imgBanner);
        tvTenQuan = view.findViewById(R.id.tvTenQuan);
        tvGioiThieu = view.findViewById(R.id.tvGioiThieu);
        tvDiaChi = view.findViewById(R.id.tvDiaChi);
        tvDienThoai = view.findViewById(R.id.tvDienThoai);
        tvGioMoCua = view.findViewById(R.id.tvGioMoCua);
        tvSoBanTrong = view.findViewById(R.id.tvSoBanTrong);
        tvSoSach = view.findViewById(R.id.tvSoSach);
        infoCard = view.findViewById(R.id.cardInfo);
        rvMonNoiBat = view.findViewById(R.id.rvMonNoiBat);
        rvSachNoiBat = view.findViewById(R.id.rvSachNoiBat);

        // Setup RecyclerViews for horizontal layout
        rvMonNoiBat.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));
        rvSachNoiBat.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));

        // Initial animation state
        tvTenQuan.setAlpha(0);
        tvGioiThieu.setAlpha(0);
        infoCard.setAlpha(0);
    }

    private void fetchTrangChuData() {
        CafebookApi api = ApiClient.getClient().create(CafebookApi.class);
        api.getTrangChuData().enqueue(new Callback<TrangChuDto>() {
            @Override
            public void onResponse(@NonNull Call<TrangChuDto> call, @NonNull Response<TrangChuDto> response) {
                if (response.isSuccessful() && response.body() != null) {
                    updateUI(response.body());
                } else if (isAdded()) {
                    Toast.makeText(getContext(), "Lỗi server: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<TrangChuDto> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Connection failed", t);
                if (isAdded()) {
                    Toast.makeText(getContext(), "Không thể kết nối đến máy chủ", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    private void updateUI(TrangChuDto data) {
        if (!isAdded()) return;

        ThongTinChungDto info = data.getInfo();
        if (info != null) {
            if (info.getBannerImageUrl() != null) {
                Glide.with(this)
                        .load(info.getBannerImageUrl())
                        .transition(DrawableTransitionOptions.withCrossFade())
                        .into(imgBanner);
            }

            tvTenQuan.setText(info.getTenQuan() != null ? info.getTenQuan() : "Cafebook");
            tvGioiThieu.setText(info.getGioiThieu() != null ? info.getGioiThieu() : "");
            tvDiaChi.setText(info.getDiaChi() != null ? info.getDiaChi() : "Chưa cập nhật địa chỉ");
            tvDienThoai.setText(info.getSoDienThoai() != null ? info.getSoDienThoai() : "Chưa cập nhật hotline");

            String time = String.format("Mở cửa: %s - %s (%s)", 
                info.getGioMoCua() != null ? info.getGioMoCua() : "--:--",
                info.getGioDongCua() != null ? info.getGioDongCua() : "--:--",
                info.getThuMoCua() != null ? info.getThuMoCua() : "Hàng ngày");
            tvGioMoCua.setText(time);

            tvSoBanTrong.setText(String.valueOf(info.getSoBanTrong()));
            tvSoSach.setText(String.valueOf(info.getSoSachSanSang()));

            fadeIn(tvTenQuan, 500);
            fadeIn(tvGioiThieu, 700);
            fadeIn(infoCard, 900);
        }

        // Setup adapters for RecyclerViews
        if (data.getMonNoiBat() != null) {
            SanPhamAdapter spAdapter = new SanPhamAdapter(data.getMonNoiBat());
            rvMonNoiBat.setAdapter(spAdapter);
        }

        if (data.getSachNoiBat() != null) {
            SachAdapter sachAdapter = new SachAdapter(data.getSachNoiBat());
            rvSachNoiBat.setAdapter(sachAdapter);
        }
    }

    private void fadeIn(View view, int duration) {
        view.animate().alpha(1f).setDuration(duration).start();
    }
}
