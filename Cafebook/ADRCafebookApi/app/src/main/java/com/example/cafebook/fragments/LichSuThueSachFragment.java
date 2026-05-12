package com.example.cafebook.fragments;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.example.cafebook.R;
import com.example.cafebook.adapters.RentalHistoryAdapter;
import com.example.cafebook.models.LichSuThueSachDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.RentalHistoryApiService;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LichSuThueSachFragment extends Fragment {

    private RecyclerView rvRentals;
    private SwipeRefreshLayout swipeRefresh;
    private List<LichSuThueSachDto.Item> rentalList = new ArrayList<>();
    private RentalHistoryAdapter adapter;
    private RentalHistoryApiService apiService;
    private int currentPage = 1;
    private int totalPages = 1;
    private boolean isLoading = false;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_rental_history, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        rvRentals = view.findViewById(R.id.rvRentals);
        swipeRefresh = view.findViewById(R.id.swipeRefreshRental);
        apiService = ApiClient.getClient(requireContext()).create(RentalHistoryApiService.class);

        setupRecyclerView();
        
        swipeRefresh.setOnRefreshListener(() -> {
            currentPage = 1;
            loadData(true);
        });

        loadData(true);
    }

    private void setupRecyclerView() {
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        rvRentals.setLayoutManager(layoutManager);
        
        adapter = new RentalHistoryAdapter(rentalList, item -> {
            // TODO: Hiển thị BottomSheet chi tiết
            Toast.makeText(getContext(), "Xem chi tiết #" + item.idPhieuThueSach, Toast.LENGTH_SHORT).show();
        });
        rvRentals.setAdapter(adapter);

        rvRentals.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                if (dy > 0 && !isLoading && currentPage < totalPages) {
                    int visibleItemCount = layoutManager.getChildCount();
                    int totalItemCount = layoutManager.getItemCount();
                    int firstVisibleItemPosition = layoutManager.findFirstVisibleItemPosition();

                    if ((visibleItemCount + firstVisibleItemPosition) >= (totalItemCount - 2)) {
                        currentPage++;
                        loadData(false);
                    }
                }
            }
        });
    }

    private void loadData(boolean isRefresh) {
        isLoading = true;
        if (isRefresh) swipeRefresh.setRefreshing(true);

        apiService.getRentalHistory(currentPage, null, null).enqueue(new Callback<LichSuThueSachDto.Response>() {
            @Override
            public void onResponse(@NonNull Call<LichSuThueSachDto.Response> call, @NonNull Response<LichSuThueSachDto.Response> response) {
                isLoading = false;
                swipeRefresh.setRefreshing(false);
                if (response.isSuccessful() && response.body() != null) {
                    if (isRefresh) rentalList.clear();
                    rentalList.addAll(response.body().items);
                    totalPages = response.body().totalPages;
                    adapter.notifyDataSetChanged();
                } else {
                    Log.e("API_ERROR", "Failed to load rental history: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<LichSuThueSachDto.Response> call, @NonNull Throwable t) {
                isLoading = false;
                swipeRefresh.setRefreshing(false);
                Log.e("API_ERROR", "Connection failed", t);
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
