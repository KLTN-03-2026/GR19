package com.example.cafebook.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.adapters.RentalBookDetailAdapter;
import com.example.cafebook.models.LichSuThueSachDto;
import java.util.Locale;
import java.util.Objects;

public class RentalDetailFragment extends Fragment {

    private static final String ARG_RENTAL_ITEM = "rental_item";
    private static final String ARG_FINE_RATE = "fine_rate";

    private LichSuThueSachDto.Item rentalItem;
    private double fineRate;

    public static RentalDetailFragment newInstance(LichSuThueSachDto.Item item, double fineRate) {
        RentalDetailFragment fragment = new RentalDetailFragment();
        Bundle args = new Bundle();
        args.putSerializable(ARG_RENTAL_ITEM, item);
        args.putDouble(ARG_FINE_RATE, fineRate);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            rentalItem = (LichSuThueSachDto.Item) getArguments().getSerializable(ARG_RENTAL_ITEM);
            fineRate = getArguments().getDouble(ARG_FINE_RATE);
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_rental_detail, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        if (rentalItem == null) return;

        initHeader(view);
        initRentalInfo(view);
        initBooksList(view);
        initFinancialSummary(view);
    }

    private void initHeader(View view) {
        ImageButton btnBack = view.findViewById(R.id.btnBack);
        btnBack.setOnClickListener(v -> requireActivity().getSupportFragmentManager().popBackStack());

        TextView tvStatus = view.findViewById(R.id.tvStatusBadge);
        tvStatus.setText(rentalItem.trangThai);
        if ("Đang thuê".equalsIgnoreCase(rentalItem.trangThai)) {
            tvStatus.setBackgroundResource(R.drawable.bg_badge_brown);
        } else if ("Đã trả".equalsIgnoreCase(rentalItem.trangThai)) {
            tvStatus.setBackgroundResource(R.drawable.bg_badge_green);
        } else {
            tvStatus.setBackgroundResource(R.drawable.bg_badge_red);
        }
    }

    private void initRentalInfo(View view) {
        ((TextView) view.findViewById(R.id.tvIdPhieu)).setText("Mã phiếu: #" + rentalItem.idPhieuThueSach);
        ((TextView) view.findViewById(R.id.tvNgayThue)).setText(formatDate(rentalItem.ngayThue));
        ((TextView) view.findViewById(R.id.tvHanTra)).setText(formatDate(rentalItem.ngayHenTra));

        if (rentalItem.ngayTra != null && !rentalItem.ngayTra.isEmpty()) {
            view.findViewById(R.id.lblNgayTra).setVisibility(View.VISIBLE);
            TextView tvNgayTra = view.findViewById(R.id.tvNgayTra);
            tvNgayTra.setVisibility(View.VISIBLE);
            tvNgayTra.setText(formatDate(rentalItem.ngayTra));
        }
    }

    private void initBooksList(View view) {
        RecyclerView rvBooks = view.findViewById(R.id.rvBooks);
        rvBooks.setLayoutManager(new LinearLayoutManager(getContext()));
        rvBooks.setAdapter(new RentalBookDetailAdapter(rentalItem.chiTietSachs));
    }

    private void initFinancialSummary(View view) {
        ((TextView) view.findViewById(R.id.tvTienCoc)).setText(String.format(Locale.getDefault(), "%,.0fđ", rentalItem.tongTienCoc));
        ((TextView) view.findViewById(R.id.tvPhiThue)).setText(String.format(Locale.getDefault(), "%,.0fđ", Objects.requireNonNullElse(rentalItem.tongPhiThue, 0.0)));
        ((TextView) view.findViewById(R.id.tvTienPhat)).setText(String.format(Locale.getDefault(), "%,.0fđ", Objects.requireNonNullElse(rentalItem.tongTienPhat, 0.0)));
        ((TextView) view.findViewById(R.id.tvCocHoan)).setText(String.format(Locale.getDefault(), "%,.0fđ", Objects.requireNonNullElse(rentalItem.tongTienCocHoan, 0.0)));

        if (rentalItem.laSoTienTamTinh) {
            view.findViewById(R.id.tvNoteEstimate).setVisibility(View.VISIBLE);
        }
    }

    private String formatDate(String dateStr) {
        if (dateStr == null || dateStr.length() < 10) return "-";
        // Convert 2026-05-10T... to 10/05/2026
        try {
            String parts[] = dateStr.substring(0, 10).split("-");
            if (parts.length == 3) {
                return parts[2] + "/" + parts[1] + "/" + parts[0];
            }
        } catch (Exception ignored) {}
        return dateStr.substring(0, 10).replace("-", "/");
    }
}
