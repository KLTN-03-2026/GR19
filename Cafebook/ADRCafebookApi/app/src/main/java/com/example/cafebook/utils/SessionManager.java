package com.example.cafebook.utils;

import com.example.cafebook.models.ProfileDto;

public class SessionManager {
    // 1. Session vãng lai: Tồn tại lâu dài (dùng SharedPreferences để không bị mất khi chuyển fragment)
    private static final String PREF_NAME = "CafebookSessionPrefs";
    private static final String KEY_GUEST_SESSION = "GUEST_SESSION_ID";

    // 2. Phòng chat Khách hàng đang mở: Để đi qua tab khác về lại đúng phòng đó
    private static String supportChatRoomId = null; 

    private static ProfileDto.Overview cachedOverview = null;
    private static ProfileDto.Info cachedInfo = null;

    public static String getGuestSessionId(android.content.Context context) {
        android.content.SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, android.content.Context.MODE_PRIVATE);
        String sessionId = prefs.getString(KEY_GUEST_SESSION, null);
        if (sessionId == null) {
            sessionId = "session_" + java.util.UUID.randomUUID().toString().replace("-", "").substring(0, 10);
            prefs.edit().putString(KEY_GUEST_SESSION, sessionId).apply();
        }
        return sessionId;
    }

    public static String getSupportChatRoomId() {
        return supportChatRoomId;
    }

    public static void setSupportChatRoomId(String id) {
        supportChatRoomId = id;
    }

    public static void clearSession() {
        supportChatRoomId = null;
        cachedOverview = null;
        cachedInfo = null;
    }

    public static void clearOnAppStart(android.content.Context context) {
        // Xóa mã vãng lai cũ để tạo phiên mới khi khởi động lại App
        context.getSharedPreferences(PREF_NAME, android.content.Context.MODE_PRIVATE)
               .edit().remove(KEY_GUEST_SESSION).apply();
        
        // Xóa bộ nhớ tạm của các Fragment Chat
        com.example.cafebook.fragments.HoTroGuestFragment.clearRamCache();
        com.example.cafebook.fragments.SupportAuthFragment.clearRamCache();

        clearSession();
    }

    public static ProfileDto.Overview getCachedOverview() { return cachedOverview; }
    public static void setCachedOverview(ProfileDto.Overview overview) { cachedOverview = overview; }
    public static ProfileDto.Info getCachedInfo() { return cachedInfo; }
    public static void setCachedInfo(ProfileDto.Info info) { cachedInfo = info; }
}
