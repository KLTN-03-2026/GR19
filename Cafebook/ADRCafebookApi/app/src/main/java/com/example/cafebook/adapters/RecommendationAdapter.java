package com.example.cafebook.adapters;

import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.cafebook.BookDetailActivity;
import com.example.cafebook.ProductDetailActivity;
import com.example.cafebook.R;

import java.util.List;

public class RecommendationAdapter extends RecyclerView.Adapter<RecommendationAdapter.ViewHolder> {
    
    public interface RecommendationItem {
        int getId();
        String getTitle();
        String getImageUrl();
        boolean isBook();
    }

    private List<RecommendationItem> list;

    public RecommendationAdapter(List<RecommendationItem> list) {
        this.list = list;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_recommendation_small, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        RecommendationItem item = list.get(position);
        holder.tvTitle.setText(item.getTitle());

        if (item.getImageUrl() != null && !item.getImageUrl().isEmpty()) {
            Glide.with(holder.itemView.getContext())
                .load(item.getImageUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(holder.imgThumb);
        }

        holder.itemView.setOnClickListener(v -> {
            Intent intent;
            if (item.isBook()) {
                intent = new Intent(v.getContext(), BookDetailActivity.class);
                intent.putExtra("BOOK_ID", item.getId());
            } else {
                intent = new Intent(v.getContext(), ProductDetailActivity.class);
                intent.putExtra("PRODUCT_ID", item.getId());
            }
            v.getContext().startActivity(intent);
        });
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView imgThumb;
        TextView tvTitle;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            imgThumb = itemView.findViewById(R.id.imgRecThumb);
            tvTitle = itemView.findViewById(R.id.tvRecTitle);
        }
    }
}
