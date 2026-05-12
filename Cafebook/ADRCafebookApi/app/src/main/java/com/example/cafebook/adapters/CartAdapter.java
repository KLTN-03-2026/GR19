package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.cafebook.R;
import com.example.cafebook.models.GioHangItemDto;

import java.util.List;
import java.util.Locale;

public class CartAdapter extends RecyclerView.Adapter<CartAdapter.CartViewHolder> {

    private List<GioHangItemDto> items;
    private OnCartItemChangeListener listener;

    public interface OnCartItemChangeListener {
        void onQuantityChange(int idSanPham, int newQuantity);
        void onRemoveItem(int idSanPham);
    }

    public CartAdapter(List<GioHangItemDto> items, OnCartItemChangeListener listener) {
        this.items = items;
        this.listener = listener;
    }

    public void updateItems(List<GioHangItemDto> newItems) {
        this.items = newItems;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public CartViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_cart, parent, false);
        return new CartViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull CartViewHolder holder, int position) {
        GioHangItemDto item = items.get(position);
        holder.tvName.setText(item.getTenSanPham());
        holder.tvPrice.setText(String.format(Locale.getDefault(), "%,.0f đ", item.getDonGia()));
        holder.tvQuantity.setText(String.valueOf(item.getSoLuong()));

        if (item.isOutOfStock()) {
            holder.tvOutOfStock.setVisibility(View.VISIBLE);
            holder.tvOutOfStock.setText(item.getOutOfStockMessage());
        } else {
            holder.tvOutOfStock.setVisibility(View.GONE);
        }

        Glide.with(holder.itemView.getContext())
                .load(item.getHinhAnhUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(holder.ivProduct);

        holder.btnPlus.setOnClickListener(v -> listener.onQuantityChange(item.getIdSanPham(), item.getSoLuong() + 1));
        holder.btnMinus.setOnClickListener(v -> {
            if (item.getSoLuong() > 1) {
                listener.onQuantityChange(item.getIdSanPham(), item.getSoLuong() - 1);
            }
        });
        holder.btnRemove.setOnClickListener(v -> listener.onRemoveItem(item.getIdSanPham()));
    }

    @Override
    public int getItemCount() {
        return items != null ? items.size() : 0;
    }

    static class CartViewHolder extends RecyclerView.ViewHolder {
        ImageView ivProduct;
        TextView tvName, tvPrice, tvQuantity, tvOutOfStock;
        ImageButton btnPlus, btnMinus, btnRemove;

        public CartViewHolder(@NonNull View itemView) {
            super(itemView);
            ivProduct = itemView.findViewById(R.id.ivProductImage);
            tvName = itemView.findViewById(R.id.tvProductName);
            tvPrice = itemView.findViewById(R.id.tvProductPrice);
            tvQuantity = itemView.findViewById(R.id.tvQuantity);
            tvOutOfStock = itemView.findViewById(R.id.tvOutOfStockWarning);
            btnPlus = itemView.findViewById(R.id.btnPlus);
            btnMinus = itemView.findViewById(R.id.btnMinus);
            btnRemove = itemView.findViewById(R.id.btnRemove);
        }
    }
}
