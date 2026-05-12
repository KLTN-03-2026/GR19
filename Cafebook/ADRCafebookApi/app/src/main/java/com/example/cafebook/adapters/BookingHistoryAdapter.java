package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.LichSuDatBanDto;
import com.google.android.material.button.MaterialButton;
import java.util.List;

public class BookingHistoryAdapter extends RecyclerView.Adapter<BookingHistoryAdapter.ViewHolder> {

    public interface OnCancelClickListener {
        void onCancel(LichSuDatBanDto.Item item);
    }

    private List<LichSuDatBanDto.Item> items;
    private OnCancelClickListener cancelListener;

    public BookingHistoryAdapter(List<LichSuDatBanDto.Item> items, OnCancelClickListener cancelListener) {
        this.items = items;
        this.cancelListener = cancelListener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_booking_history, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        LichSuDatBanDto.Item item = items.get(position);
        
        holder.tvBookingId.setText("#" + item.idPhieuDatBan);
        holder.tvTableName.setText(item.tenBan);
        holder.tvGuestCount.setText(item.soLuongKhach + " khách");
        
        // Format time (assuming ISO 8601 from API)
        String displayTime = item.thoiGianDat.replace("T", " ").substring(0, 16);
        holder.tvDateTime.setText(displayTime);

        holder.tvStatusBadge.setText(item.trangThai);
        
        // Update badge color based on status
        if ("Đã xác nhận".equals(item.trangThai)) {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_green);
        } else if ("Đã hủy".equals(item.trangThai)) {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_red);
        } else if ("Chờ xác nhận".equals(item.trangThai)) {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_orange);
        } else {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_gray);
        }

        // Only show cancel button if status allows
        if ("Chờ xác nhận".equals(item.trangThai) || "Đã xác nhận".equals(item.trangThai)) {
            holder.btnCancel.setVisibility(View.VISIBLE);
            holder.btnCancel.setOnClickListener(v -> cancelListener.onCancel(item));
        } else {
            holder.btnCancel.setVisibility(View.GONE);
        }
    }

    @Override
    public int getItemCount() {
        return items.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvBookingId, tvStatusBadge, tvTableName, tvDateTime, tvGuestCount;
        MaterialButton btnCancel;

        ViewHolder(View itemView) {
            super(itemView);
            tvBookingId = itemView.findViewById(R.id.tvBookingId);
            tvStatusBadge = itemView.findViewById(R.id.tvStatusBadge);
            tvTableName = itemView.findViewById(R.id.tvTableName);
            tvDateTime = itemView.findViewById(R.id.tvDateTime);
            tvGuestCount = itemView.findViewById(R.id.tvGuestCount);
            btnCancel = itemView.findViewById(R.id.btnCancel);
        }
    }
}
