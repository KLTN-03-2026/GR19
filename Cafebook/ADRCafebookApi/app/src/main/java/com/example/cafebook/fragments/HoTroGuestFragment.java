package com.example.cafebook.fragments;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.R;
import com.example.cafebook.models.ChatMessageDto;
import com.example.cafebook.models.HoTroDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.HoTroApiService;
import com.example.cafebook.adapters.ChatAdapter;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.UUID;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class HoTroGuestFragment extends Fragment {

    private RecyclerView rvChat;
    private EditText edtMessage;
    private FloatingActionButton btnSend;
    
    private ChatAdapter adapter;
    private List<ChatMessageDto> chatList = new ArrayList<>();
    private HoTroApiService apiService;
    private String guestSessionId;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_ho_tro, container, false);

        rvChat = view.findViewById(R.id.rvChat);
        edtMessage = view.findViewById(R.id.edtMessage);
        btnSend = view.findViewById(R.id.btnSend);

        apiService = ApiClient.getClient().create(HoTroApiService.class);
        
        setupSession();
        setupRecyclerView();
        
        loadHistory();

        btnSend.setOnClickListener(v -> sendMessage());

        return view;
    }

    private void setupSession() {
        SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookApp", Context.MODE_PRIVATE);
        guestSessionId = prefs.getString("GuestChatSession", "");
        if (guestSessionId.isEmpty()) {
            guestSessionId = "guest_adr_" + UUID.randomUUID().toString().replace("-", "").substring(0, 8);
            prefs.edit().putString("GuestChatSession", guestSessionId).apply();
        }
    }

    private void setupRecyclerView() {
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        layoutManager.setStackFromEnd(true); 
        rvChat.setLayoutManager(layoutManager);
        adapter = new ChatAdapter(chatList);
        rvChat.setAdapter(adapter);
    }

    private void scrollToBottom() {
        if (!chatList.isEmpty()) {
            rvChat.smoothScrollToPosition(chatList.size() - 1);
        }
    }

    private void loadHistory() {
        apiService.getHistory(guestSessionId).enqueue(new Callback<List<ChatMessageDto>>() {
            @Override
            public void onResponse(Call<List<ChatMessageDto>> call, Response<List<ChatMessageDto>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    chatList.clear();
                    chatList.addAll(response.body());
                    
                    if (chatList.isEmpty()) {
                        ChatMessageDto welcome = new ChatMessageDto(false);
                        welcome.loaiTinNhan = "AI";
                        welcome.noiDung = "Chào bạn! Mình là AI Cafebook. Mình có thể giúp gì được cho bạn ạ?";
                        welcome.thoiGian = new Date();
                        chatList.add(welcome);
                    }
                    
                    adapter.notifyDataSetChanged();
                    scrollToBottom();
                }
            }
            @Override public void onFailure(Call<List<ChatMessageDto>> call, Throwable t) { }
        });
    }

    private void sendMessage() {
        String text = edtMessage.getText().toString().trim();
        if (text.isEmpty()) return;

        edtMessage.setText("");
        edtMessage.setEnabled(false);
        btnSend.setEnabled(false);

        ChatMessageDto userMsg = new ChatMessageDto(false);
        userMsg.loaiTinNhan = "KhachHang";
        userMsg.noiDung = text;
        userMsg.thoiGian = new Date();
        chatList.add(userMsg);

        ChatMessageDto typingIndicator = new ChatMessageDto(true);
        chatList.add(typingIndicator);
        
        adapter.notifyItemRangeInserted(chatList.size() - 2, 2);
        scrollToBottom();

        HoTroDto.SendRequest request = new HoTroDto.SendRequest(text, guestSessionId);
        apiService.sendMessage(request).enqueue(new Callback<HoTroDto.SendResponse>() {
            @Override
            public void onResponse(Call<HoTroDto.SendResponse> call, Response<HoTroDto.SendResponse> response) {
                removeTypingIndicator();
                unlockInput();

                if (response.isSuccessful() && response.body() != null && response.body().tinNhanPhanHoi != null) {
                    chatList.add(response.body().tinNhanPhanHoi);
                    adapter.notifyItemInserted(chatList.size() - 1);
                    scrollToBottom();
                }
            }

            @Override
            public void onFailure(Call<HoTroDto.SendResponse> call, Throwable t) {
                removeTypingIndicator();
                unlockInput();
                
                ChatMessageDto errorMsg = new ChatMessageDto(false);
                errorMsg.loaiTinNhan = "AI";
                errorMsg.noiDung = "Đã có lỗi xảy ra khi kết nối đến AI. Vui lòng thử lại sau.";
                errorMsg.thoiGian = new Date();
                chatList.add(errorMsg);
                adapter.notifyItemInserted(chatList.size() - 1);
                scrollToBottom();
            }
        });
    }

    private void removeTypingIndicator() {
        if (!chatList.isEmpty() && chatList.get(chatList.size() - 1).isTypingIndicator) {
            chatList.remove(chatList.size() - 1);
            adapter.notifyItemRemoved(chatList.size());
        }
    }

    private void unlockInput() {
        edtMessage.setEnabled(true);
        btnSend.setEnabled(true);
    }
}
