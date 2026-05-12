package com.example.cafebook.fragments;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.R;
import com.example.cafebook.adapters.MenuAdapter;
import com.example.cafebook.models.SanPhamThucDonDto;
import com.example.cafebook.models.ThucDonDto;
import com.example.cafebook.models.ThucDonFilterDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MenuFragment extends Fragment {

    private RecyclerView rvMenu;
    private EditText edtSearchMenu;
    private Spinner spinnerCategory, spinnerSort;
    
    private MenuAdapter adapter;
    private List<SanPhamThucDonDto> productList = new ArrayList<>();
    private CafebookApi api;

    private int currentPage = 1, totalPages = 1;
    private boolean isLoading = false;
    private int currentCategoryId = 0;
    private String currentSort = "ten_asc";
    private String currentSearch = "";

    private Handler debounceHandler = new Handler(Looper.getMainLooper());
    private Runnable searchRunnable;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_menu, container, false);
        api = ApiClient.getClient(requireContext()).create(CafebookApi.class);
        
        initViews(view);
        setupRecyclerView();
        loadCategories();
        setupListeners();
        
        fetchProducts();
        
        return view;
    }

    private void initViews(View view) {
        rvMenu = view.findViewById(R.id.rvMenu);
        edtSearchMenu = view.findViewById(R.id.edtSearchMenu);
        spinnerCategory = view.findViewById(R.id.spinnerCategory);
        spinnerSort = view.findViewById(R.id.spinnerSort);

        String[] sortNames = {"Tên (A-Z)", "Tên (Z-A)", "Giá (Thấp-Cao)", "Giá (Cao-Thấp)"};
        final String[] sortValues = {"ten_asc", "ten_desc", "gia_asc", "gia_desc"};
        ArrayAdapter<String> sortAdapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_dropdown_item, sortNames);
        spinnerSort.setAdapter(sortAdapter);
        spinnerSort.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                String selectedSort = sortValues[position];
                if (!currentSort.equals(selectedSort)) {
                    currentSort = selectedSort;
                    resetAndLoadData();
                }
            }
            @Override public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void setupRecyclerView() {
        int screenWidthDp = getResources().getConfiguration().screenWidthDp;
        int spanCount = screenWidthDp >= 600 ? 4 : 2;
        
        GridLayoutManager layoutManager = new GridLayoutManager(getContext(), spanCount);
        rvMenu.setLayoutManager(layoutManager);
        
        adapter = new MenuAdapter(productList, getContext());
        rvMenu.setAdapter(adapter);

        rvMenu.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                if (dy > 0 && !isLoading && currentPage < totalPages) {
                    int visibleItemCount = layoutManager.getChildCount();
                    int totalItemCount = layoutManager.getItemCount();
                    int pastVisibleItems = layoutManager.findFirstVisibleItemPosition();

                    if ((visibleItemCount + pastVisibleItems) >= totalItemCount - 1) {
                        currentPage++;
                        fetchProducts();
                    }
                }
            }
        });
    }

    private void setupListeners() {
        edtSearchMenu.addTextChangedListener(new TextWatcher() {
            @Override public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            @Override public void onTextChanged(CharSequence s, int start, int before, int count) {}
            @Override
            public void afterTextChanged(Editable s) {
                debounceHandler.removeCallbacks(searchRunnable);
                searchRunnable = () -> {
                    String query = s.toString().trim();
                    if (!currentSearch.equals(query)) {
                        currentSearch = query;
                        resetAndLoadData();
                    }
                };
                debounceHandler.postDelayed(searchRunnable, 600);
            }
        });
    }

    private void loadCategories() {
        api.getMenuFilters().enqueue(new Callback<List<ThucDonFilterDto>>() {
            @Override
            public void onResponse(@NonNull Call<List<ThucDonFilterDto>> call, @NonNull Response<List<ThucDonFilterDto>> response) {
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    List<ThucDonFilterDto> categories = new ArrayList<>();
                    categories.add(new ThucDonFilterDto()); 
                    categories.addAll(response.body());

                    ArrayAdapter<ThucDonFilterDto> catAdapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_dropdown_item, categories);
                    spinnerCategory.setAdapter(catAdapter);
                    
                    spinnerCategory.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                        @Override public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                            int selectedId = categories.get(position).getId();
                            if (currentCategoryId != selectedId) {
                                currentCategoryId = selectedId;
                                resetAndLoadData();
                            }
                        }
                        @Override public void onNothingSelected(AdapterView<?> parent) {}
                    });
                }
            }
            @Override public void onFailure(@NonNull Call<List<ThucDonFilterDto>> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Failed to load menu filters", t);
            }
        });
    }

    private void resetAndLoadData() {
        currentPage = 1;
        productList.clear();
        adapter.notifyDataSetChanged();
        fetchProducts();
    }

    private void fetchProducts() {
        isLoading = true;
        api.searchMenu(currentCategoryId, currentSearch, currentSort, null, null, currentPage)
           .enqueue(new Callback<ThucDonDto>() {
               @Override
               public void onResponse(@NonNull Call<ThucDonDto> call, @NonNull Response<ThucDonDto> response) {
                   if (response.isSuccessful() && response.body() != null && isAdded()) {
                       totalPages = response.body().getTotalPages();
                       int startPos = productList.size();
                       productList.addAll(response.body().getItems());
                       adapter.notifyItemRangeInserted(startPos, response.body().getItems().size());
                   }
                   isLoading = false;
               }
               @Override public void onFailure(@NonNull Call<ThucDonDto> call, @NonNull Throwable t) {
                   isLoading = false;
                   if (isAdded()) {
                       Toast.makeText(getContext(), "Lỗi tải thực đơn", Toast.LENGTH_SHORT).show();
                   }
               }
           });
    }
}
