package com.example.cafebook.fragments;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.example.cafebook.R;
import com.example.cafebook.models.TableBookingModels;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;
import com.google.android.material.materialswitch.MaterialSwitch;
import com.google.android.material.textfield.TextInputEditText;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class BookingConfirmDialogFragment extends BottomSheetDialogFragment {

    private String tableId, tableNum, date, time;
    private int peopleCount;
    private TextInputEditText edtPhone, edtName, edtEmail, edtNote;
    private MaterialSwitch switchOrderForOther;
    private CafebookApi api;

    public static BookingConfirmDialogFragment newInstance(int tableId, String tableNum, String date, String time, int peopleCount) {
        BookingConfirmDialogFragment fragment = new BookingConfirmDialogFragment();
        Bundle args = new Bundle();
        args.putInt("TABLE_ID", tableId);
        args.putString("TABLE_NUM", tableNum);
        args.putString("DATE", date);
        args.putString("TIME", time);
        args.putInt("PEOPLE", peopleCount);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            tableId = String.valueOf(getArguments().getInt("TABLE_ID"));
            tableNum = getArguments().getString("TABLE_NUM");
            date = getArguments().getString("DATE");
            time = getArguments().getString("TIME");
            peopleCount = getArguments().getInt("PEOPLE");
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.dialog_booking_confirm, container, false);
        api = ApiClient.getClient().create(CafebookApi.class);
        initViews(view);
        return view;
    }

    private void initViews(View view) {
        TextView tvSummary = view.findViewById(R.id.tvBookingSummary);
        tvSummary.setText(String.format("Bàn %s - %s lúc %s", tableNum, date, time));

        edtPhone = view.findViewById(R.id.edtBookingPhone);
        edtName = view.findViewById(R.id.edtBookingName);
        edtEmail = view.findViewById(R.id.edtBookingEmail);
        edtNote = view.findViewById(R.id.edtBookingNote);
        switchOrderForOther = view.findViewById(R.id.switchOrderForOther);

        edtPhone.setOnFocusChangeListener((v, hasFocus) -> {
            if (!hasFocus && !switchOrderForOther.isChecked()) {
                String phone = edtPhone.getText().toString().trim();
                if (phone.length() >= 10) {
                    fetchCustomerInfo(phone);
                }
            }
        });

        view.findViewById(R.id.btnConfirmBooking).setOnClickListener(v -> submitBooking());
    }

    private void fetchCustomerInfo(String phone) {
        api.getCustomerInfo(phone).enqueue(new Callback<TableBookingModels.CustomerInfoResponse>() {
            @Override
            public void onResponse(@NonNull Call<TableBookingModels.CustomerInfoResponse> call, @NonNull Response<TableBookingModels.CustomerInfoResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    if (edtName.getText().toString().isEmpty()) edtName.setText(response.body().hoTen);
                    if (edtEmail.getText().toString().isEmpty()) edtEmail.setText(response.body().email);
                }
            }
            @Override public void onFailure(@NonNull Call<TableBookingModels.CustomerInfoResponse> call, @NonNull Throwable t) {}
        });
    }

    private void submitBooking() {
        TableBookingModels.DatBanWebRequest req = new TableBookingModels.DatBanWebRequest();
        req.hoTen = edtName.getText().toString().trim();
        req.soDienThoai = edtPhone.getText().toString().trim();
        req.email = edtEmail.getText().toString().trim();
        req.ghiChu = edtNote.getText().toString().trim();
        req.idBan = Integer.parseInt(tableId);
        req.soLuongKhach = peopleCount;
        req.ngayDat = date;
        req.gioDat = time + ":00";

        if (req.hoTen.isEmpty() || req.soDienThoai.isEmpty()) {
            Toast.makeText(getContext(), "Vui lòng nhập tên và số điện thoại", Toast.LENGTH_SHORT).show();
            return;
        }

        api.taoYeuCauDatBan(req).enqueue(new Callback<Object>() {
            @Override
            public void onResponse(@NonNull Call<Object> call, @NonNull Response<Object> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Đặt bàn thành công!", Toast.LENGTH_LONG).show();
                    dismiss();
                    if (getActivity() instanceof MainActivity) {
                        // Refresh or go home? For now just dismiss
                    }
                } else {
                    Toast.makeText(getContext(), "Lỗi: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<Object> call, @NonNull Throwable t) {
                Log.e("API_ERROR", "Booking failed", t);
                Toast.makeText(getContext(), "Không thể kết nối đến máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
