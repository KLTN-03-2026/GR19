using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CafebookModel.Model.Shared;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using AppCafebookApi.View.quanly;
using AppCafebookApi.View.nhanvien;
using CafebookModel.Model.ModelApp.NhanVien;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.Common
{
    public partial class WelcomeWindow : Window
    {
        private readonly LoginResponse? _user;
        private Window? _parentWindow;
        private string _workspace;

        public WelcomeWindow()
        {
            InitializeComponent();
            _workspace = "NhanVien";
        }

        public WelcomeWindow(LoginResponse user, Window parentWindow, string workspace)
        {
            InitializeComponent();
            _user = user;
            _parentWindow = parentWindow;
            _workspace = workspace;

            if (_parentWindow != null)
            {
                _parentWindow.Hide();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_user != null)
            {
                txtUserGreeting.Text = $"Xin chào, {_user.HoTen}";

                string avatarPath = _user.AnhDaiDien ?? string.Empty;

                if (!string.IsNullOrEmpty(avatarPath) && !avatarPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string baseUrl = AppConfigManager.GetApiServerUrl() ?? "";
                    if (!avatarPath.Contains("/")) avatarPath = $"{HinhAnhPaths.UrlAvatarNV}/{avatarPath}";
                    avatarPath = $"{baseUrl.TrimEnd('/')}/{avatarPath.TrimStart('/')}";
                }

                BitmapImage avatar = HinhAnhHelper.LoadImage(avatarPath, HinhAnhPaths.DefaultAvatar);

                if (avatar != null)
                {
                    imgAvatar.Fill = new ImageBrush(avatar) { Stretch = Stretch.UniformToFill };
                }
            }

            var delayTask = Task.Delay(2500);
            var loadDataTask = PreloadDataIntoRamAsync();

            await Task.WhenAll(delayTask, loadDataTask);

            if (_parentWindow != null) _parentWindow.Close();

            if (_workspace == "QuanLy") new ManHinhQuanly().Show();
            else new ManHinhNhanVien().Show();

            this.Close();
        }

        // =======================================================
        // KHIÊN BẢO VỆ API: Nếu 1 API lỗi, không làm sập tiến trình khác
        // =======================================================
        private async Task<T?> SafeGetAsync<T>(string url)
        {
            try
            {
                return await ApiClient.Instance.GetFromJsonAsync<T>(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CẢNH BÁO CACHE] Không thể tải {url}. Chi tiết: {ex.Message}");
                return default;
            }
        }

        private async Task PreloadDataIntoRamAsync()
        {
            try
            {
                ReportProgress(5, "Đang kết nối máy chủ...");
                if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
                {
                    ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
                }

                if (_workspace == "QuanLy") await PreloadQuanLyDataAsync();
                else await PreloadNhanVienDataAsync();

                ReportProgress(100, "Hoàn tất! Đang khởi động...");
            }
            catch
            {
                ReportProgress(100, "Sẵn sàng (Chế độ ngoại tuyến tạm thời)");
            }
        }

        // =======================================================
        // LUỒNG TẢI DỮ LIỆU QUẢN LÝ (THEO QUYỀN)
        // =======================================================
        private async Task PreloadQuanLyDataAsync()
        {
            ReportProgress(15, "Đang kiểm tra phân quyền tài khoản...");

            bool isFull = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL");
            var apiTasks = new List<Task>();

            Task<QuanLyTongQuanDto?>? tTongQuan = null;
            Task<List<QuanLyCaiDatDto>?>? tCaiDat = null;
            Task<List<QuanLyBanGridDto>?>? tBan = null;
            Task<List<QuanLyKhuVucDto>?>? tKhuVuc = null;
            Task<List<QuanLySanPhamGridDto>?>? tSanPham = null;
            Task<List<QuanLyDanhMucGridDto>?>? tDanhMuc = null;
            Task<List<QuanLyDinhLuongSPDto>?>? tDinhLuong = null;
            Task<List<QuanLySachGridDto>?>? tSach = null;
            Task<List<QuanLySachFilterLookupDto>?>? tLookupTheLoai = null;
            Task<List<QuanLySachFilterLookupDto>?>? tLookupTacGia = null;
            Task<List<QuanLySachFilterLookupDto>?>? tLookupNXB = null;
            Task<List<QuanLyDanhMucSachItemDto>?>? tTacGia = null;
            Task<List<RoleLookupDto>?>? tVaiTro = null;
            Task<List<QuanLyDanhMucSachItemDto>?>? tNhaXuatBan = null;
            Task<List<QuanLyKhachHangGridDto>?>? tKhachHang = null;
            Task<List<QuanLyKhuyenMaiGridDto>?>? tKhuyenMai = null;
            Task<List<QuanLyKhuyenMaiLookupDto>?>? tLookupSPKM = null;

            if (isFull || AuthService.CoQuyen("QL_TONG_QUAN"))
            {
                tTongQuan = SafeGetAsync<QuanLyTongQuanDto>("api/app/quanly-tongquan/summary");
                apiTasks.Add(tTongQuan);
            }
            if (isFull || AuthService.CoQuyen("CM_CAI_DAT"))
            {
                tCaiDat = SafeGetAsync<List<QuanLyCaiDatDto>>("api/app/quanly-caidat/all");
                apiTasks.Add(tCaiDat);
            }
            if (isFull || AuthService.CoQuyen("QL_BAN"))
            {
                tBan = SafeGetAsync<List<QuanLyBanGridDto>>("api/app/quanly-ban");
                apiTasks.Add(tBan);
            }
            if (isFull || AuthService.CoQuyen("QL_KHU_VUC"))
            {
                tKhuVuc = SafeGetAsync<List<QuanLyKhuVucDto>>("api/app/quanly-khuvuc");
                apiTasks.Add(tKhuVuc);
            }
            if (isFull || AuthService.CoQuyen("QL_SAN_PHAM"))
            {
                tSanPham = SafeGetAsync<List<QuanLySanPhamGridDto>>("api/app/quanly-sanpham");
                apiTasks.Add(tSanPham);
            }
            if (isFull || AuthService.CoQuyen("QL_DANH_MUC"))
            {
                tDanhMuc = SafeGetAsync<List<QuanLyDanhMucGridDto>>("api/app/quanly-danhmuc");
                apiTasks.Add(tDanhMuc);
            }
            if (isFull || AuthService.CoQuyen("QL_DINH_LUONG"))
            {
                tDinhLuong = SafeGetAsync<List<QuanLyDinhLuongSPDto>>("api/app/quanly-dinhluong/lookup-sp");
                apiTasks.Add(tDinhLuong);
            }
            if (isFull || AuthService.CoQuyen("QL_SACH"))
            {
                tSach = SafeGetAsync<List<QuanLySachGridDto>>("api/app/quanly-sach");
                tLookupTheLoai = SafeGetAsync<List<QuanLySachFilterLookupDto>>("api/app/quanly-sach/lookup/theloai");
                tLookupTacGia = SafeGetAsync<List<QuanLySachFilterLookupDto>>("api/app/quanly-sach/lookup/tacgia");
                tLookupNXB = SafeGetAsync<List<QuanLySachFilterLookupDto>>("api/app/quanly-sach/lookup/nxb");
                apiTasks.Add(tSach); apiTasks.Add(tLookupTheLoai); apiTasks.Add(tLookupTacGia); apiTasks.Add(tLookupNXB);
            }
            if (isFull || AuthService.CoQuyen("QL_DANH_MUC_SACH"))
            {
                tTacGia = SafeGetAsync<List<QuanLyDanhMucSachItemDto>>("api/app/quanly-danhmucsach/tacgia");
                tVaiTro = SafeGetAsync<List<RoleLookupDto>>("api/app/quanly-nhanvien/roles-lookup");

                tNhaXuatBan = SafeGetAsync<List<QuanLyDanhMucSachItemDto>>("api/app/quanly-danhmucsach/nhaxuatban");
                apiTasks.Add(tTacGia); apiTasks.Add(tVaiTro); apiTasks.Add(tNhaXuatBan);
            }
            if (isFull || AuthService.CoQuyen("QL_KHACH_HANG"))
            {
                tKhachHang = SafeGetAsync<List<QuanLyKhachHangGridDto>>("api/app/quanly-khachhang");
                apiTasks.Add(tKhachHang);
            }
            if (isFull || AuthService.CoQuyen("QL_KHUYEN_MAI"))
            {
                tKhuyenMai = SafeGetAsync<List<QuanLyKhuyenMaiGridDto>>("api/app/quanly-khuyenmai/all");
                tLookupSPKM = SafeGetAsync<List<QuanLyKhuyenMaiLookupDto>>("api/app/quanly-khuyenmai/filters");

                apiTasks.Add(tKhuyenMai);
                apiTasks.Add(tLookupSPKM);
            }

            ReportProgress(35, "Đang đồng bộ dữ liệu...");
            if (apiTasks.Count > 0) await Task.WhenAll(apiTasks);

            ReportProgress(75, "Đang nạp dữ liệu vào bộ nhớ (RAM)...");
            if (tTongQuan != null) GlobalDataCache.QL_TongQuanCache = await tTongQuan;
            if (tCaiDat != null) GlobalDataCache.QL_CaiDatCache = await tCaiDat ?? new List<QuanLyCaiDatDto>();
            if (tBan != null) GlobalDataCache.QL_BanCache = await tBan ?? new List<QuanLyBanGridDto>();
            if (tKhuVuc != null) GlobalDataCache.QL_KhuVucCache = await tKhuVuc ?? new List<QuanLyKhuVucDto>();
            if (tSanPham != null) GlobalDataCache.QL_SanPhamCache = await tSanPham ?? new List<QuanLySanPhamGridDto>();
            if (tDanhMuc != null) GlobalDataCache.QL_DanhMucCache = await tDanhMuc ?? new List<QuanLyDanhMucGridDto>();
            if (tDinhLuong != null) GlobalDataCache.QL_DinhLuongSPCache = await tDinhLuong ?? new List<QuanLyDinhLuongSPDto>();
            if (tSach != null) GlobalDataCache.QL_SachCache = await tSach ?? new List<QuanLySachGridDto>();
            if (tLookupTheLoai != null) GlobalDataCache.QL_LookupTheLoaiCache = await tLookupTheLoai ?? new List<QuanLySachFilterLookupDto>();
            if (tLookupTacGia != null) GlobalDataCache.QL_LookupTacGiaCache = await tLookupTacGia ?? new List<QuanLySachFilterLookupDto>();
            if (tLookupNXB != null) GlobalDataCache.QL_LookupNXBCache = await tLookupNXB ?? new List<QuanLySachFilterLookupDto>();
            if (tTacGia != null) GlobalDataCache.QL_TacGiaCache = await tTacGia ?? new List<QuanLyDanhMucSachItemDto>();
            if (tVaiTro != null) GlobalDataCache.QL_VaiTroCache = await tVaiTro ?? new List<RoleLookupDto>();
            if (tNhaXuatBan != null) GlobalDataCache.QL_NhaXuatBanCache = await tNhaXuatBan ?? new List<QuanLyDanhMucSachItemDto>();
            if (tKhachHang != null) GlobalDataCache.QL_KhachHangCache = await tKhachHang ?? new List<QuanLyKhachHangGridDto>();
            if (tKhuyenMai != null) GlobalDataCache.QL_KhuyenMaiCache = await tKhuyenMai ?? new List<QuanLyKhuyenMaiGridDto>();
            if (tLookupSPKM != null) GlobalDataCache.QL_LookupSanPhamKMCache = await tLookupSPKM ?? new List<QuanLyKhuyenMaiLookupDto>();

            ReportProgress(85, "Đang lưu trữ hình ảnh...");
            if (GlobalDataCache.QL_SanPhamCache != null)
            {
                var imgTasks = new List<Task>();
                foreach (var sp in GlobalDataCache.QL_SanPhamCache)
                {
                    if (sp.HinhAnh is string url && !string.IsNullOrEmpty(url)) imgTasks.Add(CacheImageToRamAsync(url));
                }
                if (imgTasks.Count > 0) await Task.WhenAll(imgTasks);
            }
            ReportProgress(95, "Hoàn tất lưu trữ ảnh!");
        }

        // =======================================================
        // LUỒNG TẢI DỮ LIỆU NHÂN VIÊN (THEO QUYỀN)
        // =======================================================
        private async Task PreloadNhanVienDataAsync()
        {
            ReportProgress(20, "Đang kiểm tra phân quyền tài khoản...");
            bool isFull = AuthService.CoQuyen("FULL_ADMIN", "FULL_NV");
            var apiTasks = new List<Task>();

            Task<List<KhuVucDto>?>? taskKhuVuc = null;
            Task<List<BanSoDoDto>?>? taskBan = null;
            Task<MenuResponseDto?>? taskMenu = null;

            if (isFull || AuthService.CoQuyen("NV_SO_DO_BAN"))
            {
                taskKhuVuc = SafeGetAsync<List<KhuVucDto>>("api/app/sodoban/khuvuc-list");
                taskBan = SafeGetAsync<List<BanSoDoDto>>("api/app/sodoban/tables");
                apiTasks.Add(taskKhuVuc);
                apiTasks.Add(taskBan);
            }

            if (isFull || AuthService.CoQuyen("NV_GOI_MON"))
            {
                taskMenu = SafeGetAsync<MenuResponseDto>("api/app/nhanvien/goimon/menu");
                apiTasks.Add(taskMenu);
            }

            ReportProgress(40, "Đang đồng bộ dữ liệu...");
            if (apiTasks.Count > 0) await Task.WhenAll(apiTasks);

            ReportProgress(60, "Đang giải nén dữ liệu vào bộ nhớ...");
            if (taskKhuVuc != null) GlobalDataCache.KhuVucCache = await taskKhuVuc ?? new List<KhuVucDto>();
            if (taskBan != null) GlobalDataCache.BanCache = await taskBan ?? new List<BanSoDoDto>();

            if (taskMenu != null)
            {
                var menuResult = await taskMenu;
                if (menuResult != null)
                {
                    GlobalDataCache.DanhMucCache = menuResult.DanhMucs;
                    if (menuResult.SanPhams != null)
                    {
                        int totalItems = menuResult.SanPhams.Count;
                        int processedItems = 0;

                        foreach (var item in menuResult.SanPhams)
                        {
                            string url = "";
                            if (item.HinhAnh is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                url = jsonElement.GetString() ?? "";
                            else if (item.HinhAnh is string str)
                                url = str;

                            if (!string.IsNullOrEmpty(url))
                            {
                                if (!GlobalDataCache.ImageCache.ContainsKey(url))
                                {
                                    try
                                    {
                                        byte[] imageBytes = await ApiClient.Instance.GetByteArrayAsync(url);
                                        using (var ms = new System.IO.MemoryStream(imageBytes))
                                        {
                                            var bitmap = new BitmapImage();
                                            bitmap.BeginInit();
                                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                            bitmap.DecodePixelWidth = 150;
                                            bitmap.StreamSource = ms;
                                            bitmap.EndInit();
                                            bitmap.Freeze();

                                            GlobalDataCache.ImageCache.TryAdd(url, bitmap);
                                            item.HinhAnh = bitmap;
                                        }
                                    }
                                    catch
                                    {
                                        item.HinhAnh = CafebookModel.Utils.HinhAnhPaths.DefaultFoodIcon;
                                    }
                                }
                                else
                                {
                                    item.HinhAnh = GlobalDataCache.ImageCache[url];
                                }
                            }
                            else
                            {
                                item.HinhAnh = CafebookModel.Utils.HinhAnhPaths.DefaultFoodIcon;
                            }

                            processedItems++;
                            int imgProgress = 60 + (int)((float)processedItems / totalItems * 35);
                            ReportProgress(imgProgress, $"Đang nạp ảnh món ăn ({processedItems}/{totalItems})...");
                        }
                    }
                    GlobalDataCache.SanPhamCache = menuResult.SanPhams;
                }
            }
        }

        private async Task CacheImageToRamAsync(string url)
        {
            if (string.IsNullOrEmpty(url) || GlobalDataCache.ImageCache.ContainsKey(url)) return;
            try
            {
                byte[] imageBytes = await ApiClient.Instance.GetByteArrayAsync(url);
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 150;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    GlobalDataCache.ImageCache.TryAdd(url, bitmap);
                }
            }
            catch { /* Bỏ qua ảnh lỗi */ }
        }

        private class MenuResponseDto
        {
            public List<DanhMucDto>? DanhMucs { get; set; }
            public List<SanPhamDto>? SanPhams { get; set; }
        }

        private void ReportProgress(int percent, string message)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = percent;
                txtPercent.Text = $"{percent}%";
                txtSub.Text = message;
            });
        }
    }
}