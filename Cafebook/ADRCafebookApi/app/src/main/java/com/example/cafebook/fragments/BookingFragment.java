package com.example.cafebook.fragments;

import android.app.DatePickerDialog;
import android.app.TimePickerDialog;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.R;
import com.example.cafebook.adapters.TableAdapter;
import com.example.cafebook.models.TableBookingModels;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.material.textfield.TextInputEditText;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class BookingFragment extends Fragment implements TableAdapter.OnTableClickListener {

    private TextInputEditText edtDate, edtTime, edtPeople;
    private MaterialButton btnSearchTable;
    private ChipGroup chipGroupArea;
    private RecyclerView rvTables;
    private TableAdapter adapter;
    private CafebookApi api;

    private List<TableBookingModels.BanTrong> allAvailableTables = new ArrayList<>();
    private List<TableBookingModels.KhuVucBan> allAreas = new ArrayList<>();

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_booking, container, false);
        api = ApiClient.getClient().create(CafebookApi.class);
        initViews(view);
        setupUI();
        return view;
    }

    private void initViews(View view) {
        edtDate = view.findViewById(R.id.edtDate);
        edtTime = view.findViewById(R.id.edtTime);
        edtPeople = view.findViewById(R.id.edtPeople);
        btnSearchTable = view.findViewById(R.id.btnSearchTable);
        chipGroupArea = view.findViewById(R.id.chipGroupArea);
        rvTables = view.findViewById(R.id.rvTables);
    }

    private void setupUI() {
        int spanCount = getResources().getConfiguration().screenWidthDp >= 600 ? 4 : 2;
        rvTables.setLayoutManager(new GridLayoutManager(getContext(), spanCount));
        adapter = new TableAdapter(new ArrayList<>(), this);
        rvTables.setAdapter(adapter);

        Calendar cal = Calendar.getInstance();
        edtDate.setText(String.format(Locale.getDefault(), "%d-%02d-%d", cal.get(Calendar.YEAR), cal.get(Calendar.MONTH) + 1, cal.get(Calendar.DAY_OF_MONTH)));
        edtTime.setText(String.format(Locale.getDefault(), "%02d:%02d", cal.get(Calendar.HOUR_OF_DAY), cal.get(Calendar.MINUTE)));

        edtDate.setOnClickListener(v -> showDatePicker());
        edtTime.setOnClickListener(v -> showTimePicker());
        btnSearchTable.setOnClickListener(v -> handleSearchTable());
    }

    private void showDatePicker() {
        Calendar cal = Calendar.getInstance();
        new DatePickerDialog(requireContext(), (view, year, month, dayOfMonth) -> {
            edtDate.setText(String.format(Locale.getDefault(), "%d-%02d-%02d", year, month + 1, dayOfMonth));
        }, cal.get(Calendar.YEAR), cal.get(Calendar.MONTH), cal.get(Calendar.DAY_OF_MONTH)).show();
    }

    private void showTimePicker() {
        Calendar cal = Calendar.getInstance();
        new TimePickerDialog(requireContext(), (view, hourOfDay, minute) -> {
            edtTime.setText(String.format(Locale.getDefault(), "%02d:%02d", hourOfDay, minute));
        }, cal.get(Calendar.HOUR_OF_DAY), cal.get(Calendar.MINUTE), true).show();
    }

    private void handleSearchTable() {
        String date = edtDate.getText().toString();
        String time = edtTime.getText().toString() + ":00";
        int people = Integer.parseInt(edtPeople.getText().toString());

        btnSearchTable.setEnabled(false);
        btnSearchTable.setText("ĐANG TÌM...");

        api.timBanTrong(new TableBookingModels.TimBanRequest(date, time, people)).enqueue(new Callback<List<TableBookingModels.BanTrong>>() {
            @Override
            public void onResponse(@NonNull Call<List<TableBookingModels.BanTrong>> call, @NonNull Response<List<TableBookingModels.BanTrong>> response) {
                btnSearchTable.setEnabled(true);
                btnSearchTable.setText("KIỂM TRA BÀN TRỐNG");
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    allAvailableTables = response.body();
                    updateRecyclerViewAndChips();
                    if (allAvailableTables.isEmpty()) {
                        Snackbar.make(getView(), "Hết bàn phù hợp giờ này!", Snackbar.LENGTH_LONG).show();
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<TableBookingModels.BanTrong>> call, @NonNull Throwable t) {
                btnSearchTable.setEnabled(true);
                btnSearchTable.setText("KIỂM TRA BÀN TRỐNG");
                if (isAdded()) {
                    Toast.makeText(getContext(), "Lỗi kết nối máy chủ!", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    private void updateRecyclerViewAndChips() {
        adapter.updateList(allAvailableTables);
        chipGroupArea.removeAllViews();

        List<String> areas = new ArrayList<>();
        areas.add("Tất cả");
        for (TableBookingModels.BanTrong b : allAvailableTables) {
            if (!areas.contains(b.khuVuc)) areas.add(b.khuVuc);
        }

        for (String area : areas) {
            Chip chip = new Chip(requireContext());
            chip.setText(area);
            chip.setCheckable(true);
            chip.setOnClickListener(v -> filterTablesByArea(area));
            chipGroupArea.addView(chip);
        }
    }

    private void filterTablesByArea(String areaName) {
        if (areaName.equals("Tất cả")) {
            adapter.updateList(allAvailableTables);
        } else {
            List<TableBookingModels.BanTrong> filtered = new ArrayList<>();
            for (TableBookingModels.BanTrong b : allAvailableTables) {
                if (b.khuVuc.equals(areaName)) filtered.add(b);
            }
            adapter.updateList(filtered);
        }
    }

    @Override
    public void onTableClick(TableBookingModels.BanTrong table) {
        BookingConfirmDialogFragment dialog = BookingConfirmDialogFragment.newInstance(
                table.idBan, table.soBan, edtDate.getText().toString(), edtTime.getText().toString(),
                Integer.parseInt(edtPeople.getText().toString()));
        dialog.show(getParentFragmentManager(), "BookingConfirm");
    }
}
