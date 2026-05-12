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
import com.example.cafebook.adapters.BookGridAdapter;
import com.example.cafebook.models.SachCardDto;
import com.example.cafebook.models.SachFiltersDto;
import com.example.cafebook.models.SachPhanTrangDto;
import com.example.cafebook.models.ThuVienSachFilterItemDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LibraryFragment extends Fragment {

    private RecyclerView recyclerBooks;
    private EditText edtSearch;
    private Spinner spinnerCategory, spinnerSort;
    
    private BookGridAdapter bookAdapter;
    private List<SachCardDto> bookList = new ArrayList<>();
    
    private CafebookApi api;
    
    // Pagination
    private int currentPage = 1;
    private int totalPages = 1;
    private boolean isLoading = false;
    
    // Filters
    private int currentCategoryId = 0;
    private String currentSort = "ten_asc";
    private String currentSearch = "";
    
    // Debounce
    private Handler debounceHandler = new Handler(Looper.getMainLooper());
    private Runnable searchRunnable;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_library, container, false);
        
        api = ApiClient.getClient().create(CafebookApi.class);
        
        initViews(view);
        setupRecyclerView();
        loadFilters();
        setupSearchDebounce();
        
        return view;
    }

    private void initViews(View view) {
        recyclerBooks = view.findViewById(R.id.recyclerBooks);
        edtSearch = view.findViewById(R.id.edtSearch);
        spinnerCategory = view.findViewById(R.id.spinnerCategory);
        spinnerSort = view.findViewById(R.id.spinnerSort);

        String[] sortNames = {"Tên (A-Z)", "Tên (Z-A)", "Tiền cọc (Thấp-Cao)", "Tiền cọc (Cao-Thấp)"};
        final String[] sortValues = {"ten_asc", "ten_desc", "gia_asc", "gia_desc"};
        
        ArrayAdapter<String> sortAdapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_dropdown_item, sortNames);
        spinnerSort.setAdapter(sortAdapter);
        spinnerSort.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                String selectedSort = sortValues[position];
                if (!selectedSort.equals(currentSort)) {
                    currentSort = selectedSort;
                    resetAndLoadData();
                }
            }
            @Override public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void setupRecyclerView() {
        // Adaptive grid: 2 columns for phone, 4 for tablet/landscape
        int screenWidthDp = getResources().getConfiguration().screenWidthDp;
        int spanCount = screenWidthDp >= 600 ? 4 : 2; 
        
        GridLayoutManager layoutManager = new GridLayoutManager(getContext(), spanCount);
        recyclerBooks.setLayoutManager(layoutManager);
        
        bookAdapter = new BookGridAdapter(bookList, getContext());
        recyclerBooks.setAdapter(bookAdapter);

        recyclerBooks.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                if (dy > 0) { // Scrolling down
                    int visibleItemCount = layoutManager.getChildCount();
                    int totalItemCount = layoutManager.getItemCount();
                    int pastVisibleItems = layoutManager.findFirstVisibleItemPosition();

                    if (!isLoading && currentPage < totalPages) {
                        if ((visibleItemCount + pastVisibleItems) >= totalItemCount) {
                            currentPage++;
                            fetchBooks();
                        }
                    }
                }
            }
        });
    }

    private void setupSearchDebounce() {
        edtSearch.addTextChangedListener(new TextWatcher() {
            @Override public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            @Override public void onTextChanged(CharSequence s, int start, int before, int count) {}
            
            @Override
            public void afterTextChanged(Editable s) {
                if (searchRunnable != null) {
                    debounceHandler.removeCallbacks(searchRunnable);
                }
                searchRunnable = () -> {
                    String query = s.toString().trim();
                    if (!query.equals(currentSearch)) {
                        currentSearch = query;
                        resetAndLoadData();
                    }
                };
                debounceHandler.postDelayed(searchRunnable, 500); 
            }
        });
    }

    private void loadFilters() {
        api.getBookFilters().enqueue(new Callback<SachFiltersDto>() {
            @Override
            public void onResponse(@NonNull Call<SachFiltersDto> call, @NonNull Response<SachFiltersDto> response) {
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    List<ThuVienSachFilterItemDto> categories = new ArrayList<>();
                    categories.add(new ThuVienSachFilterItemDto()); // "Tất cả thể loại"
                    categories.addAll(response.body().getTheLoais());

                    ArrayAdapter<ThuVienSachFilterItemDto> catAdapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_spinner_dropdown_item, categories);
                    spinnerCategory.setAdapter(catAdapter);
                    
                    spinnerCategory.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                        @Override
                        public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                            int selectedId = categories.get(position).getId();
                            if (selectedId != currentCategoryId) {
                                currentCategoryId = selectedId;
                                resetAndLoadData();
                            }
                        }
                        @Override public void onNothingSelected(AdapterView<?> parent) {}
                    });
                }
            }
            @Override public void onFailure(@NonNull Call<SachFiltersDto> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Failed to load filters", t);
            }
        });
    }

    private void resetAndLoadData() {
        currentPage = 1;
        bookList.clear();
        bookAdapter.notifyDataSetChanged();
        fetchBooks();
    }

    private void fetchBooks() {
        isLoading = true;
        api.searchBooks(currentSearch, currentCategoryId, "all", currentSort, currentPage).enqueue(new Callback<SachPhanTrangDto>() {
            @Override
            public void onResponse(@NonNull Call<SachPhanTrangDto> call, @NonNull Response<SachPhanTrangDto> response) {
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    totalPages = response.body().getTotalPages();
                    int startPosition = bookList.size();
                    bookList.addAll(response.body().getItems());
                    bookAdapter.notifyItemRangeInserted(startPosition, response.body().getItems().size());
                }
                isLoading = false;
            }

            @Override
            public void onFailure(@NonNull Call<SachPhanTrangDto> call, @NonNull Throwable t) {
                isLoading = false;
                if (isAdded()) {
                    Toast.makeText(getContext(), "Lỗi tải dữ liệu", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }
}
