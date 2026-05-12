package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.R;
import com.example.cafebook.models.TableBookingModels;

import java.util.List;

public class TableAdapter extends RecyclerView.Adapter<TableAdapter.ViewHolder> {
    private List<TableBookingModels.BanTrong> list;
    private OnTableClickListener listener;

    public interface OnTableClickListener {
        void onTableClick(TableBookingModels.BanTrong table);
    }

    public TableAdapter(List<TableBookingModels.BanTrong> list, OnTableClickListener listener) {
        this.list = list;
        this.listener = listener;
    }

    public void updateList(List<TableBookingModels.BanTrong> newList) {
        this.list = newList;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_table_card, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        TableBookingModels.BanTrong table = list.get(position);
        holder.tvSoBan.setText(table.soBan);
        holder.tvSoGhe.setText(String.valueOf(table.soGhe));
        holder.tvKhuVuc.setText(table.khuVuc);

        holder.itemView.setOnClickListener(v -> {
            if (listener != null) listener.onTableClick(table);
        });
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvSoBan, tvSoGhe, tvKhuVuc;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvSoBan = itemView.findViewById(R.id.tvSoBan);
            tvSoGhe = itemView.findViewById(R.id.tvSoGhe);
            tvKhuVuc = itemView.findViewById(R.id.tvKhuVucBadge);
        }
    }
}
