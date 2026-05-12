package com.example.cafebook.adapters;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.bumptech.glide.load.resource.drawable.DrawableTransitionOptions;
import com.example.cafebook.R;
import com.example.cafebook.models.SachCardDto;

import java.util.List;

public class BookAdapter extends RecyclerView.Adapter<BookAdapter.ViewHolder> {
    private List<SachCardDto> bookList;
    private Context context;

    public BookAdapter(List<SachCardDto> bookList, Context context) {
        this.bookList = bookList;
        this.context = context;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_book_card, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        SachCardDto book = bookList.get(position);
        
        holder.tvTitle.setText(book.getTieuDe());
        holder.tvAuthor.setText(book.getTacGia() != null ? book.getTacGia() : "Không rõ tác giả");
        holder.tvStock.setText("Sẵn có: " + book.getSoLuongCoSan());

        if (book.getAnhBiaUrl() != null && !book.getAnhBiaUrl().isEmpty()) {
            Glide.with(context)
                 .load(book.getAnhBiaUrl())
                 .transition(DrawableTransitionOptions.withCrossFade())
                 .placeholder(R.drawable.ic_launcher_background)
                 .into(holder.imgCover);
        }
    }

    @Override
    public int getItemCount() {
        return bookList != null ? bookList.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView imgCover;
        TextView tvTitle, tvAuthor, tvStock;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            imgCover = itemView.findViewById(R.id.imgBookCover);
            tvTitle = itemView.findViewById(R.id.tvBookTitle);
            tvAuthor = itemView.findViewById(R.id.tvAuthor);
            tvStock = itemView.findViewById(R.id.tvStock);
        }
    }
}
