package com.example.cafebook.fragments;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.example.cafebook.R;
import com.example.cafebook.adapters.BookingHistoryAdapter;
import com.example.cafebook.models.LichSuDatBanDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.BookingHistoryApiService;
import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class BookingHistoryFragment extends Fragment {

    private RecyclerView rvHistory;
    private SwipeRefreshLayout swipeRefresh;
    private BookingHistoryApiService apiService;
    private BookingHistoryAdapter adapter;
    
    private List<LichSuDatBanDto.Item> historyList = new ArrayList<>();
    private int currentPage = 1;
    private int totalPages = 1;
    private boolean isLoading = false;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_booking_history, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        rvHistory = view.findViewById(R.id.rvHistory);
        swipeRefresh = view.findViewById(R.id.swipeRefresh);
        apiService = ApiClient.getClient(requireContext()).create(BookingHistoryApiService.class);

        setupRecyclerView();
        
        swipeRefresh.setOnRefreshListener(() -> {
            currentPage = 1;
            loadData(true);
        });

        loadData(true);
    }

    private void setupRecyclerView() {
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        rvHistory.setLayoutManager(layoutManager);
        
        adapter = new BookingHistoryAdapter(historyList, this::showCancelDialog);
        rvHistory.setAdapter(adapter);

        rvHistory.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                if (dy > 0 && !isLoading && currentPage < totalPages) {
                    int visibleItemCount = layoutManager.getChildCount();
                    int totalItemCount = layoutManager.getItemCount();
                    int pastVisiblesItems = layoutManager.findFirstVisibleItemPosition();

                    if ((visibleItemCount + pastVisiblesItems) >= totalItemCount) {
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

        apiService.getHistory(currentPage, null, null, null, null).enqueue(new Callback<LichSuDatBanDto.Response>() {
            @Override
            public void onResponse(@NonNull Call<LichSuDatBanDto.Response> call, @NonNull Response<LichSuDatBanDto.Response> response) {
                isLoading = false;
                swipeRefresh.setRefreshing(false);
                if (response.isSuccessful() && response.body() != null) {
                    if (isRefresh) historyList.clear();
                    historyList.addAll(response.body().items);
                    totalPages = response.body().totalPages;
                    adapter.notifyDataSetChanged();
                } else {
                    Log.e("API_ERROR", "Failed to load history: " + response.code());
                }
            }
            @Override public void onFailure(@NonNull Call<LichSuDatBanDto.Response> call, @NonNull Throwable t) {
                isLoading = false;
                swipeRefresh.setRefreshing(false);
                Log.e("API_ERROR", "Connection failed", t);
            }
        });
    }

    private void showCancelDialog(LichSuDatBanDto.Item item) {
        View dialogView = getLayoutInflater().inflate(R.layout.dialog_cancel_reason, null);
        EditText edtReason = dialogView.findViewById(R.id.edtReason);

        new MaterialAlertDialogBuilder(requireContext())
            .setTitle("Hủy đặt bàn #" + item.idPhieuDatBan)
            .setMessage("Vui lòng cho biết lý do bạn muốn hủy bàn này?")
            .setView(dialogView)
            .setPositiveButton("Xác nhận hủy", (dialog, which) -> {
                String reason = edtReason.getText().toString().trim();
                performCancel(item.idPhieuDatBan, reason);
            })
            .setNegativeButton("Quay lại", null)
            .show();
    }

    private void performCancel(int id, String reason) {
        apiService.cancelBooking(id, reason).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(@NonNull Call<Void> call, @NonNull Response<Void> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Đã gửi yêu cầu hủy bàn thành công", Toast.LENGTH_SHORT).show();
                    currentPage = 1;
                    loadData(true);
                } else {
                    Toast.makeText(getContext(), "Không thể hủy bàn lúc này", Toast.LENGTH_SHORT).show();
                }
            }
            @Override public void onFailure(@NonNull Call<Void> call, @NonNull Throwable t) {
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
