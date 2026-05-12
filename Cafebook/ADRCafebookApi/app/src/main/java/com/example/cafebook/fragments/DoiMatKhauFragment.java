package com.example.cafebook.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.fragment.app.Fragment;
import com.example.cafebook.R;
import com.example.cafebook.models.DoiMatKhauDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.ProfileApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import com.google.android.material.textfield.TextInputEditText;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DoiMatKhauFragment extends Fragment {

    private TextInputEditText edtOldPassword, edtNewPassword, edtConfirmPassword;
    private MaterialButton btnUpdatePassword;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_doi_mat_khau, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        Toolbar toolbar = view.findViewById(R.id.toolbarDoiMatKhau);
        toolbar.setNavigationOnClickListener(v -> getParentFragmentManager().popBackStack());

        edtOldPassword = view.findViewById(R.id.edtOldPassword);
        edtNewPassword = view.findViewById(R.id.edtNewPassword);
        edtConfirmPassword = view.findViewById(R.id.edtConfirmPassword);
        btnUpdatePassword = view.findViewById(R.id.btnUpdatePassword);

        btnUpdatePassword.setOnClickListener(v -> validateAndSubmit());
    }

    private void validateAndSubmit() {
        String oldPass = edtOldPassword.getText().toString().trim();
        String newPass = edtNewPassword.getText().toString().trim();
        String confirmPass = edtConfirmPassword.getText().toString().trim();

        if (oldPass.isEmpty() || newPass.isEmpty() || confirmPass.isEmpty()) {
            Toast.makeText(getContext(), "Vui lòng nhập đầy đủ thông tin", Toast.LENGTH_SHORT).show();
            return;
        }

        if (!newPass.equals(confirmPass)) {
            Toast.makeText(getContext(), "Mật khẩu mới không khớp", Toast.LENGTH_SHORT).show();
            return;
        }

        DoiMatKhauDto model = new DoiMatKhauDto();
        model.setMatKhauCu(oldPass);
        model.setMatKhauMoi(newPass);
        model.setXacNhanMatKhauMoi(confirmPass);

        int userId = 0;
        if (getActivity() != null) {
            android.content.SharedPreferences prefs = getActivity().getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
            userId = prefs.getInt("USER_ID", 0);
        }

        if (userId == 0) {
            Toast.makeText(getContext(), "Không tìm thấy thông tin người dùng", Toast.LENGTH_SHORT).show();
            return;
        }

        ProfileApiService service = ApiClient.getClient(getContext()).create(ProfileApiService.class);
        service.changePassword(userId, model).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (response.isSuccessful()) {
                    new MaterialAlertDialogBuilder(requireContext())
                            .setTitle("Thành công")
                            .setMessage("Mật khẩu của bạn đã được thay đổi thành công.")
                            .setPositiveButton("OK", (dialog, which) -> getParentFragmentManager().popBackStack())
                            .show();
                } else {
                    Toast.makeText(getContext(), "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu cũ.", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Toast.makeText(getContext(), "Lỗi kết nối: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }
}
