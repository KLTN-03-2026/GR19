using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading; 
using Border = System.Windows.Controls.Border;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyKhachHangView : Page
    {
        private List<QuanLyKhachHangGridDto> _allKhachHangList = new();
        private Dictionary<int, string> _khachHangSearchCache = new();
        private QuanLyKhachHangDetailDto? _selectedKhachHang = null;
        private DispatcherTimer _searchTimer;

        private string _currentSearchKey = "";
        private int _currentCmbIndex = 0;
        private bool _currentHideLocked = false;

        private bool _isDataLoaded = false;

        public QuanLyKhachHangView()
        {
            InitializeComponent();
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); FilterData(); };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;

            if (!string.IsNullOrEmpty(AuthService.AuthToken)) 
                ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHACH_HANG", "QL_KHUYEN_MAI"))
            {
                MessageBox.Show("Bạn không có quyền truy cập module Khách hàng!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            await Task.Delay(350);

            if (!this.IsLoaded) return;

            try
            {
                ApplyPermissions();

                await LoadKhachHangAsync();

                _isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải danh sách khách hàng: {ex.Message}");
            }
        }


        private void ApplyPermissions()
        {
            bool hasQuyenKH = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHACH_HANG", "QL_KHUYEN_MAI", "QL_DE_XUAT");

            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyenKH ? Visibility.Collapsed : Visibility.Visible;

            if (FindName("btnSuaDiem") is Button bs) bs.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnKhoa") is Button bk) bk.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button bx) bx.Visibility = hasQuyenKH ? Visibility.Visible : Visibility.Collapsed;

            bool hasQuyenKM = AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHUYEN_MAI");
            if (FindName("btnNavKhuyenMai") is Button btnKM) btnKM.Visibility = hasQuyenKM ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadKhachHangAsync()
        {
            var loading = FindName("LoadingOverlay") as Border;
            if (loading != null) loading.Visibility = Visibility.Visible;
            try
            {
                if (GlobalDataCache.QL_KhachHangCache != null && GlobalDataCache.QL_KhachHangCache.Count > 0)
                {
                    _allKhachHangList = GlobalDataCache.QL_KhachHangCache;
                    SetupKhachHangUI();

                    if (loading != null) loading.Visibility = Visibility.Collapsed;

                    _ = BackgroundRefreshAsync();
                    return;
                }
                await FetchApiAndSetupUI();
            }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ (TÁCH LOGIC ĐỂ DÙNG CHUNG)
        // ==========================================

        private void SetupKhachHangUI()
        {
            foreach (var kh in _allKhachHangList)
            {
                kh.SearchKeyword = $"{RemoveVietnameseSigns(kh.HoTen)} {RemoveVietnameseSigns(kh.Email)} {(kh.SoDienThoai ?? "")}".ToLower();
            }

            FilterData();
        }

        private async Task BackgroundRefreshAsync()
        {
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyKhachHangGridDto>>("api/app/quanly-khachhang");
                if (res != null)
                {
                    GlobalDataCache.QL_KhachHangCache = res;
                    _allKhachHangList = res;

                    // Ghi nhớ ID đang chọn để giữ nguyên form chi tiết bên phải
                    int? currentSelectedId = _selectedKhachHang?.IdKhachHang;

                    SetupKhachHangUI();

                    if (currentSelectedId.HasValue && FindName("dgKhachHang") is DataGrid dg)
                    {
                        var itemToSelect = _allKhachHangList.FirstOrDefault(x => x.IdKhachHang == currentSelectedId);
                        if (itemToSelect != null) dg.SelectedItem = itemToSelect;
                    }
                }
            }
            catch { /* Lỗi mạng thì bỏ qua, giữ nguyên UI cũ trên RAM */ }
        }

        private async Task FetchApiAndSetupUI()
        {
            var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyKhachHangGridDto>>("api/app/quanly-khachhang");
            if (res != null)
            {
                GlobalDataCache.QL_KhachHangCache = res;
                _allKhachHangList = res;
                SetupKhachHangUI();
            }
        }

        private bool KhachHangFilter(object obj)
        {
            if (obj is not QuanLyKhachHangGridDto x) return false;

            // 1. Lọc theo từ khóa
            if (!string.IsNullOrEmpty(_currentSearchKey))
            {
                if (!x.SearchKeyword.Contains(_currentSearchKey)) return false;
            }

            // 2. Lọc theo Loại TK
            if (_currentCmbIndex == 1 && (!x.TaiKhoanTam || x.DaXoa)) return false; //[cite: 1]
            if (_currentCmbIndex == 2 && (x.TaiKhoanTam || x.DaXoa)) return false; //[cite: 1]
            if (_currentCmbIndex == 3 && (!x.BiKhoa || x.DaXoa)) return false; //[cite: 1]
            if (_currentCmbIndex == 4 && !x.DaXoa) return false; //[cite: 1]
            if (_currentCmbIndex == 0 && x.DaXoa) return false; //[cite: 1]

            // 3. Ẩn tài khoản khóa
            if (_currentHideLocked && x.BiKhoa) return false; //[cite: 1]

            return true; // Giữ lại item này
        }

        private void Filters_Changed(object sender, RoutedEventArgs e) => FilterData(); //[cite: 1]
        // ==========================================
        // THUẬT TOÁN BỎ DẤU TIẾNG VIỆT (Nâng cấp)
        // ==========================================
        private string RemoveVietnameseSigns(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            str = str.ToLower().Trim();
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return str;
        }

        //private void Filters_Changed(object sender, RoutedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (_allKhachHangList == null || !_allKhachHangList.Any()) return;

            // 1. Lấy thông tin từ giao diện
            if (FindName("txtSearch") is TextBox txt)
                _currentSearchKey = string.IsNullOrWhiteSpace(txt.Text) ? "" : RemoveVietnameseSigns(txt.Text);

            _currentCmbIndex = (FindName("cmbLoaiTK") as ComboBox)?.SelectedIndex ?? 0;
            _currentHideLocked = (FindName("chkHideLocked") as CheckBox)?.IsChecked == true;

            var filtered = _allKhachHangList.AsEnumerable();

            // 2. Lọc theo từ khóa
            if (!string.IsNullOrEmpty(_currentSearchKey))
            {
                filtered = filtered.Where(x => x.SearchKeyword.Contains(_currentSearchKey));
            }

            // 3. Lọc theo Loại TK (Combobox)
            if (_currentCmbIndex == 1) filtered = filtered.Where(x => x.TaiKhoanTam && !x.DaXoa);
            if (_currentCmbIndex == 2) filtered = filtered.Where(x => !x.TaiKhoanTam && !x.DaXoa);
            if (_currentCmbIndex == 3) filtered = filtered.Where(x => x.BiKhoa && !x.DaXoa);
            if (_currentCmbIndex == 4) filtered = filtered.Where(x => x.DaXoa);
            if (_currentCmbIndex == 0) filtered = filtered.Where(x => !x.DaXoa);

            // 4. Lọc ẩn tài khoản khóa
            if (_currentHideLocked)
            {
                filtered = filtered.Where(x => !x.BiKhoa);
            }

            // ============================================================
            // 5. PHỤC HỒI LOGIC SẮP XẾP CHUẨN (Bằng LINQ)
            // Ưu tiên: Chưa xóa -> Chưa khóa -> Mới nhất (IdKhachHang giảm dần)
            // ============================================================
            filtered = filtered.OrderBy(x => x.DaXoa)
                               .ThenBy(x => x.BiKhoa)
                               .ThenByDescending(x => x.IdKhachHang);

            // 6. Gán thẳng vào DataGrid
            if (FindName("dgKhachHang") is DataGrid dg)
            {
                dg.ItemsSource = filtered.ToList();
            }
        }

        private async void DgKhachHang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgKhachHang") is DataGrid dg && dg.SelectedItem is QuanLyKhachHangGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;

                var loading = FindName("LoadingOverlay") as Border;
                if (loading != null) loading.Visibility = Visibility.Visible;
                try
                {
                    var detail = await ApiClient.Instance.GetFromJsonAsync<QuanLyKhachHangDetailDto>($"api/app/quanly-khachhang/{item.IdKhachHang}");
                    if (detail != null)
                    {
                        _selectedKhachHang = detail;
                        if (FindName("txtHoTen") is TextBox t1) t1.Text = detail.HoTen;
                        if (FindName("txtSdt") is TextBox t2) t2.Text = detail.SoDienThoai;
                        if (FindName("txtDiem") is TextBox t3) t3.Text = detail.DiemTichLuy.ToString("N0");
                        if (FindName("txtEmail") is TextBox t4) t4.Text = detail.Email;
                        if (FindName("txtDiaChi") is TextBox t5) t5.Text = detail.DiaChi;

                        if (FindName("txtTrangThai") is TextBlock txtTT)
                        {
                            if (detail.DaXoa)
                            {
                                txtTT.Text = "Trạng thái: Đã xóa";
                                txtTT.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                                if (FindName("btnKhoa") is Button bk) bk.Visibility = Visibility.Collapsed;
                                if (FindName("btnMoKhoa") is Button bm) bm.Visibility = Visibility.Collapsed;
                                if (FindName("btnXoa") is Button bx) bx.Visibility = Visibility.Collapsed;
                                if (FindName("btnSuaDiem") is Button bsd) bsd.Visibility = Visibility.Collapsed;
                            }
                            else if (detail.BiKhoa)
                            {
                                txtTT.Text = $"Bị khóa đến: {detail.ThoiGianMoKhoa?.ToString("dd/MM/yyyy HH:mm") ?? "Vĩnh viễn"}\nLý do: {detail.LyDoKhoa}";
                                txtTT.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                                if (FindName("btnKhoa") is Button bk) bk.Visibility = Visibility.Collapsed;
                                if (FindName("btnMoKhoa") is Button bm) bm.Visibility = Visibility.Visible;
                                if (FindName("btnXoa") is Button bx) bx.Visibility = Visibility.Visible;
                                if (FindName("btnSuaDiem") is Button bsd) bsd.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                txtTT.Text = "Trạng thái: Hoạt động";
                                txtTT.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                                if (FindName("btnKhoa") is Button bk) bk.Visibility = Visibility.Visible;
                                if (FindName("btnMoKhoa") is Button bm) bm.Visibility = Visibility.Collapsed;
                                if (FindName("btnXoa") is Button bx) bx.Visibility = Visibility.Visible;
                                if (FindName("btnSuaDiem") is Button bsd) bsd.Visibility = Visibility.Visible;
                            }
                        }

                        if (FindName("AvatarPreview") is Image img)
                        {
                            string fullUrl = string.IsNullOrEmpty(detail.AnhDaiDien) ? "" : $"{(AppConfigManager.GetApiServerUrl() ?? "http://localhost").TrimEnd('/')}/{detail.AnhDaiDien.TrimStart('/')}";
                            img.Source = HinhAnhHelper.LoadImage(fullUrl, HinhAnhPaths.DefaultAvatar);
                        }
                        if (FindName("dgLichSuMua") is DataGrid dgMua)
                            dgMua.ItemsSource = detail.LichSuMuaHang;

                        if (FindName("dgLichSuThue") is DataGrid dgThue)
                            dgThue.ItemsSource = detail.LichSuThueSach;

                        if (FindName("panelLichSu") is StackPanel panelLS)
                            panelLS.Visibility = Visibility.Visible;
                    }
                }
                finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
            }
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ NÚT BẤM VÀ POPUP
        // ==========================================
        private void BtnHuyPopup_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("PopupKhoa") is Border pk) pk.Visibility = Visibility.Collapsed;
            if (FindName("PopupDiem") is Border pd) pd.Visibility = Visibility.Collapsed;
            if (FindName("PopupXemAnh") is Border pa) pa.Visibility = Visibility.Collapsed;
        }

        private void BtnKhoa_Click(object sender, RoutedEventArgs e) { if (FindName("PopupKhoa") is Border p) p.Visibility = Visibility.Visible; }
        private void BtnSuaDiem_Click(object sender, RoutedEventArgs e) { if (FindName("PopupDiem") is Border p) p.Visibility = Visibility.Visible; }

        private void Avatar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selectedKhachHang == null) return;
            if (FindName("PopupXemAnh") is Border popup && FindName("ImgFullAvatar") is Image imgFull && FindName("AvatarPreview") is Image imgThumb)
            {
                imgFull.Source = imgThumb.Source;
                popup.Visibility = Visibility.Visible;
            }
        }

        private async void BtnXacNhanKhoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHACH_HANG")) return;
            string lyDo = (FindName("txtLyDoKhoa") as TextBox)?.Text.Trim() ?? "";
            int? soNgay = null;
            if (FindName("cmbThoiGianKhoa") is ComboBox cmb && cmb.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int ngay) && ngay > 0)
                soNgay = ngay;

            if (FindName("PopupKhoa") is Border p) p.Visibility = Visibility.Collapsed;

            var loading = FindName("LoadingOverlay") as Border;
            if (loading != null) loading.Visibility = Visibility.Visible;
            try
            {
                var req = new KhoaKhachHangRequestDto { LyDoKhoa = lyDo, SoNgayKhoa = soNgay };
                await ApiClient.Instance.PostAsJsonAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}/khoa", req);
                MessageBox.Show("Khóa thành công. Hệ thống đang gửi Email.");

                _ = BackgroundRefreshAsync();
            }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXacNhanDiem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHACH_HANG")) return;
            string diemStr = (FindName("txtDiemThayDoi") as TextBox)?.Text.Trim() ?? "0";
            if (!int.TryParse(diemStr, out int diemThayDoi)) { MessageBox.Show("Sai định dạng số!"); return; }

            if (FindName("PopupDiem") is Border p) p.Visibility = Visibility.Collapsed;

            var loading = FindName("LoadingOverlay") as Border;
            if (loading != null) loading.Visibility = Visibility.Visible;
            try
            {
                var req = new CapNhatDiemKhachHangDto { DiemThayDoi = diemThayDoi, LyDo = "Cập nhật thủ công" };
                await ApiClient.Instance.PostAsJsonAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}/diem", req);
                MessageBox.Show("Cập nhật điểm thành công!");

                _ = BackgroundRefreshAsync();
            }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private async void BtnMoKhoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHACH_HANG")) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn mở khóa cho tài khoản '{_selectedKhachHang.HoTen}' không?", "Xác nhận mở khóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var loading = FindName("LoadingOverlay") as Border;
            if (loading != null) loading.Visibility = Visibility.Visible;
            try
            {
                await ApiClient.Instance.PostAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}/mokhoa", null);
                MessageBox.Show("Đã mở khóa tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                _ = BackgroundRefreshAsync();
            }
            finally { if (loading != null) loading.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedKhachHang == null || !AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHACH_HANG")) return;

            string thongBao = $"Bạn có chắc chắn muốn xóa khách hàng '{_selectedKhachHang.HoTen}' không?\n\n" +
                              $"LƯU Ý (Hệ thống áp dụng Xóa Mềm):\n" +
                              $"❌ BỊ XÓA (Ẩn đi): Khách hàng này sẽ biến mất khỏi danh sách và không thể đăng nhập hay tạo giao dịch mới.\n" +
                              $"✅ ĐƯỢC GIỮ LẠI: Toàn bộ lịch sử mua hàng, hóa đơn và phiếu thuê sách cũ của khách này vẫn được bảo lưu an toàn trong cơ sở dữ liệu để đảm bảo tính chính xác của báo cáo doanh thu.";

            if (MessageBox.Show(thongBao, "Xác nhận Xóa Khách Hàng", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var loading = FindName("LoadingOverlay") as Border;
                if (loading != null) loading.Visibility = Visibility.Visible;
                try
                {
                    var response = await ApiClient.Instance.DeleteAsync($"api/app/quanly-khachhang/{_selectedKhachHang.IdKhachHang}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Đã xóa khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (FindName("formChiTiet") is StackPanel f) f.IsEnabled = false;

                        _ = BackgroundRefreshAsync();
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi xảy ra khi xóa khách hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi API", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (loading != null) loading.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_allKhachHangList == null || !_allKhachHangList.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Title = "Lưu Danh Sách Khách Hàng",
                Filter = "Excel Workbook|*.xlsx",
                FileName = $"KhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Cafebook Admin");
                    FileInfo fileInfo = new FileInfo(sfd.FileName);
                    if (fileInfo.Exists) fileInfo.Delete();

                    using (var package = new ExcelPackage(fileInfo))
                    {
                        var ws = package.Workbook.Worksheets.Add("Danh sách Khách Hàng");

                        ws.Cells["A1"].Value = "DANH SÁCH KHÁCH HÀNG CAFEBOOK";
                        ws.Cells["A1:G1"].Merge = true;
                        ws.Cells["A1"].Style.Font.Size = 16;
                        ws.Cells["A1"].Style.Font.Bold = true;
                        ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Row(1).Height = 30;

                        ws.Cells["A2"].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        ws.Cells["A2:G2"].Merge = true;
                        ws.Cells["A2"].Style.Font.Italic = true;
                        ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells["A4"].Value = "BẢNG THỐNG KÊ TỔNG QUAN";
                        ws.Cells["A4:B4"].Merge = true;
                        ws.Cells["A4"].Style.Font.Bold = true;
                        ws.Cells["A4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells["A4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        ws.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        int tongKhach = _allKhachHangList.Count;
                        int khachVangLai = _allKhachHangList.Count(k => k.TaiKhoanTam);
                        int khachBiKhoa = _allKhachHangList.Count(k => k.BiKhoa);

                        var khachVip = _allKhachHangList.OrderByDescending(k => k.DiemTichLuy).FirstOrDefault();
                        string thongTinKhachVip = khachVip != null ? $"{khachVip.HoTen} ({khachVip.DiemTichLuy:N0} điểm)" : "Chưa có dữ liệu";

                        ws.Cells["A5"].Value = "Tổng khách hàng:";
                        ws.Cells["B5"].Value = tongKhach;
                        ws.Cells["A6"].Value = "Khách vãng lai:";
                        ws.Cells["B6"].Value = khachVangLai;
                        ws.Cells["A7"].Value = "Tài khoản bị khóa:";
                        ws.Cells["B7"].Value = khachBiKhoa;
                        ws.Cells["A8"].Value = "KH điểm cao nhất:";
                        ws.Cells["B8"].Value = thongTinKhachVip;

                        ws.Cells["A5:A8"].Style.Font.Bold = true;

                        int startRow = 11; 
                        ws.Cells[$"A{startRow}"].Value = "Mã KH";
                        ws.Cells[$"B{startRow}"].Value = "Tên Khách Hàng";
                        ws.Cells[$"C{startRow}"].Value = "SĐT";
                        ws.Cells[$"D{startRow}"].Value = "Email";
                        ws.Cells[$"E{startRow}"].Value = "Loại Tài Khoản";
                        ws.Cells[$"F{startRow}"].Value = "Điểm TL";
                        ws.Cells[$"G{startRow}"].Value = "Trạng Thái";

                        var danhSachDaSapXep = _allKhachHangList
                            .OrderBy(x => x.BiKhoa)
                            .ThenByDescending(x => x.DiemTichLuy) 
                            .ToList();

                        int currentRow = startRow + 1;
                        foreach (var item in danhSachDaSapXep)
                        {
                            ws.Cells[$"A{currentRow}"].Value = item.IdKhachHang;
                            ws.Cells[$"B{currentRow}"].Value = item.HoTen;
                            ws.Cells[$"C{currentRow}"].Value = item.SoDienThoai;
                            ws.Cells[$"D{currentRow}"].Value = item.Email;
                            ws.Cells[$"E{currentRow}"].Value = item.LoaiTaiKhoan;
                            ws.Cells[$"F{currentRow}"].Value = item.DiemTichLuy;
                            ws.Cells[$"G{currentRow}"].Value = item.TrangThai;
                            if (item.BiKhoa)
                            {
                                ws.Cells[$"A{currentRow}:G{currentRow}"].Style.Font.Bold = true;
                                ws.Cells[$"A{currentRow}:G{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            }
                            else
                            {
                                ws.Cells[$"G{currentRow}"].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                            }

                            currentRow++;
                        }

                        var tableRange = ws.Cells[startRow, 1, currentRow - 1, 7];
                        var table = ws.Tables.Add(tableRange, "TableKhachHang");
                        table.TableStyle = TableStyles.Medium9;

                        ws.Cells[ws.Dimension.Address].AutoFitColumns();

                        package.Save();
                    }

                    var result = MessageBox.Show($"Đã xuất file Excel chuẩn tại:\n{sfd.FileName}\n\n• Chọn [Yes] để mở trực tiếp.\n• Chọn [No] để mở thư mục.", "Hoàn Tất", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                    else if (result == MessageBoxResult.No) System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\"");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi Excel: " + ex.Message); }
            }
        }

        private void BtnNavKhuyenMai_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_KHUYEN_MAI"))
                this.NavigationService?.Navigate(new QuanLyKhuyenMaiView());
            else
                MessageBox.Show("Bạn không có quyền truy cập trang Khuyến mãi!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnNavDeXuat_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_DE_XUAT"))
                this.NavigationService?.Navigate(new QuanLyDeXuatView());
            else
                MessageBox.Show("Bạn không có quyền truy cập trang Đề xuất!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}