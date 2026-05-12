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

import com.example.cafebook.MainActivity;
import com.example.cafebook.R;
import com.example.cafebook.models.LienHeDto;
import com.example.cafebook.models.PhanHoiInputModel;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.CafebookApi;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.material.textfield.TextInputEditText;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LienHeFragment extends Fragment {

    private TextView tvDiaChi, tvSoDienThoai, tvEmail, tvGioHoatDong;
    private TextInputEditText edtTen, edtEmail, edtNoiDung;
    private MaterialButton btnSubmitGopY;
    private View btnOpenPolicy;
    private CafebookApi api;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_lien_he, container, false);
        
        api = ApiClient.getClient(requireContext()).create(CafebookApi.class);
        initViews(view);
        loadContactInfo();
        
        return view;
    }

    private void initViews(View view) {
        btnOpenPolicy = view.findViewById(R.id.btnOpenPolicy);
        
        // Find views from included layouts
        View layoutDiaChi = view.findViewById(R.id.layoutDiaChi);
        tvDiaChi = layoutDiaChi.findViewById(R.id.tvValue);
        ((ImageView)layoutDiaChi.findViewById(R.id.ivIcon)).setImageResource(R.drawable.ic_location);

        View layoutDienThoai = view.findViewById(R.id.layoutDienThoai);
        tvSoDienThoai = layoutDienThoai.findViewById(R.id.tvValue);
        ((ImageView)layoutDienThoai.findViewById(R.id.ivIcon)).setImageResource(R.drawable.ic_phone);

        View layoutEmail = view.findViewById(R.id.layoutEmail);
        tvEmail = layoutEmail.findViewById(R.id.tvValue);
        ((ImageView)layoutEmail.findViewById(R.id.ivIcon)).setImageResource(R.drawable.ic_mail);

        View layoutGioHoatDong = view.findViewById(R.id.layoutGioHoatDong);
        tvGioHoatDong = layoutGioHoatDong.findViewById(R.id.tvValue);
        ((ImageView)layoutGioHoatDong.findViewById(R.id.ivIcon)).setImageResource(R.drawable.ic_schedule);

        // Form
        edtTen = view.findViewById(R.id.edtTen);
        edtEmail = view.findViewById(R.id.edtEmail);
        edtNoiDung = view.findViewById(R.id.edtNoiDung);
        btnSubmitGopY = view.findViewById(R.id.btnSubmitGopY);

        btnSubmitGopY.setOnClickListener(v -> submitGopY(view));
        
        btnOpenPolicy.setOnClickListener(v -> {
            if (getActivity() instanceof MainActivity) {
                ((MainActivity) getActivity()).loadFragment(new ChinhSachFragment());
            }
        });
    }

    private void loadContactInfo() {
        api.getContactInfo().enqueue(new Callback<LienHeDto>() {
            @Override
            public void onResponse(@NonNull Call<LienHeDto> call, @NonNull Response<LienHeDto> response) {
                if (response.isSuccessful() && response.body() != null && isAdded()) {
                    LienHeDto info = response.body();
                    tvDiaChi.setText(info.diaChi != null ? info.diaChi : "08 Hà Văn Tín, Đà Nẵng");
                    tvSoDienThoai.setText(info.soDienThoai != null ? info.soDienThoai : "0376512695");
                    tvEmail.setText("cafebook.hotro@gmail.com"); // Static/Fallback as per request
                    tvGioHoatDong.setText(info.gioHoatDong != null ? info.gioHoatDong : "07:00 - 22:00 (Thứ 2 - CN)");
                }
            }

            @Override
            public void onFailure(@NonNull Call<LienHeDto> call, @NonNull Throwable t) {
                if (isAdded()) {
                    tvDiaChi.setText("08 Hà Văn Tín, Đà Nẵng");
                    tvSoDienThoai.setText("0376512695");
                    tvEmail.setText("cafebook.hotro@gmail.com");
                    tvGioHoatDong.setText("07:00 - 22:00 (Thứ 2 - CN)");
                }
            }
        });
    }

    private void submitGopY(View view) {
        String ten = edtTen.getText().toString().trim();
        String email = edtEmail.getText().toString().trim();
        String noiDung = edtNoiDung.getText().toString().trim();

        if (ten.isEmpty() || email.isEmpty() || noiDung.isEmpty()) {
            Snackbar.make(view, "Vui lòng nhập đầy đủ thông tin!", Snackbar.LENGTH_SHORT).show();
            return;
        }

        btnSubmitGopY.setEnabled(false);
        btnSubmitGopY.setText("ĐANG GỬI...");

        PhanHoiInputModel input = new PhanHoiInputModel(ten, email, noiDung);
        api.guiGopY(input).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(@NonNull Call<Void> call, @NonNull Response<Void> response) {
                if (isAdded()) {
                    btnSubmitGopY.setEnabled(true);
                    btnSubmitGopY.setText("GỬI PHẢN HỒI");

                    if (response.isSuccessful()) {
                        Snackbar.make(view, "Cảm ơn bạn! Chúng tôi đã nhận được góp ý.", Snackbar.LENGTH_LONG).show();
                        edtTen.setText(""); edtEmail.setText(""); edtNoiDung.setText("");
                    } else {
                        Toast.makeText(getContext(), "Lỗi: " + response.code(), Toast.LENGTH_SHORT).show();
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<Void> call, @NonNull Throwable t) {
                if (isAdded()) {
                    btnSubmitGopY.setEnabled(true);
                    btnSubmitGopY.setText("GỬI PHẢN HỒI");
                    Toast.makeText(getContext(), "Lỗi kết nối máy chủ!", Toast.LENGTH_SHORT).show();
                }
            }
        });
    }
}
