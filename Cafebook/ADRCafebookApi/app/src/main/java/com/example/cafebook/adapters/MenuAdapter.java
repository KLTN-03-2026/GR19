package com.example.cafebook.adapters;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.Animation;
import android.view.animation.AnimationUtils;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.engine.DiskCacheStrategy;
import com.bumptech.glide.load.resource.drawable.DrawableTransitionOptions;
import com.example.cafebook.ProductDetailActivity;
import com.example.cafebook.R;
import com.example.cafebook.models.SanPhamThucDonDto;

import java.util.List;
import java.util.Locale;

public class MenuAdapter extends RecyclerView.Adapter<MenuAdapter.ViewHolder> {
    private List<SanPhamThucDonDto> list;
    private Context context;

    public MenuAdapter(List<SanPhamThucDonDto> list, Context context) {
        this.list = list;
        this.context = context;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_menu_product, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        SanPhamThucDonDto product = list.get(position);
        holder.tvName.setText(product.getTenSanPham());
        holder.tvCategory.setText(product.getTenLoaiSP());
        holder.tvPrice.setText(String.format(Locale.getDefault(), "%,.0f đ", product.getDonGia()));

        if (product.getAnhSanPhamUrl() != null && !product.getAnhSanPhamUrl().isEmpty()) {
            Glide.with(context)
                 .load(product.getAnhSanPhamUrl())
                 .diskCacheStrategy(DiskCacheStrategy.ALL)
                 .transition(DrawableTransitionOptions.withCrossFade(400))
                 .placeholder(R.drawable.ic_launcher_background)
                 .into(holder.imgProduct);
        }

        Animation animation = AnimationUtils.loadAnimation(context, android.R.anim.fade_in);
        holder.itemView.startAnimation(animation);

        holder.itemView.setOnClickListener(v -> {
            Intent intent = new Intent(context, ProductDetailActivity.class);
            intent.putExtra("PRODUCT_ID", product.getIdSanPham());
            context.startActivity(intent);
            if (context instanceof Activity) {
                ((Activity) context).overridePendingTransition(android.R.anim.fade_in, android.R.anim.fade_out);
            }
        });

        holder.btnQuickAdd.setOnClickListener(v -> 
            Toast.makeText(context, "Chức năng thêm nhanh vào giỏ hàng đang phát triển", Toast.LENGTH_SHORT).show()
        );
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView imgProduct;
        TextView tvName, tvCategory, tvPrice;
        View btnQuickAdd;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            imgProduct = itemView.findViewById(R.id.imgProduct);
            tvName = itemView.findViewById(R.id.tvProductName);
            tvCategory = itemView.findViewById(R.id.tvCategory);
            tvPrice = itemView.findViewById(R.id.tvPrice);
            btnQuickAdd = itemView.findViewById(R.id.btnQuickAdd);
        }
    }
}
