package com.example.cafebook.fragments;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
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
import com.example.cafebook.adapters.OrderHistoryAdapter;
import com.example.cafebook.models.OrderDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.OrderApiService;
import com.google.android.material.tabs.TabLayout;
import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class OrderHistoryFragment extends Fragment {

    private TabLayout tabStatus;
    private EditText edtSearch;
    private SwipeRefreshLayout swipeRefresh;
    private RecyclerView rvOrders;
    
    private List<OrderDto.HistoryItem> allOrders = new ArrayList<>();
    private List<OrderDto.HistoryItem> filteredOrders = new ArrayList<>();
    private OrderHistoryAdapter adapter;
    private OrderApiService apiService;

    private String currentStatus = "Tất cả";
    private String currentSearch = "";

    private final String[] STATUS_TABS = {"Tất cả", "Chờ xác nhận", "Chờ lấy hàng", "Đang giao", "Hoàn thành", "Đã hủy"};

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_order_history, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        tabStatus = view.findViewById(R.id.tabStatus);
        edtSearch = view.findViewById(R.id.edtSearchOrder);
        swipeRefresh = view.findViewById(R.id.swipeRefreshOrders);
        rvOrders = view.findViewById(R.id.rvOrders);

        apiService = ApiClient.getClient(requireContext()).create(OrderApiService.class);

        setupTabs();
        setupRecyclerView();
        setupSearch();

        swipeRefresh.setOnRefreshListener(this::loadOrders);

        loadOrders();
    }

    private void setupTabs() {
        for (String status : STATUS_TABS) {
            tabStatus.addTab(tabStatus.newTab().setText(status));
        }

        tabStatus.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener() {
            @Override
            public void onTabSelected(TabLayout.Tab tab) {
                currentStatus = tab.getText().toString();
                filterOrders();
            }
            @Override public void onTabUnselected(TabLayout.Tab tab) {}
            @Override public void onTabReselected(TabLayout.Tab tab) {}
        });
    }

    private void setupRecyclerView() {
        rvOrders.setLayoutManager(new LinearLayoutManager(getContext()));
        adapter = new OrderHistoryAdapter(filteredOrders, item -> {
            OrderDetailFragment detailFragment = OrderDetailFragment.newInstance(item.idHoaDon);
            if (getActivity() instanceof com.example.cafebook.MainActivity) {
                ((com.example.cafebook.MainActivity) getActivity()).loadFragment(detailFragment);
            }
        });
        rvOrders.setAdapter(adapter);
    }

    private void setupSearch() {
        edtSearch.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                currentSearch = s.toString().toLowerCase();
                filterOrders();
            }
            @Override
            public void afterTextChanged(Editable s) {}
        });
    }

    private void loadOrders() {
        swipeRefresh.setRefreshing(true);
        apiService.getOrders().enqueue(new Callback<List<OrderDto.HistoryItem>>() {
            @Override
            public void onResponse(Call<List<OrderDto.HistoryItem>> call, Response<List<OrderDto.HistoryItem>> response) {
                swipeRefresh.setRefreshing(false);
                if (response.isSuccessful() && response.body() != null) {
                    allOrders.clear();
                    allOrders.addAll(response.body());
                    filterOrders();
                } else {
                    Toast.makeText(getContext(), "Lỗi khi tải danh sách đơn hàng", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<List<OrderDto.HistoryItem>> call, Throwable t) {
                swipeRefresh.setRefreshing(false);
                Toast.makeText(getContext(), "Lỗi kết nối", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void filterOrders() {
        filteredOrders.clear();
        for (OrderDto.HistoryItem order : allOrders) {
            boolean matchStatus = currentStatus.equals("Tất cả") || order.trangThaiGiaoHang.equalsIgnoreCase(currentStatus);
            boolean matchSearch = currentSearch.isEmpty() || 
                    order.maDonHang.toLowerCase().contains(currentSearch) || 
                    order.tenSanPham.toLowerCase().contains(currentSearch);
            
            if (matchStatus && matchSearch) {
                filteredOrders.add(order);
            }
        }
        adapter.notifyDataSetChanged();
    }
}
