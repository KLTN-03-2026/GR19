package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.SupportDto;
import java.util.List;

public class SupportSessionAdapter extends RecyclerView.Adapter<SupportSessionAdapter.SessionViewHolder> {

    private List<SupportDto.ChatSession> sessionList;
    private OnSessionClickListener listener;

    public interface OnSessionClickListener {
        void onSessionClick(SupportDto.ChatSession session);
    }

    public SupportSessionAdapter(List<SupportDto.ChatSession> sessionList, OnSessionClickListener listener) {
        this.sessionList = sessionList;
        this.listener = listener;
    }

    @NonNull
    @Override
    public SessionViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_support_session, parent, false);
        return new SessionViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull SessionViewHolder holder, int position) {
        SupportDto.ChatSession session = sessionList.get(position);
        holder.tvSessionTitle.setText(session.title);
        
        String lastActive = session.lastActive;
        if (lastActive != null && lastActive.contains("T")) {
            lastActive = lastActive.replace("T", " ").substring(0, 16);
        }
        holder.tvLastActive.setText("Hoạt động: " + lastActive);

        holder.itemView.setOnClickListener(v -> listener.onSessionClick(session));
        
        // Nếu có IdThongBao > 0 thì đổi màu indicator (Đang có nhân viên)
        if (session.idThongBao != null && session.idThongBao > 0) {
            holder.indicatorStatus.setBackgroundTintList(holder.itemView.getContext().getResources().getColorStateList(R.color.cf_orange));
        } else {
            holder.indicatorStatus.setBackgroundTintList(holder.itemView.getContext().getResources().getColorStateList(R.color.cf_dark_brown));
        }
    }

    @Override
    public int getItemCount() {
        return sessionList.size();
    }

    static class SessionViewHolder extends RecyclerView.ViewHolder {
        TextView tvSessionTitle, tvLastActive;
        View indicatorStatus;

        public SessionViewHolder(@NonNull View itemView) {
            super(itemView);
            tvSessionTitle = itemView.findViewById(R.id.tvSessionTitle);
            tvLastActive = itemView.findViewById(R.id.tvLastActive);
            indicatorStatus = itemView.findViewById(R.id.indicatorStatus);
        }
    }
}
