package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.LichSuThueSachDto;
import java.util.List;

public class RentalHistoryAdapter extends RecyclerView.Adapter<RentalHistoryAdapter.ViewHolder> {

    public interface OnDetailClickListener {
        void onDetailClick(LichSuThueSachDto.Item item);
    }

    private List<LichSuThueSachDto.Item> items;
    private OnDetailClickListener listener;

    public RentalHistoryAdapter(List<LichSuThueSachDto.Item> items, OnDetailClickListener listener) {
        this.items = items;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_rental_history, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        LichSuThueSachDto.Item item = items.get(position);

        holder.tvIdPhieu.setText("Mã phiếu: #" + item.idPhieuThueSach);
        holder.tvNgayThue.setText(item.ngayThue.replace("T", " ").substring(0, 10));
        holder.tvSoLuong.setText(item.soLuongSach + " cuốn");
        holder.tvTongCoc.setText(String.format("%,.0fđ", item.tongTienCoc));

        holder.tvStatusBadge.setText(item.trangThai);
        if ("Đang thuê".equalsIgnoreCase(item.trangThai)) {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_brown);
        } else if ("Đã trả".equalsIgnoreCase(item.trangThai)) {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_green);
        } else {
            holder.tvStatusBadge.setBackgroundResource(R.drawable.bg_badge_red);
        }

        holder.itemView.setOnClickListener(v -> listener.onDetailClick(item));
        holder.btnDetails.setOnClickListener(v -> listener.onDetailClick(item));
    }

    @Override
    public int getItemCount() {
        return items.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvIdPhieu, tvStatusBadge, tvNgayThue, tvSoLuong, tvTongCoc;
        View btnDetails;

        ViewHolder(View itemView) {
            super(itemView);
            tvIdPhieu = itemView.findViewById(R.id.tvIdPhieu);
            tvStatusBadge = itemView.findViewById(R.id.tvStatusBadge);
            tvNgayThue = itemView.findViewById(R.id.tvNgayThue);
            tvSoLuong = itemView.findViewById(R.id.tvSoLuong);
            tvTongCoc = itemView.findViewById(R.id.tvTongCoc);
            btnDetails = itemView.findViewById(R.id.btnViewDetails);
        }
    }
}
