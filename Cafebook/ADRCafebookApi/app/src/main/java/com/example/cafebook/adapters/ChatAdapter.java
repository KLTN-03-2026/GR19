package com.example.cafebook.adapters;

import android.content.Intent;
import android.net.Uri;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.ChatMessageDto;
import com.google.android.material.button.MaterialButton;
import java.text.SimpleDateFormat;
import java.util.List;
import java.util.Locale;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class ChatAdapter extends RecyclerView.Adapter<ChatAdapter.ChatViewHolder> {

    private List<ChatMessageDto> chatList;
    private SimpleDateFormat timeFormat = new SimpleDateFormat("HH:mm", Locale.getDefault());

    public ChatAdapter(List<ChatMessageDto> chatList) { this.chatList = chatList; }

    @NonNull @Override
    public ChatViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_chat, parent, false);
        return new ChatViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ChatViewHolder holder, int position) {
        ChatMessageDto msg = chatList.get(position);

        if (msg.isTypingIndicator) {
            holder.layoutUser.setVisibility(View.GONE);
            holder.layoutBot.setVisibility(View.VISIBLE);
            holder.tvBotMessage.setText("Đang gõ...");
            holder.llActionButtons.removeAllViews();
            holder.tvBotTime.setVisibility(View.GONE);
            return;
        }

        String timeStr = msg.thoiGian != null ? timeFormat.format(msg.thoiGian) : "";

        if ("KhachHang".equals(msg.loaiTinNhan)) {
            holder.layoutBot.setVisibility(View.GONE);
            holder.layoutUser.setVisibility(View.VISIBLE);
            holder.tvUserMessage.setText(msg.noiDung);
            holder.tvUserTime.setText(timeStr);
        } else {
            holder.layoutUser.setVisibility(View.GONE);
            holder.layoutBot.setVisibility(View.VISIBLE);
            holder.tvBotTime.setVisibility(View.VISIBLE);
            holder.tvBotTime.setText(timeStr);

            // LOGIC BÓC TÁCH NÚT BẤM (REGEX)
            String rawText = msg.noiDung != null ? msg.noiDung : "";
            holder.llActionButtons.removeAllViews(); // Reset container

            Pattern pattern = Pattern.compile("\\[BUTTON:\\s*(.*?)\\s*\\|\\s*(.*?)\\]");
            Matcher matcher = pattern.matcher(rawText);

            while (matcher.find()) {
                String label = matcher.group(1).trim();
                String link = matcher.group(2).trim();

                // Tạo nút bấm động
                MaterialButton btn = new MaterialButton(holder.itemView.getContext(), null, com.google.android.material.R.attr.materialButtonOutlinedStyle);
                btn.setText(label + " \u2192");
                btn.setTextColor(holder.itemView.getContext().getResources().getColor(R.color.cf_orange));
                btn.setStrokeColorResource(R.color.cf_orange);
                btn.setCornerRadius(30);
                
                LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT);
                params.setMargins(0, 8, 0, 0);
                btn.setLayoutParams(params);

                btn.setOnClickListener(v -> {
                    Intent browserIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(link));
                    holder.itemView.getContext().startActivity(browserIntent);
                });

                holder.llActionButtons.addView(btn);
            }

            // Xóa các mã [BUTTON...] ra khỏi text hiển thị
            String cleanText = matcher.replaceAll("").trim();
            holder.tvBotMessage.setText(cleanText);
        }
    }

    @Override public int getItemCount() { return chatList.size(); }

    static class ChatViewHolder extends RecyclerView.ViewHolder {
        LinearLayout layoutBot, layoutUser, llActionButtons;
        TextView tvBotMessage, tvBotTime, tvUserMessage, tvUserTime;

        public ChatViewHolder(@NonNull View itemView) {
            super(itemView);
            layoutBot = itemView.findViewById(R.id.layoutBot);
            layoutUser = itemView.findViewById(R.id.layoutUser);
            llActionButtons = itemView.findViewById(R.id.llActionButtons);
            tvBotMessage = itemView.findViewById(R.id.tvBotMessage);
            tvBotTime = itemView.findViewById(R.id.tvBotTime);
            tvUserMessage = itemView.findViewById(R.id.tvUserMessage);
            tvUserTime = itemView.findViewById(R.id.tvUserTime);
        }
    }
}
