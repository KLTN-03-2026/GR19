package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.cafebook.R;
import com.example.cafebook.models.SanPhamDto;

import java.util.List;

public class SanPhamAdapter extends RecyclerView.Adapter<SanPhamAdapter.ViewHolder> {
    private List<SanPhamDto> list;

    public SanPhamAdapter(List<SanPhamDto> list) {
        this.list = list;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_san_pham, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        SanPhamDto sp = list.get(position);
        holder.tvName.setText(sp.getTenSanPham());
        holder.tvPrice.setText(String.format("%,.0f đ", sp.getDonGia()));
        
        if (sp.getAnhSanPhamUrl() != null && !sp.getAnhSanPhamUrl().isEmpty()) {
            Glide.with(holder.itemView.getContext())
                .load(sp.getAnhSanPhamUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(holder.img);
        }
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView img;
        TextView tvName, tvPrice;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            img = itemView.findViewById(R.id.imgProduct);
            tvName = itemView.findViewById(R.id.tvProductName);
            tvPrice = itemView.findViewById(R.id.tvProductPrice);
        }
    }
}
