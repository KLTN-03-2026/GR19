package com.example.cafebook.fragments;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.transition.Slide;
import android.transition.TransitionManager;
import android.util.Log;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.cafebook.MainActivity;
import com.example.cafebook.R;
import com.example.cafebook.adapters.ChatAdapter;
import com.example.cafebook.adapters.SupportSessionAdapter;
import com.example.cafebook.models.ChatMessageDto;
import com.example.cafebook.models.SupportDto;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.network.SupportApiService;
import com.example.cafebook.utils.SessionManager;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;
import io.reactivex.rxjava3.core.Single;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class SupportAuthFragment extends Fragment {

    // ==========================================
    // RAM CACHE - GIÚP VÀO LẠI KHÔNG BỊ TRẮNG MÀN HÌNH
    // ==========================================
    private static List<SupportDto.ChatSession> ramSessionList = new ArrayList<>();
    private static List<ChatMessageDto> ramChatList = new ArrayList<>();
    private static java.util.Map<String, List<ChatMessageDto>> sessionChatCache = new java.util.HashMap<>();
    private static String ramCurrentSessionId = "";
    private static String ramChatTitle = "Trò chuyện hỗ trợ";
    private static Integer ramCurrentIdThongBao = null;

    public static void clearRamCache() {
        ramSessionList.clear();
        ramChatList.clear();
        sessionChatCache.clear();
        ramCurrentSessionId = "";
        ramChatTitle = "Trò chuyện hỗ trợ";
        ramCurrentIdThongBao = null;
    }

    private FrameLayout rootContainer;
    private LinearLayout layoutSessionList;
    private ConstraintLayout layoutChatRoom;
    private TextView tvChatTitle;
    private RecyclerView rvSessions, rvChatMessages;
    private EditText edtMessage;
    private FloatingActionButton btnSend;
    
    private SupportApiService apiService;
    private String currentSessionId = "";
    private Integer currentIdThongBaoHoTro = null;
    private int idKhachHang = 0;
    private String jwtToken = "";

    private HubConnection hubConnection;
    private List<ChatMessageDto> chatList = new ArrayList<>();
    private ChatAdapter chatAdapter;
    private List<SupportDto.ChatSession> sessionList = new ArrayList<>();
    private SupportSessionAdapter sessionAdapter;

    private final Handler reconnectHandler = new Handler(Looper.getMainLooper());

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

        view.findViewById(R.id.btnNewChat).setOnClickListener(v -> createNewChat());
        view.findViewById(R.id.btnBackToList).setOnClickListener(v -> closeChatRoom());
        btnSend.setOnClickListener(v -> sendMessage());

        android.content.SharedPreferences prefs = requireContext().getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
        idKhachHang = prefs.getInt("USER_ID", 0);
        jwtToken = prefs.getString("JWT_TOKEN", "");

        setupChatRecyclerView();
        setupSessionRecyclerView();
        setupSignalR(); 

        // PHỤC HỒI TỪ RAM
        if (!ramSessionList.isEmpty()) {
            sessionList.clear();
            sessionList.addAll(ramSessionList);
            sessionAdapter.notifyDataSetChanged();
        }

        String activeRoomId = SessionManager.getSupportChatRoomId();
        if (activeRoomId != null) {
            currentSessionId = activeRoomId;
            currentIdThongBaoHoTro = ramCurrentIdThongBao;
            tvChatTitle.setText(ramChatTitle);
            
            if (!ramChatList.isEmpty()) {
                chatList.clear();
                chatList.addAll(ramChatList);
                chatAdapter.notifyDataSetChanged();
                scrollToBottom();
            }

            layoutSessionList.setVisibility(View.GONE);
            layoutChatRoom.setVisibility(View.VISIBLE);
            
            // ẨN BOTTOM NAV NGAY LẬP TỨC KHI PHỤC HỒI PHÒNG CHAT
            if (getActivity() instanceof MainActivity) {
                ((MainActivity) getActivity()).setBottomNavigationVisibility(View.GONE);
            }
            
            loadChatHistory();
            ensureSignalRConnected();
        } else {
            loadSessions(); 
        }

        return view;
    }

    private void setupSignalR() {
        if (hubConnection != null) return;

        String rawUrl = ApiClient.BASE_URL;
        if (rawUrl.endsWith("api/")) {
            rawUrl = rawUrl.substring(0, rawUrl.length() - 4);
        }
        String hubUrl = rawUrl + "chatHub";

        hubConnection = HubConnectionBuilder.create(hubUrl)
                .withAccessTokenProvider(Single.defer(() -> Single.just(jwtToken)))
                .build();

        hubConnection.on("ReceiveMessage", (msg) -> {
            if (getActivity() == null) return;
            getActivity().runOnUiThread(() -> {
                // ĐỒNG BỘ ĐA THIẾT BỊ: Vẫn nhận tin 'KhachHang' nếu không phải từ chính App này gửi (lọc qua idChat)
                if (!chatListContains(msg.idChat)) {
                    if (msg.idThongBaoHoTro != null) {
                        currentIdThongBaoHoTro = msg.idThongBaoHoTro;
                        ramCurrentIdThongBao = msg.idThongBaoHoTro;
                    }
                    chatList.add(msg);
                    updateRamChatHistory();
                    chatAdapter.notifyItemInserted(chatList.size() - 1);
                    scrollToBottom();
                }
            });
        }, ChatMessageDto.class);

        hubConnection.on("ReloadTicketList", () -> {
            if (getActivity() == null) return;
            getActivity().runOnUiThread(this::loadSessions);
        });

        hubConnection.onClosed((exception) -> {
            if (isAdded()) {
                Log.d("SIGNALR", "Mất kết nối, đang reconnect...");
                reconnectHandler.postDelayed(this::ensureSignalRConnected, 3000);
            }
        });
    }

    private void ensureSignalRConnected() {
        if (hubConnection == null) return;

        if (hubConnection.getConnectionState() == HubConnectionState.DISCONNECTED) {
            hubConnection.start().subscribe(
                () -> {
                    Log.d("SIGNALR", "Kết nối Socket thành công!");
                    joinCurrentGroup();
                },
                throwable -> {
                    Log.e("SIGNALR", "Lỗi kết nối, thử lại sau 5s", throwable);
                    reconnectHandler.postDelayed(this::ensureSignalRConnected, 5000);
                }
            );
        } else if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            joinCurrentGroup();
        }
    }

    private void joinCurrentGroup() {
        if (hubConnection != null && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            String groupName;
            if (idKhachHang > 0) {
                groupName = "khach_" + idKhachHang;
            } else if (currentSessionId != null && !currentSessionId.isEmpty()) {
                groupName = "guest_" + currentSessionId;
            } else {
                Log.w("SIGNALR", "Không thể Join phòng: Thiếu ID");
                return;
            }

            hubConnection.send("JoinGroup", groupName);
            Log.d("SIGNALR", "Đã Join phòng: " + groupName);
        }
    }

    private boolean chatListContains(long idChat) {
        if (idChat <= 0) return false;
        for (ChatMessageDto m : chatList) { if (m.idChat == idChat) return true; }
        return false;
    }

    private void createNewChat() {
        String newSessionId = "session_" + java.util.UUID.randomUUID().toString().replace("-", "").substring(0, 10);
        ramChatList.clear(); 
        ramCurrentIdThongBao = null;
        
        // Ẩn Bottom Navigation ngay lập tức
        if (getActivity() instanceof MainActivity) {
            ((MainActivity) getActivity()).setBottomNavigationVisibility(View.GONE);
        }

        // Thêm ngay vào RAM Session List để hiện ở Sidebar
        SupportDto.ChatSession newSession = new SupportDto.ChatSession();
        newSession.sessionId = newSessionId;
        newSession.title = "Cuộc trò chuyện mới";
        newSession.lastActive = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
        newSession.idThongBao = null;
        
        // Chèn vào đầu danh sách
        sessionList.add(0, newSession);
        ramSessionList.clear();
        ramSessionList.addAll(sessionList);
        if (sessionAdapter != null) sessionAdapter.notifyItemInserted(0);

        openChatRoom(newSessionId, "Cuộc trò chuyện mới");
    }

    private void loadSessions() {
        if (!isAdded()) return;
        String guestId = SessionManager.getGuestSessionId(requireContext());
        apiService.getSessions(guestId).enqueue(new Callback<List<SupportDto.ChatSession>>() {
            @Override
            public void onResponse(Call<List<SupportDto.ChatSession>> call, Response<List<SupportDto.ChatSession>> response) {
                if (!isAdded() || getContext() == null) return;
                if (response.isSuccessful() && response.body() != null) {
                    List<SupportDto.ChatSession> body = response.body();
                    
                    // Lấy ra các session AI hiện có trong RAM (chưa có Ticket)
                    List<SupportDto.ChatSession> localAiSessions = new ArrayList<>();
                    for (SupportDto.ChatSession s : ramSessionList) {
                        if (s.idThongBao == null) localAiSessions.add(s);
                    }

                    sessionList.clear();
                    
                    // 1. Ưu tiên các session từ Server
                    sessionList.addAll(body);

                    // 2. Kiểm tra và giữ lại session AI mới tạo trong RAM nếu Server chưa kịp trả về
                    for (SupportDto.ChatSession local : localAiSessions) {
                        boolean exists = false;
                        for (SupportDto.ChatSession remote : body) {
                            if (local.sessionId.equals(remote.sessionId)) {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists) {
                            sessionList.add(0, local); // Chèn lên đầu
                        }
                    }

                    ramSessionList.clear();
                    ramSessionList.addAll(sessionList);
                    sessionAdapter.notifyDataSetChanged();
                }
            }
            @Override public void onFailure(Call<List<SupportDto.ChatSession>> call, Throwable t) { }
        });
    }

    private void setupSessionRecyclerView() {
        rvSessions.setLayoutManager(new LinearLayoutManager(getContext()));
        sessionAdapter = new SupportSessionAdapter(sessionList, session -> {
            openChatRoom(session.sessionId, session.title);
        });
        rvSessions.setAdapter(sessionAdapter);
    }

    private void setupChatRecyclerView() {
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        layoutManager.setStackFromEnd(true);
        rvChatMessages.setLayoutManager(layoutManager);
        chatAdapter = new ChatAdapter(chatList);
        rvChatMessages.setAdapter(chatAdapter);
        
        rvChatMessages.addOnLayoutChangeListener((v, left, top, right, bottom, oldLeft, oldTop, oldRight, oldBottom) -> {
            if (bottom < oldBottom) rvChatMessages.postDelayed(this::scrollToBottom, 100);
        });
    }

    private void openChatRoom(String sessionId, String title) {
        Log.d("CHAT_DEBUG", "Mở phòng: " + sessionId + " | Cache size: " + sessionChatCache.size());
        currentSessionId = sessionId;
        ramCurrentSessionId = sessionId;
        ramChatTitle = title;
        SessionManager.setSupportChatRoomId(sessionId);
        tvChatTitle.setText(title);
        if (sessionAdapter != null) sessionAdapter.setCurrentSelectedSessionId(sessionId);

        if (getActivity() instanceof MainActivity) {
            ((MainActivity) getActivity()).setBottomNavigationVisibility(View.GONE);
        }

        layoutSessionList.setVisibility(View.GONE);
        layoutChatRoom.setVisibility(View.VISIBLE);

        // PHỤC HỒI TIN NHẮN TỪ CACHE
        chatList.clear(); // Làm sạch list hiện tại trước
        if (sessionChatCache.containsKey(sessionId)) {
            List<ChatMessageDto> cachedMessages = sessionChatCache.get(sessionId);
            if (cachedMessages != null) {
                chatList.addAll(cachedMessages);
                Log.d("CHAT_DEBUG", "Đã phục hồi từ cache: " + cachedMessages.size() + " tin nhắn");
            }
        }
        
        chatAdapter.notifyDataSetChanged();
        scrollToBottom();

        loadChatHistory();
        ensureSignalRConnected();
    }

    private void closeChatRoom() {
        if (hubConnection != null) hubConnection.stop();
        SessionManager.setSupportChatRoomId(null);
        // KHÔNG xóa ramChatList ở đây để giữ lại tin nhắn khi quay lại (giống khách vãng lai)

        if (getActivity() instanceof MainActivity) {
            ((MainActivity) getActivity()).setBottomNavigationVisibility(View.VISIBLE);
        }

        Slide slide = new Slide(Gravity.START);
        slide.setDuration(300);
        TransitionManager.beginDelayedTransition(rootContainer, slide);

        layoutChatRoom.setVisibility(View.GONE);
        layoutSessionList.setVisibility(View.VISIBLE);
        
        loadSessions(); 
    }

    private void loadChatHistory() {
        final String loadingSessionId = currentSessionId; // Lưu lại ID lúc bắt đầu load
        apiService.getHistory(currentSessionId).enqueue(new Callback<List<ChatMessageDto>>() {
            @Override
            public void onResponse(Call<List<ChatMessageDto>> call, Response<List<ChatMessageDto>> response) {
                if (!isAdded() || getContext() == null) return;
                
                // KIỂM TRA: Nếu đã chuyển sang phòng khác thì không ghi đè dữ liệu phòng này
                if (!loadingSessionId.equals(currentSessionId)) return;

                if (response.isSuccessful() && response.body() != null) {
                    List<ChatMessageDto> history = response.body();
                    
                    // Chỉ cập nhật nếu có dữ liệu mới từ Server
                    // Hoặc nếu hiện tại đang trống (để tránh xóa mất cache RAM khi Server chưa load xong)
                    if (!history.isEmpty() || chatList.isEmpty()) {
                        chatList.clear();
                        chatList.addAll(history);
                        
                        if (!chatList.isEmpty()) {
                            for (int i = chatList.size() - 1; i >= 0; i--) {
                                if (chatList.get(i).idThongBaoHoTro != null) {
                                    currentIdThongBaoHoTro = chatList.get(i).idThongBaoHoTro;
                                    break;
                                }
                            }
                        }

                        updateRamChatHistory();
                        chatAdapter.notifyDataSetChanged();
                        scrollToBottom();
                    }
                }
            }
            @Override public void onFailure(Call<List<ChatMessageDto>> call, Throwable t) { }
        });
    }

    private void updateRamChatHistory() {
        if (currentSessionId != null && !currentSessionId.isEmpty()) {
            sessionChatCache.put(currentSessionId, new ArrayList<>(chatList));
            Log.d("CHAT_DEBUG", "Đã lưu cache cho: " + currentSessionId + " | Số tin: " + chatList.size());
        }
        ramChatList.clear();
        ramChatList.addAll(chatList);
    }

    private void sendMessage() {
        String text = edtMessage.getText().toString().trim();
        if (text.isEmpty()) return;

        edtMessage.setEnabled(false);
        btnSend.setEnabled(false);

        // REAL-TIME: Gửi qua socket nếu đã có nhân viên (ticket ID)
        if (currentIdThongBaoHoTro != null && hubConnection != null && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            sendMessageRealtime(text);
        } else {
            sendMessageLegacy(text);
        }
    }

    private void sendMessageRealtime(String text) {
        edtMessage.setText("");
        String groupName = (idKhachHang > 0) ? "khach_" + idKhachHang : "guest_" + currentSessionId;
        
        ChatMessageDto userMsg = new ChatMessageDto();
        userMsg.loaiTinNhan = "KhachHang";
        userMsg.noiDung = text;
        userMsg.thoiGian = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
        userMsg.idChat = System.currentTimeMillis(); // Gán ID tạm để lọc trùng
        
        chatList.add(userMsg);
        updateRamChatHistory();
        chatAdapter.notifyItemInserted(chatList.size() - 1);
        scrollToBottom();

        hubConnection.send("SendMessageFromClient", groupName, text, idKhachHang, currentSessionId, currentIdThongBaoHoTro);
        
        edtMessage.postDelayed(() -> {
            if (isAdded()) {
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);
                edtMessage.requestFocus();
            }
        }, 300);
    }

    private void sendMessageLegacy(String text) {
        edtMessage.setText("");

        ChatMessageDto userMsg = new ChatMessageDto();
        userMsg.loaiTinNhan = "KhachHang";
        userMsg.noiDung = text;
        userMsg.thoiGian = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault()).format(new Date());
        userMsg.idChat = System.currentTimeMillis();
        
        chatList.add(userMsg);
        ChatMessageDto typingIndicator = new ChatMessageDto(true);
        chatList.add(typingIndicator);
        
        updateRamChatHistory();
        chatAdapter.notifyItemRangeInserted(chatList.size() - 2, 2);
        scrollToBottom();

        SupportDto.SendRequest req = new SupportDto.SendRequest(text, currentSessionId);
        apiService.sendMessage(req).enqueue(new Callback<SupportDto.SendResponse>() {
            @Override
            public void onResponse(Call<SupportDto.SendResponse> call, Response<SupportDto.SendResponse> response) {
                if (!isAdded() || getContext() == null) return;
                removeTypingIndicator();
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);
                
                if (response.isSuccessful() && response.body() != null) {
                    currentIdThongBaoHoTro = response.body().idThongBaoHoTro;
                    ramCurrentIdThongBao = currentIdThongBaoHoTro;
                    
                    if (response.body().tinNhanPhanHoi != null) {
                        chatList.add(response.body().tinNhanPhanHoi);
                        updateRamChatHistory();
                        chatAdapter.notifyItemInserted(chatList.size() - 1);
                        scrollToBottom();
                    }
                    
                    if (response.body().daChuyenNhanVien) {
                        ramChatTitle = "Yêu cầu hỗ trợ";
                        tvChatTitle.setText(ramChatTitle);
                        ensureSignalRConnected();
                        loadSessions();   
                    }
                }
            }
            @Override public void onFailure(Call<SupportDto.SendResponse> call, Throwable t) {
                if (!isAdded() || getContext() == null) return;
                removeTypingIndicator();
                edtMessage.setEnabled(true);
                btnSend.setEnabled(true);
            }
        });
    }

    private void scrollToBottom() {
        if (!chatList.isEmpty()) rvChatMessages.smoothScrollToPosition(chatList.size() - 1);
    }

    private void removeTypingIndicator() {
        if (!chatList.isEmpty() && chatList.get(chatList.size() - 1).isTypingIndicator) {
            chatList.remove(chatList.size() - 1);
            chatAdapter.notifyItemRemoved(chatList.size());
        }
    }

    @Override
    public void onDestroyView() {
        reconnectHandler.removeCallbacksAndMessages(null);
        if (hubConnection != null) hubConnection.stop();
        super.onDestroyView();
    }
}
