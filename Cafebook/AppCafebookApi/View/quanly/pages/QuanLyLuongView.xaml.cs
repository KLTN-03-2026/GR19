using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyLuongView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyLuongBangKeDto> _previewList = new();
        private List<ThuongPhatMauLookupDto> _thuongPhatMauList = new();
        private QuanLyLuongBangKeDto? _selectedNhanVien = null;
        private DateTime _tuNgay;
        private DateTime _denNgay;

        //static QuanLyLuongView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyLuongView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_QL", "QL_LUONG", "QL_PHAT_LUONG", "QL_CHAM_CONG", "QL_THUONG_PHAT"))
            {
                MessageBox.Show("Từ chối truy cập module Quản lý Lương!");
                this.NavigationService?.GoBack(); return;
            }

            bool hasQuyen = AuthService.CoQuyen("FULL_QL", "QL_LUONG");
            if (FindName("GridDuLieu") is Grid g) g.Visibility = hasQuyen ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQuyen ? Visibility.Collapsed : Visibility.Visible;
            if (FindName("BtnNavChamCong") is Button btnChamCong)
                btnChamCong.Visibility = AuthService.CoQuyen("FULL_QL", "QL_CHAM_CONG") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("BtnNavPhatLuong") is Button btnPhatLuong)
                btnPhatLuong.Visibility = AuthService.CoQuyen("FULL_QL", "QL_PHAT_LUONG") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("BtnNavThuongPhat") is Button btnThuongPhat)
                btnThuongPhat.Visibility = AuthService.CoQuyen("FULL_QL", "QL_THUONG_PHAT") ? Visibility.Visible : Visibility.Collapsed;

            if (hasQuyen)
            {
                // Thay đổi load data cho Combobox Tháng thay vì Tuần
                if (FindName("cmbNam") is ComboBox cNam && FindName("cmbThang") is ComboBox cThang)
                {
                    int currentYear = DateTime.Now.Year;
                    for (int i = currentYear - 2; i <= currentYear + 1; i++) cNam.Items.Add(i);
                    for (int i = 1; i <= 12; i++) cThang.Items.Add(i);

                    cNam.SelectedItem = currentYear;
                    cThang.SelectedItem = DateTime.Now.Month;
                }

                await LoadThuongPhatMauAsync();
            }
        }

        private async Task LoadThuongPhatMauAsync()
        {
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<ThuongPhatMauLookupDto>>("api/app/quanly-luong/thuong-phat-mau");
                if (res != null && FindName("cmbThuongPhatMau") is ComboBox cmb)
                {
                    _thuongPhatMauList = res;
                    cmb.ItemsSource = _thuongPhatMauList;
                }
            }
            catch { }
        }

        private void CmbThoiGian_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("cmbNam") is ComboBox cNam && cNam.SelectedItem != null &&
                FindName("cmbThang") is ComboBox cThang && cThang.SelectedItem != null)
            {
                int year = (int)cNam.SelectedItem;
                int month = (int)cThang.SelectedItem;

                // Tính từ ngày đầu tháng đến ngày cuối tháng
                _tuNgay = new DateTime(year, month, 1);
                _denNgay = _tuNgay.AddMonths(1).AddDays(-1);

                if (FindName("txtKhoangThoiGian") is TextBlock txt)
                    txt.Text = $"(Từ {_tuNgay:dd/MM/yyyy} đến {_denNgay:dd/MM/yyyy})";
            }
        }

        private async void BtnTamTinh_Click(object sender, RoutedEventArgs e) => await ReloadPreviewAsync();

        private async Task ReloadPreviewAsync(bool silent = false)
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                string url = $"api/app/quanly-luong/preview?tuNgay={_tuNgay:yyyy-MM-dd}&denNgay={_denNgay:yyyy-MM-dd}";
                var response = await ApiClient.Instance.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    _previewList = await response.Content.ReadFromJsonAsync<List<QuanLyLuongBangKeDto>>() ?? new();
                    if (FindName("dgBangKe") is DataGrid dg) dg.ItemsSource = _previewList;
                    if (FindName("btnChotLuong") is Button btn) btn.IsEnabled = _previewList.Any();

                    if (_selectedNhanVien != null)
                    {
                        var updatedEmp = _previewList.FirstOrDefault(x => x.IdNhanVien == _selectedNhanVien.IdNhanVien);
                        if (updatedEmp != null && FindName("dgChiTietThuongPhat") is DataGrid dChiTiet)
                        {
                            _selectedNhanVien = updatedEmp;
                            dChiTiet.ItemsSource = updatedEmp.DanhSachThuongPhat;
                        }
                    }

                    if (!silent) MessageBox.Show("Hệ thống đã tự động tính Chuyên cần, Tăng ca và Vi phạm.\nBạn có thể thêm Thưởng/Phạt thủ công ở cột bên phải.", "Tạm tính hoàn tất");
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "Cảnh báo");
                    if (FindName("dgBangKe") is DataGrid dg) dg.ItemsSource = null;
                    if (FindName("btnChotLuong") is Button btn) btn.IsEnabled = false;
                    if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = false;
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void DgBangKe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgBangKe") is DataGrid dg && dg.SelectedItem is QuanLyLuongBangKeDto item)
            {
                _selectedNhanVien = item;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTenNhanVien") is TextBlock lbl) lbl.Text = item.TenNhanVien;
                if (FindName("dgChiTietThuongPhat") is DataGrid dChiTiet) dChiTiet.ItemsSource = item.DanhSachThuongPhat;

                ResetFormThuongPhat();
            }
        }

        private void CmbThuongPhatMau_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("cmbThuongPhatMau") is ComboBox cmb && cmb.SelectedItem is ThuongPhatMauLookupDto mau)
            {
                if (FindName("cmbLoaiThuongPhat") is ComboBox cbLoai) cbLoai.Text = mau.Loai;
                if (FindName("txtSoTien") is TextBox txtTien) txtTien.Text = mau.SoTien.ToString("0.##");
                if (FindName("txtLyDoThuongPhat") is TextBox txtLyDo) txtLyDo.Text = mau.TenMau;
            }
        }

        private void ResetFormThuongPhat()
        {
            if (FindName("cmbThuongPhatMau") is ComboBox cmb) cmb.SelectedItem = null;
            if (FindName("cmbLoaiThuongPhat") is ComboBox cbLoai) cbLoai.SelectedIndex = 0;
            if (FindName("txtSoTien") is TextBox t1) t1.Text = "";
            if (FindName("txtLyDoThuongPhat") is TextBox t2) t2.Text = "";
        }

        private async void BtnThemThuongPhat_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNhanVien == null) return;
            if (!decimal.TryParse((FindName("txtSoTien") as TextBox)?.Text, out decimal soTien) || soTien <= 0) { MessageBox.Show("Số tiền phải > 0"); return; }
            string lyDo = (FindName("txtLyDoThuongPhat") as TextBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(lyDo)) { MessageBox.Show("Vui lòng nhập lý do"); return; }

            var dto = new TaoThuongPhatDto
            {
                IdNhanVien = _selectedNhanVien.IdNhanVien,
                Loai = (FindName("cmbLoaiThuongPhat") as ComboBox)?.Text ?? "Thưởng",
                SoTien = soTien,
                LyDo = lyDo,
                IdNguoiTao = 1 // Ghi chú: Nếu hệ thống có UserID chuẩn, truyền AuthService.CurrentUserId vào đây
            };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-luong/thuong-phat", dto);
                if (res.IsSuccessStatusCode)
                {
                    await ReloadPreviewAsync(true);
                    ResetFormThuongPhat();
                    MessageBox.Show("Đã thêm khoản thủ công thành công!");
                }
                else
                {
                    string errorMsg = await res.Content.ReadAsStringAsync();
                    MessageBox.Show($"Lỗi máy chủ: {errorMsg}");
                }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoaThuongPhat_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("dgChiTietThuongPhat") is DataGrid dg && dg.SelectedItem is ChiTietThuongPhatDto item)
            {
                if (item.IsAuto) { MessageBox.Show("Không thể xóa khoản Tự động. Hãy sửa chấm công thay thế."); return; }
                if (MessageBox.Show("Xóa khoản này?", "Cảnh báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                    try
                    {
                        var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-luong/thuong-phat/{item.Id}");
                        if (res.IsSuccessStatusCode) { await ReloadPreviewAsync(true); }
                    }
                    finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
                }
            }
        }

        private async void BtnChotLuong_Click(object sender, RoutedEventArgs e)
        {
            if (!_previewList.Any()) return;
            if (MessageBox.Show("Chốt lương sẽ lưu dữ liệu vào CSDL và không thể hoàn tác. Tiếp tục?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var dto = new QuanLyLuongChotRequestDto
                    {
                        TuNgay = _tuNgay,
                        DanhSachChot = _previewList
                    };

                    HttpResponseMessage res = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-luong/chot-luong", dto);
                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Chốt lương thành công!");
                        _previewList.Clear();
                        if (FindName("dgBangKe") is DataGrid dg) dg.ItemsSource = null;
                        if (FindName("btnChotLuong") is Button btn) btn.IsEnabled = false;
                        if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = false;
                    }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnNavChamCong_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_CHAM_CONG")) this.NavigationService?.Navigate(new QuanLyChamCongView());
        }

        private void BtnNavPhatLuong_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_PHAT_LUONG")) this.NavigationService?.Navigate(new QuanLyPhatLuongView());
        }

        private void BtnNavThuongPhat_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.CoQuyen("FULL_QL", "QL_THUONG_PHAT")) this.NavigationService?.Navigate(new QuanLyThuongPhatView());
        }
    }
}