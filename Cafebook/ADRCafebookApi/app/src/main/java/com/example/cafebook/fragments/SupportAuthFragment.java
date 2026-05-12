package com.example.cafebook.fragments;

import android.os.Bundle;
import android.transition.Slide;
import android.transition.TransitionManager;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.adapters.ChatAdapter;
import com.example.cafebook.adapters.SupportSessionAdapter;
import com.example.cafebook.models.ChatMessageDto;
import com.example.cafebook.models.SupportDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.SupportApiService;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.UUID;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class SupportAuthFragment extends Fragment {

    private FrameLayout rootContainer;
    private LinearLayout layoutSessionList;
    private ConstraintLayout layoutChatRoom;
    
    private TextView tvChatTitle;
    private RecyclerView rvSessions, rvChatMessages;
    private EditText edtMessage;
    private FloatingActionButton btnSend;
    
    private SupportApiService apiService;
    private String currentSessionId = "";
    
    private List<ChatMessageDto> chatList = new ArrayList<>();
    private ChatAdapter chatAdapter;
    
    private List<SupportDto.ChatSession> sessionList = new ArrayList<>();
    private SupportSessionAdapter sessionAdapter;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_support_auth, container, false);

        rootContainer = view.findViewById(R.id.rootContainer);
        layoutSessionList = view.findViewById(R.id.layoutSessionList);
        layoutChatRoom = view.findViewById(R.id.layoutChatRoom);
        tvChatTitle = view.findViewById(R.id.tvChatTitle);
        rvSessions = view.findViewById(R.id.rvSessions);
        rvChatMessages = view.findViewById(R.id.rvChatMessages);
        edtMessage = view.findViewById(R.id.edtMessage);
        btnSend = view.findViewById(R.id.btnSend);

        apiService = ApiClient.getClient(requireContext()).create(SupportApiService.class);

        view.findViewById(R.id.btnNewChat).setOnClickListener(v -> {
            String newSessionId = "session_" + UUID.randomUUID().toString().substring(0, 8);
            openChatRoom(newSessionId, "Cuộc trò chuyện mới");
        });

        view.findViewById(R.id.btnBackToList).setOnClickListener(v -> closeChatRoom());

        btnSend.setOnClickListener(v -> sendMessage());

        setupChatRecyclerView();
        setupSessionRecyclerView();
        loadSessions();

        return view;
    }

    private void setupSessionRecyclerView() {
        rvSessions.setLayoutManager(new LinearLayoutManager(getContext()));
        sessionAdapter = new SupportSessionAdapter(sessionList, session -> {
            openChatRoom(session.sessionId, session.title);
        });
        rvSessions.setAdapter(sessionAdapter);
    }

    private void openChatRoom(String sessionId, String title) {
        currentSessionId = sessionId;
        tvChatTitle.setText(title);

        Slide slide = new Slide(Gravity.END);
        slide.setDuration(300);
        TransitionManager.beginDelayedTransition(rootContainer, slide);
        
        layoutSessionList.setVisibility(View.GONE);
        layoutChatRoom.setVisibility(View.VISIBLE);

        loadChatHistory();
    }

    private void closeChatRoom() {
        Slide slide = new Slide(Gravity.START);
        slide.setDuration(300);
        TransitionManager.beginDelayedTransition(rootContainer, slide);

        layoutChatRoom.setVisibility(View.GONE);
        layoutSessionList.setVisibility(View.VISIBLE);
        
        loadSessions(); 
    }

    private void loadSessions() {
        apiService.getSessions().enqueue(new Callback<List<SupportDto.ChatSession>>() {
            @Override
            public void onResponse(Call<List<SupportDto.ChatSession>> call, Response<List<SupportDto.ChatSession>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    sessionList.clear();
                    sessionList.addAll(response.body());
                    sessionAdapter.notifyDataSetChanged();
                }
            }
            @Override public void onFailure(Call<List<SupportDto.ChatSession>> call, Throwable t) {
                Toast.makeText(getContext(), "Không thể tải danh sách cuộc trò chuyện", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void loadChatHistory() {
        chatList.clear();
        chatAdapter.notifyDataSetChanged();

        apiService.getHistory(currentSessionId).enqueue(new Callback<List<ChatMessageDto>>() {
            @Override
            public void onResponse(Call<List<ChatMessageDto>> call, Response<List<ChatMessageDto>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    chatList.addAll(response.body());
                    chatAdapter.notifyDataSetChanged();
                    scrollToBottom();
                }
            }
            @Override public void onFailure(Call<List<ChatMessageDto>> call, Throwable t) {
                Toast.makeText(getContext(), "Không thể tải lịch sử trò chuyện", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void sendMessage() {
        String text = edtMessage.getText().toString().trim();
        if (text.isEmpty()) return;

        edtMessage.setText("");
        edtMessage.setEnabled(false);
        btnSend.setEnabled(false);

        ChatMessageDto userMsg = new ChatMessageDto();
        userMsg.loaiTinNhan = "KhachHang";
        userMsg.noiDung = text;
        userMsg.thoiGian = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
        chatList.add(userMsg);
        
        ChatMessageDto typingIndicator = new ChatMessageDto(true);
        chatList.add(typingIndicator);
        
        chatAdapter.notifyItemRangeInserted(chatList.size() - 2, 2);
        scrollToBottom();

        SupportDto.SendRequest req = new SupportDto.SendRequest(text, currentSessionId);
        apiService.sendMessage(req).enqueue(new Callback<SupportDto.SendResponse>() {
            @Override
            public void onResponse(Call<SupportDto.SendResponse> call, Response<SupportDto.SendResponse> response) {
                removeTypingIndicator();
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);
                
                if (response.isSuccessful() && response.body() != null) {
                    if (response.body().tinNhanPhanHoi != null) {
                        chatList.add(response.body().tinNhanPhanHoi);
                        chatAdapter.notifyItemInserted(chatList.size() - 1);
                        scrollToBottom();
                    }
                }
            }
            @Override public void onFailure(Call<SupportDto.SendResponse> call, Throwable t) {
                removeTypingIndicator();
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);
                Toast.makeText(getContext(), "Lỗi gửi tin nhắn", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setupChatRecyclerView() {
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        layoutManager.setStackFromEnd(true);
        rvChatMessages.setLayoutManager(layoutManager);
        chatAdapter = new ChatAdapter(chatList);
        rvChatMessages.setAdapter(chatAdapter);
        
        rvChatMessages.addOnLayoutChangeListener((v, left, top, right, bottom, oldLeft, oldTop, oldRight, oldBottom) -> {
            if (bottom < oldBottom) {
                rvChatMessages.postDelayed(this::scrollToBottom, 100);
            }
        });
    }

    private void scrollToBottom() {
        if (!chatList.isEmpty()) {
            rvChatMessages.smoothScrollToPosition(chatList.size() - 1);
        }
    }

    private void removeTypingIndicator() {
        if (!chatList.isEmpty() && chatList.get(chatList.size() - 1).isTypingIndicator) {
            chatList.remove(chatList.size() - 1);
            chatAdapter.notifyItemRemoved(chatList.size());
        }
    }
}
