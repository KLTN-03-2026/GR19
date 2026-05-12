package com.example.cafebook.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cafebook.R;
import com.example.cafebook.adapters.PromotionAdapter;
import com.example.cafebook.models.GioHangKhuyenMaiDto;
import com.google.android.material.button.MaterialButton;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

public class PromotionSelectionFragment extends Fragment {

    private RecyclerView rvPromotions;
    private EditText edtPromoCode;
    private MaterialButton btnApplyManual;
    private List<GioHangKhuyenMaiDto> promoList;

    public static PromotionSelectionFragment newInstance(List<GioHangKhuyenMaiDto> list) {
        PromotionSelectionFragment fragment = new PromotionSelectionFragment();
        Bundle args = new Bundle();
        args.putSerializable("PROMO_LIST", (Serializable) list);
        fragment.setArguments(args);
        return fragment;
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        return inflater.inflate(R.layout.fragment_promotion_selection, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        ViewCompat.setOnApplyWindowInsetsListener(view.findViewById(R.id.promo_selection_root), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        rvPromotions = view.findViewById(R.id.rvPromotions);
        edtPromoCode = view.findViewById(R.id.edtPromoCode);
        btnApplyManual = view.findViewById(R.id.btnApplyManual);

        if (getArguments() != null) {
            promoList = (List<GioHangKhuyenMaiDto>) getArguments().getSerializable("PROMO_LIST");
        }

        setupRecyclerView();

        view.findViewById(R.id.btnBack).setOnClickListener(v -> requireActivity().getOnBackPressedDispatcher().onBackPressed());

        btnApplyManual.setOnClickListener(v -> {
            String manualCode = edtPromoCode.getText().toString().trim();
            if (!manualCode.isEmpty()) {
                returnResult(manualCode);
            }
        });
    }

    private void setupRecyclerView() {
        rvPromotions.setLayoutManager(new LinearLayoutManager(getContext()));
        PromotionAdapter adapter = new PromotionAdapter(promoList, promo -> returnResult(promo.getMaKhuyenMai()));
        rvPromotions.setAdapter(adapter);
    }

    private void returnResult(String code) {
        Bundle result = new Bundle();
        result.putString("SELECTED_PROMO_CODE", code);
        getParentFragmentManager().setFragmentResult("PROMO_REQUEST_KEY", result);
        requireActivity().getOnBackPressedDispatcher().onBackPressed();
    }
}
