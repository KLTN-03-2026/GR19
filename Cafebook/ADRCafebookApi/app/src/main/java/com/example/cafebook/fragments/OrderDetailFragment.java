package com.example.cafebook.fragments;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.RatingBar;
import android.widget.TextView;
import android.widget.Toast;
import com.bumptech.glide.Glide;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.adapters.OrderProductAdapter;
import com.example.cafebook.adapters.TrackingAdapter;
import com.example.cafebook.models.OrderDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.OrderApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import com.google.android.material.imageview.ShapeableImageView;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class OrderDetailFragment extends Fragment {

    private static final String ARG_ORDER_ID = "order_id";
    private int orderId;
    private OrderApiService apiService;

    private TextView tvMaDon, tvStatus, tvTime, tvNamePhone, tvAddress;
    private TextView tvSubtotal, tvShipping, tvDiscount, tvTotal;
    private RecyclerView rvTracking, rvItems;
    private MaterialButton btnCancel, btnRepay;
    private View layoutDeliveryPhoto;
    private ShapeableImageView imgDeliveryPhoto;

    private Uri selectedImageUri;
    private ShapeableImageView imgRatingSelected;

    public static OrderDetailFragment newInstance(int orderId) {
        OrderDetailFragment fragment = new OrderDetailFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_ORDER_ID, orderId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            orderId = getArguments().getInt(ARG_ORDER_ID);
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_order_detail, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        initViews(view);
        apiService = ApiClient.getClient(requireContext()).create(OrderApiService.class);

        loadOrderDetail();
    }

    private void initViews(View view) {
        tvMaDon = view.findViewById(R.id.tvDetailMaDon);
        tvStatus = view.findViewById(R.id.tvDetailStatus);
        tvTime = view.findViewById(R.id.tvDetailTime);
        tvNamePhone = view.findViewById(R.id.tvDetailNamePhone);
        tvAddress = view.findViewById(R.id.tvDetailAddress);
        tvSubtotal = view.findViewById(R.id.tvDetailSubtotal);
        tvShipping = view.findViewById(R.id.tvDetailShipping);
        tvDiscount = view.findViewById(R.id.tvDetailDiscount);
        tvTotal = view.findViewById(R.id.tvDetailTotal);
        rvTracking = view.findViewById(R.id.rvTracking);
        rvItems = view.findViewById(R.id.rvOrderItems);
        btnCancel = view.findViewById(R.id.btnCancelOrder);
        btnRepay = view.findViewById(R.id.btnRepay);
        layoutDeliveryPhoto = view.findViewById(R.id.layoutDeliveryPhoto);
        imgDeliveryPhoto = view.findViewById(R.id.imgDeliveryPhoto);

        rvTracking.setLayoutManager(new LinearLayoutManager(getContext()));
        rvItems.setLayoutManager(new LinearLayoutManager(getContext()));
    }

    private void loadOrderDetail() {
        apiService.getOrderDetail(orderId).enqueue(new Callback<OrderDto.Detail>() {
            @Override
            public void onResponse(Call<OrderDto.Detail> call, Response<OrderDto.Detail> response) {
                if (response.isSuccessful() && response.body() != null) {
                    bindData(response.body());
                } else {
                    Toast.makeText(getContext(), "Không tìm thấy thông tin đơn hàng", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<OrderDto.Detail> call, Throwable t) {
                Toast.makeText(getContext(), "Lỗi kết nối", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void bindData(OrderDto.Detail detail) {
        tvMaDon.setText(detail.maDonHang);
        
        String displayStatus = detail.trangThaiGiaoHang;
        if ("Đã hủy".equalsIgnoreCase(detail.trangThaiThanhToan)) {
            displayStatus = "Đã hủy";
        }
        tvStatus.setText(displayStatus);

        // Badge colors based on status (matching OrderHistoryAdapter)
        if ("Hoàn thành".equalsIgnoreCase(displayStatus)) {
            tvStatus.setTextColor(getResources().getColor(R.color.cf_green, null));
        } else if ("Đã hủy".equalsIgnoreCase(displayStatus)) {
            tvStatus.setTextColor(getResources().getColor(R.color.cf_red, null));
        } else {
            tvStatus.setTextColor(getResources().getColor(R.color.cf_orange, null));
        }

        tvTime.setText(detail.thoiGianTao != null ? detail.thoiGianTao.replace("T", " ").substring(0, 16) : "");
        tvNamePhone.setText(detail.hoTen + " | " + detail.soDienThoai);
        tvAddress.setText(detail.diaChiGiaoHang);

        tvSubtotal.setText(String.format("%,.0fđ", detail.tongTienHang));
        tvShipping.setText(String.format("%,.0fđ", detail.phiGiaoHang));
        tvDiscount.setText(String.format("-%,.0fđ", detail.giamGia));
        tvTotal.setText(String.format("%,.0fđ", detail.thanhTien));

        rvTracking.setAdapter(new TrackingAdapter(detail.trackingEvents));

        // Delivery Photo
        if (detail.anhXacNhanGiaoHangUrl != null && !detail.anhXacNhanGiaoHangUrl.isEmpty()) {
            layoutDeliveryPhoto.setVisibility(View.VISIBLE);
            Glide.with(this)
                    .load(detail.anhXacNhanGiaoHangUrl)
                    .placeholder(R.color.gray_200)
                    .error(R.color.gray_200)
                    .into(imgDeliveryPhoto);
        } else {
            layoutDeliveryPhoto.setVisibility(View.GONE);
        }
        
        boolean isCompleted = "Hoàn thành".equalsIgnoreCase(detail.trangThaiGiaoHang);
        rvItems.setAdapter(new OrderProductAdapter(detail.items, isCompleted, this::showRatingDialog));

        // Action buttons
        // 1. Logic nút Hủy: Chỉ hiện ở "Chờ xác nhận" hoặc "Chờ thanh toán"
        if (("Chờ xác nhận".equalsIgnoreCase(detail.trangThaiGiaoHang) ||
                "Chờ thanh toán".equalsIgnoreCase(detail.trangThaiThanhToan)) &&
                !"Đã hủy".equalsIgnoreCase(detail.trangThaiThanhToan)) {
            btnCancel.setVisibility(View.VISIBLE);
            btnCancel.setOnClickListener(v -> showCancelDialog());
        } else {
            btnCancel.setVisibility(View.GONE);
        }

        // 2. Logic nút Thanh toán lại (VNPay): Chỉ khi đang chờ thanh toán
        if ("Chờ thanh toán".equalsIgnoreCase(detail.trangThaiThanhToan) &&
                "VNPay".equalsIgnoreCase(detail.phuongThucThanhToan)) {
            btnRepay.setVisibility(View.VISIBLE);
            btnRepay.setOnClickListener(v -> handleRepay());
        } else {
            btnRepay.setVisibility(View.GONE);
        }
    }

    private void showCancelDialog() {
        View dialogView = LayoutInflater.from(getContext()).inflate(R.layout.dialog_cancel_order, null);
        EditText edtReason = dialogView.findViewById(R.id.edtCancelReason);

        new MaterialAlertDialogBuilder(requireContext())
                .setTitle("Hủy đơn hàng")
                .setView(dialogView)
                .setPositiveButton("Hủy đơn", (dialog, which) -> {
                    String reason = edtReason.getText().toString().trim();
                    if (reason.isEmpty()) reason = "Khách hàng muốn hủy";
                    cancelOrder(reason);
                })
                .setNegativeButton("Quay lại", null)
                .show();
    }

    private void cancelOrder(String reason) {
        apiService.cancelOrder(orderId, new OrderDto.CancelOrderRequest(reason)).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Đã hủy đơn hàng thành công", Toast.LENGTH_SHORT).show();
                    loadOrderDetail();
                } else {
                    Toast.makeText(getContext(), "Không thể hủy đơn hàng lúc này", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Toast.makeText(getContext(), "Lỗi kết nối", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void handleRepay() {
        String returnUrl = "cafebook://payment-result"; // Placeholder or actual scheme if handled
        apiService.repayOrder(orderId, returnUrl).enqueue(new Callback<OrderDto.RepayResponse>() {
            @Override
            public void onResponse(Call<OrderDto.RepayResponse> call, Response<OrderDto.RepayResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(response.body().paymentUrl));
                    startActivity(intent);
                }
            }
            @Override public void onFailure(Call<OrderDto.RepayResponse> call, Throwable t) {}
        });
    }

    private void showRatingDialog(OrderDto.OrderItem item) {
        View dialogView = LayoutInflater.from(getContext()).inflate(R.layout.dialog_rating, null);
        RatingBar ratingBar = dialogView.findViewById(R.id.ratingBar);
        EditText edtComment = dialogView.findViewById(R.id.edtComment);
        imgRatingSelected = dialogView.findViewById(R.id.imgSelected);
        MaterialButton btnSubmit = dialogView.findViewById(R.id.btnSubmitRating);

        AlertDialog dialog = new MaterialAlertDialogBuilder(requireContext())
                .setView(dialogView)
                .create();

        imgRatingSelected.setOnClickListener(v -> {
            Intent intent = new Intent(Intent.ACTION_PICK, MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
            pickImageLauncher.launch(intent);
        });

        btnSubmit.setOnClickListener(v -> {
            submitRating(item.idSanPham, (int) ratingBar.getRating(), edtComment.getText().toString(), dialog);
        });

        dialog.show();
    }

    private final ActivityResultLauncher<Intent> pickImageLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            result -> {
                if (result.getResultCode() == Activity.RESULT_OK && result.getData() != null) {
                    selectedImageUri = result.getData().getData();
                    if (imgRatingSelected != null) {
                        imgRatingSelected.setImageURI(selectedImageUri);
                    }
                }
            }
    );

    private void submitRating(int productId, int stars, String comment, AlertDialog dialog) {
        RequestBody rbIdHoaDon = RequestBody.create(MediaType.parse("text/plain"), String.valueOf(orderId));
        RequestBody rbIdSp = RequestBody.create(MediaType.parse("text/plain"), String.valueOf(productId));
        RequestBody rbSao = RequestBody.create(MediaType.parse("text/plain"), String.valueOf(stars));
        RequestBody rbComment = RequestBody.create(MediaType.parse("text/plain"), comment);

        MultipartBody.Part imagePart = null;
        if (selectedImageUri != null) {
            File file = getFileFromUri(selectedImageUri);
            if (file != null) {
                RequestBody requestFile = RequestBody.create(MediaType.parse("image/*"), file);
                imagePart = MultipartBody.Part.createFormData("hinhAnh", file.getName(), requestFile);
            }
        }

        apiService.submitRating(rbIdHoaDon, rbIdSp, rbSao, rbComment, imagePart).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Cảm ơn bạn đã đánh giá!", Toast.LENGTH_SHORT).show();
                    dialog.dismiss();
                    loadOrderDetail();
                } else {
                    Toast.makeText(getContext(), "Lỗi khi gửi đánh giá", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Toast.makeText(getContext(), "Lỗi kết nối", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private File getFileFromUri(Uri uri) {
        try {
            InputStream inputStream = requireContext().getContentResolver().openInputStream(uri);
            File file = new File(requireContext().getCacheDir(), "rating_image.jpg");
            FileOutputStream outputStream = new FileOutputStream(file);
            byte[] buffer = new byte[1024];
            int read;
            while ((read = inputStream.read(buffer)) != -1) {
                outputStream.write(buffer, 0, read);
            }
            outputStream.flush();
            outputStream.close();
            inputStream.close();
            return file;
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
}
