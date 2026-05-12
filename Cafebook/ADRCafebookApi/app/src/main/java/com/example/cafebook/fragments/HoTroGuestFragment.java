package com.example.cafebook.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.MainActivity;
import com.example.cafebook.R;
import com.example.cafebook.models.ChatMessageDto;
import com.example.cafebook.models.HoTroDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.HoTroApiService;
import com.example.cafebook.adapters.ChatAdapter;
import com.example.cafebook.utils.SessionManager;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class HoTroGuestFragment extends Fragment {

    // Cache lại tin nhắn vào RAM để hiện ngay lập tức khi quay lại
    private static List<ChatMessageDto> ramChatHistory = new ArrayList<>();

    public static void clearRamCache() {
        ramChatHistory.clear();
    }

    private RecyclerView rvChat;
    private EditText edtMessage;
    private FloatingActionButton btnSend;
    private ChatAdapter adapter;
    private List<ChatMessageDto> chatList = new ArrayList<>();
    private HoTroApiService apiService;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_ho_tro, container, false);

        rvChat = view.findViewById(R.id.rvChat);
        edtMessage = view.findViewById(R.id.edtMessage);
        btnSend = view.findViewById(R.id.btnSend);

        Toolbar toolbar = view.findViewById(R.id.toolbarChat);
        toolbar.setNavigationIcon(R.drawable.ic_arrow_back);
        toolbar.setNavigationOnClickListener(v -> getParentFragmentManager().popBackStack());

        apiService = ApiClient.getClient(requireContext()).create(HoTroApiService.class);

        if (getActivity() instanceof MainActivity) {
            ((MainActivity) getActivity()).setBottomNavigationVisibility(View.GONE);
        }

        setupRecyclerView();
        loadHistory();

        btnSend.setOnClickListener(v -> sendMessage());

        return view;
    }

    private void setupRecyclerView() {
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        layoutManager.setStackFromEnd(true); 
        rvChat.setLayoutManager(layoutManager);
        adapter = new ChatAdapter(chatList);
        rvChat.setAdapter(adapter);

        rvChat.addOnLayoutChangeListener((v, left, top, right, bottom, oldLeft, oldTop, oldRight, oldBottom) -> {
            if (bottom < oldBottom) rvChat.postDelayed(this::scrollToBottom, 100);
        });
    }

    private void scrollToBottom() {
        if (!chatList.isEmpty()) rvChat.smoothScrollToPosition(chatList.size() - 1);
    }

    private void loadHistory() {
        // 1. PHỤC HỒI NGAY LẬP TỨC TỪ RAM CHỐNG TRẮNG MÀN HÌNH
        if (!ramChatHistory.isEmpty()) {
            chatList.clear();
            chatList.addAll(ramChatHistory);
            adapter.notifyDataSetChanged();
            scrollToBottom();
        }

        // 2. LẤY SESSION ID TỪ Ổ CỨNG (SESSIONMANAGER)
        String sessionId = SessionManager.getGuestSessionId(requireContext());
        
        apiService.getHistory(sessionId).enqueue(new Callback<List<ChatMessageDto>>() {
            @Override
            public void onResponse(Call<List<ChatMessageDto>> call, Response<List<ChatMessageDto>> response) {
                if (!isAdded() || getContext() == null) return;
                if (response.isSuccessful() && response.body() != null) {
                    List<ChatMessageDto> history = response.body();
                    
                    if (history.isEmpty()) {
                        if (chatList.isEmpty()) {
                            ChatMessageDto welcome = new ChatMessageDto(false);
                            welcome.loaiTinNhan = "AI";
                            welcome.noiDung = "Chào bạn! Mình là AI Cafebook. Mình có thể giúp gì được cho bạn ạ?";
                            welcome.thoiGian = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
                            chatList.add(welcome);
                        }
                    } else {
                        chatList.clear();
                        chatList.addAll(history);
                    }
                    
                    // Cập nhật lại RAM cache
                    ramChatHistory.clear();
                    ramChatHistory.addAll(chatList);

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
        userMsg.thoiGian = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
        chatList.add(userMsg);

        ChatMessageDto typingIndicator = new ChatMessageDto(true);
        chatList.add(typingIndicator);
        
        adapter.notifyItemRangeInserted(chatList.size() - 2, 2);
        scrollToBottom();

        // Lưu tạm vào RAM Cache để tránh mất tin nhắn vừa gửi nếu thoát ra ngay
        ramChatHistory.clear();
        ramChatHistory.addAll(chatList);

        String sessionId = SessionManager.getGuestSessionId(requireContext());
        HoTroDto.SendRequest request = new HoTroDto.SendRequest(text, sessionId);
        apiService.sendMessage(request).enqueue(new Callback<HoTroDto.SendResponse>() {
            @Override
            public void onResponse(Call<HoTroDto.SendResponse> call, Response<HoTroDto.SendResponse> response) {
                if (!isAdded() || getContext() == null) return;
                removeTypingIndicator();
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);

                if (response.isSuccessful() && response.body() != null && response.body().tinNhanPhanHoi != null) {
                    chatList.add(response.body().tinNhanPhanHoi);
                    
                    // Cập nhật RAM Cache khi có phản hồi AI
                    ramChatHistory.clear();
                    ramChatHistory.addAll(chatList);

                    adapter.notifyItemInserted(chatList.size() - 1);
                    scrollToBottom();
                }
            }

            @Override
            public void onFailure(@NonNull Call<HoTroDto.SendResponse> call, @NonNull Throwable t) {
                if (!isAdded() || getContext() == null) return;
                removeTypingIndicator();
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);
            }
        });
    }

    private void removeTypingIndicator() {
        if (!chatList.isEmpty() && chatList.get(chatList.size() - 1).isTypingIndicator) {
            chatList.remove(chatList.size() - 1);
            adapter.notifyItemRemoved(chatList.size());
        }
    }

    @Override
    public void onDestroyView() {
        if (getActivity() instanceof MainActivity) {
            ((MainActivity) getActivity()).setBottomNavigationVisibility(View.VISIBLE);
        }
        super.onDestroyView();
    }
}
