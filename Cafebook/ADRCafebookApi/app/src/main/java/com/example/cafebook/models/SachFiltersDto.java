package com.example.cafebook.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class SachFiltersDto {
    @SerializedName("theLoais") private List<ThuVienSachFilterItemDto> theLoais;
    public List<ThuVienSachFilterItemDto> getTheLoais() { return theLoais; }
}
