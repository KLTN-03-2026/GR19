using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyXuatHuyView : Page
    {
        private static readonly HttpClient httpClient;
        private List<QuanLyXuatHuyGridDto> _phieuHuyList = new();
        private List<LookupXuatHuyDto> _nlList = new();
        private ObservableCollection<QuanLyChiTietXuatHuyDto> _chiTietList = new();

        private bool _isViewing = false;

        static QuanLyXuatHuyView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyXuatHuyView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("QL_XUAT_HUY")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            if (FindName("dgChiTiet") is DataGrid dg) dg.ItemsSource = _chiTietList;
            await LoadMasterDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_XUAT_HUY");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnThemNL") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadMasterDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var nl = await httpClient.GetFromJsonAsync<List<LookupXuatHuyDto>>("api/app/quanly-xuathuy/lookup-nl");
                if (nl != null && FindName("cmbNguyenLieu") is ComboBox cb) { _nlList = nl; cb.ItemsSource = _nlList; }

                await LoadPhieuHuyAsync();
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async Task LoadPhieuHuyAsync()
        {
            try
            {
                var res = await httpClient.GetFromJsonAsync<List<QuanLyXuatHuyGridDto>>("api/app/quanly-xuathuy");
                if (res != null) { _phieuHuyList = res; FilterData(); }
            }
            catch { }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();
        private void FilterData()
        {
            if (!(FindName("dgPhieuHuy") is DataGrid dg)) return;
            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            dg.ItemsSource = string.IsNullOrEmpty(k) ? _phieuHuyList : _phieuHuyList.Where(x => x.LyDoHuy.ToLower().Contains(k) || x.IdPhieuXuatHuy.ToString().Contains(k)).ToList();
        }

        private async void DgPhieuHuy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhieuHuy") is DataGrid dg && dg.SelectedItem is QuanLyXuatHuyGridDto item)
            {
                _isViewing = true;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = $"Chi tiết Phiếu Hủy #{item.IdPhieuXuatHuy}";
                if (FindName("btnLuu") is Button btn) btn.Visibility = Visibility.Collapsed;
                if (FindName("btnThemNL") is Button btnA) btnA.IsEnabled = false;

                try
                {
                    var detail = await httpClient.GetFromJsonAsync<QuanLyXuatHuyDetailDto>($"api/app/quanly-xuathuy/{item.IdPhieuXuatHuy}");
                    if (detail != null)
                    {
                        if (FindName("txtLyDoChung") is TextBox t1) { t1.Text = detail.LyDoHuy; t1.IsReadOnly = true; }
                        _chiTietList.Clear();
                        foreach (var ct in detail.ChiTiet) _chiTietList.Add(ct);
                    }
                }
                catch { }
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_XUAT_HUY")) return;
            _isViewing = false;
            _chiTietList.Clear();
            if (FindName("dgPhieuHuy") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Tạo Phiếu Hủy Mới";
            if (FindName("btnLuu") is Button btn) btn.Visibility = Visibility.Visible;
            if (FindName("btnThemNL") is Button btnA) btnA.IsEnabled = true;

            if (FindName("txtLyDoChung") is TextBox t1) { t1.Text = ""; t1.IsReadOnly = false; }
            if (FindName("cmbNguyenLieu") is ComboBox c2) c2.SelectedItem = null;
            if (FindName("txtSoLuong") is TextBox t2) t2.Text = "";
        }

        private void BtnThemNL_Click(object sender, RoutedEventArgs e)
        {
            if (_isViewing) return;
            int idNl = (FindName("cmbNguyenLieu") as ComboBox)?.SelectedValue as int? ?? 0;
            string tenNl = (FindName("cmbNguyenLieu") as ComboBox)?.Text ?? "";
            decimal.TryParse((FindName("txtSoLuong") as TextBox)?.Text, out decimal sl);

            if (idNl == 0 || sl <= 0) { MessageBox.Show("Vui lòng nhập Nguyên liệu và Số lượng hủy hợp lệ."); return; }

            var exist = _chiTietList.FirstOrDefault(x => x.IdNguyenLieu == idNl);
            if (exist != null) { exist.SoLuong += sl; }
            else _chiTietList.Add(new QuanLyChiTietXuatHuyDto { IdNguyenLieu = idNl, TenNguyenLieu = tenNl, SoLuong = sl });

            // Reset input
            if (FindName("cmbNguyenLieu") is ComboBox c2) c2.SelectedItem = null;
            if (FindName("txtSoLuong") is TextBox t2) t2.Text = "";

            if (FindName("dgChiTiet") is DataGrid dg) dg.Items.Refresh();
        }

        private void BtnXoaNL_Click(object sender, RoutedEventArgs e)
        {
            if (_isViewing) return;
            if (sender is Button btn && btn.DataContext is QuanLyChiTietXuatHuyDto item)
            {
                _chiTietList.Remove(item);
            }
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_XUAT_HUY") || _isViewing) return;
            if (!_chiTietList.Any()) { MessageBox.Show("Phiếu hủy chưa có nguyên liệu nào!"); return; }

            string lyDoChung = (FindName("txtLyDoChung") as TextBox)?.Text ?? "";
            if (string.IsNullOrEmpty(lyDoChung)) { MessageBox.Show("Vui lòng nhập lý do hủy chung!"); return; }

            var dto = new QuanLyXuatHuySaveDto
            {
                LyDoHuy = lyDoChung,
                ChiTiet = _chiTietList.Select(x => new QuanLyChiTietXuatHuySaveDto { IdNguyenLieu = x.IdNguyenLieu, SoLuong = x.SoLuong }).ToList()
            };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await httpClient.PostAsJsonAsync("api/app/quanly-xuathuy", dto);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tạo phiếu xuất hủy thành công. Đã trừ tồn kho!");
                    BtnLamMoiForm_Click(this, new RoutedEventArgs());
                    await LoadPhieuHuyAsync();
                }
                else
                {
                    string error = await res.Content.ReadAsStringAsync();
                    MessageBox.Show($"Lỗi: {error}", "Từ chối thực hiện", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}