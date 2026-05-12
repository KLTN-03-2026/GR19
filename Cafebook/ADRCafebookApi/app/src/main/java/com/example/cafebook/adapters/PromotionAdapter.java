package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.R;
import com.example.cafebook.models.GioHangKhuyenMaiDto;
import com.google.android.material.button.MaterialButton;

import java.util.List;

public class PromotionAdapter extends RecyclerView.Adapter<PromotionAdapter.ViewHolder> {

    private List<GioHangKhuyenMaiDto> list;
    private OnPromotionSelectedListener listener;

    public interface OnPromotionSelectedListener {
        void onSelected(GioHangKhuyenMaiDto promo);
    }

    public PromotionAdapter(List<GioHangKhuyenMaiDto> list, OnPromotionSelectedListener listener) {
        this.list = list;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_voucher_card, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        GioHangKhuyenMaiDto promo = list.get(position);
        holder.tvName.setText(promo.getTenChuongTrinh());
        holder.tvCondition.setText(promo.getDieukienApDung());
        
        if (promo.isEligible()) {
            holder.tvIneligible.setVisibility(View.GONE);
            holder.btnSelect.setEnabled(true);
            holder.itemView.setAlpha(1.0f);
        } else {
            holder.tvIneligible.setVisibility(View.VISIBLE);
            holder.tvIneligible.setText(promo.getIneligibilityReason());
            holder.btnSelect.setEnabled(false);
            holder.itemView.setAlpha(0.6f);
        }

        holder.btnSelect.setOnClickListener(v -> listener.onSelected(promo));
    }

    @Override
    public int getItemCount() {
        return list != null ? list.size() : 0;
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvName, tvCondition, tvIneligible, tvExpiry;
        MaterialButton btnSelect;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvName = itemView.findViewById(R.id.tvPromoName);
            tvCondition = itemView.findViewById(R.id.tvPromoCondition);
            tvIneligible = itemView.findViewById(R.id.tvIneligibleReason);
            tvExpiry = itemView.findViewById(R.id.tvExpiry);
            btnSelect = itemView.findViewById(R.id.btnSelectPromo);
        }
    }
}
