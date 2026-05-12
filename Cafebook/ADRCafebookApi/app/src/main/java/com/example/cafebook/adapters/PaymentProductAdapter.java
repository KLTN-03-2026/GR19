package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.R;
import com.example.cafebook.models.GioHangItemDto;

import java.util.List;
import java.util.Locale;

public class PaymentProductAdapter extends RecyclerView.Adapter<PaymentProductAdapter.ViewHolder> {

    private final List<GioHangItemDto> items;

    public PaymentProductAdapter(List<GioHangItemDto> items) {
        this.items = items;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_payment_product, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        GioHangItemDto item = items.get(position);
        holder.tvName.setText(item.getTenSanPham());
        holder.tvQuantity.setText(String.format(Locale.getDefault(), "x%d", item.getSoLuong()));
        holder.tvPrice.setText(String.format(Locale.getDefault(), "%,.0fđ", item.getThanhTien()));
    }

    @Override
    public int getItemCount() {
        return items != null ? items.size() : 0;
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvName, tvQuantity, tvPrice;

        ViewHolder(View itemView) {
            super(itemView);
            tvName = itemView.findViewById(R.id.tvProductName);
            tvQuantity = itemView.findViewById(R.id.tvProductQuantity);
            tvPrice = itemView.findViewById(R.id.tvProductPrice);
        }
    }
}
