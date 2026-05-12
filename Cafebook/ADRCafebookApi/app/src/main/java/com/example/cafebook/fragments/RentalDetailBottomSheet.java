package com.example.cafebook.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.example.cafebook.R;
import com.example.cafebook.models.LichSuThueSachDto;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;
import java.io.Serializable;

public class RentalDetailBottomSheet extends BottomSheetDialogFragment {

    private LichSuThueSachDto.Item rentalItem;

    public static RentalDetailBottomSheet newInstance(LichSuThueSachDto.Item item) {
        RentalDetailBottomSheet fragment = new RentalDetailBottomSheet();
        Bundle args = new Bundle();
        args.putSerializable("RENTAL_ITEM", (Serializable) item);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            rentalItem = (LichSuThueSachDto.Item) getArguments().getSerializable("RENTAL_ITEM");
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_rental_detail_bottom_sheet, container, false);
        initViews(view);
        return view;
    }

    private void initViews(View view) {
        if (rentalItem == null) return;

        TextView tvStatus = view.findViewById(R.id.tvStatusBadge);
        tvStatus.setText(rentalItem.trangThai);
        if ("Đang thuê".equalsIgnoreCase(rentalItem.trangThai)) {
            tvStatus.setBackgroundResource(R.drawable.bg_badge_brown);
        } else if ("Đã trả".equalsIgnoreCase(rentalItem.trangThai)) {
            tvStatus.setBackgroundResource(R.drawable.bg_badge_green);
        } else {
            tvStatus.setBackgroundResource(R.drawable.bg_badge_red);
        }

        ((TextView) view.findViewById(R.id.tvIdPhieu)).setText("#" + rentalItem.idPhieuThueSach);
        ((TextView) view.findViewById(R.id.tvNgayThue)).setText(formatDate(rentalItem.ngayThue));
        ((TextView) view.findViewById(R.id.tvHanTra)).setText(formatDate(rentalItem.ngayHenTra));

        if (rentalItem.ngayTra != null && !rentalItem.ngayTra.isEmpty()) {
            view.findViewById(R.id.lblNgayTra).setVisibility(View.VISIBLE);
            TextView tvNgayTra = view.findViewById(R.id.tvNgayTra);
            tvNgayTra.setVisibility(View.VISIBLE);
            tvNgayTra.setText(formatDate(rentalItem.ngayTra));
        }

        // Books list
        LinearLayout llBooks = view.findViewById(R.id.llBooksContainer);
        if (rentalItem.chiTietSachs != null) {
            for (LichSuThueSachDto.Detail detail : rentalItem.chiTietSachs) {
                View itemView = getLayoutInflater().inflate(R.layout.item_rental_book_detail, llBooks, false);
                ((TextView) itemView.findViewById(R.id.tvBookName)).setText(detail.tenSach);
                
                String condition = "Độ mới: " + detail.doMoiKhiThue + "%";
                if (detail.doMoiKhiTra != null) {
                    condition += " -> " + detail.doMoiKhiTra + "%";
                }
                ((TextView) itemView.findViewById(R.id.tvCondition)).setText(condition);
                
                double fineForBook = detail.tienPhatTre + detail.tienPhatHuHong;
                ((TextView) itemView.findViewById(R.id.tvFine)).setText("Phạt: " + String.format("%,.0fđ", fineForBook));
                
                String note = "Ghi chú: " + detail.ghiChuKhiThue;
                if (detail.ghiChuKhiTra != null && !detail.ghiChuKhiTra.equals("-")) {
                    note += " | Trả: " + detail.ghiChuKhiTra;
                }
                ((TextView) itemView.findViewById(R.id.tvNote)).setText(note);
                
                llBooks.addView(itemView);
            }
        }

        // Financials
        ((TextView) view.findViewById(R.id.tvTienCoc)).setText(String.format("%,.0fđ", rentalItem.tongTienCoc));
        ((TextView) view.findViewById(R.id.tvPhiThue)).setText(String.format("%,.0fđ", rentalItem.tongPhiThue != null ? rentalItem.tongPhiThue : 0));
        ((TextView) view.findViewById(R.id.tvTienPhat)).setText(String.format("%,.0fđ", rentalItem.tongTienPhat != null ? rentalItem.tongTienPhat : 0));
        ((TextView) view.findViewById(R.id.tvCocHoan)).setText(String.format("%,.0fđ", rentalItem.tongTienCocHoan != null ? rentalItem.tongTienCocHoan : 0));

        if (rentalItem.laSoTienTamTinh) {
            view.findViewById(R.id.tvNoteEstimate).setVisibility(View.VISIBLE);
        }

        view.findViewById(R.id.btnClose).setOnClickListener(v -> dismiss());
    }

    private String formatDate(String dateStr) {
        if (dateStr == null || dateStr.length() < 10) return "-";
        return dateStr.substring(0, 10).replace("-", "/");
    }
}
