package com.example.cafebook.utils;

import java.util.UUID;

public class SessionManager {
    private static String guestSessionId = null;

    public static String getGuestSessionId() {
        if (guestSessionId == null) {
            guestSessionId = "guest_adr_" + UUID.randomUUID().toString().replace("-", "").substring(0, 8);
        }
        return guestSessionId;
    }

    public static void clearSession() {
        guestSessionId = null;
    }
}
