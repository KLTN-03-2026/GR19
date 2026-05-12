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
import com.example.cafebook.models.SachDto;

import java.util.List;

public class SachAdapter extends RecyclerView.Adapter<SachAdapter.ViewHolder> {
    private List<SachDto> list;

    public SachAdapter(List<SachDto> list) {
        this.list = list;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_sach, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        SachDto sach = list.get(position);
        holder.tvTitle.setText(sach.getTieuDe());
        holder.tvAuthor.setText(sach.getTacGia());

        if (sach.getAnhBiaUrl() != null && !sach.getAnhBiaUrl().isEmpty()) {
            Glide.with(holder.itemView.getContext())
                .load(sach.getAnhBiaUrl())
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
        TextView tvTitle, tvAuthor;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            img = itemView.findViewById(R.id.imgBook);
            tvTitle = itemView.findViewById(R.id.tvBookTitle);
            tvAuthor = itemView.findViewById(R.id.tvBookAuthor);
        }
    }
}
