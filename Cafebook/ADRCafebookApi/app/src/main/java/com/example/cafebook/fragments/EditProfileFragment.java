package com.example.cafebook.fragments;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;
import android.util.Log;
import android.util.Patterns;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.Toast;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.bumptech.glide.Glide;
import com.example.cafebook.R;
import com.example.cafebook.models.ProfileDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.ProfileApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class EditProfileFragment extends Fragment {

    private ImageView imgAvatar;
    private TextInputLayout tilFullName, tilUsername, tilPhone, tilEmail;
    private TextInputEditText edtFullName, edtUsername, edtPhone, edtEmail, edtAddress;
    private MaterialButton btnSave;
    
    private ProfileApiService apiService;
    private int userId;
    private Uri selectedImageUri = null;

    private final ActivityResultLauncher<Intent> imagePickerLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            result -> {
                if (result.getResultCode() == Activity.RESULT_OK && result.getData() != null) {
                    selectedImageUri = result.getData().getData();
                    Glide.with(this).load(selectedImageUri).circleCrop().into(imgAvatar);
                }
            });

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_edit_profile, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
        userId = prefs.getInt("USER_ID", 0);

        imgAvatar = view.findViewById(R.id.imgAvatar);
        tilFullName = view.findViewById(R.id.tilFullName);
        tilUsername = view.findViewById(R.id.tilUsername);
        tilPhone = view.findViewById(R.id.tilPhone);
        tilEmail = view.findViewById(R.id.tilEmail);
        
        edtFullName = view.findViewById(R.id.edtFullName);
        edtUsername = view.findViewById(R.id.edtUsername);
        edtPhone = view.findViewById(R.id.edtPhone);
        edtEmail = view.findViewById(R.id.edtEmail);
        edtAddress = view.findViewById(R.id.edtAddress);
        btnSave = view.findViewById(R.id.btnSave);

        apiService = ApiClient.getClient(requireContext()).create(ProfileApiService.class);

        view.findViewById(R.id.btnBack).setOnClickListener(v -> requireActivity().onBackPressed());
        view.findViewById(R.id.flAvatarContainer).setOnClickListener(v -> openGallery());
        btnSave.setOnClickListener(v -> submitProfile(view));

        loadCurrentProfile();
    }

    private void openGallery() {
        Intent intent = new Intent(Intent.ACTION_PICK, MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
        imagePickerLauncher.launch(intent);
    }

    private void loadCurrentProfile() {
        if (userId == 0) return;
        apiService.getPersonalInfo(userId).enqueue(new Callback<ProfileDto.Info>() {
            @Override
            public void onResponse(@NonNull Call<ProfileDto.Info> call, @NonNull Response<ProfileDto.Info> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ProfileDto.Info info = response.body();
                    edtFullName.setText(info.hoTen);
                    edtUsername.setText(info.tenDangNhap);
                    edtPhone.setText(info.soDienThoai);
                    edtEmail.setText(info.email);
                    edtAddress.setText(info.diaChi);

                    if (getContext() != null) {
                        Glide.with(getContext())
                             .load(info.anhDaiDienUrl + "?v=" + System.currentTimeMillis())
                             .placeholder(R.drawable.ic_person)
                             .circleCrop()
                             .into(imgAvatar);
                    }
                }
            }
            @Override public void onFailure(@NonNull Call<ProfileDto.Info> call, @NonNull Throwable t) {
                Log.e("EDIT_PROFILE", "Load failed", t);
            }
        });
    }

    private void submitProfile(View view) {
        boolean isValid = true;
        tilFullName.setError(null); tilUsername.setError(null); tilPhone.setError(null); tilEmail.setError(null);

        String hoTen = edtFullName.getText().toString().trim();
        String tenDN = edtUsername.getText().toString().trim();
        String sdt = edtPhone.getText().toString().trim();
        String email = edtEmail.getText().toString().trim();
        String diaChi = edtAddress.getText().toString().trim();

        if (hoTen.isEmpty()) { tilFullName.setError("Họ tên không được trống"); isValid = false; }
        if (tenDN.length() < 6) { tilUsername.setError("Tên đăng nhập từ 6 ký tự"); isValid = false; }
        if (!sdt.isEmpty() && !sdt.matches("^(0[3|5|7|8|9])+([0-9]{8})$")) { tilPhone.setError("SĐT không hợp lệ"); isValid = false; }
        if (!email.isEmpty() && !Patterns.EMAIL_ADDRESS.matcher(email).matches()) { tilEmail.setError("Email không hợp lệ"); isValid = false; }

        if (!isValid) return;

        btnSave.setEnabled(false);
        btnSave.setText("ĐANG CẬP NHẬT...");

        RequestBody rbHoTen = RequestBody.create(MediaType.parse("text/plain"), hoTen);
        RequestBody rbTenDN = RequestBody.create(MediaType.parse("text/plain"), tenDN);
        RequestBody rbSdt = RequestBody.create(MediaType.parse("text/plain"), sdt);
        RequestBody rbEmail = RequestBody.create(MediaType.parse("text/plain"), email);
        RequestBody rbDiaChi = RequestBody.create(MediaType.parse("text/plain"), diaChi);

        MultipartBody.Part avatarPart = null;
        if (selectedImageUri != null) {
            try {
                InputStream is = requireContext().getContentResolver().openInputStream(selectedImageUri);
                if (is != null) {
                    ByteArrayOutputStream bos = new ByteArrayOutputStream();
                    byte[] buffer = new byte[1024];
                    int len;
                    while ((len = is.read(buffer)) != -1) bos.write(buffer, 0, len);
                    byte[] bytes = bos.toByteArray();

                    RequestBody reqFile = RequestBody.create(MediaType.parse("image/jpeg"), bytes);
                    avatarPart = MultipartBody.Part.createFormData("avatarFile", "avatar_app.jpg", reqFile);
                }
            } catch (Exception e) {
                Log.e("EDIT_PROFILE", "File error", e);
            }
        }

        apiService.updateProfile(userId, rbHoTen, rbSdt, rbEmail, rbDiaChi, rbTenDN, avatarPart).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(@NonNull Call<Void> call, @NonNull Response<Void> response) {
                btnSave.setEnabled(true);
                btnSave.setText("CẬP NHẬT THÔNG TIN");

                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Cập nhật thành công!", Toast.LENGTH_SHORT).show();
                    requireActivity().onBackPressed();
                } else {
                    Snackbar.make(view, "Cập nhật thất bại. Lỗi " + response.code(), Snackbar.LENGTH_LONG)
                            .setBackgroundTint(getResources().getColor(R.color.cf_orange)).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<Void> call, @NonNull Throwable t) {
                btnSave.setEnabled(true);
                btnSave.setText("CẬP NHẬT THÔNG TIN");
                Toast.makeText(getContext(), "Lỗi kết nối máy chủ", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
