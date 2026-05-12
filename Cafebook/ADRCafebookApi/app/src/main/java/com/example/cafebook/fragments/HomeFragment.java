package com.example.cafebook.fragments;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.AnimationUtils;
import android.view.animation.LayoutAnimationController;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.resource.drawable.DrawableTransitionOptions;
import com.example.cafebook.CartActivity;
import com.example.cafebook.MainActivity;
import com.example.cafebook.R;
import com.example.cafebook.SearchActivity;
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
    private SwipeRefreshLayout swipeRefresh;
    
    // Simple Cache to reduce server load
    private static TrangChuDto cachedData = null;
    private static long lastFetchTime = 0;
    private static final long CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

    private Call<TrangChuDto> currentCall;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);
        initViews(view);
        setupSwipeRefresh();
        
        if (shouldFetchNewData()) {
            fetchTrangChuData(false);
        } else {
            updateUI(cachedData);
        }
        
        return view;
    }

    private boolean shouldFetchNewData() {
        return cachedData == null || (System.currentTimeMillis() - lastFetchTime > CACHE_DURATION);
    }

    private void initViews(View view) {
        imgBanner = view.findViewById(R.id.imgBanner);
        tvTenQuan = view.findViewById(R.id.tvTenQuan);
        tvGioiThieu = view.findViewById(R.id.tvGioiThieu);
        
        // Find views from included layouts
        View layoutDiaChi = view.findViewById(R.id.layoutDiaChi);
        tvDiaChi = layoutDiaChi.findViewById(R.id.tvValue);
        ((ImageView)layoutDiaChi.findViewById(R.id.ivIcon)).setImageResource(android.R.drawable.ic_menu_mylocation);

        View layoutDienThoai = view.findViewById(R.id.layoutDienThoai);
        tvDienThoai = layoutDienThoai.findViewById(R.id.tvValue);
        ((ImageView)layoutDienThoai.findViewById(R.id.ivIcon)).setImageResource(android.R.drawable.ic_menu_call);

        View layoutGioMoCua = view.findViewById(R.id.layoutGioMoCua);
        tvGioMoCua = layoutGioMoCua.findViewById(R.id.tvValue);
        ((ImageView)layoutGioMoCua.findViewById(R.id.ivIcon)).setImageResource(android.R.drawable.ic_menu_recent_history);

        tvSoBanTrong = view.findViewById(R.id.tvSoBanTrong);
        tvSoSach = view.findViewById(R.id.tvSoSach);
        infoCard = view.findViewById(R.id.cardInfo);
        rvMonNoiBat = view.findViewById(R.id.rvMonNoiBat);
        rvSachNoiBat = view.findViewById(R.id.rvSachNoiBat);
        swipeRefresh = view.findViewById(R.id.swipeRefresh);

        // Setup RecyclerViews
        rvMonNoiBat.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));
        rvSachNoiBat.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));

        // Setup Search Trigger
        view.findViewById(R.id.btnOpenSearch).setOnClickListener(v -> {
            v.animate().scaleX(0.95f).scaleY(0.95f).setDuration(100).withEndAction(() -> {
                v.animate().scaleX(1f).scaleY(1f).setDuration(100).start();
                Intent intent = new Intent(getActivity(), SearchActivity.class);
                startActivity(intent);
                if (getActivity() != null) {
                    getActivity().overridePendingTransition(android.R.anim.fade_in, android.R.anim.fade_out);
                }
            }).start();
        });

        // Initial state for animations
        infoCard.setAlpha(0);
        infoCard.setTranslationY(50);
    }


    private void setupSwipeRefresh() {
        swipeRefresh.setColorSchemeResources(R.color.cafe_orange, R.color.cafe_brown);
        swipeRefresh.setOnRefreshListener(() -> fetchTrangChuData(true));
    }

    private void fetchTrangChuData(boolean isRefresh) {
        if (currentCall != null) currentCall.cancel();
        
        if (!isRefresh) swipeRefresh.setRefreshing(true);

        CafebookApi api = ApiClient.getClient(requireContext()).create(CafebookApi.class);
        currentCall = api.getTrangChuData();
        currentCall.enqueue(new Callback<TrangChuDto>() {
            @Override
            public void onResponse(@NonNull Call<TrangChuDto> call, @NonNull Response<TrangChuDto> response) {
                swipeRefresh.setRefreshing(false);
                if (response.isSuccessful() && response.body() != null) {
                    cachedData = response.body();
                    lastFetchTime = System.currentTimeMillis();
                    updateUI(cachedData);
                } else if (isAdded()) {
                    Toast.makeText(getContext(), "Lỗi server: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<TrangChuDto> call, @NonNull Throwable t) {
                swipeRefresh.setRefreshing(false);
                if (call.isCanceled()) return;
                Log.e("API_ERROR", "Connection failed", t);
                if (isAdded()) {
                    Toast.makeText(getContext(), "Không thể kết nối đến máy chủ", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    private void updateUI(TrangChuDto data) {
        if (!isAdded() || data == null) return;

        ThongTinChungDto info = data.getInfo();
        if (info != null) {
            if (info.getBannerImageUrl() != null) {
                Glide.with(this)
                        .load(info.getBannerImageUrl())
                        .transition(DrawableTransitionOptions.withCrossFade())
                        .into(imgBanner);
            }

            tvTenQuan.setText(info.getTenQuan() != null ? info.getTenQuan() : "Cafebook");
            tvGioiThieu.setText(info.getGioiThieu() != null ? info.getGioiThieu() : "Cà phê & Sách");
            tvDiaChi.setText(info.getDiaChi() != null ? info.getDiaChi() : "Chưa cập nhật địa chỉ");
            tvDienThoai.setText(info.getSoDienThoai() != null ? info.getSoDienThoai() : "Chưa cập nhật hotline");

            String time = String.format("%s - %s (%s)", 
                info.getGioMoCua() != null ? info.getGioMoCua() : "--:--",
                info.getGioDongCua() != null ? info.getGioDongCua() : "--:--",
                info.getThuMoCua() != null ? info.getThuMoCua() : "Hàng ngày");
            tvGioMoCua.setText(time);

            tvSoBanTrong.setText(String.valueOf(info.getSoBanTrong()));
            tvSoSach.setText(String.valueOf(info.getSoSachSanSang()));

            // Card entrance animation
            infoCard.animate().alpha(1f).translationY(0).setDuration(600).start();
        }

        // Setup adapters for RecyclerViews
        if (data.getMonNoiBat() != null) {
            SanPhamAdapter spAdapter = new SanPhamAdapter(data.getMonNoiBat());
            rvMonNoiBat.setAdapter(spAdapter);
            runLayoutAnimation(rvMonNoiBat);
        }

        if (data.getSachNoiBat() != null) {
            SachAdapter sachAdapter = new SachAdapter(data.getSachNoiBat());
            rvSachNoiBat.setAdapter(sachAdapter);
            runLayoutAnimation(rvSachNoiBat);
        }
    }

    private void runLayoutAnimation(final RecyclerView recyclerView) {
        if (recyclerView.getAdapter() == null) return;
        final Context context = recyclerView.getContext();
        final LayoutAnimationController controller =
                AnimationUtils.loadLayoutAnimation(context, R.anim.layout_animation_fall_down);

        recyclerView.setLayoutAnimation(controller);
        recyclerView.getAdapter().notifyDataSetChanged();
        recyclerView.scheduleLayoutAnimation();
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        if (currentCall != null) currentCall.cancel();
    }
}
