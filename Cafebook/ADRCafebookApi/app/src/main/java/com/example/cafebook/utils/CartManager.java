package com.example.cafebook.utils;

import android.content.Context;
import android.content.SharedPreferences;

import com.example.cafebook.models.GioHangItemDto;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;

public class CartManager {
    private static final String PREF_NAME = "CafebookCart";
    private static final String KEY_CART_ITEMS = "cart_items";
    private static final String KEY_PROMO_CODE = "promo_code";

    private static CartManager instance;
    private SharedPreferences prefs;
    private Gson gson;

    private CartManager(Context context) {
        prefs = context.getApplicationContext().getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        gson = new Gson();
    }

    public static synchronized CartManager getInstance(Context context) {
        if (instance == null) {
            instance = new CartManager(context);
        }
        return instance;
    }

    public List<GioHangItemDto> getCartItems() {
        String json = prefs.getString(KEY_CART_ITEMS, null);
        if (json == null) {
            return new ArrayList<>();
        }
        Type type = new TypeToken<List<GioHangItemDto>>() {}.getType();
        return gson.fromJson(json, type);
    }

    public void saveCartItems(List<GioHangItemDto> items) {
        String json = gson.toJson(items);
        prefs.edit().putString(KEY_CART_ITEMS, json).apply();
    }

    public void addToCart(int idSanPham, int soLuong) {
        List<GioHangItemDto> items = getCartItems();
        boolean found = false;
        for (GioHangItemDto item : items) {
            if (item.getIdSanPham() == idSanPham) {
                item.setSoLuong(item.getSoLuong() + soLuong);
                found = true;
                break;
            }
        }
        if (!found) {
            items.add(new GioHangItemDto(idSanPham, soLuong));
        }
        saveCartItems(items);
    }

    public void removeFromCart(int idSanPham) {
        List<GioHangItemDto> items = getCartItems();
        for (int i = 0; i < items.size(); i++) {
            if (items.get(i).getIdSanPham() == idSanPham) {
                items.remove(i);
                break;
            }
        }
        saveCartItems(items);
    }

    public void updateQuantity(int idSanPham, int soLuong) {
        List<GioHangItemDto> items = getCartItems();
        for (GioHangItemDto item : items) {
            if (item.getIdSanPham() == idSanPham) {
                item.setSoLuong(soLuong);
                break;
            }
        }
        saveCartItems(items);
    }

    public void clearCart() {
        prefs.edit().remove(KEY_CART_ITEMS).remove(KEY_PROMO_CODE).apply();
    }

    public String getPromoCode() {
        return prefs.getString(KEY_PROMO_CODE, "");
    }

    public void setPromoCode(String promoCode) {
        prefs.edit().putString(KEY_PROMO_CODE, promoCode).apply();
    }

    public int getCartCount() {
        List<GioHangItemDto> items = getCartItems();
        int count = 0;
        for (GioHangItemDto item : items) {
            count += item.getSoLuong();
        }
        return count;
    }
}
