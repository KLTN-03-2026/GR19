package com.example.cafebook.adapters;

import android.content.Context;
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
import com.example.cafebook.R;
import com.example.cafebook.models.TimKiemSachCardDto;

import java.util.List;
import java.util.Locale;

public class BookSearchAdapter extends RecyclerView.Adapter<BookSearchAdapter.ViewHolder> {
    private List<TimKiemSachCardDto> list;
    private Context context;

    public BookSearchAdapter(List<TimKiemSachCardDto> list, Context context) {
        this.list = list;
        this.context = context;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_book_search_card, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        TimKiemSachCardDto book = list.get(position);
        holder.tvTitle.setText(book.getTieuDe());
        holder.tvPrice.setText(String.format(Locale.getDefault(), "%,.0f đ", book.getGiaBia()));

        if (book.getAnhBiaUrl() != null && !book.getAnhBiaUrl().isEmpty()) {
            Glide.with(context)
                .load(book.getAnhBiaUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(holder.imgCover);
        }

        holder.itemView.setOnClickListener(v -> {
            Intent intent = new Intent(context, BookDetailActivity.class);
            intent.putExtra("BOOK_ID", book.getIdSach());
            context.startActivity(intent);
        });
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView imgCover;
        TextView tvTitle, tvPrice;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            imgCover = itemView.findViewById(R.id.imgBookCover);
            tvTitle = itemView.findViewById(R.id.tvBookTitle);
            tvPrice = itemView.findViewById(R.id.tvBookPrice);
        }
    }
}
