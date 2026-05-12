package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.LichSuThueSachDto;
import java.util.List;
import java.util.Locale;

public class RentalBookDetailAdapter extends RecyclerView.Adapter<RentalBookDetailAdapter.ViewHolder> {

    private List<LichSuThueSachDto.Detail> items;

    public RentalBookDetailAdapter(List<LichSuThueSachDto.Detail> items) {
        this.items = items;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_rental_book_detail_full, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        LichSuThueSachDto.Detail detail = items.get(position);
        
        holder.tvBookName.setText(detail.tenSach);
        
        String condition = "Độ mới: " + detail.doMoiKhiThue + "%";
        if (detail.doMoiKhiTra != null) {
            condition += " -> " + detail.doMoiKhiTra + "%";
        }
        holder.tvCondition.setText(condition);
        
        double fineForBook = detail.tienPhatTre + detail.tienPhatHuHong;
        holder.tvFine.setText("Phạt: " + String.format(Locale.getDefault(), "%,.0fđ", fineForBook));
        
        String note = "Ghi chú: " + detail.ghiChuKhiThue;
        if (detail.ghiChuKhiTra != null && !"-".equals(detail.ghiChuKhiTra)) {
            note += " | Trả: " + detail.ghiChuKhiTra;
        }
        holder.tvNote.setText(note);
    }

    @Override
    public int getItemCount() {
        return items != null ? items.size() : 0;
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvBookName, tvCondition, tvNote, tvFine;

        ViewHolder(View itemView) {
            super(itemView);
            tvBookName = itemView.findViewById(R.id.tvBookName);
            tvCondition = itemView.findViewById(R.id.tvCondition);
            tvNote = itemView.findViewById(R.id.tvNote);
            tvFine = itemView.findViewById(R.id.tvFine);
        }
    }
}
