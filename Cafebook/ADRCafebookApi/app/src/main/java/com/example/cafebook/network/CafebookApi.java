package com.example.cafebook.network;

import com.example.cafebook.models.ChiTietSachDto;
import com.example.cafebook.models.ChiTietSanPhamDto;
import com.example.cafebook.models.ChinhSachDto;
import com.example.cafebook.models.DanhGiaChiTietDto;
import com.example.cafebook.models.LienHeDto;
import com.example.cafebook.models.PhanHoiInputModel;
import com.example.cafebook.models.SachFiltersDto;
import com.example.cafebook.models.SachPhanTrangDto;
import com.example.cafebook.models.TableBookingModels;
import com.example.cafebook.models.ThucDonDto;
import com.example.cafebook.models.ThucDonFilterDto;
import com.example.cafebook.models.TimKiemSachResultDto;
import com.example.cafebook.models.TrangChuDto;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface CafebookApi {
    @GET("api/web/trangchu/data")
    Call<TrangChuDto> getTrangChuData();

    @GET("api/web/thuvien/filters")
    Call<SachFiltersDto> getBookFilters();

    @GET("api/web/thuvien/search")
    Call<SachPhanTrangDto> searchBooks(
            @Query("search") String search,
            @Query("theLoaiId") int theLoaiId,
            @Query("trangThai") String trangThai,
            @Query("sortBy") String sortBy,
            @Query("pageNum") int pageNum
    );

    @GET("api/web/chitietsach/{id}")
    Call<ChiTietSachDto> getBookDetails(@Path("id") int id);

    @GET("api/web/chitietsanpham/{id}")
    Call<ChiTietSanPhamDto> getProductDetails(@Path("id") int id);

    @GET("api/web/chitietsanpham/{id}/danhgia")
    Call<List<DanhGiaChiTietDto>> getProductReviews(@Path("id") int id);

    @GET("api/web/timkiemsach")
    Call<TimKiemSachResultDto> searchBooksStandalone(
            @Query("search") String search,
            @Query("idTacGia") Integer idTacGia,
            @Query("idTheLoai") Integer idTheLoai,
            @Query("idNXB") Integer idNXB,
            @Query("pageNum") int pageNum
    );

    @GET("api/web/thucdon/filters")
    Call<List<ThucDonFilterDto>> getMenuFilters();

    @GET("api/web/thucdon/search")
    Call<ThucDonDto> searchMenu(
            @Query("loaiId") int loaiId,
            @Query("search") String search,
            @Query("sortBy") String sortBy,
            @Query("giaMin") Double giaMin,
            @Query("giaMax") Double giaMax,
            @Query("pageNum") int pageNum
    );

    @GET("api/web/lienhe/info")
    Call<LienHeDto> getContactInfo();

    @POST("api/web/lienhe/gui-gop-y")
    Call<Void> guiGopY(@Body PhanHoiInputModel input);

    @GET("api/web/khachhang/chinhsach/data")
    Call<ChinhSachDto> getChinhSachData();

    @GET("api/web/datban/get-opening-hours")
    Call<TableBookingModels.OpeningHours> getOpeningHours();

    @GET("api/web/datban/get-all-tables-by-area")
    Call<List<TableBookingModels.KhuVucBan>> getAllTablesByArea();

    @POST("api/web/datban/tim-ban")
    Call<List<TableBookingModels.BanTrong>> timBanTrong(@Body TableBookingModels.TimBanRequest req);

    @POST("api/web/datban/tao-yeu-cau")
    Call<Object> taoYeuCauDatBan(@Body TableBookingModels.DatBanWebRequest req);

    @GET("api/web/datban/get-customer-info")
    Call<TableBookingModels.CustomerInfoResponse> getCustomerInfo(@Query("phone") String phone);
}
