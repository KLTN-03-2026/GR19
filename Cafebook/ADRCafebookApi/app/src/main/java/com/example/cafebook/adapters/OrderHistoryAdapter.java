package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.bumptech.glide.Glide;
import com.example.cafebook.R;
import com.example.cafebook.models.OrderDto;
import com.google.android.material.imageview.ShapeableImageView;
import java.util.List;

public class OrderHistoryAdapter extends RecyclerView.Adapter<OrderHistoryAdapter.ViewHolder> {

    public interface OnItemClickListener {
        void onItemClick(OrderDto.HistoryItem item);
    }

    private List<OrderDto.HistoryItem> items;
    private OnItemClickListener listener;

    public OrderHistoryAdapter(List<OrderDto.HistoryItem> items, OnItemClickListener listener) {
        this.items = items;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_order_history, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        OrderDto.HistoryItem item = items.get(position);

        holder.tvOrderId.setText(item.maDonHang);
        holder.tvOrderDate.setText(item.thoiGianTao != null ? item.thoiGianTao.replace("T", " ").substring(0, 16) : "");
        holder.tvProductName.setText(item.tenSanPham);
        
        if (item.soLuongKhach > 0) {
            holder.tvOtherCount.setVisibility(View.VISIBLE);
            holder.tvOtherCount.setText("và " + item.soLuongKhach + " sản phẩm khác");
        } else {
            holder.tvOtherCount.setVisibility(View.GONE);
        }

        holder.tvTotalAmount.setText(String.format("%,.0fđ", item.thanhTien));
        holder.tvPaymentMethod.setText("Thanh toán: " + item.phuongThucThanhToan);
        
        String displayStatus = item.trangThaiGiaoHang;
        if ("Đã hủy".equalsIgnoreCase(item.trangThaiThanhToan)) {
            displayStatus = "Đã hủy";
        }
        holder.tvOrderStatus.setText(displayStatus);

        // Badge colors based on status
        if ("Hoàn thành".equalsIgnoreCase(displayStatus)) {
            holder.tvOrderStatus.setBackgroundResource(R.drawable.bg_badge_green);
        } else if ("Đã hủy".equalsIgnoreCase(displayStatus)) {
            holder.tvOrderStatus.setBackgroundResource(R.drawable.bg_badge_red);
        } else if ("Đang giao".equalsIgnoreCase(displayStatus)) {
            holder.tvOrderStatus.setBackgroundResource(R.drawable.bg_badge_orange);
        } else {
            holder.tvOrderStatus.setBackgroundResource(R.drawable.bg_badge_gray);
        }

        Glide.with(holder.itemView.getContext())
                .load(item.hinhAnhUrl)
                .placeholder(R.drawable.ic_menu)
                .error(R.drawable.ic_menu)
                .into(holder.imgProduct);

        holder.itemView.setOnClickListener(v -> listener.onItemClick(item));
    }

    @Override
    public int getItemCount() {
        return items.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvOrderId, tvOrderDate, tvProductName, tvOtherCount, tvTotalAmount, tvPaymentMethod, tvOrderStatus;
        ShapeableImageView imgProduct;

        ViewHolder(View itemView) {
            super(itemView);
            tvOrderId = itemView.findViewById(R.id.tvOrderId);
            tvOrderDate = itemView.findViewById(R.id.tvOrderDate);
            tvProductName = itemView.findViewById(R.id.tvProductName);
            tvOtherCount = itemView.findViewById(R.id.tvOtherCount);
            tvTotalAmount = itemView.findViewById(R.id.tvTotalAmount);
            tvPaymentMethod = itemView.findViewById(R.id.tvPaymentMethod);
            tvOrderStatus = itemView.findViewById(R.id.tvOrderStatus);
            imgProduct = itemView.findViewById(R.id.imgProduct);
        }
    }
}
