package com.example.cafebook;

import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;

import androidx.activity.EdgeToEdge;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.graphics.Insets;
import androidx.core.splashscreen.SplashScreen;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.fragment.app.Fragment;

import com.example.cafebook.fragments.BookingFragment;
import com.example.cafebook.fragments.ChinhSachFragment;
import com.example.cafebook.fragments.HoTroGuestFragment;
import com.example.cafebook.fragments.OrderDetailFragment;
import com.example.cafebook.fragments.SupportAuthFragment;
import com.example.cafebook.fragments.SettingsFragment;
import com.example.cafebook.fragments.HomeFragment;
import com.example.cafebook.fragments.LienHeFragment;
import com.example.cafebook.fragments.LibraryFragment;
import com.example.cafebook.fragments.MenuFragment;
import com.example.cafebook.fragments.ProfileFragment;
import com.example.cafebook.utils.SessionManager;
import com.example.cafebook.fragments.auth.DangKyFragment;
import com.example.cafebook.fragments.auth.DangNhapFragment;
import com.example.cafebook.fragments.auth.QuenMatKhauFragment;
import com.example.cafebook.fragments.auth.XacMinhOtpFragment;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        SplashScreen splashScreen = SplashScreen.installSplashScreen(this);
        EdgeToEdge.enable(this);
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        // Reset toàn bộ phiên chat cũ khi mở lại App từ đầu
        if (savedInstanceState == null) {
            com.example.cafebook.utils.SessionManager.clearOnAppStart(this);
        }

        // Handle Window Insets (Edge-to-Edge and Keyboard)
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            Insets ime = insets.getInsets(WindowInsetsCompat.Type.ime());
            
            int bottomPadding = Math.max(systemBars.bottom, ime.bottom);
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, bottomPadding);
            return WindowInsetsCompat.CONSUMED;
        });

        // 1. Setup Top Toolbar
        Toolbar toolbar = findViewById(R.id.topToolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayShowTitleEnabled(false);
        }
        setupToolbarIcons();

        // 2. Setup Bottom Navigation
        BottomNavigationView bottomNav = findViewById(R.id.bottomNavigation);
        bottomNav.setOnItemSelectedListener(item -> {
            Fragment selectedFragment = null;
            int id = item.getItemId();

            if (id == R.id.nav_home) {
                selectedFragment = new HomeFragment();
            } else if (id == R.id.nav_menu) {
                selectedFragment = new MenuFragment();
            } else if (id == R.id.nav_library) {
                selectedFragment = new LibraryFragment();
            } else if (id == R.id.nav_booking) {
                selectedFragment = new BookingFragment();
            } else if (id == R.id.nav_profile) {
                // Kiểm tra trạng thái đăng nhập
                android.content.SharedPreferences prefs = getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
                String token = prefs.getString("JWT_TOKEN", "");
                if (!token.isEmpty()) {
                    selectedFragment = new ProfileFragment();
                } else {
                    selectedFragment = new DangNhapFragment();
                }
            }

            if (selectedFragment != null) {
                loadFragment(selectedFragment);
                return true;
            }
            return false;
        });

        // Load default fragment
        if (savedInstanceState == null) {
            handleIntent(getIntent());
            bottomNav.postDelayed(() -> {
                if (getSupportFragmentManager().getBackStackEntryCount() == 0) {
                    bottomNav.setSelectedItemId(R.id.nav_home);
                }
            }, 100);
        }

        getSupportFragmentManager().addOnBackStackChangedListener(() -> {
            Fragment currentFragment = getSupportFragmentManager().findFragmentById(R.id.fragment_container);
            updateUiVisibility(currentFragment);
        });
    }

    @Override
    protected void onNewIntent(@NonNull android.content.Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        handleIntent(intent);
    }

    private void handleIntent(android.content.Intent intent) {
        if (intent != null && intent.hasExtra("OPEN_ORDER_ID")) {
            int orderId = intent.getIntExtra("OPEN_ORDER_ID", 0);
            if (orderId > 0) {
                loadFragment(OrderDetailFragment.newInstance(orderId));
            }
        }
    }

    private void setupToolbarIcons() {
        android.widget.ImageButton btnBack = findViewById(R.id.btnBack);
        android.widget.ImageButton btnContact = findViewById(R.id.action_contact);
        android.widget.ImageButton btnAiSupport = findViewById(R.id.action_support);
        android.widget.ImageButton btnSettings = findViewById(R.id.action_settings);
        android.widget.ImageButton btnCart = findViewById(R.id.action_cart);

        if (btnBack != null) {
            btnBack.setOnClickListener(v -> onBackPressed());
        }

        // Tạo hàm gắn hiệu ứng nhún (Scale) khi chạm
        View.OnTouchListener bounceTouchListener = (v, event) -> {
            switch (event.getAction()) {
                case android.view.MotionEvent.ACTION_DOWN:
                    v.animate().scaleX(0.85f).scaleY(0.85f).setDuration(150).start();
                    break;
                case android.view.MotionEvent.ACTION_UP:
                case android.view.MotionEvent.ACTION_CANCEL:
                    v.animate().scaleX(1f).scaleY(1f).setDuration(150).start();
                    break;
            }
            return false; // Trả về false để sự kiện onClick vẫn hoạt động
        };

        if (btnContact != null) btnContact.setOnTouchListener(bounceTouchListener);
        if (btnAiSupport != null) btnAiSupport.setOnTouchListener(bounceTouchListener);
        if (btnSettings != null) btnSettings.setOnTouchListener(bounceTouchListener);
        if (btnCart != null) btnCart.setOnTouchListener(bounceTouchListener);

        // Gắn sự kiện click
        if (btnContact != null) {
            btnContact.setOnClickListener(v -> loadFragment(new LienHeFragment()));
        }

        if (btnAiSupport != null) {
            btnAiSupport.setOnClickListener(v -> {
                android.content.SharedPreferences prefs = getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
                String token = prefs.getString("JWT_TOKEN", "");
                if (!token.isEmpty()) {
                    loadFragment(new SupportAuthFragment());
                } else {
                    loadFragment(new HoTroGuestFragment());
                }
            });
        }

        if (btnSettings != null) {
            btnSettings.setOnClickListener(v -> loadFragment(new SettingsFragment()));
        }

        if (btnCart != null) {
            btnCart.setOnClickListener(v -> {
                android.content.SharedPreferences prefs = getSharedPreferences("CafebookAuth", android.content.Context.MODE_PRIVATE);
                String token = prefs.getString("JWT_TOKEN", "");
                if (!token.isEmpty()) {
                    startActivity(new android.content.Intent(this, CartActivity.class));
                } else {
                    android.widget.Toast.makeText(this, "Vui lòng đăng nhập để sử dụng tính năng giỏ hàng", android.widget.Toast.LENGTH_SHORT).show();
                    loadFragment(new DangNhapFragment());
                }
            });
        }
    }

    public void updateUiVisibility(Fragment fragment) {
        View mainAppBar = findViewById(R.id.mainAppBar);
        View bottomNav = findViewById(R.id.bottomNavigation);
        View btnBack = findViewById(R.id.btnBack);
        View btnSettings = findViewById(R.id.action_settings);
        if (mainAppBar == null || bottomNav == null) return;

        // ƯU TIÊN HIỂN THỊ TOOLBAR CHUNG - Chỉ ẩn ở các màn hình Chat/Support có Toolbar riêng phức tạp
        if (fragment instanceof HoTroGuestFragment || 
            fragment instanceof SupportAuthFragment) {
            mainAppBar.setVisibility(View.GONE);
        } else {
            mainAppBar.setVisibility(View.VISIBLE);
        }

        if (btnSettings != null) {
            if (fragment instanceof ProfileFragment) {
                btnSettings.setVisibility(View.VISIBLE);
            } else {
                btnSettings.setVisibility(View.GONE);
            }
        }

        if (btnBack != null) {
            if (fragment instanceof HomeFragment) {
                btnBack.setVisibility(View.GONE);
            } else {
                btnBack.setVisibility(View.VISIBLE);
            }
        }

        // Luôn hiện BottomNav trừ các màn hình Auth/Chat sâu
        if (fragment instanceof XacMinhOtpFragment ||
            fragment instanceof DangNhapFragment ||
            fragment instanceof DangKyFragment ||
            fragment instanceof QuenMatKhauFragment ||
            fragment instanceof HoTroGuestFragment) {
            bottomNav.setVisibility(View.GONE);
            if (btnBack != null && !(fragment instanceof HoTroGuestFragment)) {
                btnBack.setVisibility(View.GONE); // Hide back button for auth
            }
        } else {
            bottomNav.setVisibility(View.VISIBLE);
        }
    }

    public void setBottomNavigationVisibility(int visibility) {
        View bottomNav = findViewById(R.id.bottomNavigation);
        if (bottomNav != null) {
            bottomNav.setVisibility(visibility);
        }
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull android.view.MenuItem item) {
        return super.onOptionsItemSelected(item);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        return true;
    }

    public void loadFragment(Fragment fragment) {
        updateUiVisibility(fragment);
        updateToolbarTitle(fragment);

        // Nếu là HomeFragment, xóa sạch backstack để tránh lặp
        if (fragment instanceof HomeFragment) {
            getSupportFragmentManager().popBackStack(null, androidx.fragment.app.FragmentManager.POP_BACK_STACK_INCLUSIVE);
        }

        getSupportFragmentManager().beginTransaction()
                .replace(R.id.fragment_container, fragment)
                .setCustomAnimations(android.R.anim.fade_in, android.R.anim.fade_out)
                .addToBackStack(null)
                .commit();
    }

    private void updateToolbarTitle(Fragment fragment) {
        android.widget.TextView tvTitle = findViewById(R.id.toolbarTitle);
        if (tvTitle == null) return;

        if (fragment instanceof HomeFragment) tvTitle.setText("Cafebook");
        else if (fragment instanceof MenuFragment) tvTitle.setText("Thực đơn");
        else if (fragment instanceof LibraryFragment) tvTitle.setText("Thư viện sách");
        else if (fragment instanceof BookingFragment) tvTitle.setText("Đặt bàn");
        else if (fragment instanceof ProfileFragment) tvTitle.setText("Tài khoản");
        else if (fragment instanceof DangNhapFragment) tvTitle.setText("Đăng nhập");
        else if (fragment instanceof DangKyFragment) tvTitle.setText("Đăng ký");
        else if (fragment instanceof LienHeFragment) tvTitle.setText("Liên hệ");
        else if (fragment instanceof SettingsFragment) tvTitle.setText("Cài đặt");
        else if (fragment instanceof HoTroGuestFragment || fragment instanceof SupportAuthFragment) tvTitle.setText("Hỗ trợ AI");
        else tvTitle.setText("Cafebook");
    }
}
