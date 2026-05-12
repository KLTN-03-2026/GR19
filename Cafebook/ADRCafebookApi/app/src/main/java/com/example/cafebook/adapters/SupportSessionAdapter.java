package com.example.cafebook.adapters;

import android.util.TypedValue;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.SupportDto;
import com.google.android.material.card.MaterialCardView;
import java.util.List;

public class SupportSessionAdapter extends RecyclerView.Adapter<SupportSessionAdapter.SessionViewHolder> {

    private List<SupportDto.ChatSession> sessionList;
    private OnSessionClickListener listener;
    private String currentSelectedSessionId = "";

    public interface OnSessionClickListener {
        void onSessionClick(SupportDto.ChatSession session);
    }

    public SupportSessionAdapter(List<SupportDto.ChatSession> sessionList, OnSessionClickListener listener) {
        this.sessionList = sessionList;
        this.listener = listener;
    }

    public void setCurrentSelectedSessionId(String sessionId) {
        this.currentSelectedSessionId = sessionId;
        notifyDataSetChanged();
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
        } else if (lastActive == null) {
            lastActive = "Vừa xong";
        }
        holder.tvLastActive.setText(lastActive);

        // KIỂM TRA TRẠNG THÁI: Nếu là phiên chat đang mở
        if (session.sessionId != null && session.sessionId.equals(currentSelectedSessionId)) {
            TypedValue typedValue = new TypedValue();
            holder.itemView.getContext().getTheme().resolveAttribute(com.google.android.material.R.attr.colorPrimaryContainer, typedValue, true);
            holder.cardSession.setCardBackgroundColor(typedValue.data);

            holder.itemView.getContext().getTheme().resolveAttribute(com.google.android.material.R.attr.colorSecondary, typedValue, true);
            holder.cardSession.setStrokeColor(typedValue.data);
            holder.cardSession.setStrokeWidth(3); // Viền dày lên
        } else {
            TypedValue typedValue = new TypedValue();
            holder.itemView.getContext().getTheme().resolveAttribute(com.google.android.material.R.attr.colorSurface, typedValue, true);
            holder.cardSession.setCardBackgroundColor(typedValue.data);

            holder.itemView.getContext().getTheme().resolveAttribute(com.google.android.material.R.attr.colorOutline, typedValue, true);
            holder.cardSession.setStrokeColor(typedValue.data);
            holder.cardSession.setStrokeWidth(1);
        }

        holder.itemView.setOnClickListener(v -> {
            currentSelectedSessionId = session.sessionId;
            notifyDataSetChanged();
            listener.onSessionClick(session);
        });
    }

    @Override
    public int getItemCount() {
        return sessionList.size();
    }

    static class SessionViewHolder extends RecyclerView.ViewHolder {
        TextView tvSessionTitle, tvLastActive;
        MaterialCardView cardSession;

        public SessionViewHolder(@NonNull View itemView) {
            super(itemView);
            tvSessionTitle = itemView.findViewById(R.id.tvSessionTitle);
            tvLastActive = itemView.findViewById(R.id.tvLastActive);
            cardSession = itemView.findViewById(R.id.cardSession);
        }
    }
}
