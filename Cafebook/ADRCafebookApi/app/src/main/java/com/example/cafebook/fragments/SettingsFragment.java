package com.example.cafebook.fragments;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatDelegate;
import androidx.appcompat.widget.Toolbar;
import androidx.fragment.app.Fragment;
import com.example.cafebook.R;
import com.example.cafebook.network.ApiClient;
import com.example.cafebook.utils.SessionManager;
import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import com.google.android.material.switchmaterial.SwitchMaterial;

public class SettingsFragment extends Fragment {

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_settings, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        Toolbar toolbar = view.findViewById(R.id.toolbarSettings);
        toolbar.setNavigationOnClickListener(v -> getParentFragmentManager().popBackStack());

        SwitchMaterial switchDarkMode = view.findViewById(R.id.switchDarkMode);
        
        // Lấy trạng thái Theme từ SharedPreferences
        SharedPreferences themePrefs = requireContext().getSharedPreferences("Settings", Context.MODE_PRIVATE);
        boolean isDark = themePrefs.getBoolean("DARK_MODE", false);
        switchDarkMode.setChecked(isDark);

        switchDarkMode.setOnCheckedChangeListener((buttonView, isChecked) -> {
            themePrefs.edit().putBoolean("DARK_MODE", isChecked).apply();
            if (isChecked) {
                AppCompatDelegate.setDefaultNightMode(AppCompatDelegate.MODE_NIGHT_YES);
            } else {
                AppCompatDelegate.setDefaultNightMode(AppCompatDelegate.MODE_NIGHT_NO);
            }
        });

        view.findViewById(R.id.btnEditProfile).setOnClickListener(v -> {
            getParentFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new EditProfileFragment())
                    .addToBackStack(null)
                    .commit();
        });

        view.findViewById(R.id.btnLogout).setOnClickListener(v -> handleLogout());
    }

    private void handleLogout() {
        new MaterialAlertDialogBuilder(requireContext())
                .setTitle("Đăng xuất")
                .setMessage("Bạn có chắc chắn muốn đăng xuất khỏi tài khoản này?")
                .setPositiveButton("Đăng xuất", (dialog, which) -> {
                    SharedPreferences prefs = requireActivity().getSharedPreferences("CafebookAuth", Context.MODE_PRIVATE);
                    prefs.edit().clear().apply();
                    
                    SessionManager.clearSession();
                    ApiClient.reset();

                    if (getActivity() instanceof com.example.cafebook.MainActivity) {
                        com.example.cafebook.MainActivity activity = (com.example.cafebook.MainActivity) getActivity();
                        activity.getSupportFragmentManager().popBackStack(null, androidx.fragment.app.FragmentManager.POP_BACK_STACK_INCLUSIVE);
                        activity.loadFragment(new com.example.cafebook.fragments.auth.DangNhapFragment());
                        
                        View nav = activity.findViewById(R.id.bottomNavigation);
                        if (nav instanceof com.google.android.material.bottomnavigation.BottomNavigationView) {
                            ((com.google.android.material.bottomnavigation.BottomNavigationView) nav).getMenu().findItem(R.id.nav_home).setChecked(true);
                        }
                    }
                })
                .setNegativeButton("Hủy", null)
                .show();
    }
}
