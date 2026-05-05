using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelApp.QuanLy;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace AppCafebookApi.Services
{

    public static class GlobalDataCache

    {
        // --- 1. CACHE PHÂN HỆ NHÂN VIÊN ---
        public static List<KhuVucDto>? KhuVucCache { get; set; }
        public static List<BanSoDoDto>? BanCache { get; set; }
        public static List<DanhMucDto>? DanhMucCache { get; set; }
        public static List<SanPhamDto>? SanPhamCache { get; set; }

        // --- 2. CACHE PHÂN HỆ QUẢN LÝ ---
        public static QuanLyTongQuanDto? QL_TongQuanCache { get; set; }
        public static List<QuanLyCaiDatDto>? QL_CaiDatCache { get; set; }
        public static List<QuanLyBanGridDto>? QL_BanCache { get; set; }
        public static List<QuanLyKhuVucDto>? QL_KhuVucCache { get; set; }
        public static List<QuanLySanPhamGridDto>? QL_SanPhamCache { get; set; }
        public static List<QuanLyDanhMucGridDto>? QL_DanhMucCache { get; set; }
        public static List<QuanLyDinhLuongSPDto>? QL_DinhLuongSPCache { get; set; }
        public static List<QuanLySachGridDto>? QL_SachCache { get; set; }
        public static List<QuanLySachFilterLookupDto>? QL_LookupTheLoaiCache { get; set; }
        public static List<QuanLySachFilterLookupDto>? QL_LookupTacGiaCache { get; set; }
        public static List<QuanLySachFilterLookupDto>? QL_LookupNXBCache { get; set; }
        public static List<QuanLyDanhMucSachItemDto>? QL_TacGiaCache { get; set; }
        public static List<QuanLyDanhMucSachItemDto>? QL_TheLoaiCache { get; set; }
        public static List<QuanLyDanhMucSachItemDto>? QL_NhaXuatBanCache { get; set; }
        public static List<QuanLyNhanVienGridDto>? QL_NhanVienCache { get; set; }
        public static List<RoleLookupDto>? QL_VaiTroCache { get; set; }
        public static List<QuanLyKhachHangGridDto>? QL_KhachHangCache { get; set; }
        public static List<QuanLyKhuyenMaiGridDto>? QL_KhuyenMaiCache { get; set; }
        public static List<QuanLyKhuyenMaiLookupDto>? QL_LookupSanPhamKMCache { get; set; }


        // --- 3. CACHE HÌNH ẢNH CHUNG ---
        public static ConcurrentDictionary<string, BitmapImage> ImageCache { get; set; } = new ConcurrentDictionary<string, BitmapImage>();

        public static void ClearAll()
        {
            // Xóa cache Nhân Viên
            KhuVucCache?.Clear(); KhuVucCache = null;
            BanCache?.Clear(); BanCache = null;
            DanhMucCache?.Clear(); DanhMucCache = null;
            SanPhamCache?.Clear(); SanPhamCache = null;

            // Xóa cache Quản Lý
            QL_TongQuanCache = null;
            QL_CaiDatCache?.Clear(); QL_CaiDatCache = null;
            QL_BanCache?.Clear(); QL_BanCache = null;
            QL_KhuVucCache?.Clear(); QL_KhuVucCache = null;
            QL_SanPhamCache?.Clear(); QL_SanPhamCache = null;
            QL_DanhMucCache?.Clear(); QL_DanhMucCache = null;
            QL_DinhLuongSPCache?.Clear(); QL_DinhLuongSPCache = null;
            QL_SachCache?.Clear(); QL_SachCache = null;
            QL_TacGiaCache?.Clear(); QL_TacGiaCache = null;
            QL_NhanVienCache?.Clear(); QL_NhanVienCache = null;
            QL_KhachHangCache?.Clear(); QL_KhachHangCache = null;
            QL_KhuyenMaiCache?.Clear(); QL_KhuyenMaiCache = null;
            QL_LookupTheLoaiCache?.Clear(); QL_LookupTheLoaiCache = null;
            QL_LookupTacGiaCache?.Clear(); QL_LookupTacGiaCache = null;
            QL_LookupNXBCache?.Clear(); QL_LookupNXBCache = null;
            QL_TheLoaiCache?.Clear(); QL_TheLoaiCache = null;
            QL_NhaXuatBanCache?.Clear(); QL_NhaXuatBanCache = null;
            QL_VaiTroCache?.Clear(); QL_VaiTroCache = null;
            QL_LookupSanPhamKMCache?.Clear(); QL_LookupSanPhamKMCache = null;

            ImageCache.Clear();

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }
    }
}