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
import com.google.android.material.button.MaterialButton;
import com.google.android.material.imageview.ShapeableImageView;
import java.util.List;

public class OrderProductAdapter extends RecyclerView.Adapter<OrderProductAdapter.ViewHolder> {

    public interface OnRateClickListener {
        void onRateClick(OrderDto.OrderItem item);
    }

    private List<OrderDto.OrderItem> items;
    private boolean isCompleted;
    private OnRateClickListener rateListener;

    public OrderProductAdapter(List<OrderDto.OrderItem> items, boolean isCompleted, OnRateClickListener rateListener) {
        this.items = items;
        this.isCompleted = isCompleted;
        this.rateListener = rateListener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_order_product, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        OrderDto.OrderItem item = items.get(position);

        holder.tvProductName.setText(item.tenSanPham);
        holder.tvProductQuantity.setText("Số lượng: " + item.soLuong);
        holder.tvProductPrice.setText(String.format("%,.0fđ", item.donGia));

        Glide.with(holder.itemView.getContext())
                .load(item.hinhAnhUrl)
                .placeholder(R.drawable.default_food_icon)
                .error(R.drawable.default_food_icon)
                .into(holder.imgProduct);

        if (isCompleted) {
            if (item.daDanhGia) {
                holder.btnRate.setVisibility(View.GONE);
                holder.tvRatedLabel.setVisibility(View.VISIBLE);
            } else {
                holder.btnRate.setVisibility(View.VISIBLE);
                holder.tvRatedLabel.setVisibility(View.GONE);
                holder.btnRate.setOnClickListener(v -> rateListener.onRateClick(item));
            }
        } else {
            holder.btnRate.setVisibility(View.GONE);
            holder.tvRatedLabel.setVisibility(View.GONE);
        }
    }

    @Override
    public int getItemCount() {
        return items.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvProductName, tvProductQuantity, tvProductPrice, tvRatedLabel;
        ShapeableImageView imgProduct;
        MaterialButton btnRate;

        ViewHolder(View itemView) {
            super(itemView);
            tvProductName = itemView.findViewById(R.id.tvProductName);
            tvProductQuantity = itemView.findViewById(R.id.tvProductQuantity);
            tvProductPrice = itemView.findViewById(R.id.tvProductPrice);
            tvRatedLabel = itemView.findViewById(R.id.tvRatedLabel);
            imgProduct = itemView.findViewById(R.id.imgProduct);
            btnRate = itemView.findViewById(R.id.btnRate);
        }
    }
}
