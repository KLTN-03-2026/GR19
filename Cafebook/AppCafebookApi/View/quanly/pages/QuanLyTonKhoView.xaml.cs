using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;
// using AppCafebookApi.View.common; // Nếu cần để gọi Báo Cáo

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyTonKhoView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyTonKhoDto> _tonKhoList = new();

        //static QuanLyTonKhoView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }
        public QuanLyTonKhoView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // BẢO MẬT LỚP 1: Chìa khóa Cổng
            bool hasAnyPermission = AuthService.CoQuyen("FULL_QL", "QL_TON_KHO", "QL_NGUYEN_LIEU", "QL_NHAP_KHO", "QL_XUAT_HUY", "QL_KIEM_KHO", "QL_NHA_CUNG_CAP", "QL_DON_VI_CHUYEN_DOI");
            if (!hasAnyPermission)
            {
                MessageBox.Show("Bạn không có quyền truy cập phân hệ Kho!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            // BẢO MẬT LỚP 2: Chìa khóa Phòng (Xem danh sách Tồn kho)
            if (AuthService.CoQuyen("FULL_QL", "QL_TON_KHO"))
            {
                await LoadTonKhoAsync();
            }
            else
            {
                if (FindName("GridDuLieuKho") is Grid gridData) gridData.Visibility = Visibility.Collapsed;
                if (FindName("txtThongBaoKhongCoQuyen") is Border txtThongBao) txtThongBao.Visibility = Visibility.Visible;
            }
        }

        private void ApplyPermissions()
        {
            // BẢO MẬT LỚP 1 + FINDNAME: Ẩn/hiện nút điều hướng theo quyền chi tiết
            if (FindName("btnNavNL") is Button b1) b1.Visibility = AuthService.CoQuyen("QL_NGUYEN_LIEU") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavNhap") is Button b2) b2.Visibility = AuthService.CoQuyen("QL_NHAP_KHO") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavXuat") is Button b3) b3.Visibility = AuthService.CoQuyen("QL_XUAT_HUY") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavKiem") is Button b4) b4.Visibility = AuthService.CoQuyen("QL_KIEM_KHO") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavNCC") is Button b5) b5.Visibility = AuthService.CoQuyen("QL_NHA_CUNG_CAP") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadTonKhoAsync()
        {
            if (FindName("LoadingOverlay") is Border l1) l1.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyTonKhoDto>>("api/app/quanly-kho/tonkho");
                if (res != null) { _tonKhoList = res; FilterData(); }
            }
            catch { MessageBox.Show("Lỗi kết nối API"); }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            // Lọc DataGrid Tồn Kho
            if (FindName("dgTonKho") is DataGrid dg1)
            {
                string key = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
                dg1.ItemsSource = string.IsNullOrEmpty(key) ? _tonKhoList : _tonKhoList.Where(x => x.TenNguyenLieu.ToLower().Contains(key)).ToList();
            }

            // Gán dữ liệu cho DataGrid Cảnh báo (bên phải)
            if (FindName("dgCanhBao") is DataGrid dg2)
            {
                dg2.ItemsSource = _tonKhoList.Where(nl => nl.TinhTrang == "Sắp hết" || nl.TinhTrang == "Hết hàng").ToList();
            }
        }

        // ==========================================
        // ĐIỀU HƯỚNG MÔ ĐUN CON (Bảo mật Lớp 2)
        // ==========================================
        private void BtnNavNL_Click(object sender, RoutedEventArgs e) {
            if (AuthService.CoQuyen("FULL_QL", "QL_NGUYEN_LIEU")) 
                this.NavigationService?.Navigate(new QuanLyNguyenLieuView()); 
        }
        private void BtnNavNhap_Click(object sender, RoutedEventArgs e) { 
            if (AuthService.CoQuyen("FULL_QL", "QL_NHAP_KHO")) 
                this.NavigationService?.Navigate(new QuanLyNhapKhoView()); 
        }
        private void BtnNavXuat_Click(object sender, RoutedEventArgs e) { 
            if (AuthService.CoQuyen("FULL_QL", "QL_XUAT_HUY")) 
                this.NavigationService?.Navigate(new QuanLyXuatHuyView()); 
        }
        private void BtnNavKiem_Click(object sender, RoutedEventArgs e) { 
            if (AuthService.CoQuyen("FULL_QL", "QL_KIEM_KHO")) 
                this.NavigationService?.Navigate(new QuanLyKiemKhoView()); 
        }
        private void BtnNavNCC_Click(object sender, RoutedEventArgs e) { 
            if (AuthService.CoQuyen("FULL_QL", "QL_NHA_CUNG_CAP")) 
                this.NavigationService?.Navigate(new QuanLyNhaCungCapView()); 
        }
    }
}