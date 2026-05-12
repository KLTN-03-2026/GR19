package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.RatingBar;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.cafebook.R;
import com.example.cafebook.models.DanhGiaChiTietDto;

import java.util.List;

public class ReviewAdapter extends RecyclerView.Adapter<ReviewAdapter.ViewHolder> {
    private List<DanhGiaChiTietDto> list;

    public ReviewAdapter(List<DanhGiaChiTietDto> list) {
        this.list = list;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_review, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        DanhGiaChiTietDto review = list.get(position);
        holder.tvName.setText(review.getTenKhachHang());
        holder.tvDate.setText(review.getNgayTao());
        holder.tvContent.setText(review.getBinhLuan());
        holder.ratingBar.setRating(review.getSoSao());

        Glide.with(holder.itemView.getContext())
            .load(review.getAvatarKhachHang())
            .placeholder(R.drawable.default_avatar)
            .error(R.drawable.default_avatar)
            .circleCrop()
            .into(holder.imgAvatar);
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView imgAvatar;
        TextView tvName, tvDate, tvContent;
        RatingBar ratingBar;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            imgAvatar = itemView.findViewById(R.id.imgReviewAvatar);
            tvName = itemView.findViewById(R.id.tvReviewName);
            tvDate = itemView.findViewById(R.id.tvReviewDate);
            tvContent = itemView.findViewById(R.id.tvReviewContent);
            ratingBar = itemView.findViewById(R.id.ratingBarReview);
        }
    }
}
