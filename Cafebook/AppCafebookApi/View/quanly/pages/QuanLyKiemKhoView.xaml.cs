using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyKiemKhoView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyKiemKhoGridDto> _phieuKiemList = new();
        private ObservableCollection<QuanLyKiemKhoNguyenLieuDto> _nlKiemKhoList = new();
        private bool _isViewing = true;

        //static QuanLyKiemKhoView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyKiemKhoView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            // BẢO MẬT LỚP 2
            if (!AuthService.CoQuyen("QL_KIEM_KHO")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            if (FindName("dgKiemKhoMoi") is DataGrid dgMoi) dgMoi.ItemsSource = _nlKiemKhoList;
            await LoadPhieuKiemAsync();
        }

        private void ApplyPermissions()
        {
            // BẢO MẬT LỚP 1 VÀ FINDNAME
            bool canEdit = AuthService.CoQuyen("QL_KIEM_KHO");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadPhieuKiemAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyKiemKhoGridDto>>("api/app/quanly-kiemkho");
                if (res != null && FindName("dgPhieuKiem") is DataGrid dg) dg.ItemsSource = res;
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void DgPhieuKiem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhieuKiem") is DataGrid dg && dg.SelectedItem is QuanLyKiemKhoGridDto item)
            {
                _isViewing = true;
                if (FindName("panelTaoMoi") is StackPanel p1) p1.Visibility = Visibility.Collapsed;
                if (FindName("panelChiTiet") is StackPanel p2) p2.Visibility = Visibility.Visible;
                if (FindName("lblTitleChiTiet") is TextBlock title) title.Text = $"Chi tiết Phiếu Kiểm #{item.IdPhieuKiemKho} - {item.NgayKiem:dd/MM/yyyy HH:mm}";

                try
                {
                    var detail = await ApiClient.Instance.GetFromJsonAsync<QuanLyKiemKhoDetailDto>($"api/app/quanly-kiemkho/{item.IdPhieuKiemKho}");
                    if (detail != null && FindName("dgChiTiet") is DataGrid dgc) dgc.ItemsSource = detail.ChiTiet;
                }
                catch { }
            }
        }

        private async void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_KIEM_KHO")) return;
            _isViewing = false;
            if (FindName("dgPhieuKiem") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("panelChiTiet") is StackPanel p1) p1.Visibility = Visibility.Collapsed;
            if (FindName("panelTaoMoi") is StackPanel p2) p2.Visibility = Visibility.Visible;

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var data = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyKiemKhoNguyenLieuDto>>("api/app/quanly-kiemkho/lookup-nl");
                if (data != null)
                {
                    _nlKiemKhoList.Clear();
                    foreach (var it in data) _nlKiemKhoList.Add(it);
                }
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_KIEM_KHO") || _isViewing) return;
            if (_nlKiemKhoList.Count == 0) return;

            if (MessageBox.Show("Hệ thống sẽ lưu phiếu và tự động Cập nhật lại Tồn Kho thực tế theo số liệu bạn đã nhập. Bạn có chắc chắn?", "Xác nhận cân bằng kho", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            var dto = new QuanLyKiemKhoSaveDto
            {
                ChiTiet = _nlKiemKhoList.Select(x => new QuanLyChiTietKiemKhoSaveDto
                {
                    IdNguyenLieu = x.IdNguyenLieu,
                    TonKhoHeThong = x.TonKhoHeThong,
                    TonKhoThucTe = x.TonKhoThucTe,
                    LyDoChenhLech = x.LyDoChenhLech ?? ""
                }).ToList()
            };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-kiemkho", dto);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Kiểm kho và Cân bằng kho thành công!");
                    if (FindName("panelTaoMoi") is StackPanel p2) p2.Visibility = Visibility.Collapsed;
                    await LoadPhieuKiemAsync();
                }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void TxtSearchNLKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FindName("dgKiemKhoMoi") is DataGrid dg && dg.ItemsSource != null)
            {
                string keyword = (FindName("txtSearchNLKiem") as TextBox)?.Text.ToLower() ?? "";

                // Lấy View hiện tại của DataGrid để lọc (Lọc View không làm mất/thay đổi dữ liệu gốc trong _nlKiemKhoList)
                var view = CollectionViewSource.GetDefaultView(dg.ItemsSource);

                view.Filter = (item) =>
                {
                    if (item is QuanLyKiemKhoNguyenLieuDto dto)
                    {
                        if (string.IsNullOrEmpty(keyword)) return true; // Hiện tất cả nếu ô tìm kiếm trống
                        return dto.TenNguyenLieu.ToLower().Contains(keyword);
                    }
                    return false;
                };

                view.Refresh(); // Làm mới DataGrid
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}