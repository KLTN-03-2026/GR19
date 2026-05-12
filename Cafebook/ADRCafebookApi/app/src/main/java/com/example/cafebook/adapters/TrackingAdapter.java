package com.example.cafebook.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.OrderDto;
import java.util.List;

public class TrackingAdapter extends RecyclerView.Adapter<TrackingAdapter.ViewHolder> {

    private List<OrderDto.TrackingEvent> events;

    public TrackingAdapter(List<OrderDto.TrackingEvent> events) {
        this.events = events;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_tracking_event, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        OrderDto.TrackingEvent event = events.get(position);

        holder.tvStatus.setText(event.status);
        holder.tvDesc.setText(event.description);
        holder.tvTime.setText(event.timestamp != null ? event.timestamp.replace("T", " ").substring(0, 16) : "");

        // Dot color and line visibility
        if (event.isCurrent) {
            holder.imgDot.setImageResource(R.drawable.bg_badge_orange);
            holder.tvStatus.setTextColor(holder.itemView.getContext().getResources().getColor(R.color.cf_orange));
        } else {
            holder.imgDot.setImageResource(R.drawable.bg_circle_brown);
            holder.tvStatus.setTextColor(holder.itemView.getContext().getResources().getColor(R.color.cf_dark_brown));
        }

        holder.viewLineTop.setVisibility(position == 0 ? View.INVISIBLE : View.VISIBLE);
        holder.viewLineBottom.setVisibility(position == events.size() - 1 ? View.INVISIBLE : View.VISIBLE);
    }

    @Override
    public int getItemCount() {
        return events.size();
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvStatus, tvDesc, tvTime;
        ImageView imgDot;
        View viewLineTop, viewLineBottom;

        ViewHolder(View itemView) {
            super(itemView);
            tvStatus = itemView.findViewById(R.id.tvTrackingStatus);
            tvDesc = itemView.findViewById(R.id.tvTrackingDesc);
            tvTime = itemView.findViewById(R.id.tvTrackingTime);
            imgDot = itemView.findViewById(R.id.imgDot);
            viewLineTop = itemView.findViewById(R.id.viewLineTop);
            viewLineBottom = itemView.findViewById(R.id.viewLineBottom);
        }
    }
}
